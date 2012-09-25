namespace Rawr.WarlockSim
{
    /// <summary>
    /// A simple class used to help track statistics for a particular damage segment (e.g. direct damage hits or crits, dot ticks or crits).
    /// </summary>
    public class DamageSegment
    {
        /// <summary>
        /// The count of items in this segment.
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// The total damage for this segment.
        /// </summary>
        public float Total { get; set; }
        /// <summary>
        /// The average damage for this segment.
        /// </summary>
        public float Average()
        {
            return (Count > 0) ? (Total / Count) : 0f;
        }
    }

    /// <summary>
    /// A class to track various combat statistics.
    /// </summary>
    public class Statistics
    {
        #region damage inflicted on the target
        /// <summary>
        /// Records non-critical hits on the target caused by direct damage abilities.
        /// </summary>
        public DamageSegment Hits { get; set; }
        /// <summary>
        /// Records critical hits on the target caused by direct damage abilities.
        /// </summary>
        public DamageSegment Crits { get; set; }
        /// <summary>
        /// Records non-critical hits on the target caused by dot abilities.
        /// </summary>
        public DamageSegment TickHits { get; set; }
        /// <summary>
        /// Records critical hits on the target caused by direct damage abilities.
        /// </summary>
        public DamageSegment TickCrits { get; set; }
        #endregion

        #region damage lost
        /// <summary>
        /// Records damage lost due to low spell hit.
        /// </summary>
        public DamageSegment Misses { get; set; }
        /// <summary>
        /// Records damage lost due to target resistance.
        /// </summary>
        public DamageSegment Resists { get; set; }
        /// <summary>
        /// Records damage absorbed by the target.
        /// </summary>
        public DamageSegment Absorbs { get; set; }
        /// <summary>
        /// Records damage lost due to target immunity.
        /// </summary>
        public DamageSegment Immune { get; set; }
        #endregion

        public Statistics()
        {
            Hits = new DamageSegment();
            Crits = new DamageSegment();
            TickHits = new DamageSegment();
            TickCrits = new DamageSegment();
            Misses = new DamageSegment();
            Resists = new DamageSegment();
            Absorbs = new DamageSegment();
            Immune = new DamageSegment();
        }
        #region general properties
        /// <summary>
        /// The number of times that the ability was used during combat.
        /// </summary>
        public int CastCount { get; set; }
        /// <summary>
        /// The total mana consumed.
        /// </summary>
        public double ManaUsed { get; set; }
        /// <summary>
        /// The amount of time that a spell was active.
        /// </summary>
        public double ActiveTime { get; set; }
        #endregion

        #region calculation methods
        /// <summary>
        /// The total direct damage inflicted on the target.
        /// </summary>
        public float DirectDamage()
        {
            return (Hits.Total + Crits.Total);
        }
        /// <summary>
        /// The total damage over time that was inflicted on the target.
        /// </summary>
        public float DotDamage()
        {
            return (TickHits.Total + TickCrits.Total);
        }
        /// <summary>
        /// The total damage that hit, crit or ticked on the target.
        /// </summary>
        public double OverallDamage()
        {
            return (DirectDamage() + DotDamage());
        }
        #endregion
    }
}