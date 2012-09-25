using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Rawr.Warlock
{
    [Rawr.Calculations.RawrModelInfo("Warlock", "Spell_Nature_FaerieFire", CharacterClass.Warlock)]
    public class CalculationsWarlock : CalculationsBase
    {
        // Basic Model Functionality
        public override CharacterClass TargetClass { get { return CharacterClass.Warlock; } }

        private CalculationOptionsPanelWarlock _calculationOptionsPanel = null;
        public override ICalculationOptionsPanel CalculationOptionsPanel
        {
            get
            {
                if (_calculationOptionsPanel == null)
                {
                    _calculationOptionsPanel = new CalculationOptionsPanelWarlock();
                }
                return _calculationOptionsPanel;
            }
        }

        public override ComparisonCalculationBase CreateNewComparisonCalculation()
        {
            return new ComparisonCalculationWarlock();
        }

        public override CharacterCalculationsBase CreateNewCharacterCalculations()
        {
            return new CharacterCalculationsWarlock();
        }

        public const float AVG_UNHASTED_CAST_TIME = 2f; // total SWAG
        
        public override void SetDefaults(Character character)
        {
            character.ActiveBuffsAdd("Fel Armor");
        }

        private string[] _characterDisplayCalculationLabels = null;
        public override string[] CharacterDisplayCalculationLabels
        {
            get
            {
                if (_characterDisplayCalculationLabels == null)
                {
                    _characterDisplayCalculationLabels = new string[] {
                        "Simulation:Personal DPS",
                        "Simulation:Pet DPS",
                        "Simulation:Total DPS",
                        "Basic Stats:Health",
                        "Basic Stats:Mana",
                        "Basic Stats:Bonus Damage",
                        "Basic Stats:Hit Rating",
                        "Basic Stats:Crit Chance",
                        "Basic Stats:Average Haste",
                        "Basic Stats:Mastery",
                        "Pet Stats:Pet Stamina",
                        "Pet Stats:Pet Intellect",
                        "Pet Stats:Pet Health",
                        "Affliction:Corruption",
                        "Affliction:Bane Of Agony",
                        "Affliction:Bane Of Doom",
                        "Affliction:Curse Of The Elements",
                        "Affliction:Drain Life",
                        "Affliction:Drain Soul",
                        "Affliction:Haunt",
                        "Affliction:Life Tap",
                        "Affliction:Unstable Affliction",
                        "Demonology:Immolation Aura",
                        "Destruction:Chaos Bolt",
                        "Destruction:Conflagrate",
                        "Destruction:Fel Flame",
                        "Destruction:Immolate",
                        "Destruction:Incinerate",
                        "Destruction:Incinerate (Under Backdraft)",
                        "Destruction:Incinerate (Under Molten Core)",
                        "Destruction:Searing Pain",
                        "Destruction:Soul Fire",
                        "Destruction:Shadow Bolt",
                        "Destruction:Shadow Bolt (Instant)",
                        "Destruction:Shadowburn"
                    };
                }
                return _characterDisplayCalculationLabels;
            }
        }

        private string[] _optimizableCalculationLabels = null;
        public override string[] OptimizableCalculationLabels
        {
            get
            {
                if (_optimizableCalculationLabels == null)
                {
                    _optimizableCalculationLabels = new string[] { "Miss Chance"};
                }
                return _optimizableCalculationLabels;
            }
        }

        private string[] _customChartNames = null;
        public override string[] CustomChartNames
        {
            get
            {
                if (_customChartNames == null)
                {
                    _customChartNames = new string[] { };
                }
                return _customChartNames;
            }
        }

        private Dictionary<string, Color> _subPointNameColors = null;
        public override Dictionary<string, Color> SubPointNameColors
        {
            get
            {
                if (_subPointNameColors == null)
                {
                    _subPointNameColors = new Dictionary<string, Color>();
                    _subPointNameColors.Add("DPS", Color.FromArgb(255, 255, 0, 0));
                    _subPointNameColors.Add("Pet DPS", Color.FromArgb(255, 0, 0, 255));
                }
                return _subPointNameColors;
            }
        }

        // Basic Calcuations
        public override ICalculationOptionBase DeserializeDataObject(string xml) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CalculationOptionsWarlock));
            StringReader reader = new StringReader(xml);
            CalculationOptionsWarlock calcOpts = serializer.Deserialize(reader) as CalculationOptionsWarlock;
            return calcOpts;
        }

        public override CharacterCalculationsBase GetCharacterCalculations(Character character, Item addlItem, bool _u1, bool _u2, bool _u3)
        {
            return new CharacterCalculationsWarlock(character, GetCharacterStats(character, addlItem), GetPetBuffStats(character));
        }

        private Stats GetPetBuffStats(Character character)
        {
            List<Buff> buffs = new List<Buff>();
            foreach (Buff buff in character.ActiveBuffs)
            {
                string group = buff.Group;
                if (   group != "Profession Buffs"
                    && group != "Set Bonuses"
                    && group != "Food"
                    && group != "Potion"
                    && group != "Elixirs and Flasks"
                    && group != "Focus Magic, Spell Critical Strike Chance")
                {
                    buffs.Add(buff);
                }
            }
            Stats stats = GetBuffsStats(buffs);
            var options = character.CalculationOptions as CalculationOptionsWarlock;
            ApplyPetsRaidBuff(stats, options.Pet, character.WarlockTalents, character.ActiveBuffs, options);
            return stats;
        }

        public override Stats GetCharacterStats(Character character, Item additionalItem)
        {
            WarlockTalents talents = character.WarlockTalents;
            CalculationOptionsWarlock options = character.CalculationOptions as CalculationOptionsWarlock;
            Stats stats = BaseStats.GetBaseStats(character);
            
            AccumulateItemStats(stats, character, additionalItem);
            AccumulateBuffsStats(stats, character.ActiveBuffs);
            ApplyPetsRaidBuff(stats, options.Pet, talents, character.ActiveBuffs, options);

            float[] demonicEmbraceValues = { 0f, .04f, .07f, .1f };
            Stats statsTalents = new Stats {
                BonusStaminaMultiplier = demonicEmbraceValues[talents.DemonicEmbrace], //Demonic Embrace
                BonusIntellectMultiplier = options.PlayerLevel > 50 ? .05f : 0 // Nethermancy
            };

            if (talents.Eradication > 0)
            {
                float[] eradicationValues = { 0f, .06f, .12f, .20f };
                statsTalents.AddSpecialEffect(
                    new SpecialEffect(
                        Trigger.CorruptionTick,
                        new Stats() {
                            SpellHaste = eradicationValues[talents.Eradication]
                        },
                        10f, //duration
                        0f,  //cooldown
                        .06f)); //chance
            }

            stats.Accumulate(statsTalents);
            stats.ManaRestoreFromMaxManaPerSecond
                = Math.Max(
                    stats.ManaRestoreFromMaxManaPerSecond,
                    .001f * Spell.CalcUprate(talents.SoulLeech > 0 ? 1f : 0f, 15f, options.Duration * 1.1f));
            return stats;
        }

        private void ApplyPetsRaidBuff(Stats stats, string pet, WarlockTalents talents, List<Buff> activeBuffs, CalculationOptionsWarlock options)
        {
            stats.Health += CalcPetHealthBuff(pet, talents, activeBuffs, options);
            stats.Mana += CalcPetManaBuff(pet, talents, activeBuffs, options);
            stats.Mp5 += CalcPetMP5Buff(pet, talents, activeBuffs, options);
        }

        private static float[] buffBaseValues = { 125f, 308f, 338f, 375f, 407f, 443f };
        public static float CalcPetHealthBuff(string pet, WarlockTalents talents, List<Buff> activeBuffs, CalculationOptionsWarlock options)
        {
            if (!pet.Equals("Imp"))
            {
                return 0f;
            }

            //spell ID 6307, effect ID 2190
            float SCALE = 1.3200000525f;
            return StatUtils.GetBuffEffect(activeBuffs, SCALE * buffBaseValues[options.PlayerLevel - 80], "Health", s => s.Health);
        }

        public static float CalcPetManaBuff(string pet, WarlockTalents talents, List<Buff> activeBuffs, CalculationOptionsWarlock options)
        {
            if (!pet.Equals("Felhunter"))
            {
                return 0f;
            }

            //spell ID 54424, effect ID 47202
            float SCALE = 4.8000001907f;
            return StatUtils.GetBuffEffect(activeBuffs, SCALE * buffBaseValues[options.PlayerLevel - 80], "Mana", s => s.Mana);
        }

        public static float CalcPetMP5Buff(string pet, WarlockTalents talents, List<Buff> activeBuffs, CalculationOptionsWarlock options)
        {
            if (!pet.Equals("Felhunter"))
            {
                return 0f;
            }

            //spell ID 54424, effect ID 47203
            float SCALE = 0.7360000014f;
            return StatUtils.GetBuffEffect(activeBuffs, SCALE * buffBaseValues[options.PlayerLevel - 80], "Mana Regeneration", s => s.Mp5);
        }

        public override ComparisonCalculationBase[] GetCustomChartData(Character character, string chartName)
        {
            return new ComparisonCalculationBase[0];
        }

        // Relevancy
        private List<ItemType> _relevantItemTypes = null;
        public override List<ItemType> RelevantItemTypes
        {
            get
            {
                if (_relevantItemTypes == null)
                {
                    _relevantItemTypes = new List<ItemType>(6) { 
                        ItemType.None, ItemType.Cloth, ItemType.Dagger, ItemType.Wand, ItemType.OneHandSword, ItemType.Staff 
                    };
                }
                return _relevantItemTypes;
            }
        }

        private List<GemmingTemplate> _defaultGemmingTemplates = null;
        public override List<GemmingTemplate> DefaultGemmingTemplates
        {
            get
            {
                if (_defaultGemmingTemplates == null)
                {
                    _defaultGemmingTemplates = new List<GemmingTemplate>();
                    AddGemmingTemplateGroup(_defaultGemmingTemplates, "Rare", true, 52207, 52239, 52208, 52205, 52217, 68780);
                    AddGemmingTemplateGroup(_defaultGemmingTemplates, "Rare (Jewelcrafting)", false, 52257, 52239, 52208, 52205, 52217, 68780);
                }
                return _defaultGemmingTemplates;
            }
        }

        private void AddGemmingTemplateGroup(List<GemmingTemplate> list, string name, bool enabled, int brilliant, int potent, int reckless, int artful, int blue, int meta)
        {
            list.Add(new GemmingTemplate() { Model = "Warlock", Group = name, RedId = brilliant, YellowId = brilliant, BlueId = brilliant, PrismaticId = brilliant, MetaId = meta, Enabled = enabled });
            list.Add(new GemmingTemplate() { Model = "Warlock", Group = name, RedId = brilliant, YellowId = potent, BlueId = blue, PrismaticId = brilliant, MetaId = meta, Enabled = enabled });
            list.Add(new GemmingTemplate() { Model = "Warlock", Group = name, RedId = brilliant, YellowId = reckless, BlueId = blue, PrismaticId = brilliant, MetaId = meta, Enabled = enabled });
            if (artful != 0)
            {
                list.Add(new GemmingTemplate() { Model = "Warlock", Group = name, RedId = brilliant, YellowId = artful, BlueId = blue, PrismaticId = brilliant, MetaId = meta, Enabled = enabled });
            }
        }

        public static bool IsSupportedProc(Trigger trigger)
        {
            switch (trigger)
            {
                // damage effects
                case Trigger.DamageDone:
                case Trigger.DamageOrHealingDone:
                case Trigger.DamageSpellCast:
                case Trigger.DamageSpellCrit:
                case Trigger.DamageSpellHit:
                // spell effects
                case Trigger.SpellCast:
                case Trigger.SpellCrit:
                case Trigger.SpellHit:
                case Trigger.SpellMiss:
                case Trigger.DoTTick:
                // warlock specific
                case Trigger.CorruptionTick:

                    return true;
            }
            return false;
        }

        public static bool IsSupportedUseEffect(SpecialEffect effect)
        {
            bool hasteEffect;
            bool stackingEffect;
            return IsSupportedUseEffect(effect, out hasteEffect, out stackingEffect);
        }

        public static bool IsSupportedUseEffect(SpecialEffect effect, out bool hasteEffect, out bool stackingEffect)
        {
            stackingEffect = false;
            hasteEffect = false;
            if (effect.MaxStack == 1 && effect.Trigger == Trigger.Use)
            {
                // check if it is a stacking use effect
                Stats effectStats = effect.Stats;
                for (int i = 0; i < effectStats._rawSpecialEffectDataSize; i++)
                {
                    SpecialEffect e = effectStats._rawSpecialEffectData[i];
                    if (e.Chance == 1f && e.Cooldown == 0f && (e.Trigger == Trigger.DamageSpellCast || e.Trigger == Trigger.DamageSpellHit || e.Trigger == Trigger.SpellCast || e.Trigger == Trigger.SpellHit))
                    {
                        if (e.Stats.HasteRating > 0)
                        {
                            hasteEffect = true;
                            stackingEffect = true;
                            break;
                        }
                    }
                    if (e.Chance == 1f && e.Cooldown == 0f && (e.Trigger == Trigger.DamageSpellCrit || e.Trigger == Trigger.SpellCrit))
                    {
                        if (e.Stats.CritRating < 0 && effect.Stats.CritRating > 0)
                        {
                            stackingEffect = true;
                            break;
                        }
                    }
                }
                if (stackingEffect)
                {
                    return true;
                }
                if (effect.Stats.HasteRating > 0)
                {
                    hasteEffect = true;
                }
                return effect.Stats.SpellPower + effect.Stats.HasteRating + effect.Stats.Intellect + effect.Stats.HighestStat > 0;
            }
            return false;
        }

        public static bool IsSupportedSpellPowerProc(SpecialEffect effect)
        {
            return (effect.MaxStack == 1 && effect.Stats.SpellPower > 0 && IsSupportedProc(effect.Trigger));
        }

        public static bool IsSupportedMasteryProc(SpecialEffect effect)
        {
            return (effect.MaxStack == 1 && effect.Stats.MasteryRating > 0 && IsSupportedProc(effect.Trigger));
        }

        public static bool IsSupportedIntellectProc(SpecialEffect effect)
        {
            return (effect.MaxStack == 1 && (effect.Stats.Intellect + effect.Stats.HighestStat) > 0 && IsSupportedProc(effect.Trigger));
        }

        public static bool IsSupportedDamageProc(SpecialEffect effect)
        {
            return (effect.MaxStack == 1 && (effect.Stats.ArcaneDamage + effect.Stats.FireDamage + effect.Stats.FrostDamage + effect.Stats.NatureDamage + effect.Stats.ShadowDamage + effect.Stats.HolyDamage + effect.Stats.ValkyrDamage > 0) && (IsSupportedProc(effect.Trigger) || effect.Trigger == Trigger.Use));
        }

        public static bool IsSupportedHasteProc(SpecialEffect effect)
        {
            if (effect.MaxStack == 1 && effect.Stats.HasteRating > 0)
            {
                if (effect.Cooldown >= effect.Duration && (effect.Trigger == Trigger.DamageSpellCrit || effect.Trigger == Trigger.SpellCrit || effect.Trigger == Trigger.DamageSpellHit || effect.Trigger == Trigger.SpellHit || effect.Trigger == Trigger.SpellCast || effect.Trigger == Trigger.DamageSpellCast))
                {
                    return true;
                }
                if (effect.Cooldown == 0 && (effect.Trigger == Trigger.SpellCrit || effect.Trigger == Trigger.DamageSpellCrit))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSupportedManaRestoreProc(SpecialEffect effect)
        {
            return (effect.MaxStack == 1 && effect.Stats.ManaRestore > 0 && (IsSupportedProc(effect.Trigger) || effect.Trigger == Trigger.Use));
        }

        public static bool IsSupportedMp5Proc(SpecialEffect effect)
        {
            return (effect.MaxStack == 1 && effect.Stats.Mp5 > 0 && (IsSupportedProc(effect.Trigger) || effect.Trigger == Trigger.Use));
        }

        public static bool IsSupportedStackingEffect(SpecialEffect effect)
        {
            if (effect.MaxStack > 1 && effect.Chance == 1f && effect.Cooldown == 0f && (effect.Trigger == Trigger.DamageSpellCast || effect.Trigger == Trigger.DamageSpellHit || effect.Trigger == Trigger.SpellCast || effect.Trigger == Trigger.SpellHit))
            {
                if (HasEffectStats(effect.Stats))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSupportedDotTickStackingEffect(SpecialEffect effect)
        {
            if (effect.MaxStack > 1 && effect.Chance == 1f && effect.Cooldown == 0f && effect.Trigger == Trigger.DoTTick && effect.Stats.SpellPower > 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsSupportedResetStackingEffect(SpecialEffect effect)
        {
            if (effect.MaxStack == 1 && effect.Chance == 1 && (effect.Trigger == Trigger.DamageSpellCast || effect.Trigger == Trigger.DamageSpellHit || effect.Trigger == Trigger.SpellCast || effect.Trigger == Trigger.SpellHit))
            {
                Stats effectStats = effect.Stats;
                for (int i = 0; i < effectStats._rawSpecialEffectDataSize; i++)
                {
                    SpecialEffect e = effectStats._rawSpecialEffectData[i];
                    if (e.Chance == 1f && e.Cooldown == 0f && e.MaxStack > 1 && (e.Trigger == Trigger.DamageSpellCast || e.Trigger == Trigger.DamageSpellHit || e.Trigger == Trigger.SpellCast || e.Trigger == Trigger.SpellHit))
                    {
                        if (e.Stats.SpellPower > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsDarkIntentEffect(SpecialEffect effect)
        {
            if (effect.MaxStack > 1 && effect.Chance == 1 && effect.Trigger == Trigger.DarkIntentCriticalPeriodicDamageOrHealing && effect.Cooldown == 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsSupportedEffect(SpecialEffect effect)
        {
            return IsSupportedUseEffect(effect) ||
                IsSupportedSpellPowerProc(effect) ||
                IsSupportedMasteryProc(effect) ||
                IsSupportedIntellectProc(effect) ||
                IsSupportedDamageProc(effect) ||
                IsSupportedHasteProc(effect) ||
                IsSupportedManaRestoreProc(effect) ||
                IsSupportedMp5Proc(effect) ||
                IsSupportedStackingEffect(effect) ||
                IsSupportedDotTickStackingEffect(effect) ||
                IsSupportedResetStackingEffect(effect) ||
                IsDarkIntentEffect(effect);
        }

        public override Stats GetRelevantStats(Stats stats)
        {
            Stats s = new Stats {
                SpellPower = stats.SpellPower,
                Intellect = stats.Intellect,
                HitRating = stats.HitRating,
                SpellHit = stats.SpellHit,
                HasteRating = stats.HasteRating,
                SpellHaste = stats.SpellHaste,
                CritRating = stats.CritRating,
                SpellCrit = stats.SpellCrit,
                SpellCritOnTarget = stats.SpellCritOnTarget,
                MasteryRating = stats.MasteryRating,

                ShadowDamage = stats.ShadowDamage,
                SpellShadowDamageRating = stats.SpellShadowDamageRating,
                FireDamage = stats.FireDamage,
                SpellFireDamageRating = stats.SpellFireDamageRating,

                BonusIntellectMultiplier = stats.BonusIntellectMultiplier,
                BonusSpellCritMultiplier = stats.BonusSpellCritMultiplier,
                BonusDamageMultiplier = stats.BonusDamageMultiplier,
                BonusShadowDamageMultiplier = stats.BonusShadowDamageMultiplier,
                BonusFireDamageMultiplier = stats.BonusFireDamageMultiplier,

                BonusHealthMultiplier = stats.BonusHealthMultiplier,
                BonusSpellPowerMultiplier = stats.BonusSpellPowerMultiplier,
                BonusManaMultiplier = stats.BonusManaMultiplier,

                CritBonusDamage = stats.CritBonusDamage,

                Warlock2T7 = stats.Warlock2T7,
                Warlock4T7 = stats.Warlock4T7,
                Warlock2T8 = stats.Warlock2T8,
                Warlock4T8 = stats.Warlock4T8,
                Warlock2T9 = stats.Warlock2T9,
                Warlock4T9 = stats.Warlock4T9,
                Warlock2T10 = stats.Warlock2T10,
                Warlock4T10 = stats.Warlock4T10,
                Warlock2T11 = stats.Warlock2T11,
                Warlock4T11 = stats.Warlock4T11,

                Stamina = stats.Stamina,
                Health = stats.Health,
                Mana = stats.Mana,
                Mp5 = stats.Mp5,

                HighestStat = stats.HighestStat,                                    //trinket - darkmoon card: greatness
                ManaRestoreFromBaseManaPPM = stats.ManaRestoreFromBaseManaPPM,      //paladin buff: judgement of wisdom
                ManaRestoreFromMaxManaPerSecond = stats.ManaRestoreFromMaxManaPerSecond,    //replenishment
                BonusManaPotion = stats.BonusManaPotion,                            //triggered when a mana pot is consumed
                ThreatReductionMultiplier = stats.ThreatReductionMultiplier,        //Bracing Eathsiege Diamond (metagem) effect
                ManaRestore = stats.ManaRestore,                                    //quite a few items that restore mana on spell cast or crit. Also used to model replenishment.
                SpellsManaReduction = stats.SpellsManaReduction,                    //spark of hope -> http://www.wowhead.com/?item=45703
            };

            foreach (SpecialEffect effect in stats.SpecialEffects())
            {
                if (IsSupportedEffect(effect))
                {
                    s.AddSpecialEffect(effect);
                }
            }
            return s;
        }

        private static bool HasEffectStats(Stats stats)
        {
            float commonStats = stats.CritRating + stats.HasteRating + stats.HitRating;
            return HasWarlockStats(stats) || (commonStats > 0);
        }

        protected static bool HasWarlockStats(Stats stats)
        {
            // These stats automatically count as relevant.
            return (stats.SpellPower
                  + stats.Intellect
                  + stats.ShadowDamage + stats.SpellShadowDamageRating
                  + stats.FireDamage + stats.SpellFireDamageRating
                  + stats.BonusIntellectMultiplier
                  + stats.BonusDamageMultiplier + stats.BonusShadowDamageMultiplier + stats.BonusFireDamageMultiplier
                  + stats.Warlock2T7 + stats.Warlock4T7
                  + stats.Warlock2T8 + stats.Warlock4T8
                  + stats.Warlock2T9 + stats.Warlock4T9
                  + stats.Warlock2T10 + stats.Warlock4T10
                  + stats.Warlock2T11 + stats.Warlock4T11 > 0);
        }

        public override bool HasRelevantStats(Stats stats)
        {
            foreach (SpecialEffect effect in stats.SpecialEffects())
            {
                if (IsSupportedEffect(effect))
                {
                    return true;
                }
            }
            return ( HasWarlockStats(stats) || (HasCommonStats(stats) && !HasIgnoreStats(stats)) );
        }

        protected bool HasCommonStats(Stats stats)
        {
            // These stats are only relevant if none of the ignore stats were found.
            // That way Str + Crit, etc. are rejected, but an item with only Hit + Crit, etc. would be accepted.
            return (stats.Stamina + stats.Health
                  + stats.HitRating + stats.SpellHit
                  + stats.HasteRating + stats.SpellHaste
                  + stats.CritRating + stats.SpellCrit + stats.SpellCritOnTarget + stats.BonusSpellCritMultiplier
                  + stats.MasteryRating
                  + stats.Mana + stats.Mp5
                  + stats.HighestStat                     //darkmoon card: greatness
                  + stats.SpellsManaReduction             //spark of hope -> http://www.wowhead.com/?item=45703
                  + stats.BonusManaPotion                 //triggered when a mana pot is consumed
                  + stats.ManaRestoreFromBaseManaPPM      //judgement of wisdom
                  + stats.ManaRestoreFromMaxManaPerSecond //replenishment sources
                  + stats.ManaRestore                     //quite a few items that restore mana on spell cast or crit. Also used to model replenishment.
                  + stats.ThreatReductionMultiplier       //bracing earthsiege diamond (metagem) effect
            ) > 0;
        }

        protected bool HasIgnoreStats(Stats stats)
        {
            // These stats automatically count as irrelevant.
            return (stats.Resilience
                  + stats.Agility
                  + stats.ArmorPenetration + stats.TargetArmorReduction
                  + stats.Strength + stats.AttackPower
                  + stats.Expertise + stats.ExpertiseRating
                  + stats.Dodge + stats.DodgeRating
                  + stats.Parry + stats.ParryRating
                  + stats.Block + stats.BlockRating
                  + stats.SpellNatureDamageRating
             > 0);
        }

        public override bool EnchantFitsInSlot(Enchant enchant, Character character, ItemSlot slot)
        {
            if (slot == ItemSlot.Ranged) return false;
            if (slot == ItemSlot.OffHand) return (enchant.Id == 4091);
            return base.EnchantFitsInSlot(enchant, character, slot);
        }

        public override bool ItemFitsInSlot(Item item, Character character, CharacterSlot slot, bool ignoreUnique)
        {
            if (slot == CharacterSlot.OffHand && item.Slot == ItemSlot.OneHand) return false;
            return base.ItemFitsInSlot(item, character, slot, ignoreUnique);
        }

        private static List<string> _relevantGlyphs;
        public override List<string> GetRelevantGlyphs()
        {
            if (_relevantGlyphs == null)
            {
                _relevantGlyphs
                    = new List<string>{
                        "Glyph of Metamorphosis",
                        "Glyph of Corruption",
                        "Glyph of Life Tap",
                        "Glyph of Bane of Agony",
                        "Glyph of Lash of Pain",
                        "Glyph of Shadowburn",
                        "Glyph of Unstable Affliction",
                        "Glyph of Haunt",
                        "Glyph of Chaos Bolt",
                        "Glyph of Immolate",
                        "Glyph of Incinerate",
                        "Glyph of Conflagrate",
                        "Glyph of Imp",
                        "Glyph of Felguard",
                        "Glyph of Shadow Bolt"};
            }
            return _relevantGlyphs;
        }
    }
}