using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Rawr.WarlockSim 
{
    public static class SpellFactory
    {
        public static Spell CreateSpell(String name, Stats stats, Character character, CalculationOptionsWarlock options)
        {
            WarlockTalents talents = character.WarlockTalents;
            switch (name)
            {
                #region shadow school
                case "Shadow Bolt": return new ShadowBolt(stats, character, options);
                case "Haunt": return (talents.Haunt > 0 ? new Haunt(stats, character, options) : null);
                case "Corruption": return new Corruption(stats, character, options);
                case "Curse of Agony": return new CurseOfAgony(stats, character, options);
                case "Curse of Doom": return new CurseOfDoom(stats, character, options);
                case "Unstable Affliction": return (talents.UnstableAffliction > 0 ? new UnstableAffliction(stats, character, options) : null);
                case "Death Coil": return new DeathCoil(stats, character, options);
                case "Drain Life": return new DrainLife(stats, character, options);
                case "Drain Soul": return new DrainSoul(stats, character, options);
                case "Seed of Corruption": return new SeedOfCorruption(stats, character, options);
                case "Shadowflame": return new Shadowflame(stats, character, options);
                case "Shadowburn": return (talents.Shadowburn > 0 ? new Shadowburn(stats, character, options) : null);
                case "Shadowfury": return (talents.Shadowfury > 0 ? new Shadowfury(stats, character, options) : null);
                case "Life Tap": return new LifeTap(stats, character, options);
                case "Dark Pact": return (talents.DarkPact > 0 ? new DarkPact(stats, character, options) : null);
                #endregion

                #region fire school
                case "Immolate": return new Immolate(stats, character, options);
                case "Immolation Aura": return (talents.Metamorphosis > 0 ? new ImmolationAura(stats, character, options): null);
                case "Conflagrate": return (talents.Conflagrate > 0 ? new Conflagrate(stats, character, options) : null);
                case "Chaos Bolt": return (talents.ChaosBolt > 0 ? new ChaosBolt(stats, character, options) : null);
                case "Incinerate": return new Incinerate(stats, character, options);
                case "Searing Pain": return new SearingPain(stats, character, options);
                case "Soul Fire": return new SoulFire(stats, character, options);
                case "Rain of Fire": return new RainOfFire(stats, character, options);
                case "Hellfire": return new Hellfire(stats, character, options);
                #endregion

                default: return null;
            }
        }
    }

    /// <summary>
    /// The base class from which all warlock spells are derived.
    /// </summary>
    public abstract class Spell : Ability, IComparable<Spell>
    {
        #region warlock self buff consts
        /// <summary>
        /// The firestone increases direct damage by 1%.
        /// </summary>
        public const float FirestoneDirectDamageMultiplier = 0.01f;
        /// <summary>
        /// The firestone increases spell crit rating by 49.
        /// </summary>
        public const float FirestoneSpellCritRating = 49;
        /// <summary>
        /// The spellstone increases peridoc damage by 1%.
        /// </summary>
        public const float SpellstoneDotDamageMultiplier = 0.01f;
        /// <summary>
        /// The spellstone increases spell haste rating by 60.
        /// </summary>
        public const float SpellstoneHasteRating = 60;
        #endregion


        #region additional properties (used by dot spells)
        /// <summary>
        /// The base interval of time between 2 consecutive ticks.
        /// </summary>
        public float BaseTickTime { get; protected set; }
        /// <summary>
        /// The base periodic damage inflicted per tick.
        /// </summary>
        public float BaseTickDamage { get; protected set; }
        /// <summary>
        /// The value indicating how much spellpower contributes to tick damage.
        /// </summary>
        public float BaseTickDamageCoefficient { get; protected set; }
        /// <summary>
        /// The value (based on talent improvements) that must be applied when calculating the periodic tick damage result.
        /// </summary>
        public float BaseTickDamageMultiplier { get; protected set; }
        /// <summary>
        /// Indicates if ticks can crit, or not.
        /// </summary>
        public bool TicksMayCrit { get; protected set; }
        /// <summary>
        /// The number of ticks over which the dot damage is spread.
        /// </summary>
        public int NumberOfTicks { get; protected set; }
        /// <summary>
        /// Indicates if the ticks are affected by haste or not.
        /// </summary>
        public bool HastedTicks { get; protected set; }
        /// <summary>
        /// Indicates if the spell is currently ticking or not.
        /// </summary>
        public bool IsTicking { get; protected set; }

        #endregion

        #region aura flags
        public Boolean BackdraftIsActive { get; protected set; }
        #endregion

        #region constructor
        protected Spell(String name, int rank, int level, float baseMinDamage, float baseMaxDamage, float baseTickDamage, float baseCost, Stats stats, Character character, CalculationOptionsWarlock options, Color color, MagicSchool magicSchool, SpellTree spellTree)
            : base(name, rank, level, baseMinDamage, baseMaxDamage, baseCost, stats, character, options, color, magicSchool, spellTree)
        {
            BaseTickDamage = baseTickDamage;

            //defaults
            BaseTickTime = 3;
            BaseTickDamageMultiplier = 1;
        }
        #endregion

        #region Calculation methods
        /// <summary>
        /// Returns the actual time to execute this spell
        /// </summary>
        public override float ExecuteTime()
        {
            return BaseExecuteTime > 0 ? Math.Max(1.0f, (BaseExecuteTime / (1 + Stats.SpellHaste))) : BaseExecuteTime;
        }

        /// <summary>
        /// Calculates the GCD including haste.
        /// </summary>
        public override float GlobalCooldown()
        {
            return BaseGlobalCooldown == 1.0 ? Math.Max(0.5f, (BaseGlobalCooldown / (1 + Stats.SpellHaste))) : Math.Max(1.0f, (BaseGlobalCooldown / (1 + Stats.SpellHaste)));
        }

        /// <summary>
        /// Returns the actual time (i.e. including haste) for each spell tick.
        /// </summary>
        public virtual float TickTime()
        {
            return HastedTicks ? BaseTickTime / (1 + Stats.SpellHaste) : BaseTickTime;
        }

        /// <summary>
        /// Returns the period of time (in seconds) to wait before a direct damage spell can be re-executed, 
        /// or the next tick of a dot spell becomes due.
        /// </summary>
        public override float GetTimeDelay()
        {
            return NumberOfTicks > 0 ? TickTime() : base.GetTimeDelay();
        }

        /// <summary>
        /// Calculates the Range from BaseRange and any talent modifiers.
        /// </summary>
        public override int Range()
        {
            if (SpellTree == SpellTree.Destruction)
            {
                return (int)Math.Round(BaseRange * (1 + Character.WarlockTalents.DestructiveReach * 0.1f));
            }
            if (SpellTree == SpellTree.Affliction)
            {
                return (int)Math.Round(BaseRange * (1 + Character.WarlockTalents.GrimReach * 0.1));   
            }
            //demonology does not have any Range modifiers
            return BaseRange;
        }

        /// <summary>
        /// Calculates the total cost (a percentage of BaseMana) from the basecost and any talent modifiers.
        /// </summary>
        public override float Cost()
        {
            if (SpellTree == SpellTree.Destruction)
            {
                float cataclysm;
                switch (Character.WarlockTalents.Cataclysm)
                {
                    case 1: cataclysm = 0.04f; break;
                    case 2: cataclysm = 0.07f; break;
                    case 3: cataclysm = 0.10f; break;
                    default:
                        cataclysm = 0.00f; break;
                }
                return (BaseCost * (1f - cataclysm));
            }
            if (SpellTree == SpellTree.Affliction)
            {
                return (BaseCost * (1f - (Character.WarlockTalents.Suppression * 0.02f)));
            }
            //demonology spells do not have any mana cost reductions at the moment
            return BaseCost;
        }

        /// <summary>
        /// Calculates the minimum (non-critical) direct hit damage per spell cast.
        /// </summary>
        public virtual float MinHitDamage()
        {
            float minDamage = CalculateDamage(BaseMinDamage, BaseDirectDamageCoefficient, BaseDirectDamageMultiplier);
            //firestone increases direct damage by 1%
            return (minDamage * (1 + FirestoneDirectDamageMultiplier));
        }
        /// <summary>
        /// Calculates the maximum (non-critical) direct hit damage per spell cast.
        /// </summary>
        public virtual float MaxHitDamage()
        {
            float maxDamage = CalculateDamage(BaseMaxDamage, BaseDirectDamageCoefficient, BaseDirectDamageMultiplier);
            //firestone increases direct damage by 1%
            return (maxDamage * (1 + FirestoneDirectDamageMultiplier));
        }

        /// <summary>
        /// Calculates the (non-critical) periodic damage per tick
        /// </summary>
        public virtual float TickHitDamage()
        {
            float tickDamage = CalculateDamage(BaseTickDamage, BaseTickDamageCoefficient, BaseTickDamageMultiplier);
            //spellstone increases periodic damage by 1%
            return (tickDamage * (1 + SpellstoneDotDamageMultiplier));
        }

        /// <summary>
        /// Calculates the crit damage per tick.
        /// </summary>
        public virtual float TickCritDamage()
        {
            return TicksMayCrit ? TickHitDamage() * CritMultiplier() : 0f;
        }

        /// <summary>
        /// A common function to calculate minimum, maximum or tick damage.
        /// </summary>
        /// <param name="baseValue">The base min, max or tick value.</param>
        /// <param name="coefficient">The base directdamage or tickdamage coefficient.</param>
        /// <param name="multiplier">A value based on talent improvements (or set bonuses).</param>
        /// <returns></returns>
        protected float CalculateDamage(float baseValue, float coefficient, float multiplier)
        {
            float spellpower = Stats.SpellPower;
            float additional = (MagicSchool == MagicSchool.Shadow)
                              ? Stats.SpellShadowDamageRating
                              : Stats.SpellFireDamageRating;

            spellpower += additional;
            
            //The basic spell damage formula is as follows: 
            //  D = ((B + (Sp * C)) * M)
            //where 
            //  D = damage, B = basevalue, Sp = spellpower, C = coefficient & M = multiplier
            float damage = ((baseValue + (spellpower * coefficient)) * multiplier);

            //apply any talents that increase spell damage
            damage *= (1 + (Character.WarlockTalents.DemonicPact * 0.01f));   //increases spell damage by 1/2/3/4/5%

            //apply buffs category: Damage(%) [sanctified ret / ferocious inspiration / arcane empowerment]
            damage *= (1 + Stats.BonusDamageMultiplier);

            //apply buffs category: Spell Damage Taken [curse of the elements / ebon plaguebringer / earth & moon]
            if (MagicSchool == MagicSchool.Shadow)
            {
                damage *= (1 + Stats.BonusShadowDamageMultiplier);
            }
            else if (MagicSchool == MagicSchool.Fire)
            {
                damage *= (1 + Stats.BonusFireDamageMultiplier);
            }

            return damage;
        }

        /// <summary>
        /// Returns the value to be used in spell critical strike damage calculations by taking the BaseCritMultiplier
        /// and applying any talent (i.e. Ruin / Pandemic) modifiers plus any other bonuses (i.e. crit metagem - 3% increased damage, stacking multiplicatively).
        /// </summary>
        /// <remarks>
        /// A normal spell hit does 100% of normal damage.
        /// A spell crit with no talents or other bonuses does (by default) 150% of normal damage [i.e 50% increased damage], therefore the BaseCritMultiplier is 1.5
        /// A spell crit with the crit metagem only (no Ruin / Pandemic talents) does 154.5% ((100% + 50%) x 1.03%) of normal damage.
        /// A spell crit with Ruin (5/5) only (no crit metagem) does 200% [100% + (50% * 2)] of normal damage.
        /// A spell crit with Ruin + metagem does 209% [(100% + (50% * 2)) * 1.03] of normal damage.
        /// </remarks>
        public float CritMultiplier()
        {
            // By default, a spell crit does 150% of normal damage [BaseSpellCritMultiplier]
            // [i.e. a default crit damage bonus of 50% (for all spells)]
            float critBonus = (BaseCritMultiplier - 1f);

            // The Chaotic Skyflare/Skyfire Diamond metagems increase spell crit damage by 3%,
            // therefore the crit damage bonus of the metagem is 4.5% (3% x 150%)
            float metagemBonus = (Stats.BonusSpellCritMultiplier * 1.5f);

            // So, a spell crit (incl. metagem) = 54.5% bonus damage
            float damageBonus = critBonus + metagemBonus;

            // Talent:Ruin - Increases the critical strike damage bonus of your Destruction spells
            // A spell crit with Ruin does (100% + 50% * 2) = 200% damage.
            // A spell crit with Ruin + metagem does (100% + 54.5% * 2) = 209% damage.
            if (SpellTree == SpellTree.Destruction)
            {
                damageBonus *= (1f + (Character.WarlockTalents.Ruin * 0.20f));
            }

            // Talent: Pandemic - increases critical strike damage of the following spells
            if ((Name == "Haunt") || (Name == "Corruption") || (Name == "Unstable Affliction"))
            {
                damageBonus *= (1f + (Character.WarlockTalents.Pandemic * 1f));
            }

            return (1f + damageBonus);
        }

        /// <summary>
        /// Returns the total crit chance from the BaseCritChance (talent improvements + any set bonuses) and current equipment.
        /// </summary>
        public float CritChance()
        {
            return (Stats.SpellCrit + BaseCritChance);
        }

        /// <summary>
        /// Returns the total hit chance. Capped at 100%.
        /// </summary>
        /// <remarks>
        /// Hit from talents, buffs and gear has already been factored into Stats.SpellHit at this stage
        /// </remarks>
        public float HitChance()
        {
            return Math.Min(Stats.SpellHit + Options.TargetHit, 1.00f);
        }

        /// <summary>
        /// Returns the average (non-critical) direct damage per spell cast.
        /// </summary>
        public float AvgHitDamage() { return (MinHitDamage() + MaxHitDamage()) / 2; }
        /// <summary>
        /// Returns the minimum crit damage per spell cast.
        /// </summary>
        public float MinCritDamage() { return MinHitDamage() * CritMultiplier(); }
        /// <summary>
        /// Returns the maximum crit damage per spell cast.
        /// </summary>
        public float MaxCritDamage() { return MaxHitDamage() * CritMultiplier(); }
        /// <summary>
        /// Returns the average crit damage per spell cast.
        /// </summary>
        public float AvgCritDamage() { return (MinCritDamage() + MaxCritDamage()) / 2; }
        /// <summary>
        /// Returns the average direct damage (including crits) per spell cast.
        /// </summary>
        public float AvgDirectDamage() { return (AvgHitDamage() * (HitChance() - CritChance())) + (AvgCritDamage() * CritChance()); }
        /// <summary>
        /// Returns the average damage (including crits) per tick.
        /// </summary>
        public float AvgTickDamage()
        {
            if (TicksMayCrit)
            {
                return (TickHitDamage() * (HitChance() - CritChance())) + (TickCritDamage() * CritChance());
            }
            return TickHitDamage();
        }
        /// <summary>
        /// Returns the average dot damage (including crits) per spell cast.
        /// </summary>
        public float AvgDotDamage() { return (AvgTickDamage() * NumberOfTicks); }
        /// <summary>
        /// Returns the average total direct+dot (including crit) damage per spellcast. 
        /// </summary>
        public float AvgTotalDamage() { return (AvgDirectDamage() + AvgDotDamage()); }
        /// <summary>
        /// Returns the damage per casttime. The GCD is used as the casttime for instant-cast spells.
        /// </summary>
        public float DpCT() { return AvgTotalDamage() / ((ExecuteTime() > 0) ? ExecuteTime() : GlobalCooldown()); }
        /// <summary>
        /// Returns the damage per second.
        /// </summary>
        public float DpS() { return AvgTotalDamage() / ((Cooldown() > 0) ? Cooldown() : (Duration() > 0) ? Duration() : (ExecuteTime() > 0) ? ExecuteTime() : GlobalCooldown()); }
        /// <summary>
        /// Returns the damage per mana.
        /// </summary>
        public float DpM() { return AvgTotalDamage() / Mana(); }
        /// <summary>
        /// Returns the effective mana cost per sec.
        /// </summary>
        public float MpS() { return Mana() / ((ExecuteTime() > 0) ? ExecuteTime() : GlobalCooldown()); }
        #endregion

        #region Combat simulation helpers
        /// <summary>
        /// This method contains the implementation of the 3 outcomes (miss, critical or normal hit) model and the 2-roll system used by ranged combat mechanics.
        /// </summary>        
        /// <remarks>
        /// <para>
        /// Additionally, this method will record combat stats and also raise events so that aura subscribers can trigger their effects
        /// (for example: conflagrate triggers the backdraft aura, which in turn applies a temporary haste effect to all destruction spells).
        /// </para>
        /// <para>
        /// <![CDATA[
        /// Useful links describing these concepts in more detail:
        /// - http://elitistjerks.com/f47/t15685-ranged_combat_mechanics/
        /// - http://www.wowwiki.com/Spell_hit 
        /// - http://forums.wow-europe.com/thread.html?topicId=14551513&sid=1  
        /// ]]>
        /// </para>
        /// <para>
        /// In the 2-roll system, the 1st roll determines miss or hit, while the 2nd determines if the hit will be critical or normal damage.
        /// The decision for each roll is made by comparing the actual vs expected miss, crit or hit rates.
        /// It does NOT use RNG at all. A system that uses RNG would require thousands of iterations to smooth out the randomness to arrive at a realistic result.
        /// </para>
        /// </remarks>
        /// <example>
        /// Lets assume a level 80 warlock with 30% crit chance and zero hit from gear/talents/buffs is attacking a level 83 boss target. 
        /// The attack table will be as follows:
        /// - 17% chance to miss the target
        /// - 83% chance to hit the target, of which 30% will be crits and the rest just normal damage hits
        /// So if we cast 100 shadowbolts, it would result in:
        /// - 17 misses, 
        /// - 25 critical hits [83% * 30% = 0.249 ~ 25] (the effect of spell hit on crit chance)
        /// - 58 normal hits
        /// </example>
        /// <example>
        /// A level 80 warlock with 30% crit chance and hit capped attacking a level 83 boss target. 
        /// The attack table will be as follows:
        /// - 0% chance to miss the target
        /// - 100% chance to hit the target, of which 30% will be crits and the rest just normal damage hits
        /// So if we cast 100 shadowbolts, it would result in:
        /// - 0 misses, 
        /// - 30 critical hits [100% * 30% = 0.3 ~ 30] (the effect of spell hit on crit chance)
        /// - 70 normal hits
        /// </example>        
        /// todo: damage lost due to resists, absorbs or target immunity is not yet implemented.
        public override void Execute()
        {
            if (IsTicking)
            {
                if (TicksMayCrit)
                {
                    float totalTickHitsAndCrits = Statistics.TickHits.Count + Statistics.TickCrits.Count;
                }
            }

            int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            float timer = ScheduledTime;
            float latency = (Options.Latency / 1000);   //in milliseconds
            float casttime = ExecuteTime();

            Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - begins casting: {2} [{3:0.00}s cast, {4:0.00}s latency] - finish casting @ {5:0.00}s", threadID, timer, Name, casttime, latency, (timer + casttime + latency)));

            // always increment the cast counter whenever a spell is cast 
            Statistics.CastCount += 1;

            #region The first roll determines if the spell should Miss or Hit the target.
            float totalHitChance = HitChance();
            float missChance = (totalHitChance >= 1) ? 0: (1 - totalHitChance);
            bool missed = false;

            if (missChance > 0)
            {
                //calculate the actual hitRate and compare it to the expected hitChance
                float hitRate = 0f;
                float totalDirectHitsAndCrits = Statistics.Hits.Count + Statistics.Crits.Count;
                if (totalDirectHitsAndCrits > 0)
                {
                    hitRate = (totalDirectHitsAndCrits / Statistics.CastCount);
                }
                
                if (hitRate > totalHitChance)
                {
                    //we've had enough hits - time to let a spell miss
                    missed = true;
                }
            }
            #endregion

            if (missed)
            {
                //boo! it missed :/
                Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - {2} misses the target", threadID, timer, Name));
                Statistics.Misses.Count += 1;                  //let's track the number of misses
                Statistics.Misses.Total += AvgTotalDamage();   //and also the potential damage that was lost
            }
            else
            {
                //yay! its a hit :) 
                #region The 2nd roll determines if this will be a regular Hit, or a Crit.
                if (MayCrit)
                {
                    //The effect of Hit chance on Critical Hit chance -> http://www.wowwiki.com/Spell_hit
                    float expectedCritRate = (totalHitChance * CritChance());

                    //The way WoW calculates crit rate is over ALL attacks. Crit rate is not based on hits only. In other words, if you have a 5% crit rate, that 5% chance includes misses. 
                    //http://forums.wow-europe.com/thread.html?topicId=14551513&sid=1
                    float actualCritRate = 0f;
                    if (Statistics.Crits.Count > 0)
                    {
                        actualCritRate = (float)Statistics.Crits.Count / Statistics.CastCount;
                    }

                    if (actualCritRate < expectedCritRate)
                    {
                        //critical hit
                        OnSpellDirectCrit(threadID, timer);
                    }
                    else
                    {
                        //normal hit
                        OnSpellDirectHit(threadID, timer);
                    }
                }
                else
                {
                    //spell cannot crit - normal hits only
                    OnSpellDirectHit(threadID, timer);
                }
                #endregion

                //and finally, dont forget to account for DoT spells
                if (NumberOfTicks > 0)
                {
                    IsTicking = true;
                }

                //raise an event so that subscribers can be notified whenever this spell has been cast
                OnSpellCast();
            }

            Statistics.ManaUsed += Mana();             //track mana consumption
            Statistics.ActiveTime += (ExecuteTime() > 0 ? ExecuteTime() : GlobalCooldown());   //and spell activity
        }

        /// <summary>
        /// This event is raised by the 'Execute' method whenever a spell is cast.
        /// Subscribers must be attached or removed via the "+=" or "-=" operators.
        /// </summary>
        protected internal event EventHandler SpellCast;

        /// <summary>
        /// This method ensures that subscribers are notified when the event has been raised.
        /// </summary>
        protected void OnSpellCast()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = SpellCast;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Normal hit damage
        /// </summary>
        protected void OnSpellDirectHit(float threadID, float timer)
        {
            float damage = AvgHitDamage();    //((MinDmg+MaxDmg)/2)  [excludes crit damage]
            Statistics.Hits.Count += 1;
            Statistics.Hits.Total += damage;
            Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - {2} hits the target for {3:0} damage", threadID, timer, Name, damage));
        }

        /// <summary>
        /// Critical hit damage
        /// </summary>
        protected void OnSpellDirectCrit(float threadID, float timer)
        {
            float damage = AvgCritDamage();     //((MinCritDamage+MaxCritDamage)/2)
            Statistics.Crits.Count += 1;
            Statistics.Crits.Total += damage;
            Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - {2} *crits* the target for {3:0} damage", threadID, timer, Name, damage));
        }

        protected void OnSpellTickHit()
        {
            
        }

        protected void OnSpellTickCrit()
        {
            
        }

        #endregion

        #region Aura handlers
        /// <summary>
        /// This method handles event notifications raised by the 'Backdraft' aura class.
        /// </summary>
        /// <param name="sender">An instance of the Backdraft aura class.</param>
        /// <param name="e">An instance of the generic EventArgs class - set to EventArgs.Empty because its unused.</param>
        protected internal virtual void BackdraftAuraHandler(Object sender, EventArgs e)
        {
            if (SpellTree == SpellTree.Destruction)
            {
                Backdraft backdraft = (Backdraft)sender;
                BackdraftIsActive = backdraft.Active;
            }
        }

        /// <summary>
        /// This method handles event notifications raised by the ShadowEmbrace aura class.
        /// </summary>
        /// <param name="sender">An instance of the ShadowEmbrace aura class</param>
        /// <param name="e">An instance of the generic EventArgs class - set to EventArgs.Empty because its unused.</param>
        protected internal virtual void ShadowEmbraceAuraHandler(Object sender, EventArgs e)
        {
            ShadowEmbrace shadowEmbrace = (ShadowEmbrace) sender;
            if (shadowEmbrace.Active)
            {
                //Debug.WriteLine(String.Format("thread:[{0}] | ShadowEmbrace aura is active - {1} [shadow school] periodic damage increased by 5%", System.Threading.Thread.CurrentThread.ManagedThreadId, Name));
            }
            else
            {
                //Debug.WriteLine(String.Format("thread:[{0}] | ShadowEmbrace aura has been removed - {1} periodic damage loses 5% damage bonus", System.Threading.Thread.CurrentThread.ManagedThreadId, Name));
            }
        }
        #endregion

        /// <summary>
        /// Returns the mother of all tooltips...
        /// </summary>
        public override string ToString() 
        {
            string castInfo = (ExecuteTime() == 0) ? String.Format("Instant Cast")
                                                : String.Format("{0:0.00} sec cast", ExecuteTime());
            if (Cooldown() > 0)
            {
                castInfo += String.Format(" ({0:0.00} sec cooldown)", Cooldown());
            }

            if (Channeled)
            {
                castInfo = String.Format("Channeled (Lasts {0:0.00} sec)", Duration());
            }

            float effectiveSpellpower = (Stats.SpellPower * BaseDirectDamageCoefficient * BaseDirectDamageMultiplier)
                                      + (Stats.SpellPower * (BaseTickDamageCoefficient * NumberOfTicks) * BaseTickDamageMultiplier);

            string advancedInfo = String.Format("{0:0}\tEffective Spellpower\r\n"
                                                + "{5:0.00%}\tTotal Coefficient (Direct + Dot)\r"
                                                + "{1:0.00%}\tBase Multiplier\r\n"
                                                + "{2:0.00%}\tCrit Multiplier\r\n"
                                                + "{3:0.00%}\tCrit\r\n"
                                                + "{4:0.00%}\tHit\r\n",
                                                effectiveSpellpower,
                                                (1 + (BaseDirectDamageMultiplier - 1) + (BaseTickDamageMultiplier - 1)),
                                                CritMultiplier(),
                                                CritChance(),
                                                HitChance(),
                                                BaseDirectDamageCoefficient + (BaseTickDamageCoefficient * NumberOfTicks)
                                                );

            string ddInfo = String.Format("Direct Damage breakdown:\r\n"
                                          + "- Coeff:\t{0:0.00%}\r\n"
                                          + "- Hit:\t{1:0} [{2:0}-{3:0}]\r\n"
                                          + "- Crit:\t{4:0} [{5:0}-{6:0}]\r\n"
                                          + "- Avg:\t{7:0}\r\n",
                                          BaseDirectDamageCoefficient,
                                          AvgHitDamage(), MinHitDamage(), MaxHitDamage(),
                                          AvgCritDamage(), MinCritDamage(), MaxCritDamage(),
                                          AvgDirectDamage()
                                          );

            string dotInfo = String.Format("Dot Damage breakdown (per tick):\r\n"
                                           + "- Coeff:\t{0:0.00%}\r\n"
                                           + "- Ticks:\t{1:0}\r\n"
                                           + "- Hit:\t{2:0}\r\n"
                                           + "- Crit:\t{3:0} (This applies to Corr, CoA and UA only)\r\n"
                                           + "- Avg:\t{4:0} [total={5:0}]\r\n",
                                           BaseTickDamageCoefficient,
                                           NumberOfTicks,
                                           TickHitDamage(),
                                           TickCritDamage(),
                                           AvgTickDamage(), AvgDotDamage()
                                           );

            string dmgInfo = String.Format("Overall Damage:\r\n" 
                                           + "- Total:\t{0:0}\r\n", 
                                           AvgTotalDamage()
                                           );

            string statsInfo = String.Format("Stats:\r\n"
                                             + "- DpS:\t{0:0}\r\n"
                                             + "- DpCT:\t{1:0}\r\n"
                                             + "- DpM:\t{2:0}\r\n"
                                             + "- MpS:\t{3:0}\r\n",
                                             DpS(), DpCT(), DpM(), MpS()
                                             );

            return String.Format("{0:0}*{1} (Rank {2})\r\n"
                                + "{3:0} mana ({4:0} yd range)\r\n"
                                + "{5}\r\n\r\n"
                                + "{6}\r\n\r\n" 
                                + "{7}\r\n" 
                                + "{8}\r\n"
                                + "{9}\r\n"
                                + "{10}\r\n"
                                + "{11}\r\n",
                                AvgTotalDamage(),
                                Name, Rank,
                                Mana(), Range(),
                                castInfo,
                                Description,
                                advancedInfo,
                                ddInfo,
                                dotInfo,
                                dmgInfo,
                                statsInfo
                                );
        }

        #region IComparable implementation
        /// <summary>
        /// Compares the scheduled time (calculated during the combat simulation) of the current action to another action.
        /// </summary>
        /// <param name="other">Some other action to compare with.</param>
        /// <returns>Returns an int that specifies whether the current instance is less than, equal to or greater than the value of the specified instance.</returns>
        public int CompareTo(Spell other)
        {
            return ScheduledTime.CompareTo(other.ScheduledTime);
        }
        #endregion
    }

    /// <summary>
    /// Yet another spell that gains a small bonus to its base min/max damage for each level above its highest rank.
    /// The rank 13 (highest) tooltip shows that it does 690/770 shadow damage @ level 79.
    /// This increases to 694/775 @ level 80 - a bonus of 4/5!
    /// </summary>
    public class ShadowBolt : Spell 
    {
        public ShadowBolt(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Shadow Bolt", 13, 79, 694, 775, 0, 0.17f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Destruction) 
        {
            MayCrit = true;
            BaseExecuteTime = 3;

            BaseDirectDamageCoefficient = (BaseExecuteTime / 3.5f) * (1 + (character.WarlockTalents.ShadowAndFlame * 0.04f));

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f) 
                                            + (character.WarlockTalents.ImprovedShadowBolt * 0.01f)
                                          );
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f) 
                           + stats.Warlock4T8 
                           + stats.Warlock2T10;
        }

        public override float ExecuteTime()
        {
            return Math.Max(1.0f, ((BaseExecuteTime - (Character.WarlockTalents.Bane * 0.1f)) / (1 + Stats.SpellHaste)));
        }

        public override float Cost()
        {
            float cost = base.Cost();
            cost *= (1f - (Character.WarlockTalents.GlyphSB ? 0.10f : 0f));
            return cost;
        }

        public override string Description
        {
            get
            {
                return String.Format("Sends a shadowy bolt at the enemy, causing {0:0} to {1:0} Shadow damage.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Deals 582 to 676 Fire damage to your target and an additional 145.5 to 169.0 Fire damage if the target is affected by an Immolate spell.
    /// </summary>
    /// todo: implement the additional damage bonus + Fire&Brimstone talent bonus if immolate is on the target
    public class Incinerate : Spell
    {
        public Incinerate(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Incinerate", 4, 80, 582, 676, 0, 0.14f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            MayCrit = true;
            BaseExecuteTime = 2.5f;

            BaseDirectDamageCoefficient = (BaseExecuteTime / 3.5f) * (1 + (character.WarlockTalents.ShadowAndFlame * 0.04f));

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f)
                                            + (character.WarlockTalents.GlyphIncinerate ? 0.05f : 0f)
                                         );
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseCritChance  = (character.WarlockTalents.Devastation * 0.05f) 
                            + stats.Warlock4T8
                            + stats.Warlock2T10;
        }

        public override float ExecuteTime()
        {
            return Math.Max(1.0f, ((BaseExecuteTime - (Character.WarlockTalents.Emberstorm * 0.05f)) / (1f + Stats.SpellHaste)));
        }

        public override string Description
        {
            get
            {
                return String.Format("Deals {0:0} to {1:0} Fire damage to your target and an additional 145.5 to 169 Fire damage if the target is affected by an Immolate spell.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Curses the target with agony, causing 1740 Shadow damage over 24 sec.
    /// This damage is dealt slowly at first, and builds up as the Curse reaches its full duration.
    /// Only one Curse per Warlock can be active on any one target.
    /// </summary>
    /// <remarks>
    /// A quick example showing how damage is spread over the ticks:
    /// (based on my warlock standing naked in IF, with just weapon equipped - yeah baby /flex)
    /// -> 191, 191, 191, 191, 288, 288, 288, 288, 385, 385, 385, 385, 482, 482
    ///   |------ weak ------||----- average ----||----- strong -----||--glyphed--|  (phases)
    ///   |------------------ Total(unglyphed) ----------------------|
    /// 
    /// - 4 weak ticks at the start (191 * 4 = 764 damage)
    /// - 4 average ticks in the middle (288 * 4 = 1152 damage)
    /// - 4 strong ticks at the end (385 * 4 = 1540 damage)
    /// - 2 extra ticks if the warlock has the glyph (482 * 2 = 964 damage)
    /// - total(unglyphed) = 764 + 1152 + 1540 = 3456
    /// - total(glyphed) = 3456 + 964 = 4420
    /// 
    /// To calculate the damage formulas for each tick type, we only need to look at the first 12 ticks
    /// because glyphed ticks do 25% more damage than strong ticks (482/385 ~= 1.25).
    /// 
    /// An average tick is = Total(unglyphed) * 1/12, 
    /// therefore, damage for the the average tick phase is calculated as:
    /// ->  T(average) = Total(unglyphed) * 4/12 
    ///                = 3456 * 4/12 
    ///                = 1152 
    /// The other 8 ticks of damage during the (weak + strong) phases is equivalent to:
    /// ->    T(other) = Total(unglyphed) * 8/12
    ///                = 3456 * 8/12               
    ///                = 2304 
    /// which is split as follows:
    /// - weak ticks represent 1/3 of T(other) -> (764/2304 ~= 0.33)
    /// - strong ticks represent 2/3 of T(other) -> (1540 / 2304 ~= 0.67)
    /// 
    /// The formulas to calculate each tick value:
    /// - weaktick = (T(other) * 1/3 * 1/4) - 1
    /// - averagetick = T(unglyphed) * 1/12
    /// - strongtick = (T(other) * 2/3 * 1/4) + 1
    /// - glyphtick = strongtick * 1.25
    /// </remarks>
    //  TODO: the increasing damage ticks are currently not implemented - using average ticks for the moment
    public class CurseOfAgony : Spell
    {
        public CurseOfAgony(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Curse of Agony", 9, 79, 0, 0, 145, 0.10f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 2;
            NumberOfTicks = 12;
            BaseDuration = 24;
            BaseGlobalCooldown = (character.WarlockTalents.AmplifyCurse > 0) ? 1.0f : 1.5f;

            if (character.WarlockTalents.GlyphCoA)
            {
                NumberOfTicks += 2;
            }

            //The coefficient is capped at 120% (10% per tick), which is an exception to the general rule (for DoT spells) of : C = Duration / 15
            //http://www.wowwiki.com/Spell_damage_and_healing#Exceptions
            BaseTickDamageCoefficient = 0.10f;

            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.ImprovedCurseOfAgony * 0.05f)
                                          + (character.WarlockTalents.ShadowMastery * 0.03f) 
                                          + (character.WarlockTalents.Contagion * 0.01f)
                                       );
            BaseTickDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Curses the target with agony, causing {0:0} Shadow damage over {1:0.00} sec.\r\nThis damage is dealt slowly at first, and builds up as the Curse reaches its full duration.\r\nOnly one Curse per Warlock can be active on any one target.", (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }

    }

    /// <summary>
    /// Curses the target with impending doom, causing 7300 Shadow damage after 1 min.
    /// If the target yields experience or honor when it dies from this damage, a Doomguard will be summoned.
    /// Cannot be cast on players.
    /// </summary>
    public class CurseOfDoom : Spell
    {
        public CurseOfDoom(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Curse of Doom", 3, 80, 0, 0, 7300, 0.15f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 60;
            NumberOfTicks = 1;
            BaseGlobalCooldown = (character.WarlockTalents.AmplifyCurse > 0) ? 1.0f : 1.5f;

            //The CoD spell coefficient has been capped at 200%, so its also an exception to the general spell coeff formula for DoT spells
            BaseTickDamageCoefficient = 2;

            //DrDamage addon does not take ShadowMastery or Malediction talents into account
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
            BaseTickDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Curses the target with impending doom, causing {0:0} Shadow damage after 1 min.\r\nIf the target yields experience or honor when it dies from this damage, a Doomguard will be summoned.\r\nCannot be cast on players.", (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Corrupts the target, causing 1080 Shadow damage over 18 sec.
    /// </summary>
    public class Corruption : Spell
    {
        public Corruption(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Corruption", 10, 77, 0, 0, 180, 0.14f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 3;
            NumberOfTicks = 6;
            BaseDuration = 18;

            BaseTickDamageCoefficient = ((BaseDuration / 15f) / NumberOfTicks);    //i.e. 20% per tick
            BaseTickDamageCoefficient += (character.WarlockTalents.EmpoweredCorruption * (0.12f / NumberOfTicks) ); //each talent point increases damage by 12/24/36%, which must be divided by numberofticks
            BaseTickDamageCoefficient += (character.WarlockTalents.EverlastingAffliction * 0.01f);

            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.ImprovedCorruption * 0.02f)
                                        + (character.WarlockTalents.ShadowMastery * 0.03f)
                                        + (character.WarlockTalents.Contagion * 0.01f)
                                        + (character.WarlockTalents.SiphonLife * 0.05f)
                                        + stats.Warlock4T9
                                       );
            BaseTickDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            if (character.WarlockTalents.Pandemic > 0)
            {
                TicksMayCrit = true;
                BaseCritChance = (character.WarlockTalents.Malediction * 0.03f)
                               + stats.Warlock2T10;
            }

            if (character.WarlockTalents.GlyphQuickDecay)
            {
                HastedTicks = false;
            }
        }

        public override string Description
        {
            get
            {
                return String.Format("Corrupts the target, causing {0:0} Shadow damage over {1:0.00} sec.", (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// Shadow energy slowly destroys the target, causing 1150 damage over 15 sec.
    /// In addition, if the Unstable Affliction is dispelled it will cause 2070 damage to the dispeller and silence them for 5 sec.
    /// </summary>
    public class UnstableAffliction : Spell
    {
        public UnstableAffliction(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Unstable Affliction", 5, 80, 0, 0, 230, 0.15f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 1.5f;
            BaseTickTime = 3;
            NumberOfTicks = 5;
            BaseDuration = 15;

            BaseTickDamageCoefficient = (BaseTickTime / 15);
            BaseTickDamageCoefficient += (character.WarlockTalents.EverlastingAffliction * 0.01f);

            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f)
                                          + (character.WarlockTalents.SiphonLife * 0.05f)
                                          + stats.Warlock2T8
                                          + stats.Warlock4T9
                                        );
            BaseTickDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            if (character.WarlockTalents.Pandemic > 0)
            {
                TicksMayCrit = true;
                BaseCritChance = (character.WarlockTalents.Malediction * 0.03f);
            }
        }

        public override float ExecuteTime()
        {
            float castTime = base.ExecuteTime();
            if (Character.WarlockTalents.GlyphUA)
            {
                castTime -= 0.2f;
            }

            return castTime;
        }

        public override string Description
        {
            get
            {
                return String.Format("Shadow energy slowly destroys the target, causing {0:0} damage over {1:0.00} sec.\r\nIn addition, if the Unstable Affliction is dispelled it will cause 2070 damage to the dispeller and silence them for 5 sec.", (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// Causes the enemy target to run in horror for 3 sec and causes 790 Shadow damage.
    /// The caster gains 300% of the damage caused in health.
    /// </summary>
    /// <remarks>
    /// Death Coil gains an additional +5 to base min/max damage for each level above its highest rank.
    /// So, the 790 shadow damage @ level 78 becomes:
    /// - 795 shadow damage @ level 79, 
    /// - 800 shadow damage @ level 80.
    /// </remarks>
    public class DeathCoil : Spell
    {
        public DeathCoil(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Death Coil", 6, 78, 800, 800, 0, 0.23f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            Binary = true;
            MayCrit = true;
            BaseExecuteTime = 0;
            BaseDuration = 3;
            BaseCooldown = 120;

            if (character.WarlockTalents.GlyphDeathCoil )
            {
                BaseDuration += 0.5f;
            }

            BaseDirectDamageCoefficient = ((1.5f / 3.5f) / 2);
            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
            BaseDirectDamageMultiplier *= (1 + (Character.WarlockTalents.Malediction * 0.01f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Causes the enemy target to run in horror for {0:0.00} sec and causes {1:0} Shadow damage.\r\nThe caster gains 300% of the damage caused in health.", Duration(), (BaseMinDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Transfers 133 health every 1 sec from the target to the caster. Lasts 5 sec.
    /// </summary>
    public class DrainLife : Spell
    {
        public DrainLife(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Drain Life", 9, 78, 0, 0, 133, 0.17f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 1;
            NumberOfTicks = 5;
            BaseDuration = 5;
            Binary = true;
            Channeled = true;
            HastedTicks = true;
            BaseTickDamageCoefficient = ((BaseTickTime / 3.5f) / 2);
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Transfers {0:0} health every {1:0.00} sec from the target to the caster. Lasts {2:0.00} sec.", (BaseTickDamage * BaseTickDamageMultiplier), TickTime(), Duration());
            }
        }
    }

    /// <summary>
    /// Drains the soul of the target, causing 710 Shadow damage over 15 sec.  
    /// If the target is at or below 25% health, Drain Soul causes four times the normal damage. 
    /// </summary>
    /// <remarks>Soul shard gain is not implemented because its irrelevant for the purposes of our simulation.</remarks>
    public class DrainSoul : Spell
    {
        public DrainSoul(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Drain Soul", 6, 77, 0, 0, 142, 0.14f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 3;
            NumberOfTicks = 5;
            BaseDuration = 15;
            Binary = true;
            Channeled = true;
            HastedTicks = true;
            BaseTickDamageCoefficient = ((BaseTickTime / 3.5f) / 2f);
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Drains the soul of the target, causing {0:0} Shadow damage over {1:0.00} sec.\r\nIf the target is at or below 25% health, Drain Soul causes four times the normal damage.", (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// You send a ghostly soul into the target, dealing 645 to 753 Shadow damage and increasing 
    /// all damage done by your Shadow damage-over-time effects on the target by 20% for 12 sec. 
    /// When the Haunt spell ends or is dispelled, the soul returns to you, healing you for 100% 
    /// of the damage it did to the target.
    /// </summary>
    public class Haunt : Spell
    {
        public Haunt(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Haunt", 4, 80, 645, 753, 0, 0.12f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 1.5f;
            BaseDuration = 12;
            BaseCooldown = 8;
            MayCrit = true;

            BaseDirectDamageCoefficient = (BaseExecuteTime / 3.5f);

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.GlyphHaunt ? 0.03f : 0f));     //haunt glyph is multiplicative
        }

        public override string Description
        {
            get
            {
                return String.Format("You send a ghostly soul into the target, dealing {0:0} to {1:0} Shadow damage and increasing\r\nall damage done by your Shadow damage-over-time effects on the target by 20% for {2:0.00} sec.\r\nWhen the Haunt spell ends or is dispelled, the soul returns to you, healing you for 100%\r\nof the damage it did to the target.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// Embeds a demon seed in the enemy target, causing 1518 Shadow damage over 18 sec.  
    /// When the target takes 1518 total damage or dies, the seed will inflict 1633 to 1897 
    /// Shadow damage to all enemies within 15 yards of the target. 
    /// Only one Corruption spell per Warlock can be active on any one target.
    /// </summary>
    /// <remarks>Shadowflame is another hybrid spell - using the same formula for both.</remarks>
    // TODO: damage cap not implemented
    public class SeedOfCorruption : Spell
    {
        public SeedOfCorruption(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Seed of Corruption", 3, 80, 1633, 1897, 253, 0.34f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            BaseExecuteTime = 2;
            BaseTickTime = 3;
            NumberOfTicks = 6;
            BaseDuration = 18;
            MayCrit = true;

            // WoWWiki states that the spell coefficients for hybrid spells are calculated as follows:
            //       x = Duration / 15      => (18 / 15) = 1.2
            //       y = ExecuteTime / 3.5     => (2 / 3.5) = 0.57142
            //     CDD = y^2 / (x + y)      => (0.57142)^2 / (1.2 + 0.57142) => 0.184332 = 18.43%
            //    CDoT = x^2 / (x + y)      => (1.2)^2 / (1.2 + 0.57142) => 0.812903  = 81.29% [or 13.55% per tick]
            //  CTotal = CDD + CDot         => 99.72%
            // [source: http://www.wowwiki.com/Spell_damage_and_healing#Hybrid_spells_.28Combined_standard_and_over-time_spells.29]

            // EJ states that the spell coefficients for hybrid spells are calculated as follows: 
            //   (use http://www.forkosh.dreamhost.com/source_mathtex.html#preview to view the formulas)
            //    DD portion = \frac{2}{3.5}*\frac{x}{x+y}  
            //   Dot portion = \frac{18}{15}*\frac{y}{x+y}
            // where 
            //  - x is the avg base direct damage, in this case => ((1633 + 1897) / 2) = 1765
            //  - y is the base dot damage (tickdamage * numberofticks), in this case => 1518 [or (253 * 6)]
            //  - x+y = 1765 + 1518 => 3283
            // This works out to:
            //     CDD = (2 / 3.5) * (1765/3283)  => 0.571428 * 0.53761 => 0.30721 => 30.72%
            //    CDoT = (18 / 15 ) * (1518/3283) => 1.2 * 0.46238      => 0.55485 => 55.49 % [or 9.247% per tick]
            //  CTotal = CDD + CDot => 0.86206    => 86.21%
            // [source: http://elitistjerks.com/f47/t19038-spell_coefficients/#Warlock ]

            // Using the EJ formula because their theorycrafting has always been pretty accurate.

            float x = ((BaseMinDamage + BaseMaxDamage) / 2);    //avg base damage
            float y = (BaseTickDamage * NumberOfTicks);         //dot damage
            float t = (x + y);

            BaseDirectDamageCoefficient = ((BaseExecuteTime / 3.5f) * (x / t));
            BaseTickDamageCoefficient = (((BaseDuration / 15f) * (y / t)) / NumberOfTicks); 

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f)
                                            + (character.WarlockTalents.Contagion * 0.01f)
                                            + (character.WarlockTalents.SiphonLife * 0.05f)
                                          );
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseTickDamageMultiplier = BaseDirectDamageMultiplier;

            BaseCritChance = (character.WarlockTalents.ImprovedCorruption * 0.01f);
        }

        public override string Description
        {
            get
            {
                return String.Format("Embeds a demon seed in the enemy target, causing {0:0} Shadow damage over {1:0.00} sec.\r\n" 
                    + "When the target takes {0:0} total damage or dies, the seed will inflict {2:0} to {3:0}\r\n" 
                    + "Shadow damage to all enemies within 15 yards of the target.\r\n" 
                    + "Only one Corruption spell per Warlock can be active on any one target.", 
                    (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), 
                    Duration(),
                    (BaseMinDamage * BaseDirectDamageMultiplier), 
                    (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Targets in a cone in front of the caster take 615 to 671 Shadow damage and an additional 644 Fire damage over 8 sec.
    /// </summary>
    /// <remarks>
    /// 1) The directdamage portion is shadow, while the dot portion is fire!
    /// 2) Seed of Corruption is another hybrid spell - using the same formula for both.
    /// </remarks>
    /// TODO: damage cap not implemented
    public class Shadowflame : Spell
    {
        public Shadowflame(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Shadowflame", 2, 80, 615, 671, 161, 0.25f, stats, character, options, Color.FromArgb(255, 255, 215, 0), MagicSchool.Shadow, SpellTree.Destruction) 
        {
            BaseRange = 10;
            BaseExecuteTime = 0;
            BaseTickTime = 2;
            NumberOfTicks = 4;
            BaseDuration = 8;
            BaseCooldown = 15;
            MayCrit = true;

            // The spell coefficients are hardcoded in SimCraft as:
            //      DD = 14.29f & DoT = 28%
            // but there is no formula or explanation of how this was calculated

            // WoWWiki states that the spell coefficients for hybrid spells are calculated as follows:
            //       x = Duration / 15       => (8/15) = 0.53333
            //       y = ExecuteTime / 3.5      => (1.5/3.5) = 0.42857
            //     CDD = y^2 / (x + y)     => (0.42857)^2 / (0.53333 + 0.42857) => 0.190948 = 19.095%
            //    CDoT = x^2 / (x + y)    => (0.53333)^2 / (0.53333 + 0.42857) => 0.29571  = 29.571% [or 7.39% per tick]
            //  CTotal = CDD + CDot     => 48.67%
            // [source: http://www.wowwiki.com/Spell_damage_and_healing#Hybrid_spells_.28Combined_standard_and_over-time_spells.29]

            // EJ states that the spell coefficients for hybrid spells are calculated as follows: 
            //   (use http://www.forkosh.dreamhost.com/source_mathtex.html#preview to view the formulas)
            //    DD portion = \frac{1.5}{3.5}*\frac{x}{x+y}  
            //   Dot portion = \frac{8}{15}*\frac{y}{x+y}
            // where 
            //  - x is the avg base direct damage, in this case ((615+671)/2 =643), 
            //  - y is the base dot damage (tickdamage * numberofticks), in this case (=644) or (161 * 4),
            //  - x+y = 643+644 = 1287
            // This works out to:
            //     CDD = (1.5/3.5) * (643/(643 + 644)) => 0.42857 * 0.4996 => 0.214119 => 21.41%
            //    CDoT = (8/15 ) * (644/(643 + 644)) => 0.5333 * 0.5003 => 0.26687 => 26.69% [or 6.67% per tick]
            //  CTotal = CDD + CDot => 0.480993 => 48.10%
            // [source: http://elitistjerks.com/f47/t19038-spell_coefficients/#Warlock ]

            // Two very different formulas, but similar results.
            // Anyhoo, using the EJ formula because their theorycrafting has always been pretty accurate.

            float x = ((BaseMinDamage + BaseMaxDamage) / 2);
            float y = (BaseTickDamage * NumberOfTicks);
            float t = (x + y);

            BaseDirectDamageCoefficient = ((1.5f / 3.5f) * (x / t));
            //shadowmastery applies to shadow spells, which is the directdamage portion of shadowflame
            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseTickDamageCoefficient = (((BaseDuration / 15f) * (y / t)) / NumberOfTicks); 
            //emberstorm applies to fire spells, which is the dot portion of shadowflame
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));
            BaseTickDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f);
        }

        public override string Description
        {
            get
            {
                return String.Format("Targets in a cone in front of the caster take {0:0} to {1:0} Shadow damage and an additional {2:0} Fire damage over {3:0.00} sec.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier), (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// Instantly blasts the target for 775 to 865 Shadow damage. 
    /// </summary>
    /// <remarks>Soul shard gain is not implemented because its irrelevant for the purposes of our simulation.</remarks>
    public class Shadowburn : Spell
    {
        public Shadowburn(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Shadowburn", 10, 80, 775, 865, 0, 0.20f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Destruction) 
        {
            BaseExecuteTime = 0;
            BaseRange = 20;
            BaseCooldown = 15;
            MayCrit = true;
            BaseDirectDamageCoefficient = (1.5f / 3.5f) * (1 + (character.WarlockTalents.ShadowAndFlame * 0.04f));
            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f);
        }

        public override string Description
        {
            get
            {
                return String.Format("Instantly blasts the target for {0:0} to {1:0} Shadow damage.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Shadowfury is unleashed, causing 968 to 1152 Shadow damage and stunning all enemies within 8 yds for 3 sec.
    /// </summary>
    /// todo: implement shadowfury aoe effect
    public class Shadowfury : Spell
    {
        public Shadowfury(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Shadowfury", 5, 80, 968, 1152, 0, 0.27f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Destruction) 
        {
            MayCrit = true;
            BaseExecuteTime = 0;
            BaseCooldown = 20;
            BaseDuration = 3;
            BaseRange = 8;
            BaseDirectDamageCoefficient = (1.5f / 3.5f);
            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.ShadowMastery * 0.03f));
            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f);
        }

        public override string Description
        {
            get
            {
                return String.Format("Shadowfury is unleashed, causing {0:0} to {1:0} Shadow damage and stunning all enemies within {2:0} yds for {3:0.00} sec.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier), Range(), Duration());
            }
        }
    }

    /// <summary>
    /// Burns the enemy for 460 Fire damage and then an additional 785 Fire damage over 15 sec.
    /// </summary>
    public class Immolate : Spell
    {
        public Immolate(Stats stats, Character character, CalculationOptionsWarlock options) 
            : base("Immolate", 11, 80, 460, 460, 157, 0.17f, stats, character, options, Color.FromArgb(255, 255, 215, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            BaseExecuteTime = 2;
            BaseTickTime = 3;
            NumberOfTicks = 5 + (character.WarlockTalents.MoltenCore * 1);
            BaseDuration = 15 + (character.WarlockTalents.MoltenCore * 3);
            MayCrit = true;
            TicksMayCrit = true;    //from patch 3.3.3

            //immolate coefficients were hotfixed to 120% in patch 3.0
            //direct damage = 20%, and 100% for the dot (20% per tick) [before talents & other bonuses] 
            //[this is another exception to the spell coeff formula for hybrid spells]
            BaseDirectDamageCoefficient = 0.20f;
            BaseTickDamageCoefficient = 0.20f;

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f)
                                            + (character.WarlockTalents.ImprovedImmolate * 0.1f)
                                            + (character.WarlockTalents.GlyphImmolate ? 0.10f : 0f)
                                            + (stats.Warlock2T8 / 2)
                                            + stats.Warlock4T9
                                         );
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseTickDamageMultiplier = (BaseDirectDamageMultiplier + (character.WarlockTalents.Aftermath * 0.03f));

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f);
        }

        public override float ExecuteTime()
        {
            return Math.Max(1, ((BaseExecuteTime - (Character.WarlockTalents.Bane * 0.1f)) / (1 + Stats.SpellHaste)));
        }

        public override string Description
        {
            get
            {
                return String.Format("Burns the enemy for {0:0} Fire damage and then an additional {1:0} Fire damage over {2:0.00} sec.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// Yet another spell that gains a small bonus for each level above its highest rank.
    /// In this case, the rank 7 (highest) tooltip shows that it causes 2700 fire damage over 8 sec (i.e. 675 per tick).
    /// This increases to 2708 fire damage over 8 sec @ level 80 - i.e. 677 per tick!
    /// </summary>
    /// <remarks>
    /// Some theorycrafting mumbo jumbo for whoever is interested:
    /// According to http://www.wowwiki.com/Spell_damage_and_healing#Area_of_effect_spells, the coefficient is calculated as follows:
    ///     (C) = (Duration / 7) => 8/7  => ~114% (or 28.57% per tick)
    /// This is supported by theorycrafting on EJ -> http://elitistjerks.com/f47/t19038-spell_coefficients/#Warlock
    ///     (C) = (ExecuteTime / 3.5), which is then divided by 2 (for AOE spells), which results in ~114% (28.57% per tick).
    ///         => ((2 / 3.5) / 2) => 28.57% per tick
    /// However, http://www.wowwiki.com/Rain_of_Fire states that RoF receives 33% of the bonus from +damage gear (which is then split over the 4 ticks),
    /// while http://www.wowwiki.com/Spell_power_coefficient has the RoF coefficient as 57.26% per tick (no calculation provided in either case, but clearly both are wrong) ...
    /// Anyhoo - going with 28.57% per tick because thats validated by the EJ theorycrafting. The RoF and Spellpower coefficient pages on wowwiki probably just need to be updated.
    /// </remarks>
    /// TODO: the damage cap is currently not implemented
    public class RainOfFire : Spell
    {
        public RainOfFire(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Rain of Fire", 7, 79, 0, 0, 677, 0.57f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 2;
            NumberOfTicks = 4;
            BaseDuration = 8;
            Channeled = true;
            AreaOfEffect = true;
            TicksMayCrit = true;
            HastedTicks = true;
            BaseTickDamageCoefficient = ((BaseDuration / 7) / NumberOfTicks);
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));
            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f);
        }

        public override string Description
        {
            get
            {
                return String.Format("Calls down a fiery rain to burn enemies in the area of effect for {0:0} Fire damage over {1:0.00} sec.", (BaseTickDamage * NumberOfTicks * BaseTickDamageMultiplier), Duration());
            }
        }
    }

    /// <summary>
    /// Another spell that gains a small bonus for each level above its highest rank.
    /// In this case, the rank 5 (highest) tooltip shows that it causes 451 fire damage (to everyone, including self) every second (for 15 seconds) @ level 78.
    /// This increases to 453 fire damage @ level 80!
    /// </summary>
    /// <remarks>
    /// The coefficient formula: http://www.wowwiki.com/Spell_damage_and_healing#Area_of_effect_spells
    ///      C = Duration / 7 => 15/7  => 2.1428 => 214% (or 14.28% per tick over 15 second duration)
    /// </remarks>
    /// TODO: damage cap not implemented
    public class Hellfire : Spell
    {
        public Hellfire(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Hellfire", 5, 78, 0, 0, 453, 0.64f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 1;
            NumberOfTicks = 15;
            BaseDuration = 15;
            BaseRange = 10;
            Channeled = true;
            AreaOfEffect = true;
            HastedTicks = true;
            BaseTickDamageCoefficient = ((BaseDuration / 7) / NumberOfTicks);
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Ignites the area surrounding the caster, causing {0:0} Fire self-damage and {0:0} Fire damage to all nearby enemies every {1:0.00} sec.  Lasts {2:0.00} sec.", (BaseTickDamage * BaseTickDamageMultiplier), TickTime(), Duration());
            }
        }
    }

    /// <summary>
    /// Ignites the area surrounds you, causing 481 Fire damage to all nearby enemies every 1 sec. Lasts 15 sec.
    /// </summary>
    /// <remarks>
    /// Note: the wowhead tooltip states 251 fire damage per sec - the correct value is infact 481 fire damage per sec.
    /// coefficient -> http://www.wowwiki.com/Spell_damage_and_healing#Area_of_effect_spells
    ///      C = Duration / 7 => 15/7  => 2.1428 => 214% (or 14.28% per tick over 15 second duration)
    /// </remarks>
    /// TODO: damage cap not implemented
    public class ImmolationAura : Spell
    {
        public ImmolationAura(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Immolation Aura", 1, 60, 0, 0, 481, 0.64f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Demonology)
        {
            BaseExecuteTime = 0;
            BaseTickTime = 1;
            NumberOfTicks = 15;
            BaseDuration = 15;
            BaseCooldown = 30;
            BaseRange = 10;
            Channeled = true;
            AreaOfEffect = true;
            HastedTicks = true;
            BaseTickDamageCoefficient = ((BaseDuration / 7) / NumberOfTicks);
            BaseTickDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));
        }

        public override string Description
        {
            get
            {
                return String.Format("Ignites the area surrounds you, causing {0} Fire damage to all nearby enemies every {1:0.00} sec. Lasts {2:0.00} sec.", (BaseTickDamage * BaseTickDamageMultiplier), TickTime(), Duration());
            }
        }
    }

    /// <summary>
    /// Yet another spell that gains a bonus to its base min/max damage at level 80.
    /// The rank 10 (highest) tooltip shows 343/405 fire damage @ lvl 79.
    /// This increases to 347/410 fire damage @ lvl 80 - a bonus of 4/5!
    /// </summary>
    /// TODO: Threat gain not implemented.
    public class SearingPain : Spell
    {
        public SearingPain(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Searing Pain", 10, 79, 343, 405, 0, 0.08f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            BaseExecuteTime = 1.5f;
            MayCrit = true;

            BaseDirectDamageCoefficient = (BaseExecuteTime / 3.5f);

            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));
            BaseDirectDamageMultiplier *= (1 + (character.WarlockTalents.Malediction * 0.01f));

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f)
                           + (character.WarlockTalents.ImprovedSearingPain > 0 ? 0.01f + 0.03f * character.WarlockTalents.ImprovedSearingPain: 0);
        }

        public override string Description
        {
            get
            {
                return String.Format("Inflict searing pain on the enemy target, causing {0:0} to {1:0} Fire damage. Causes a high amount of threat.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Burn the enemy's soul, causing 1323 to 1657 Fire damage.
    /// </summary>
    /// <remarks>
    /// Soul Fire: Reagent cost not implemented.
    /// The spell coefficient formula for direct damage spells is usually:
    ///      C = ExecuteTime / 3.5f
    /// However, soulfire is an exception to this rule because it has been capped at 115%.
    /// source: http://www.wowwiki.com/Spell_damage_and_healing#Exceptions & http://elitistjerks.com/f47/t19038-spell_coefficients/#Warlock
    /// </remarks>
    public class SoulFire : Spell
    {
        public SoulFire(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Soul Fire", 6, 80, 1323, 1657, 0, 0.09f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            MayCrit = true;
            BaseExecuteTime = 6;
            BaseDirectDamageCoefficient = 1.15f;
            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f)
                           + stats.Warlock2T10;
        }

        public override float ExecuteTime()
        {
            return Math.Max(1, ((BaseExecuteTime - (Character.WarlockTalents.Bane * 0.4f)) / (1 + Stats.SpellHaste)));
        }

        public override string Description
        {
            get
            {
                return String.Format("Burn the enemy's soul, causing {0:0} to {1:0} Fire damage.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Causes (or consumes) an Immolate or Shadowflame effect on the enemy target to instantly deal 
    /// damage equal to 60% of your Immolate or Shadowflame, and causes an additional 20% damage over 6 sec.
    /// </summary>
    public class Conflagrate : Spell
    {
        public Conflagrate(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Conflagrate", 8, 80, 0, 0, 0, 0.16f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            BaseExecuteTime = 0;
            BaseTickTime = 2;
            NumberOfTicks = 3;
            BaseDuration = 6;
            BaseCooldown = 10;
            MayCrit = true;         //the initial direct conflag hit can crit
            TicksMayCrit = true;    //the conflag dot ticks can also crit

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f)
                           + (character.WarlockTalents.FireAndBrimstone * 0.05f);
        }

        public override string Description
        {
            get
            {
                return String.Format("Causes (or consumes) an Immolate or Shadowflame effect on the enemy target to instantly deal\r\ndamage equal to 60% of your Immolate or Shadowflame, and causes an additional 20% damage over 6 sec.\r\n\r\nConflagrate is calculated from Immolate or Shadowflame dot damage, so you wont see effective sp or coefficients below.");
            }
        }

        /// <summary>
        /// The direct damage portion of Conflag instantly hits for 60% of the Immolate or Shadowflame DOT damage!
        /// </summary>
        /// TODO: check for immolate or shadowflame on the target 
        public override float MinHitDamage()
        {
            Spell immolate = new Immolate(Stats, Character, Options);
            float damage = (immolate.TickHitDamage() * immolate.NumberOfTicks);
            return (damage * 0.60f);
        }

        /// <summary>
        /// The direct damage portion of Conflag instantly hits for 60% of the Immolate or Shadowflame DOT damage!
        /// </summary>
        public override float MaxHitDamage()
        {
            Spell immolate = new Immolate(Stats, Character, Options);
            float damage = (immolate.TickHitDamage() * immolate.NumberOfTicks);
            return (damage * 0.60f);
        }

        /// <summary>
        /// The dot portion of Conflag deals 20% of the Immolate or Shadowflame DOT damage over 6 seconds [3 ticks]!
        /// </summary>
        public override float TickHitDamage()
        {
            Spell immolate = new Immolate(Stats, Character, Options);
            float dotdamage = ((immolate.TickHitDamage() * immolate.NumberOfTicks) * 0.20f);
            float tickDamage = (dotdamage / NumberOfTicks);
            return tickDamage;
        }
    }

    /// <summary>
    /// Sends a bolt of chaotic fire at the enemy, dealing 837 to 1061 Fire damage.
    /// Chaos Bolt cannot be resisted, and pierces through all absorption effects.
    /// </summary>
    public class ChaosBolt : Spell
    {
        public ChaosBolt(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Chaos Bolt", 4, 80, 1429, 1813, 0, 0.07f, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Fire, SpellTree.Destruction) 
        {
            Binary = true;
            MayCrit = true;
            BaseCooldown = 12 - (character.WarlockTalents.GlyphChaosBolt ? 2 : 0);
            BaseExecuteTime = 2.5f;

            BaseDirectDamageCoefficient = (BaseExecuteTime / 3.5f) * (1 + (character.WarlockTalents.ShadowAndFlame * 0.04f));
            BaseDirectDamageMultiplier = (1 + (character.WarlockTalents.Emberstorm * 0.03f));

            BaseCritChance = (character.WarlockTalents.Devastation * 0.05f);
        }

        public override float ExecuteTime()
        {
            return Math.Max(1, ((BaseExecuteTime - (Character.WarlockTalents.Bane * 0.1f)) / (1 + Stats.SpellHaste)));
        }

        public override string Description
        {
            get
            {
                return String.Format("Sends a bolt of chaotic fire at the enemy, dealing {0:0} to {1:0} Fire damage.\r\nChaos Bolt cannot be resisted, and pierces through all absorption effects.", (BaseMinDamage * BaseDirectDamageMultiplier), (BaseMaxDamage * BaseDirectDamageMultiplier));
            }
        }
    }

    /// <summary>
    /// Converts 2000 health into (2000 + [Spellpower * 0.5]) mana.
    /// </summary>
    /// <remarks>
    /// The tooltip lies - dont believe it!
    /// </remarks>
    public class LifeTap : Spell
    {
        private const int BaseLifeTap = 2000;

        public LifeTap(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Life Tap", 8, 80, 0, 0, 0, 0, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            Harmful = false;
        }

        /// <summary>
        /// Returns the amount of health that will be converted to mana.
        /// </summary>
        public float HealthLost()
        {
            return BaseLifeTap;
        }

        /// <summary>
        /// Returns the amount of mana that will be gained. 
        /// </summary>
        public float ManaGained()
        {
            return (float)Math.Floor((BaseLifeTap + (Stats.SpellPower * 0.5f)) * (1 + (Character.WarlockTalents.ImprovedLifeTap * 0.10f)));
        }

        public override string Description
        {
            get
            {
                return String.Format("Converts {0:0} health into {1:0} mana.", HealthLost(), ManaGained());
            }
        }

        public override string ToString()
        {
            float lifetapFromSpellpower = (Stats.SpellPower * 0.5f);
            float lifetapFromTalent = (ManaGained() - lifetapFromSpellpower - BaseLifeTap);

            return String.Format("{0:0}*{1} (Rank {2})\r\nInstant\r\n{3}\r\n\r\nBreakdown:\r\n{4:0}\tfrom base health\r\n{5:0}\tfrom Spellpower (50%)\r\n{6:0}\tfrom Talent: Improved Life Tap",
                ManaGained(), 
                Name, 
                Rank,
                Description,
                HealthLost(),
                lifetapFromSpellpower,
                lifetapFromTalent);
        }
    }

    /// <summary>
    /// Drains 1200 of your summoned demon's Mana, returning 100% to you.
    /// </summary>
    public class DarkPact : Spell
    {
        private const int BaseManaFromPet = 1200;

        public DarkPact(Stats stats, Character character, CalculationOptionsWarlock options)
            : base("Dark Pact", 5, 80, 0, 0, 0, 0, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.Affliction) 
        {
            Harmful = false;
            //http://www.wowwiki.com/Dark_Pact - receives 96% of your +shadow damage bonus
            BaseDirectDamageCoefficient = 0.96f;
        }

        /// <summary>
        /// Calculates the amount of mana returned by DarkPact.
        /// This scales with spellpower from patch 3.3.
        /// </summary>
        protected float ManaDrainedFromPet()
        {
            return (float)Math.Floor(BaseManaFromPet + (Stats.SpellPower + Stats.SpellShadowDamageRating) * BaseDirectDamageCoefficient);
        }

        public override string Description
        {
            get
            {
                return String.Format("Drains {0} of your summoned demon's Mana, returning 100% to you.", ManaDrainedFromPet());
            }
        }

        public override string ToString()
        {
            float darkpactFromSpellpower = ((Stats.SpellPower + Stats.SpellShadowDamageRating) * BaseDirectDamageCoefficient);

            return String.Format("{0:0}*{1} (Rank {2})\r\nInstant\r\n{3}\r\n\r\nBreakdown:\r\n{4:0}\tfrom base mana\r\n{5:0}\tfrom {6:0} Spellpower [{7:0%} coefficient]",
                ManaDrainedFromPet(), 
                Name, 
                Rank, 
                Description,
                BaseManaFromPet,
                darkpactFromSpellpower,
                Stats.SpellPower,
                BaseDirectDamageCoefficient);
        }
    }
}