using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rawr.WarlockSim.Minions;

namespace Rawr.WarlockSim 
{
    public class CharacterCalculationsWarlock : CharacterCalculationsBase
    {
        //private static readonly TraceSource WarlockLogger = new TraceSource("WarlockCombatLogger");

        #region properties
        private Character Character { get; set; }
        private Stats TotalStats { get; set; }
        private CalculationOptionsWarlock Options { get; set; }
        #endregion

        public string Name { get; protected set; }
        public string Abilities { get; protected set; }
        public float OverallDamage { get; protected set; }
        public float TotalManaCost { get; protected set; }
        public float ActiveTime { get; set; }

        //replaced by a generic dictionary<string, double> type
        //public class ManaSource
        //{
        //    public string Name { get; set; }
        //    public float Value { get; set; }

        //    public ManaSource(string name, float value)
        //    {
        //        Name = name;
        //        Value = value;
        //    }
        //}
        //public List<ManaSource> ManaSources { get; set; }
        public Dictionary<string, double> ManaSources = new Dictionary<string, double>();

        /// <summary>
        /// A collection of spells that will be used during combat.
        /// </summary>
        public List<Spell> SpellPriority { get; protected set; }

        /// <summary>
        /// The events that occurred during combat.
        /// </summary>
        public List<Spell> Events = new List<Spell>();

        #region priority queues
        private readonly PriorityQueue<Spell> _warlockAbilities = new PriorityQueue<Spell>();
        private readonly PriorityQueue<Spell> _petAbilities = new PriorityQueue<Spell>();
        #endregion

        #region combat auras
        public Backdraft Backdraft = new Backdraft();
        public Decimation Decimation = new Decimation();
        public ShadowEmbrace ShadowEmbrace = new ShadowEmbrace();
        public Pyroclasm Pyroclasm = new Pyroclasm();
        #endregion
        
        public override float OverallPoints 
        {
            get 
            {
                float overall = 0f;
                foreach (var value in _subPoints) { overall += value; }
                return overall;
            }
            set { }
        }

        private float[] _subPoints = new[] { 0f, 0f };
        public override float[] SubPoints 
        {
            get { return _subPoints; }
            set { _subPoints = value; }
        }

        public float DpsPoints 
        {
            get { return _subPoints[0]; }
            set { _subPoints[0] = value; }
        }

        public float PetDPSPoints 
        {
            get { return _subPoints[1]; }
            set { _subPoints[1] = value; }
        }

        #region constructors
        public CharacterCalculationsWarlock( )
        {
            Name = "Priority-based";
            Abilities = "Abilities:";
            SpellPriority = new List<Spell>();
        }

        public CharacterCalculationsWarlock(Character character, Stats stats) : this()
        {
            Character = character;
            TotalStats = stats;

            if (Character.CalculationOptions == null)
            {
                Character.CalculationOptions = new CalculationOptionsWarlock();
            }

            Options = (CalculationOptionsWarlock)Character.CalculationOptions;
            if (Options.SpellPriority.Count > 0)
            {
                _warlockAbilities.Clear();

                foreach (String name in Options.SpellPriority)
                {
                    Abilities += "\r\n- " + name;

                    Spell spell = SpellFactory.CreateSpell(name, stats, character, Options);
                    if (spell == null) continue;    //i.e. skip over the following lines if null
                    SpellPriority.Add(spell);       //used by "DPS Sources" comparison calcs
                    _warlockAbilities.Enqueue(spell);

                    //wire up the event handlers
                    #region backdraft notifications
                    if (character.WarlockTalents.Backdraft > 0)
                    {
                        if (spell.SpellTree == SpellTree.Destruction)
                        {
                            Backdraft.AuraUpdate += spell.BackdraftAuraHandler;
                            spell.SpellCast += Backdraft.SpellCastHandler;
                        }
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region methods
        public void Calculate()
        {
            if (_warlockAbilities.Count > 0)
            {
                SimulateCombat(_warlockAbilities);
            }

            //if (_petAbilities.Count > 0)
            //{
            //    SimulateCombat(_petAbilities);    
            //}

            int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;

            //calculate totals 
            foreach (Spell spell in SpellPriority)
            {
                OverallDamage += (float)spell.Statistics.OverallDamage();
                TotalManaCost += (float)spell.Statistics.ManaUsed;
                ActiveTime += (float)spell.Statistics.ActiveTime;

                //Debug.WriteLine(String.Format("thread:[{0}] | - {1}: #Hits={2} [Damage={3:0}, Average={4:0}], #Crits={5} [Damage={6:0}, Average={7:0}], #Misses={8}, ActiveTime={9:0.00}",
                //                              threadid, spell.Name,
                //                              spell.Statistics.HitCount, spell.Statistics.HitDamage, spell.Statistics.HitAverage(),
                //                              spell.Statistics.CritCount, spell.Statistics.CritDamage, spell.Statistics.CritAverage(),
                //                              spell.Statistics.MissCount,
                //                              spell.Statistics.ActiveTime
                //                             )
                //               );
            }

            DpsPoints = (OverallDamage / ActiveTime);
            //DpsPoints = (OverallDamage / Options.Duration);
            //Debug.WriteLine(string.Format("thread:[{0}] | ActiveTime(total)={1}", threadid, ActiveTime));

            //StringBuilder sb =  new StringBuilder();
            //foreach (Spell spell in Events)
            //{
            //    sb.AppendLine(string.Format("{0:0.00} {1} casts {2} [damage: {3:0}]", spell.ScheduledTime, Character.Name, spell.Name, spell.MaxHitDamage));
            //}

            //Options.castseq = sb.ToString();
        }

        /// <summary>
        /// A discrete event simulation of combat over time.
        /// </summary>
        private void SimulateCombat(PriorityQueue<Spell> queue)
        {
            int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

            float timer = 0;
            float timelimit = Options.Duration;         //in seconds
            float latency = (Options.Latency / 1000);   //in milliseconds

            DateTime start = DateTime.Now;

            Debug.WriteLine("-------------------");
            Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - simulation starts [timelimit: {2:0.00}]", threadID, timer, timelimit));
            //Trace.TraceInformation("Combat simulation started!");
            //Trace.Flush();

            while (queue.Count != 0)
            {
                //get the spell at the front of the queue
                Spell spell = queue.Dequeue();

                //this will be the next event that must occur in the combat timeline
                //so align the simulation timer to match the scheduledtime of the event
                timer = (spell.ScheduledTime);

                //events that are scheduled to occur after the simulation has ended are irrelevant
                if (timer >= timelimit)
                {
                    timer = timelimit;
                    queue.Clear();
                    continue;
                }

                #region if this is a dot spell, check that its ready
                if (spell.IsTicking)
                {
                    //perform the tick
                    Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - {2} ticks for {3:0} damage", threadID, timer, spell.Name, spell.TickHitDamage()));

                    //schedule the next tick
                    spell.ScheduledTime += spell.TickTime();

                    //add it back to the queue
                    queue.Enqueue(spell);

                    //and skip to the next iteration
                    continue;
                }
                #endregion

                //get the cast time or the GCD for this ability
                float casttime = spell.ExecuteTime() > 0 ? spell.ExecuteTime() : spell.GlobalCooldown();

                //check if there is enough time left to cast this spell
                if ((timer + casttime + latency) < timelimit)
                {
                    //TODO: recalculate stats to account for all combat effects (e.g. +spell, +haste, +spi, +crit bonuses etc)
                    //spell.Stats = updatedStats;
                    //or
                    //spell.Execute(updatedStats);

                    //the spell lands on the target, so calculate damage and trigger any related effects
                    spell.Execute();

                    //prioritise - i.e. the next 'event' in the timeline 
                    //(think of this as the time when the spell can be recast [for direct damage spells] or the next tick [for dots]).
                    spell.ScheduledTime += spell.GetTimeDelay();
                    timer += latency;

                    //append to the events history
                    //Events.Add(spell);

                    if (spell.ScheduledTime < timelimit)
                    {
                        //this spell can be re-cast before the simulation ends, so add it back to the queue
                        queue.Enqueue(spell);
                    }
                    else
                    {
                        //the simulation will end before this spell can be re-cast, so we wont bother adding it to the queue
                        Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - removing {2} - the simulation ends before it can be re-casted", threadID, timer, spell.Name));
                    }
                }
                else
                {
                    //There is still some simulation time left, but not enough to cast the current spell.
                    //This means that the simulation is almost finished.
                    //However, there might be enough time to cast the next spell in the queue ...
                    if (queue.Count > 0)
                    {
                        Spell next = queue.Peek();
                        Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - not enough time to cast {2} [{3:0.00}s cast, {4:0.00}s lat] - trying next spell: {5}", threadID, timer, spell.Name, spell.ExecuteTime(), latency, next.Name));
                    }
                    else
                    {
                        Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - not enough time to cast {2} - [this was the last spell in the queue]", threadID, timer, spell.Name));
                    }
                }
            }

            Debug.WriteLine(String.Format("thread:[{0}] | time: {1:0.00} - simulation ends [no spells left in the queue]", threadID, timer));
            DateTime stop = DateTime.Now;
            Debug.WriteLine(String.Format("thread:[{0}] | simulation time: {1} seconds", threadID, (stop - start).Seconds));

            //Trace.TraceInformation("Combat simulation finished!");
            //Trace.Flush();
        }

        private double GetManaRegenFromSpiritAndIntellect()
        {
            return (Math.Floor(5f * StatConversion.GetSpiritRegenSec(TotalStats.Spirit, TotalStats.Intellect)));
        }

        private double GetManaRegenInCombat()
        {
            return (GetManaRegenFromSpiritAndIntellect() * TotalStats.SpellCombatManaRegeneration + TotalStats.Mp5);
        }

        private double GetManaRegenOutOfCombat()
        {
            return (GetManaRegenFromSpiritAndIntellect() + TotalStats.Mp5);
        }

        /// <summary>
        /// Builds a dictionary containing the values to display for each of the calculations defined in 
        /// CharacterDisplayCalculationLabels. The key should be the Label of each display calculation, 
        /// and the value should be the value to display, optionally appended with '*' followed by any 
        /// string you'd like displayed as a tooltip on the value.
        /// </summary>
        /// <returns>A Dictionary<string, string> containing the values to display for each of the 
        /// calculations defined in CharacterDisplayCalculationLabels.</returns>
        public override Dictionary<string, string> GetCharacterDisplayCalculationValues() 
        {
            Dictionary<string, string> dictValues = new Dictionary<string, string>();

            #region Simulation stats
            dictValues.Add("Rotation", String.Format("{0}*{1}", Name, Abilities));
            dictValues.Add("Warlock DPS", String.Format("{0:0}", DpsPoints));
            dictValues.Add("Pet DPS", String.Format("{0:0}", PetDPSPoints));
            dictValues.Add("Total DPS", String.Format("{0:0}", OverallPoints));
            dictValues.Add("Damage Done", String.Format("{0:0}", OverallDamage));
            dictValues.Add("Mana Used", String.Format("{0:0}", TotalManaCost));
            #endregion

            #region HP/Mana stats
            dictValues.Add("Health", String.Format("{0:0}", TotalStats.Health));
            dictValues.Add("Mana", String.Format("{0:0}", TotalStats.Mana));
            #endregion

            #region Base stats
            dictValues.Add("Strength", String.Format("{0}", TotalStats.Strength));
            dictValues.Add("Agility", String.Format("{0}", TotalStats.Agility));
            dictValues.Add("Stamina", String.Format("{0}", TotalStats.Stamina));
            dictValues.Add("Intellect", String.Format("{0}", TotalStats.Intellect));
            dictValues.Add("Spirit", String.Format("{0}", TotalStats.Spirit));
            dictValues.Add("Armor", String.Format("{0}", TotalStats.Armor));
            #endregion

            #region Pet stats
            Minion minion = MinionFactory.CreateMinion(Options.Pet, Character, TotalStats);
            if (minion != null)
            {
                dictValues.Add("Pet Strength", String.Format("{0}", minion.Strength()));
                dictValues.Add("Pet Agility", String.Format("{0}", minion.Agility()));
                dictValues.Add("Pet Stamina", String.Format("{0:0}", minion.Stamina()));
                dictValues.Add("Pet Intellect", String.Format("{0:0}", minion.Intellect()));
                dictValues.Add("Pet Spirit", String.Format("{0}", minion.Spirit()));
                dictValues.Add("Pet Armor", String.Format("{0}", minion.Armor()));
            }
            #endregion

            #region Spell stats
            //pet scaling consts: http://www.wowwiki.com/Warlock_minions
            const float petInheritedAttackPowerPercentage = 0.57f;
            const float petInheritedSpellPowerPercentage = 0.15f;

            dictValues.Add("Bonus Damage", String.Format("{0}*Shadow Damage\t{1}\r\nFire Damage\t{2}\r\n\r\nYour Fire Damage increases your pet's Attack Power by {3} and Spell Damage by {4}.",
                TotalStats.SpellPower,
                TotalStats.SpellPower + TotalStats.SpellShadowDamageRating,
                TotalStats.SpellPower + TotalStats.SpellFireDamageRating,
                Math.Round((TotalStats.SpellPower + TotalStats.SpellFireDamageRating) * petInheritedAttackPowerPercentage, 0),
                Math.Round((TotalStats.SpellPower + TotalStats.SpellFireDamageRating) * petInheritedSpellPowerPercentage, 0)
                ));

            #region Hit / Miss chance
            //float bonusHit = TotalStats.SpellHit;
            float onePercentOfHitRating = (1 / StatConversion.GetSpellHitFromRating(1));
            float hitFromRating = StatConversion.GetSpellHitFromRating(TotalStats.HitRating);
            float hitFromTalents = (Character.WarlockTalents.Suppression * 0.01f);
            float hitFromBuffs = (TotalStats.SpellHit - hitFromRating - hitFromTalents);
            float targetHit = (Options.TargetHit / 100f);
            float totalHit = (targetHit + TotalStats.SpellHit);
            float missChance = (totalHit > 1 ? 0 : (1 - totalHit));

            //calculate the amount of hit rating that is over or under the cap
            float hitDelta = (totalHit > 1)
                                 ? (float) Math.Floor((totalHit - 1) * onePercentOfHitRating)
                                 : (float) Math.Ceiling((1 - totalHit) * onePercentOfHitRating);
            //now we can calculate the hitcap value
            float hitCap = (totalHit > 1)
                               ? TotalStats.HitRating - hitDelta
                               : TotalStats.HitRating + hitDelta;

            dictValues.Add("Hit Rating", String.Format("{0}*{1:0.00%} Hit Chance (max 100%) | {2:0.00%} Miss Chance \r\n\r\n" 
                + "{3:0.00%}\t Base Hit Chance on a Level {4:0} target\r\n" 
                + "{5:0.00%}\t from {6:0} Hit Rating [gear, food and/or flasks]\r\n" 
                + "{7:0.00%}\t from Talent: Suppression\r\n" 
                + "{8:0.00%}\t from Buffs: Racial and/or Spell Hit Chance Taken\r\n\r\n" 
                + "{9}\r\n\r\n" 
                + "Hit Rating caps:\r\n" 
                + "446 - hard cap (no hit from talents, gear or buffs)\r\n"
                + "420 - Heroic Presence\r\n" 
                + "368 - Suppression\r\n" 
                + "342 - Suppression and Heroic Presence\r\n"
                + "289 - Suppression, Improved Faerie Fire / Misery\r\n" 
                + "263 - Suppression, Improved Faerie Fire / Misery and Heroic Presence",
                TotalStats.HitRating, totalHit, missChance,
                targetHit, Options.TargetLevel,
                hitFromRating, TotalStats.HitRating,
                hitFromTalents,
                hitFromBuffs,
                String.Format("You are {0} hit rating {1} the {2} cap.", hitDelta, ((totalHit > 1) ? "above" : "below"), hitCap)
                ));

            dictValues.Add("Miss chance", String.Format("{0:0.00%}", missChance));
            #endregion
            
            #region Crit %
            Stats statsBase = BaseStats.GetBaseStats(Character);
            float critFromRating = StatConversion.GetSpellCritFromRating(TotalStats.CritRating);
            float critFromIntellect = StatConversion.GetSpellCritFromIntellect(TotalStats.Intellect);
            float critFromBuffs = TotalStats.SpellCrit - statsBase.SpellCrit - critFromRating - critFromIntellect 
                                - (Character.WarlockTalents.DemonicTactics * 0.02f) 
                                - (Character.WarlockTalents.Backlash * 0.01f);

            dictValues.Add("Crit Chance", String.Format("{0:0.00%}*" 
                                                + "{1:0.00%}\tfrom {2:0} Spell Crit rating\r\n" 
                                                + "{3:0.00%}\tfrom {4:0} Intellect\r\n" 
                                                + "{5:0.000%}\tfrom Warlock Class Bonus\r\n" 
                                                + "{6:0%}\tfrom Talent: Demonic Tactics\r\n" 
                                                + "{7:0%}\tfrom Talent: Backlash\r\n" 
                                                + "{8:0%}\tfrom Buffs",
                    TotalStats.SpellCrit,
                    critFromRating, TotalStats.CritRating,
                    critFromIntellect, TotalStats.Intellect,
                    statsBase.SpellCrit,
                    (Character.WarlockTalents.DemonicTactics * 0.02f),
                    (Character.WarlockTalents.Backlash * 0.01f),
                    critFromBuffs
                ));
            #endregion

            dictValues.Add("Haste Rating", String.Format("{0}%*{1}%\tfrom {2} Haste rating\r\n{3}%\tfrom Buffs\r\n{4}s\tGlobal Cooldown",
                (TotalStats.SpellHaste * 100f).ToString("0.00"),
                (StatConversion.GetSpellHasteFromRating(TotalStats.HasteRating) * 100f).ToString("0.00"),
                TotalStats.HasteRating,
                (TotalStats.SpellHaste * 100f - StatConversion.GetSpellHasteFromRating(TotalStats.HasteRating) * 100f).ToString("0.00"),
                Math.Max(1.0f, 1.5f / (1 + TotalStats.SpellHaste)).ToString("0.00")));

            dictValues.Add("Mana Regen", String.Format("{0}*{0} mana regenerated every 5 seconds while not casting\r\n{1} mana regenerated every 5 seconds while casting", 
                GetManaRegenOutOfCombat(),
                GetManaRegenInCombat()));
            #endregion

            #region Shadow school
            dictValues.Add("Shadow Bolt", new ShadowBolt(TotalStats, Character, Options).ToString());
            if (Character.WarlockTalents.Haunt > 0)
                dictValues.Add("Haunt", new Haunt(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Haunt", "- *Required talent not available");
            dictValues.Add("Corruption", new Corruption(TotalStats, Character, Options).ToString());
            dictValues.Add("Curse of Agony", new CurseOfAgony(TotalStats, Character, Options).ToString());
            dictValues.Add("Curse of Doom", new CurseOfDoom(TotalStats, Character, Options).ToString());
            if (Character.WarlockTalents.UnstableAffliction > 0)
                dictValues.Add("Unstable Affliction", new UnstableAffliction(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Unstable Affliction", "- *Required talent not available");
            dictValues.Add("Death Coil", new DeathCoil(TotalStats, Character, Options).ToString());
            dictValues.Add("Drain Life", new DrainLife(TotalStats, Character, Options).ToString());
            dictValues.Add("Drain Soul", new DrainSoul(TotalStats, Character, Options).ToString());
            dictValues.Add("Seed of Corruption", new SeedOfCorruption(TotalStats, Character, Options).ToString());
            dictValues.Add("Shadowflame", new Shadowflame(TotalStats, Character, Options).ToString());
            if (Character.WarlockTalents.Shadowburn > 0)
                dictValues.Add("Shadowburn", new Shadowburn(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Shadowburn", "- *Required talent not available");
            if (Character.WarlockTalents.Shadowfury > 0)
                dictValues.Add("Shadowfury", new Shadowfury(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Shadowfury", "- *Required talent not available");
            dictValues.Add("Life Tap", new LifeTap(TotalStats, Character, Options).ToString());
            if (Character.WarlockTalents.DarkPact > 0)
                dictValues.Add("Dark Pact", new DarkPact(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Dark Pact", "- *Required talent not available");
            
            #endregion

            #region Fire school
            dictValues.Add("Incinerate", new Incinerate(TotalStats, Character, Options).ToString());
            dictValues.Add("Immolate", new Immolate(TotalStats, Character, Options).ToString());
            if (Character.WarlockTalents.Conflagrate > 0)
                dictValues.Add("Conflagrate", new Conflagrate(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Conflagrate", "- *Required talent not available");
            if (Character.WarlockTalents.ChaosBolt > 0)
                dictValues.Add("Chaos Bolt", new ChaosBolt(TotalStats, Character, Options).ToString());
            else
                dictValues.Add("Chaos Bolt", "- *Required talent not available");
            dictValues.Add("Rain of Fire", new RainOfFire(TotalStats, Character, Options).ToString());
            dictValues.Add("Hellfire", new Hellfire(TotalStats, Character, Options).ToString());

            if (Character.WarlockTalents.Metamorphosis > 0)
                dictValues.Add("Immolation Aura", new ImmolationAura(TotalStats, Character, Options).ToString());
            else 
                dictValues.Add("Immolation Aura", "- *Required talent not available");

            dictValues.Add("Searing Pain", new SearingPain(TotalStats, Character, Options).ToString());
            dictValues.Add("Soul Fire", new SoulFire(TotalStats, Character, Options).ToString());
            #endregion

            return dictValues;
        }

        public override float GetOptimizableCalculationValue(string calculation)
        {
            switch (calculation)
            {
                case "Hit Rating": return TotalStats.HitRating;
                case "Haste Rating": return TotalStats.HasteRating;
                case "Crit Rating": return TotalStats.CritRating;
                case "Spirit": return TotalStats.Spirit;
            }
            return 0;
        }
        #endregion
    }
}