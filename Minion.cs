using System;
using System.Collections.Generic;
using System.Drawing;

namespace Rawr.Warlock.Minions
{
    /// <summary>
    /// The base class from which all warlock minions are derived.
    /// </summary>
    /// <remarks>
    /// Base stats extracted via the UnitStat() WOW API.
    /// Scaling stats are taken from PaperDollFrame.lua (with a nice summary at http://www.wowwiki.com/Warlock_minions)
    /// Health / Power modifiers extracted via UnitHealthModifier(), UnitMaxHealthModifier and UnitPowerModifier() API's.
    /// </remarks>
    public abstract class Minion
    {
        /// <summary>
        /// The general name of the minion - Imp / Voidwalker / Succubus, etc
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// A reference to the minion's owner so that we can obtain any talents/buffs that should also be applied to the minion.
        /// </summary>
        protected Character Master { get; set; }
        /// <summary>
        /// The owner's accumulated stats.
        /// </summary>
        protected Stats OwnerStats { get; set; }
        /// <summary>
        /// The minion's accumulated stats.
        /// </summary>
        protected Stats MinionStats { get; set; }

        #region base properties
        protected float BaseStrength;
        protected float BaseAgility;
        protected float BaseStamina;
        protected float BaseIntellect;
        protected float BaseSpirit;
        protected float BaseArmor;
        protected float BaseMinDamage;
        protected float BaseMaxDamage;
        protected float BaseHealth;
        protected float BasePower;
        protected float BaseMp5;
        protected float BaseAttackCrit;
        #endregion

        #region minion scaling factors
        /// <summary>
        /// minion inherits 75% of the master's stamina.
        /// </summary>
        protected const float InheritedStamina = 0.75f;
        /// <summary>
        /// minion inherits 30% of the master's intellect.
        /// </summary>
        protected const float InheritedIntellect = 0.30f;
        /// <summary>
        /// minion inherits 35% of the master's armor.
        /// </summary>
        protected const float InheritedArmor = 0.35f;
        /// <summary>
        /// minion inherits 40% of the master's resistances.
        /// </summary>
        protected const float InheritedResistances = 0.40f;
        /// <summary>
        /// minion inherits 15% of the master's spell power.
        /// </summary>
        protected const float InheritedSpellDamage = 0.15f;
        /// <summary>
        /// minion attack power is increased by 57% of the master's spell power.
        /// </summary>
        protected const float InheritedAttackPower = 0.57f;
        /// <summary>
        /// The minion's crit chance is increased by 10/20/30% of the master's crit chance,
        /// but only if the master has the Improved Demonic Tactics talent(s).
        /// </summary>
        protected float InheritedCriticalStrikeChance;
        /// <summary>
        /// minion inherits 100% of the master's spell penetration.
        /// </summary>
        protected const float InheritedSpellPenetration = 1.00f;
        /// <summary>
        /// minion inherits 40% of the master's resilience.
        /// </summary>
        protected const float InheritedResilience = 0.40f;
        /// <summary>
        /// minion inherits 100% of the master's hit chance - so if the master is hit capped, then the minion is too.
        /// This applies to Expertise too (according to the 3.2 patch notes), however warlocks dont have expertise ...
        /// </summary>
        protected const float InheritedHitChance = 1.00f;
        #endregion

        #region minion health/power modifiers
        /// <summary>
        /// The minion's health modifier.
        /// </summary>
        protected float HealthModifier;
        /// <summary>
        /// The minion's maxhealth modifier.
        /// </summary>
        protected float MaxHealthModifier;
        /// <summary>
        /// The minion's power modifier.
        /// </summary>
        protected float PowerModifier;
        #endregion

        #region constants - most of these are defined in the WoW LUA source code
        /// <summary>
        /// A constant value to be used in the conversion of attack power to damage.
        /// </summary>
        protected const float AttackPowerMagicNumber = 14;
        /// <summary>
        /// The amount of health gained per point of stamina.
        /// </summary>
        protected const float HealthPerStamina = 10;
        /// <summary>
        /// The amount of mana gained per point of intellect.
        /// </summary>
        protected const float ManaPerIntellect = 15;
        /// <summary>
        /// The amount of mana regenerated per point of spirit.
        /// </summary>
        protected const float ManaRegenPerSpirit = 0.2f;
        /// <summary>
        /// A magical stat adjustment value that is taken into consideration when calculating various attributes, e.g Health, Mana, AttackPower
        /// </summary>
        /// <remarks>This value is not a pre-defined constant in the wow lua sourcecode per se, but it certainly appears in quite a few formulas.</remarks>
        protected const float StatAdjustment = 20;
        #endregion

        /// <summary>
        /// Doomguards and Infernals are the only minions that gain additional health from their bonus stamina.
        /// </summary>
        protected float HealthPerBonusStamina;
        /// <summary>
        /// Doomguards are the only minions that gain additional power from their bonus intellect.
        /// </summary>
        protected float ManaPerBonusIntellect;

        protected float Mp5PerIntellect;
        protected float InitialAttackPowerPerStrength;
        protected float InitialAttackCritPerAgility;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">The minion's type name [imp, succy, felguard, etc]</param>
        /// <param name="master">A reference to the minion owner's character sheet - required because there are talents / buffs that apply to the minion too.</param>
        /// <param name="stats">A reference to the minion owner's cumulative stats - required because many stats are inherited by the minion.</param>
        protected Minion( String name, Character master, Stats stats )
        {
            Name = name;
            Master = master;
            OwnerStats = stats;

            HealthModifier = 1;
            MaxHealthModifier = 1;
            PowerModifier = 1;

            InheritedCriticalStrikeChance = (master.WarlockTalents.ImprovedDemonicTactics * 0.10f);

            #region minion buffs
            //A minion will inherit nearly all the buffs applied to its master,
            //so (to keep things simple), we copy all buffs on the master and remove the ones that dont apply
            CalculationsWarlock calcs = new CalculationsWarlock();
            Stats buffs = calcs.GetBuffsStats(master.ActiveBuffs);

            //focus magic cannot be applied to minions
            if (master.ActiveBuffsContains("Focus Magic"))
            {
                buffs -= Buff.GetBuffByName("Focus Magic").Stats;
            }

            //food and flask/elixir/potions consumed by the master do not apply to minions
            foreach (Buff buff in master.ActiveBuffs)
            {
                if (buff.Group == "Elixirs and Flasks")
                {
                    buffs -= buff.Stats;
                    //TODO: Remove any buff improvements
                    //foreach (Buff ImpBuff in buff.Improvements) buffs -= ImpBuff.Stats; ;
                }
                if (buff.Group == "Food") buffs -= buff.Stats;
            }

            //however, minions do eat food (e.g. Kibler's), so we'll assume that if the master has food, then the minion does too.
            //TODO: minion food buffs

            MinionStats = buffs;
            #endregion

        }

        /// <summary>
        /// Returns the minion's strength.
        /// </summary>
        public virtual float Strength()
        {
            return BaseStrength * (1 + OwnerStats.BonusStrengthMultiplier);
        }

        /// <summary>
        /// Returns the minion's agility.
        /// </summary>
        public virtual float Agility()
        {
            return BaseAgility * (1 + OwnerStats.BonusAgilityMultiplier);
        }

        /// <summary>
        /// Returns the minion's stamina.
        /// </summary>
        /// <remarks>
        /// Overridden in the derived classes for Doomguards and Infernals because they dont benefit from Fel Vitality.
        /// </remarks>
        public virtual float Stamina()
        {
            float staminaFromMaster = (float)Math.Floor(OwnerStats.Stamina * InheritedStamina);
            float minionStamina = (BaseStamina + staminaFromMaster) * (1 + OwnerStats.BonusStaminaMultiplier);

            return (float)Math.Floor(minionStamina * (1 + (Master.WarlockTalents.FelVitality * 0.05f)));
        }

        /// <summary>
        /// Returns the minion's intellect.
        /// </summary>
        /// <remarks>
        /// Overridden in the derived classes for Doomguards and Infernals because they dont benefit from Fel Vitality.
        /// </remarks>
        public virtual float Intellect()
        {
            float intellectFromMaster = (float)Math.Floor(OwnerStats.Intellect * InheritedIntellect);
            float minionIntellect = (BaseIntellect + intellectFromMaster) * (1 + OwnerStats.BonusIntellectMultiplier);

            return (float)Math.Floor(minionIntellect * (1 + (Master.WarlockTalents.FelVitality * 0.05f)));
        }

        /// <summary>
        /// Returns the minion's spirit.
        /// </summary>
        public virtual float Spirit()
        {
            return BaseSpirit * (1 + OwnerStats.BonusSpiritMultiplier);
        }

        /// <summary>
        /// Return the minion's armor.
        /// </summary>
        public virtual float Armor()
        {
            float armorFromMaster = (float)Math.Floor(OwnerStats.Armor * InheritedArmor);
            return (BaseArmor + armorFromMaster);
        }

        /// <summary>
        /// Return the minion's health
        /// </summary>
        /// <remarks>
        /// The general formula for calculating the minion's health gained from stamina can be found in the PetPaperDollFrame_SetStats() function in "PetPaperDollFrame.lua".
        /// BaseHealth for each standard minion is calculated as: UnitMaxHealth(unit) - [health gained from stamina]
        /// Overridden for Infernals / Doomguards because those receive additional health per point of *bonus* stamina.
        /// </remarks>
        public virtual float Health()
        {
            float healthFromStamina = (((Stamina() - StatAdjustment) * HealthPerStamina) + StatAdjustment) * HealthModifier;

            return (BaseHealth + healthFromStamina);
        }

        /// <summary>
        /// Return the minion's mana.
        /// </summary>
        /// <remarks>
        /// This formula can also be found in the PetPaperDollFrame_SetStats() function in "PetPaperDollFrame.lua".
        /// BasePower was calculated for each pet as: UnitPowerMax(unit,0) - [power gained from int]
        /// </remarks>
        public virtual float Power()
        {
            float powerFromIntellect = (((Intellect() - StatAdjustment) * ManaPerIntellect) + StatAdjustment) * PowerModifier;

            return (BasePower + powerFromIntellect);
        }

        /// <summary>
        /// Return the attack power (based on strength) for a melee minion. 
        /// The imp is the only ranged minion, so its AP calculation (based on agility) is handled in the derived class.
        /// </summary>
        /// <remarks>
        /// The general attack power formula is:
        ///     Melee AP = ((strength * 2) - 20) + [57% of master's spellpower]
        ///     Ranged AP = (agility - 10) + [57% of master's spellpower]
        /// </remarks>
        public virtual float AttackPower()
        {
            float baseAttackPower = (BaseStrength * 2) - StatAdjustment;
            float bonusAttackPower = (OwnerStats.SpellPower * InheritedAttackPower);

            return (baseAttackPower + bonusAttackPower);
        }

        /// <summary>
        /// Calculates the dps for a melee minion.
        /// </summary>
        public virtual float DPS()
        {
            const float attackSpeed = 2;
            float baseDamage = (BaseMinDamage + BaseMaxDamage) / 2;

            return (baseDamage/attackSpeed) + (AttackPower()/AttackPowerMagicNumber);
        }

    }

    /// <summary>
    /// Imp class definition
    /// </summary>
    public class Imp : Minion
    {
        public Imp(Character master, Stats stats) : base("Imp", master, stats)
        {
            BaseStrength = 297;
            BaseAgility = 79;
            BaseStamina = 118;
            BaseIntellect = 369;    //424 in simcraft - looks like they included fel vitality directly in the base value because 369 * 1.15 = 424. Obviously this is wrong because fel vitality is not guaranteed to be talented.
            BaseSpirit = 367;

            BaseHealth = 3028;      //+1330 with Blood Pact, but we exclude that here because it will be added back in later via minion buffs
            BasePower = 1175;

            HealthModifier = 0.83999f;
            MaxHealthModifier = 1;
            PowerModifier = 0.33f;

            BaseMinDamage = 315;
            BaseMaxDamage = 467;
            BaseArmor = 6273;

            Mp5PerIntellect = 5.0f / 6.0f;
            BaseMp5 = -257;
        }

        /// <summary>
        /// The imp is a ranged minion, so its attackpower is calculated from agility instead. (melee minion attackpower is calculated from strength).
        /// </summary>
        public override float AttackPower()
        {
            float baseAttackPower = (BaseAgility - 10);
            float bonusAttackPower = (OwnerStats.SpellPower * InheritedAttackPower);

            return (baseAttackPower + bonusAttackPower);
        }
    }

    /// <summary>
    /// Felhunter class definition
    /// </summary>
    public class Felhunter : Minion
    {
        public Felhunter(Character master, Stats stats) : base("Felhunter", master, stats)
        {
            BaseStrength = 314;
            BaseAgility = 90;
            BaseStamina = 328;
            BaseIntellect = 150;
            BaseSpirit = 209;

            BaseHealth = 1842;
            BasePower = 1558;

            HealthModifier = 0.94999f;
            MaxHealthModifier = 1;
            PowerModifier = 0.76999f;

            BaseMinDamage = 333;
            BaseMaxDamage = 466;
            BaseArmor = 7782;

            InitialAttackPowerPerStrength = 2.0f;
            InitialAttackCritPerAgility = 0.01f / 52.0f;
            Mp5PerIntellect = 8.0f / 324.0f;
            BaseMp5 = 11.22f;
            BaseAttackCrit = 0.0327f;

            //melee = new warlock_pet_melee_t(this, "felhunter_melee");

        }
    }

    /// <summary>
    /// Felguard class definition
    /// </summary>
    public class Felguard : Minion
    {
        public Felguard(Character master, Stats stats) : base("Felguard", master, stats)
        {
            BaseStrength = 314;
            BaseAgility = 90;
            BaseStamina = 377;
            BaseIntellect = 172;
            BaseSpirit = 209;

            BaseHealth = 2018;
            BasePower = 1558;

            HealthModifier = 1.10f;
            MaxHealthModifier = 1;
            PowerModifier = 0.76999f;

            //melee = new melee_t(this);
        }

        /// <summary>
        /// The Glyph of Felguard increases its attack power by 20%.
        /// </summary>
        public override float AttackPower()
        {
            return (base.AttackPower() * (Master.WarlockTalents.GlyphFelguard ? 1.2f : 1));
        }

    }

    /// <summary>
    /// Succubus class definition
    /// </summary>
    public class Succubus : Minion
    {
        public Succubus(Character master, Stats stats) : base("Succubus", master, stats)
        {
            BaseStrength = 314;
            BaseAgility = 90;
            BaseStamina = 328;
            BaseIntellect = 150;
            BaseSpirit = 209;

            BaseHealth = 1784;
            BasePower = 1558;

            HealthModifier = 0.89999f;
            MaxHealthModifier = 1;
            PowerModifier = 0.76999f;

            BaseMinDamage = 437;
            BaseMaxDamage = 611;
            BaseArmor = 9706;
        }
    }

    /// <summary>
    /// Voidwalker class definition
    /// </summary>
    /// todo: implement glyph of voidwalker? (20% increased stamina)
    public class Voidwalker : Minion
    {
        public Voidwalker(Character master, Stats stats) : base("Voidwalker", master, stats)
        {
            BaseStrength = 314;
            BaseAgility = 90;
            BaseStamina = 328;
            BaseIntellect = 150;
            BaseSpirit = 209;

            BaseHealth = 2018;
            BasePower = 1558;

            HealthModifier = 1.10f;
            MaxHealthModifier = 1;
            PowerModifier = 0.76999f;

            BaseMinDamage = 361;
            BaseMaxDamage = 504;
            BaseArmor = 16148;
        }
    }

    /// <summary>
    /// Doomguard class definition
    /// </summary>
    public class Doomguard : Minion
    {
        public Doomguard(Character master, Stats stats) : base("Doomguard", master, stats) 
        {
            BaseStrength = 314;
            BaseAgility = 90;
            BaseStamina = 328;
            BaseIntellect = 150;
            BaseSpirit = 209;

            BaseHealth = 14007;
            BasePower = 4261;

            //Doomguard's gain extra health and mana from their bonus stamina and intellect.
            HealthPerBonusStamina = 13;
            //the 23.4 is an approximation because I havent been able to work out the exact formula yet :/
            ManaPerBonusIntellect = 23.4f;

            BaseMinDamage = 489;    //825
            BaseMaxDamage = 692;    //1152

            BaseArmor = 14514;
        }

        /// <summary>
        /// Returns the Doomguard's stamina.
        /// </summary>
        public override float Stamina()
        {
            float staminaFromMaster = (float)Math.Floor(OwnerStats.Stamina * InheritedStamina);
            return (BaseStamina + staminaFromMaster) * (1 + OwnerStats.BonusStaminaMultiplier);
        }

        /// <summary>
        /// Returns the Doomguard's intellect.
        /// </summary>
        public override float Intellect()
        {
            float intellectFromMaster = (float)Math.Floor(OwnerStats.Intellect * InheritedIntellect);
            return (BaseIntellect + intellectFromMaster) * (1 + OwnerStats.BonusIntellectMultiplier);
        }

        /// <summary>
        /// Doomguards gain 10 health per base stamina, and 13 health per bonus stamina.
        /// </summary>
        /// <remarks>
        /// The formula to calculate the Doomguard's health from base stamina is different from other minions because they do not have a health modifier.
        /// The UnitHealthModifier() and UnitMaxHealthModifier() API's will return 0 for a Doomguard or Infernal.
        /// </remarks>
        public override float Health()
        {
            //calculate health from base stamina as per normal (except that it does not have the HealthModifier)
            float healthFromBaseStamina = (((BaseStamina - StatAdjustment) * HealthPerStamina) + StatAdjustment);

            //now calculate the health gained from bonus stamina
            float bonusStamina = Stamina() - BaseStamina;
            float healthFromBonusStamina = (bonusStamina * HealthPerBonusStamina);

            return (BaseHealth + healthFromBaseStamina + healthFromBonusStamina);
        }

        /// <summary>
        /// Doomguards gain 15 mana per base intellect, and ~23.4 mana per bonus intellect.
        /// </summary>
        /// <remarks>
        /// The formula to calculate the Doomguard's power from base intellect is different from other minions because they do not have a power modifier.
        /// The UnitPowerModifier() API will return 0 for a Doomguard.
        /// The ~23.4 is an approximation because I have not been able to work out the exact formula just yet.
        /// </remarks>
        public override float Power()
        {
            //calculate power from base intellect as per normal (except that it does not have the PowerModifier)
            float powerFromBaseInt = (((BaseIntellect - StatAdjustment) * ManaPerIntellect) + StatAdjustment);

            //now calculate power from bonus intellect
            float bonusIntellect = Intellect() - BaseIntellect;
            float powerFromBonusInt = (bonusIntellect * ManaPerBonusIntellect);

            return (BasePower + powerFromBaseInt + powerFromBonusInt);
        }

    }

    /// <summary>
    /// Infernal class definition
    /// </summary>
    /// <remarks>
    /// Interesting fact: Infernal's are Doomguards too! [as per the UnitCreatureFamily() api]. 
    /// </remarks>
    public class Infernal : Minion
    {
        public Infernal(Character master, Stats stats) : base("Infernal", master, stats)
        {
            BaseStrength = 331;
            BaseAgility = 113;
            BaseStamina = 361;
            BaseIntellect = 65;
            BaseSpirit = 109;

            BaseHealth = 21831;
            BasePower = 0;

            //Infernals gain a whopping 19 extra health per point of bonus stamina!
            HealthPerBonusStamina = 19;

            BaseMinDamage = 1349;
            BaseMaxDamage = 1877;
            //OH Damage = 938.378

            BaseArmor = 17037;
        }

        /// <summary>
        /// Returns the Infernal's stamina.
        /// </summary>
        public override float Stamina()
        {
            float staminaFromMaster = (float)Math.Floor(OwnerStats.Stamina * InheritedStamina);
            return (BaseStamina + staminaFromMaster) * (1 + OwnerStats.BonusStaminaMultiplier);
        }

        /// <summary>
        /// Returns the Doomguard's intellect.
        /// </summary>
        public override float Intellect()
        {
            float intellectFromMaster = (float)Math.Floor(OwnerStats.Intellect * InheritedIntellect);
            return (BaseIntellect + intellectFromMaster) * (1 + OwnerStats.BonusIntellectMultiplier);
        }

        /// <summary>
        /// Infernals gain 10 health per base stamina, and 19 health per bonus stamina.
        /// </summary>
        /// <returns></returns>
        public override float Health()
        {
            //calculate health gained from base stamina as per normal
            float healthFromBaseStamina = (((BaseStamina - StatAdjustment) * HealthPerStamina) + StatAdjustment);

            //now calculate the health gained from bonus stamina
            float bonusStamina = Stamina() - BaseStamina;
            float healthFromBonusStamina = (bonusStamina * HealthPerBonusStamina);

            return (BaseHealth + healthFromBaseStamina + healthFromBonusStamina);
        }

        /// <summary>
        /// Infernals do not have any Power().
        /// </summary>
        public override float Power()
        {
            return 0;
        }
    }


    public static class MinionFactory
    {
        public static Minion CreateMinion(String name, Character master, Stats stats)
        {
            switch (name)
            {
                case "Imp":
                    return new Imp(master, stats);
                case "Felhunter":
                    return new Felhunter(master, stats);
                case "Felguard":
                    return new Felguard(master, stats);
                case "Succubus":
                    return new Succubus(master, stats);
                case "Voidwalker":
                    return new Voidwalker(master, stats);
                case "Doomguard":
                    return new Doomguard(master, stats);
                case "Infernal":
                    return new Infernal(master, stats);
                default:
                    return null;
            }
        }
    }

 
    /// <summary>
    /// The generic base class for all minion spell templates.
    /// </summary>
    public abstract class MinionSpell : Spell
    {
        protected MinionSpell(String name, Stats stats, Character character, IEnumerable<SpellRank> spellRanks, Color color, MagicSchool magicSchool, SpellTree spellTree)
            : base(name, stats, character, spellRanks, color, magicSchool, spellTree)
        {
            
        }

        /// <summary>
        /// Minions do not benefit from haste, so override the default calculation to exclude the effect of haste.
        /// </summary>
        public override float CastTime()
        {
            return Math.Max(1.0f, BaseExecuteTime);
        }
    }

    /// <summary>
    /// An instant attack that lashes the target, causing 237 Shadow damage.
    /// </summary>
    public class LashOfPain : MinionSpell
    {
        static readonly List<SpellRank> SpellRankTable = new List<SpellRank> 
        { 
            new SpellRank(9, 80, 237, 237, 0, 250)
        };

        public LashOfPain(Stats stats, Character character)
            : base("Lash of Pain", stats, character, SpellRankTable, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, Warlock.SpellTree.None)
        {
            MayCrit = true;

            BaseExecuteTime = 0;
            BaseDuration = 0;

            //demonic power reduces the cooldown by 3secs per point
            Cooldown = 12 - (3 * character.WarlockTalents.DemonicPower);

            BaseDirectDamageCoefficient = (1.5f / 3.5f);
        }
    }

    /// <summary>
    /// Deals 199 to 223 Fire damage to a target.	
    /// </summary>
    public class FireBolt : MinionSpell
    {
        static readonly List<SpellRank> SpellRankTable = new List<SpellRank> 
        { 
            new SpellRank(8, 78, 199, 223, 0, 180, 4, 4, 0)
        };

        public FireBolt(Stats stats, Character character)
            : base("Firebolt", stats, character, SpellRankTable, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.None)
        {
            MayCrit = true;

            BaseExecuteTime = 2.5f;
            BaseDirectDamageCoefficient = (BaseExecuteTime / 3.5f);

            //improved imp only applies to the imp's spellpower - it does not apply to the spellpower inherited from the master.
            BaseMinDamage *= (1 + (character.WarlockTalents.ImprovedImp * 0.10f));
            BaseMaxDamage *= (1 + (character.WarlockTalents.ImprovedImp * 0.10f));

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.EmpoweredImp * 0.10f));
        }

        /// <summary>
        /// The Demonic Power talent increases the imp's firebolt casting speed by 0.25 seconds per point.
        /// </summary>
        /// <returns></returns>
        public override float CastTime()
        {
            return (base.CastTime() - (Character.WarlockTalents.DemonicPower * 0.25f));
        }
    }


}