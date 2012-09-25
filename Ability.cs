using System;
using System.Windows.Media;

namespace Rawr.WarlockSim
{
    /// <summary>
    /// Warlock abilities always belong to one of their talent trees: affliction, demonology or destruction.
    /// Minions do not have talent trees, so their abilities are defaulted to 'none'.
    /// </summary>
    public enum SpellTree { None, Affliction, Demonology, Destruction }

    /// <summary>
    /// The base class used for warlock or pet abilities.
    /// </summary>
    public abstract class Ability : IComparable<Ability>
    {
        #region general properties
        /// <summary>
        /// The name of the ability.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The highest rank for this ability.
        /// </summary>
        public int Rank { get; protected set; }
        /// <summary>
        /// The level at which this ability becomes available.
        /// </summary>
        public int Level { get; protected set; }
        /// <summary>
        /// The description for this ability which will be included in the tooltip.
        /// </summary>
        public virtual string Description { get; protected set; }
        /// <summary>
        /// A binary ability means that it can only have the full effect or none at all; partial effects (due to resists) are not possible. 
        /// Normally, damage spells are only binary if they have an additional non-damage effect (e.g. Death Coil, which has a fear effect) 
        /// but there are exceptions (e.g. Chaos Bolt is binary but it has no additional effect).
        /// </summary>
        public bool Binary { get; protected set; }
        /// <summary>
        /// Indicates if the ability is channeled or not.
        /// </summary>
        public bool Channeled { get; protected set; }
        /// <summary>
        /// Indicates if the ability has an area of effect.
        /// </summary>
        public bool AreaOfEffect { get; protected set; }
        /// <summary>
        /// Indicates if the ability causes damage to the enemy target, or not.
        /// </summary>
        public bool Harmful { get; protected set; }
        /// <summary>
        /// The accumulated stats [gear, talents, buffs, etc] to be used in the calculations.
        /// </summary>
        public Stats Stats { get; protected set; }
        /// <summary>
        /// The current character profile to be used in the calculations.
        /// </summary>
        public Character Character { get; protected set; }
        /// <summary>
        /// The calculation options.
        /// </summary>
        public CalculationOptionsWarlock Options { get; protected set; }
        /// <summary>
        /// The color to be used on the graphs.
        /// </summary>
        public Color GraphColor { get; protected set; }
        /// <summary>
        /// A summary of the major statistics [number of casts, hits, misses, crits, total damage, etc] as calculated during the combat simulation.
        /// </summary>
        public Statistics Statistics { get; protected set; }
        /// <summary>
        /// The school of magic associated with this ability.
        /// </summary>
        public MagicSchool MagicSchool { get; protected set; }
        /// <summary>
        /// The talent tree associated with this ability.
        /// </summary>
        public SpellTree SpellTree { get; protected set; }
        #endregion

        #region properties that could be modified by talents / glyphs
        /// <summary>
        /// The base mana cost percentage. 
        /// </summary>
        public float BaseCost { get; protected set; }
        /// <summary>
        /// The base time (in seconds) to execute this ability. 
        /// </summary>
        public float BaseExecuteTime { get; protected set; }
        /// <summary>
        /// The base global cooldown value.
        /// </summary>
        public float BaseGlobalCooldown { get; protected set; }
        /// <summary>
        /// The base minimum direct damage for this ability.
        /// </summary>
        public float BaseMinDamage { get; protected set; }
        /// <summary>
        /// The base maximum direct damage for this ability
        /// </summary>
        public float BaseMaxDamage { get; protected set; }
        /// <summary>
        /// The value indicating how much spellpower contributes to direct damage.
        /// </summary>
        public float BaseDirectDamageCoefficient { get; protected set; }
        /// <summary>
        /// The value (based on talent improvements) that must be applied when calculating the direct damage result.
        /// </summary>
        public float BaseDirectDamageMultiplier { get; protected set; }
        /// <summary>
        /// The base critical strike chance (based on talent improvements) for this ability.
        /// </summary>
        public float BaseCritChance { get; protected set; }
        /// <summary>
        /// The value that is applied to to normal damage when calculating critical strike damage.
        /// </summary>
        /// <remarks>
        /// Warlock spell critical strikes generally deal 150% normal damage without talents. 
        /// This can be increased to 200% with the following talents:
        /// - Ruin [applies to all destruction spells], and
        /// - Pandemic [applies to the Haunt, Corruption and Unstable Affliction only]
        /// Additionally, the Chaotic Skyflare/Skyfire Diamond metagems increase spell crit damage by 3%
        /// and this in turn stacks with Ruin - for a total of 209%!
        /// </remarks>
        public float BaseCritMultiplier { get; protected set; }
        /// <summary>
        /// The base range (in yards) for this ability.
        /// </summary>
        public int BaseRange { get; protected set; }
        /// <summary>
        /// The base cooldown time (in seconds) for this ability.
        /// </summary>
        public float BaseCooldown { get; protected set; }
        /// <summary>
        /// The base duration (in seconds) for this ability.
        /// </summary>
        public float BaseDuration { get; protected set; }
        /// <summary>
        /// Indicates if this ability can crit, or not.
        /// </summary>
        public bool MayCrit { get; protected set; }
        #endregion

        #region constructor
        protected Ability(String name, int rank, int level, float baseMinDamage, float baseMaxDamage, float baseCost,
            Stats stats, Character character, CalculationOptionsWarlock options, Color color, MagicSchool magicSchool, SpellTree spellTree)
        {
            Name = name;
            Rank = rank;
            Level = level;
            BaseMinDamage = baseMinDamage;
            BaseMaxDamage = baseMaxDamage;
            BaseCost = baseCost;

            Stats = stats;
            Character = character;
            Options = options;
            GraphColor = color;
            MagicSchool = magicSchool;
            SpellTree = spellTree;

            //all properties default to zero, except for the following:
            BaseExecuteTime = 3;
            BaseGlobalCooldown = 1.5f;
            BaseDirectDamageMultiplier = 1;
            BaseCritMultiplier = 1.5f;
            BaseRange = 30;
            Harmful = true;

            Statistics = new Statistics();
        }
        #endregion

        #region Calculation methods - overridden by the derived classes to include things like haste effects, talent modifiers or glyphs.
        /// <summary>
        /// Return the base execution time to perform this ability.
        /// </summary>
        public virtual float ExecuteTime()
        {
            return BaseExecuteTime;
        }

        /// <summary>
        /// Return the global cooldown value.
        /// </summary>
        public virtual float GlobalCooldown()
        {
            return BaseGlobalCooldown;
        }

        /// <summary>
        /// Returns the cooldown for this ability.
        /// </summary>
        public virtual float Cooldown()
        {
            return BaseCooldown;
        }

        /// <summary>
        /// Returns the duration for this ability.
        /// </summary>
        public virtual float Duration()
        {
            return BaseDuration;
        }

        /// <summary>
        /// Returns the range for this ability.
        /// </summary>
        public virtual int Range()
        {
            return BaseRange;
        }

        /// <summary>
        /// Returns the cost (percentage) to perform this ability.
        /// </summary>
        public virtual float Cost()
        {
            return BaseCost;
        }

        /// <summary>
        /// Calculate the mana required to perform this ability.
        /// </summary>
        public virtual float Mana()
        {
            Stats statsBase = BaseStats.GetBaseStats(Character);
            return (float)Math.Floor(statsBase.Mana * Cost());
        }

        #endregion

        #region Combat simulation helpers
        /// <summary>
        /// Stores the time offset (in the combat simulation) when this ability is scheduled to be executed.
        /// </summary>
        public float ScheduledTime { get; set; }
        /// <summary>
        /// Returns the time (in seconds) to wait before this ability can be executed again.
        /// </summary>
        public virtual float GetTimeDelay()
        {
            //if the ability has a cooldown, return it.
            //if there is no cooldown, return its duration instead.
            //if there is no duration, or cooldown, then return its executiontime.
            //if there is no executiontime (i.e. instant cast abilities), return the GCD.
            return (Cooldown() > 0) ? Cooldown() : (Duration() > 0) ? Duration() : (ExecuteTime() > 0) ? ExecuteTime() : GlobalCooldown();
        }

        /// <summary>
        /// Refer to the derived classes for the actual implementation.
        /// </summary>
        /// <remarks>
        /// Spells use a simple 3-outcomes attack model: 
        ///     Miss > Hit > Crit 
        /// Melee abilities are resolved in the following order:
        ///     Miss > Dodge > Parry > Glancing Blow > Block > Critical Hit > Crushing Blow > Ordinary Hit
        /// There are 2 types of melee abilities:
        ///   - white damage (auto-attack and and other abilities that consume no mana, energy or rage) 
        ///   - yellow damage (special attacks) consume mana, energy or rage. Additionally, they are not subject to glancing blows.
        /// </remarks>
        public abstract void Execute();

        #endregion

        /// <summary>
        /// The derived classes will override this method to return a much more detailed string which will be used for tooltips.
        /// For now, we'll just return the name of the ability.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #region IComparable implementation
        /// <summary>
        /// Compares the scheduled time (calculated during the combat simulation) of the current ability with another.
        /// </summary>
        /// <param name="other">Some other ability to compare with.</param>
        /// <returns>Returns an int that specifies whether the current instance is less than, equal to or greater than the value of the specified instance.</returns>
        public int CompareTo(Ability other)
        {
            return ScheduledTime.CompareTo(other.ScheduledTime);
        }
        #endregion

    }

    /// <summary>
    /// A template defining the Melee ability.
    /// </summary>
    public class Melee : Ability
    {
        public Melee(Stats stats, Character character, CalculationOptionsWarlock options, float baseMinDamage, float baseMaxDamage)
            : base("Melee", 1, 1, baseMinDamage, baseMaxDamage, 0, stats, character, options, Color.FromArgb(255, 255, 0, 0), MagicSchool.Shadow, SpellTree.None)
        {
        }

        /// <summary>
        /// Todo: Implement melee attack system
        /// </summary>
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
