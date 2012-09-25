using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rawr.Warlock
{
    /// <summary>
    /// A base class for implementing alternate warlock modelling solutions.
    /// </summary>
    public abstract class Solver
    {
        protected Solver()
        {

        }

        public virtual void Calculate()
        {
            
        }
    }

    /// <summary>
    /// A very simple warlock model.
    /// </summary>
    /// <remarks>
    /// The methodology used in this approach will be to calculate the number of casts per fight 
    /// while taking its cast time, cooldown or duration into account and then calculating the overall damage.
    /// This can be loosely expressed as the following formulas:
    ///   NumberOfCasts = TimeLeft / (CastTime + (Duration or Cooldown))
    ///   OverallDamage = NumberOfCasts * AverageDamagePerCast
    /// Items are rated by their DPS value, which can be calculated as follows:
    ///   ActiveTime = NumberOfCasts * CastTime
    ///   DPS = OverallDamage / ActiveTime
    /// It remains to be seen how accurate this will be compared to the simulation model.
    /// </remarks>
    /// <example>
    /// Standard 5min tank-n-spank fight and the affliction rotation - SB>Haunt>Corr>CoA>UA.
    /// - at the start, timeleft = 300s (5 * 60)
    /// - timeleft must be adjusted for each spell in the rotation (i.e. deduct the cast time of the preceding spells)
    /// - to keep the implementation simple, dot spells are handled first and spammable spells last. 
    /// - Corruption (instant cast, 18s duration) is a special dot because the Everlasting Affliction talent gives it 
    ///   a chance to be refreshed whenever shadowbolt, haunt, drain life or drain soul is cast during any 18s window.
    ///   This model will assume that an affliction warlock will always be taking 5/5 in Everlasting Affliction for a 100% chance to have Corruption refreshed,
    ///   which in turn keeps things really simple because Corruption only gets cast once for the entire fight.
    ///   [If an affliction warlock does not have 5/5 in Everlasting Afflicion, then they are just plain crazy.]
    /// - CoA (instant cast, 24s duration):
    ///     timeleft = (300 - SB - Haunt - Corr) = 300 - 2.5 - 1.5 - 1.5 = 294.5
    ///     numberofcasts = 294.5 / (1.5 + 24) => ~11 casts (which means that we spent 16.5s (11 * 1.5) casting CoA)
    /// - UA (1.5s cast, 15s duration) = (294.5 - 16.5) / (1.5 + 15) => ~16 casts (which means that we spent 24s (16 * 1.5) casting UA)
    /// - Haunt (1.5s cast, 8s cooldown) = (294.5 - 16.5 - 24) / (1.5 + 8) => ~26 casts (which means 39s casting Haunt)
    /// - SB (2.5s unhasted) = (300 - Haunt - Corr - CoA - UA) 
    ///                      => (300 - 39 - 1.5 - 16.5 - 24) 
    ///                      => 219s for spamming
    ///   therefore SB casts = 219 / 2.5 =~ 87.6
    /// Of course none of this takes latency or haste into account.
    /// </example>
    public class SimpleSolver : Solver
    {
        public SimpleSolver() : base()
        {
        }
    }

    /// <summary>
    /// A discrete event simulation of priority-based combat over time.
    /// </summary>
    /// <remarks>
    /// This is unquestionably the most accurate method for evaluating gear choices.
    /// The SimpleSolver method is much faster because it is less complex to implement, 
    /// but I would argue that this simulation is fast enough that the performance difference doesnt matter.
    /// </remarks>
    public class SimulationSolver : Solver
    {
        public SimulationSolver() : base()
        {
        }
    }
}