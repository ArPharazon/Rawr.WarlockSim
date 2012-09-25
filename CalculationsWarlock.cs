using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Rawr.WarlockSim 
{
    [Rawr.Calculations.RawrModelInfo("Simulation", "Spell_Nature_FaerieFire", CharacterClass.Warlock)]
    public class CalculationsWarlock : CalculationsBase 
    {
        public override List<GemmingTemplate> DefaultGemmingTemplates 
        {
            get 
            {
				//Relevant Gem IDs for Warlocks
				//Red
                int[] runed = {39911, 39998, 40113, 42144};

                //Purple
				int[] purified = {39941, 40026, 40133};

				//Orange
				int[] reckless = {39959, 40051, 40155};
				int[] veiled = {39957, 40049, 40153};

				//Meta
				const int ember = 41333;
				const int chaotic = 41285;

				return new List<GemmingTemplate>
                {
                    #region uncommon
                    new GemmingTemplate
                    {
                        Model = "Warlock", Group = "Uncommon", //Max SP - Ember
                        RedId = runed[0], YellowId = runed[0], BlueId = runed[0], PrismaticId = runed[0], MetaId = ember
                    },
                    new GemmingTemplate
                    {
                        Model = "Warlock", Group = "Uncommon", //SP/Hit - Ember
                        RedId = runed[0], YellowId = veiled[0], BlueId = purified[0], PrismaticId = runed[0], MetaId = ember
                    },
                    new GemmingTemplate
                    {
                        Model = "Warlock", Group = "Uncommon", //SP/Haste - Ember
                        RedId = runed[0], YellowId = reckless[0], BlueId = purified[0], PrismaticId = runed[0], MetaId = ember
                    },
                    new GemmingTemplate
                    {
                        Model = "Warlock", Group = "Uncommon", //Max SP - Chaotic
                        RedId = runed[0], YellowId = runed[0], BlueId = runed[0], PrismaticId = runed[0], MetaId = chaotic
                    },
                    new GemmingTemplate
                    {
                        Model = "Warlock", Group = "Uncommon", //SP/Hit - Chaotic
                        RedId = runed[0], YellowId = veiled[0], BlueId = purified[0], PrismaticId = runed[0], MetaId = chaotic
                    },
                    new GemmingTemplate
                    {
                        Model = "Warlock", Group = "Uncommon", //SP/Haste - Chaotic
                        RedId = runed[0], YellowId = reckless[0], BlueId = purified[0], PrismaticId = runed[0], MetaId = chaotic
                    },
                    #endregion

                    #region rare
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Rare", //Max SP - Ember
				        RedId = runed[1], YellowId = runed[1], BlueId = runed[1], PrismaticId = runed[1], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Rare", //SP/Hit - Ember
				        RedId = runed[1], YellowId = veiled[1], BlueId = purified[1], PrismaticId = runed[1], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Rare", //SP/Haste - Ember
				        RedId = runed[1], YellowId = reckless[1], BlueId = purified[1], PrismaticId = runed[1], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Rare", //Max SP - Chaotic
				        RedId = runed[1], YellowId = runed[1], BlueId = runed[1], PrismaticId = runed[1], MetaId = chaotic
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Rare", //SP/Hit - Chaotic
				        RedId = runed[1], YellowId = veiled[1], BlueId = purified[1], PrismaticId = runed[1], MetaId = chaotic
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Rare", //SP/Haste - Chaotic
				        RedId = runed[1], YellowId = reckless[1], BlueId = purified[1], PrismaticId = runed[1], MetaId = chaotic
                    },
                    #endregion

                    #region epic
                    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Epic", Enabled = true, //Max SP - Ember
				        RedId = runed[2], YellowId = runed[2], BlueId = runed[2], PrismaticId = runed[2], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Epic", Enabled = true, //SP/Hit - Ember
				        RedId = runed[2], YellowId = veiled[2], BlueId = purified[2], PrismaticId = runed[2], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Epic", Enabled = true, //SP/Haste - Ember
				        RedId = runed[2], YellowId = reckless[2], BlueId = purified[2], PrismaticId = runed[2], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Epic", Enabled = true, //Max SP - Chaotic
				        RedId = runed[2], YellowId = runed[2], BlueId = runed[2], PrismaticId = runed[2], MetaId = chaotic
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Epic", Enabled = true, //SP/Hit - Chaotic
				        RedId = runed[2], YellowId = veiled[2], BlueId = purified[2], PrismaticId = runed[2], MetaId = chaotic
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Epic", Enabled = true, //SP/Haste - Chaotic
				        RedId = runed[2], YellowId = reckless[2], BlueId = purified[2], PrismaticId = runed[2], MetaId = chaotic
                    },
                    #endregion

                    #region jeweler
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Jeweler", //Max SP - Ember
				        RedId = runed[3], YellowId = runed[3], BlueId = runed[3], PrismaticId = runed[3], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Jeweler", //SP/Hit - Ember
				        RedId = runed[2], YellowId = runed[3], BlueId = runed[3], PrismaticId = runed[2], MetaId = ember
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Jeweler", //Max SP - Chaotic
				        RedId = runed[3], YellowId = runed[3], BlueId = runed[3], PrismaticId = runed[3], MetaId = chaotic
                    },
				    new GemmingTemplate
				    {
                        Model = "Warlock", Group = "Jeweler", //SP/Hit - Chaotic
				        RedId = runed[2], YellowId = runed[3], BlueId = runed[3], PrismaticId = runed[2], MetaId = chaotic
                    },
                    #endregion
                };
            }
        }

        public override CharacterClass TargetClass { get { return CharacterClass.Warlock; } }

        private string _currentChartName;
        private float _currentChartTotal;

        private Dictionary<string, Color> _subPointNameColors;
        public override Dictionary<string, Color> SubPointNameColors 
        {
            get 
            {
                _subPointNameColors = new Dictionary<string, Color>();
                switch (_currentChartName) 
                {
                    case "Damage Sources":
                        _subPointNameColors.Add(String.Format("Damage Sources ({0:0} total)", _currentChartTotal), Color.FromArgb(255, 255, 0, 0));
                        break;
                    case "DPS Sources": 
                        _subPointNameColors.Add(String.Format("DPS Sources ({0:0} total)", _currentChartTotal), Color.FromArgb(255, 255, 0, 0));
                        _subPointNameColors.Add("# of casts", Color.FromArgb(255, 255, 0, 0)); 
                        break;
					case "Mana Sources": 
                        _subPointNameColors.Add(String.Format("Mana Sources ({0:0} Total)", _currentChartTotal), Color.FromArgb(255, 0, 0, 255)); 
                        break;
					case "Mana Usage": 
                        _subPointNameColors.Add(String.Format("Mana Usage ({0:0} total)", _currentChartTotal), Color.FromArgb(255, 0, 0, 255)); 
                        break;
					case "Haste Rating Gain": _subPointNameColors.Add("DPS", Color.FromArgb(255, 255, 0, 0)); break;
                    case "Crit Rating Gain": _subPointNameColors.Add("DPS", Color.FromArgb(255, 255, 0, 0)); break;
                    case "Item Budget (10 point steps)": _subPointNameColors.Add("DPS", Color.FromArgb(255, 255, 0, 0)); break;
                    default:
                        _subPointNameColors.Add("DPS", Color.FromArgb(255, 255, 0, 0));
                        _subPointNameColors.Add("Pet DPS", Color.FromArgb(255, 0, 0, 255));
                        break;
                }
                _currentChartName = null;
                return _subPointNameColors;
            }
        }

        private string[] _characterDisplayCalculationLabels;
        public override string[] CharacterDisplayCalculationLabels 
        {
            get
            {
                return _characterDisplayCalculationLabels ?? (_characterDisplayCalculationLabels = new []
                                                                                                       {
                                                                                                           "Simulation:Rotation",
                                                                                                           "Simulation:Warlock DPS",
                                                                                                           "Simulation:Pet DPS",
                                                                                                           "Simulation:Total DPS",
                                                                                                           "Simulation:Damage Done",
                                                                                                           "Simulation:Mana Used",
                                                                                                           "HP/Mana Stats:Health",
                                                                                                           "HP/Mana Stats:Mana",
                                                                                                           "Base Stats:Strength",
                                                                                                           "Base Stats:Agility",
                                                                                                           "Base Stats:Stamina",
                                                                                                           "Base Stats:Intellect",
                                                                                                           "Base Stats:Spirit",
                                                                                                           "Base Stats:Armor",
                                                                                                           "Pet Stats:Pet Strength",
                                                                                                           "Pet Stats:Pet Agility",
                                                                                                           "Pet Stats:Pet Stamina",
                                                                                                           "Pet Stats:Pet Intellect",
                                                                                                           "Pet Stats:Pet Spirit",
                                                                                                           "Pet Stats:Pet Armor",
                                                                                                           "Spell:Bonus Damage",
                                                                                                           "Spell:Hit Rating",
                                                                                                           "Spell:Miss Chance",
                                                                                                           "Spell:Crit Chance",
                                                                                                           "Spell:Haste Rating",
                                                                                                           "Spell:Mana Regen",
                                                                                                           "Shadow School:Shadow Bolt",
                                                                                                           "Shadow School:Haunt",
                                                                                                           "Shadow School:Corruption",
                                                                                                           "Shadow School:Curse of Agony",
                                                                                                           "Shadow School:Curse of Doom",
                                                                                                           "Shadow School:Unstable Affliction",
                                                                                                           "Shadow School:Death Coil",
                                                                                                           "Shadow School:Drain Life",
                                                                                                           "Shadow School:Drain Soul",
                                                                                                           "Shadow School:Seed of Corruption",
                                                                                                           "Shadow School:Shadowflame",
                                                                                                           "Shadow School:Shadowburn",
                                                                                                           "Shadow School:Shadowfury",
                                                                                                           "Shadow School:Life Tap",
                                                                                                           "Shadow School:Dark Pact",
                                                                                                           "Fire School:Incinerate",
                                                                                                           "Fire School:Immolate",
                                                                                                           "Fire School:Conflagrate",
                                                                                                           "Fire School:Chaos Bolt",
                                                                                                           "Fire School:Rain of Fire",
                                                                                                           "Fire School:Hellfire",
                                                                                                           "Fire School:Immolation Aura",
                                                                                                           "Fire School:Searing Pain",
                                                                                                           "Fire School:Soul Fire"
                                                                                                       });
            }
        }

#if RAWR3
        private ICalculationOptionsPanel _calculationOptionsPanel = null;
		public override ICalculationOptionsPanel CalculationOptionsPanel
#else
		private CalculationOptionsPanelBase _calculationOptionsPanel;
		public override CalculationOptionsPanelBase CalculationOptionsPanel
#endif
        {
            get { return _calculationOptionsPanel ?? (_calculationOptionsPanel = new CalculationOptionsPanelWarlock()); }
        }

        private string[] _optimizableCalculationLabels;
	    /// <summary>
	    /// Labels of the stats available to the Optimizer
	    /// </summary>
	    public override string[] OptimizableCalculationLabels 
        {
            get
            {
                return _optimizableCalculationLabels ?? (_optimizableCalculationLabels = new[]
                                                                                             {
                                                                                                 "Hit Rating",
                                                                                                 "Haste Rating",
                                                                                                 "Crit Rating",
                                                                                                 "Spirit"
                                                                                             });
            }
        }

        private string[] _customChartNames;
        public override string[] CustomChartNames 
        {
            get
            {
                return _customChartNames ?? (_customChartNames = new[]
                                                                     {
                                                                         "Damage Sources",
                                                                         "DPS Sources",
                                                                         "Mana Sources",
                                                                         "Mana Usage",
                                                                         "Haste Rating Gain",
                                                                         "Crit Rating Gain",
                                                                         "Item Budget (10 point steps)",
                                                                     });
            }
        }

        private string[] _customRenderedChartNames;
        public override string[] CustomRenderedChartNames
        {
            get { return _customRenderedChartNames ?? (_customRenderedChartNames = new[] {"Stats Graph"}); }
        }

        public override ComparisonCalculationBase CreateNewComparisonCalculation() { return new ComparisonCalculationWarlock(); }
        public override CharacterCalculationsBase CreateNewCharacterCalculations() { return new CharacterCalculationsWarlock(); }

        public override ComparisonCalculationBase[] GetCustomChartData(Character character, string chartName) 
        {
            List<ComparisonCalculationBase> comparisonList = new List<ComparisonCalculationBase>();
            ComparisonCalculationBase comparison;

            _currentChartTotal = 0;
            _currentChartName = chartName;

            CharacterCalculationsWarlock baseCalc = (CharacterCalculationsWarlock)GetCharacterCalculations(character);
            baseCalc.Calculate();

            switch (chartName) 
            {
                case "Damage Sources":
                    foreach (var spell in baseCalc.SpellPriority)
                    {
                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = String.Format("{0}\n[{1:0.00}s]", spell.Name, spell.Statistics.ActiveTime);
                        comparison.SubPoints[0] = (float)spell.Statistics.OverallDamage();
                        _currentChartTotal += comparison.SubPoints[0];
                        comparison.OverallPoints = comparison.SubPoints[0];
                        comparison.Equipped = false;
                        comparisonList.Add(comparison);
                    }
                    return comparisonList.ToArray();

                case "DPS Sources":
                    foreach (var spell in baseCalc.SpellPriority)
                    {
                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = String.Format("{0} [{1}s]", spell.Name, spell.Statistics.ActiveTime);
                        comparison.SubPoints[0] = (float)(spell.Statistics.OverallDamage() / spell.Statistics.ActiveTime);
                        _currentChartTotal += comparison.SubPoints[0];
                        comparison.OverallPoints = comparison.SubPoints[0];
                        comparison.Equipped = false;
                        comparisonList.Add(comparison);
                    }

                    //if (baseCalc.CalculationOptions.Pet != "None")
                    //{
                    //    comparison = CreateNewComparisonCalculation();
                    //    comparison.Name = baseCalc.CalculationOptions.Pet;
                    //    comparison.SubPoints[0] = baseCalc.PetDPS;
                    //    _currentChartTotal += comparison.SubPoints[0];
                    //    comparison.OverallPoints = comparison.SubPoints[0];
                    //    comparison.Equipped = false;
                    //    comparisonList.Add(comparison);
                    //}
                    //if (baseCalc.TotalDPS - _currentChartTotal > 1)
                    //{
                    //    comparison = CreateNewComparisonCalculation();
                    //    comparison.Name = "Other";
                    //    comparison.SubPoints[0] = baseCalc.TotalDPS - _currentChartTotal;
                    //    _currentChartTotal += comparison.SubPoints[0];
                    //    comparison.OverallPoints = comparison.SubPoints[0];
                    //    comparison.Equipped = false;
                    //    comparisonList.Add(comparison);
                    //}
                    return comparisonList.ToArray();

                case "Mana Sources":
                    foreach (var source in baseCalc.ManaSources)
                    {
                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = source.Key;
                        comparison.SubPoints[0] = (float)source.Value;
                        _currentChartTotal += comparison.SubPoints[0];
                        comparison.OverallPoints = comparison.SubPoints[0];
                        comparison.Equipped = false;
                        comparisonList.Add(comparison);
                    }
                    return comparisonList.ToArray();

                case "Mana Usage":
                    foreach (var spell in baseCalc.SpellPriority)
                    {
                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = spell.Name;
                        comparison.SubPoints[0] = (float)(spell.Statistics.ManaUsed);
                        _currentChartTotal += comparison.SubPoints[0];
                        comparison.OverallPoints = comparison.SubPoints[0];
                        comparison.Equipped = false;
                        comparisonList.Add(comparison);
                    }
                    return comparisonList.ToArray();

                case "Haste Rating Gain":
                    for (var x = 0; x <= 200; x += 10)
                    {
                        Item additionalItem = new Item { Stats = new Stats { HasteRating = x } };
                        CharacterCalculationsWarlock hasteGain = (CharacterCalculationsWarlock)GetCharacterCalculations(character, additionalItem);
                        hasteGain.Calculate();

                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = String.Format("{0} HasteRating", x);
                        comparison.SubPoints[0] = hasteGain.DpsPoints - baseCalc.DpsPoints;
                        comparison.SubPoints[1] = hasteGain.PetDPSPoints - baseCalc.PetDPSPoints;
                        comparison.OverallPoints = comparison.SubPoints[0] + comparison.SubPoints[1];
                        comparison.Equipped = false;
                        comparisonList.Add(comparison);
                    }
                    return comparisonList.ToArray();

                case "Crit Rating Gain":
                    for (var x = 0; x <= 200; x += 10)
                    {
                        Item additionalItem = new Item { Stats = new Stats { CritRating = x } };
                        CharacterCalculationsWarlock critGain = (CharacterCalculationsWarlock)GetCharacterCalculations(character, additionalItem);
                        critGain.Calculate();

                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = String.Format("{0} CritRating", x);
                        comparison.SubPoints[0] = critGain.DpsPoints - baseCalc.DpsPoints;
                        comparison.SubPoints[1] = critGain.PetDPSPoints - baseCalc.PetDPSPoints;
                        comparison.OverallPoints = comparison.SubPoints[0] + comparison.SubPoints[1];
                        comparison.Equipped = false;
                        comparisonList.Add(comparison);
                    }
                    return comparisonList.ToArray();

                case "Item Budget (10 point steps)":
                    Item[] itemList = new [] {
                        new Item { Stats = new Stats { SpellPower = 10 } },
                        new Item { Stats = new Stats { HitRating = 10 } },
                        new Item { Stats = new Stats { HasteRating = 10 } },
                        new Item { Stats = new Stats { CritRating = 10 } },
                        new Item { Stats = new Stats { Spirit = 10 } },
                    };
                    string[] statList = new [] {
                        "Spell Power",
                        "Hit Rating",
                        "Haste Rating",
                        "Crit Rating",
                        "Spirit",
                    };

                    for (int index = 0; index < itemList.Length; index++)
                    {
                        Item additionalItem = itemList[index];
                        CharacterCalculationsWarlock itemBudget = (CharacterCalculationsWarlock)GetCharacterCalculations(character, additionalItem);
                        itemBudget.Calculate();

                        comparison = CreateNewComparisonCalculation();
                        comparison.Name = statList[index];
                        comparison.Equipped = false;
                        comparison.OverallPoints = itemBudget.OverallPoints - baseCalc.OverallPoints;
                        float[] subPoints = new float[itemBudget.SubPoints.Length];
                        for (int i = 0; i < itemBudget.SubPoints.Length; i++)
                        {
                            subPoints[i] = itemBudget.SubPoints[i] - baseCalc.SubPoints[i];
                        }
                        comparison.SubPoints = subPoints;

                        comparisonList.Add(comparison);
                    }
                    return comparisonList.ToArray();

                default:
                    //_customChartNames = null;
                    _currentChartName = null;
                    return new ComparisonCalculationBase[0];
            }
        }

        public override void RenderCustomChart(Character character, string chartName, Graphics g, int width, int height)
        {
            height -= 2;
            switch (chartName)
            {
                case "Stats Graph":
                    Stats[] statsList = new [] 
                    {
                        new Stats { SpellPower = 1 },
                        new Stats { HitRating = 1 },
                        new Stats { HasteRating = 1 },
                        new Stats { CritRating = 1 },
                        new Stats { Intellect = 1 },
                        new Stats { Spirit = 1 },
                    };

                    Color[] statsColors = new [] 
                    { 
                        Color.FromArgb(255, 255, 0, 0), 
                        Color.DarkBlue, 
                        Color.FromArgb(255, 255, 165, 0), 
                        Color.Olive, 
                        Color.FromArgb(255, 154, 205, 50), 
                        Color.Aqua 
                    };

                    Base.Graph.RenderStatsGraph(g, width, height, character, statsList, statsColors, 200, "blah blah", "DPS", Base.Graph.Style.Mage);
                    break;
            }
        }

        public override CharacterCalculationsBase GetCharacterCalculations(Character character, Item additionalItem, bool referenceCalculation, bool significantChange, bool needsDisplayCalculations) 
        {
            Stats stats = GetCharacterStats(character, additionalItem);
            CharacterCalculationsWarlock results = new CharacterCalculationsWarlock(character, stats);
            //results.Calculate();

            if ((character.Name != null) && (character.Class == CharacterClass.Warlock))
            {
                results.Calculate();
            }

            return results;
        }

        //public Stats GetBuffStats(Character character) { return GetBuffsStats(character.ActiveBuffs); }
        public Stats GetBuffsStats(Character character, CalculationOptionsWarlock calcOpts) {
            List<Buff> removedBuffs = new List<Buff>();
            List<Buff> addedBuffs = new List<Buff>();

            //float hasRelevantBuff;

            #region Special Pot Handling
            /*foreach (Buff potionBuff in character.ActiveBuffs.FindAll(b => b.Name.Contains("Potion")))
            {
                if (potionBuff.Stats._rawSpecialEffectData != null
                    && potionBuff.Stats._rawSpecialEffectData[0] != null)
                {
                    Stats newStats = new Stats();
                    newStats.AddSpecialEffect(new SpecialEffect(potionBuff.Stats._rawSpecialEffectData[0].Trigger,
                                                                potionBuff.Stats._rawSpecialEffectData[0].Stats,
                                                                potionBuff.Stats._rawSpecialEffectData[0].Duration,
                                                                calcOpts.Duration,
                                                                potionBuff.Stats._rawSpecialEffectData[0].Chance,
                                                                potionBuff.Stats._rawSpecialEffectData[0].MaxStack));

                    Buff newBuff = new Buff() { Stats = newStats };
                    character.ActiveBuffs.Remove(potionBuff);
                    character.ActiveBuffsAdd(newBuff);
                    removedBuffs.Add(potionBuff);
                    addedBuffs.Add(newBuff);
                }
            }*/
            #endregion

            Stats statsBuffs = GetBuffsStats(character.ActiveBuffs);

            foreach (Buff b in removedBuffs) {
                character.ActiveBuffsAdd(b);
            }
            foreach (Buff b in addedBuffs) {
                character.ActiveBuffs.Remove(b);
            }

            return statsBuffs;
        }

        public override void SetDefaults(Character character)
        {
            ////class buffs
            //character.ActiveBuffsAdd(("Fel Armor"));

            ////contagion should be present in all good affliction builds
            ////[spellstone for affliction specs, firestone for destro/demo specs]
            //if (character.WarlockTalents.Contagion > 0)
            //{
            //    character.ActiveBuffsAdd(("Grand Spellstone"));
            //}
            //else
            //{
            //    character.ActiveBuffsAdd(("Grand Firestone"));
            //}

            ////flasks & food
            //character.ActiveBuffsAdd(("Fish Feast"));
            //character.ActiveBuffsAdd(("Flask of the Frost Wyrm"));
            //if (character.PrimaryProfession == Profession.Alchemy || character.SecondaryProfession == Profession.Alchemy)
            //{
            //    character.ActiveBuffsAdd(("Flask of Frost Wyrm (Mixology)"));
            //}

            ////replenishment
            //if (character.WarlockTalents.ImprovedSoulLeech > 0)
            //{
            //    character.ActiveBuffsAdd(("Improved Soul Leech"));
            //}
        }

        public override Stats GetCharacterStats(Character character, Item additionalItem) 
        {
            CalculationOptionsWarlock options = (CalculationOptionsWarlock)character.CalculationOptions;
            WarlockTalents talents = character.WarlockTalents;

            Stats statsBase = BaseStats.GetBaseStats(character);
            Stats statsItem = GetItemStats(character, additionalItem);

            ////Potion of Speed is a consumable that can only be used once per fight even tho its tooltip / wowhead info indicates it has a 1 min cooldown.
            ////This means that its actual cooldown is equal to the length of the fight.
            ////At the moment, it has been hardcoded [in Buff.cs (rawr.base)] for a 20min fight, so we have to correct it here to get the appropriate +haste bonus effect.
            //if (character.ActiveBuffsContains("Potion of Speed"))
            //{
            //    //get the fight length
            //    CalculationOptionsWarlock options = (CalculationOptionsWarlock)character.CalculationOptions;
            //    float fightLength = (options.Duration * 60); //i.e. in seconds

            //    //remove the existing speedpotion buff (which has the incorrect cooldown)
            //    Buff speedpotion = Buff.GetBuffByName("Potion of Speed");
            //    character.ActiveBuffs.Remove(speedpotion);

            //    //redefine its stats (this time using the correct cooldown)
            //    speedpotion.Stats = new Stats();
            //    speedpotion.Stats.AddSpecialEffect(new SpecialEffect(Trigger.Use, new Stats { HasteRating = 500f }, 15f, fightLength));
            //    character.ActiveBuffs.Add(speedpotion);

            //    //now repeat the process with the if the pot trick buff had been selected
            //    if (character.ActiveBuffsContains("Potion of Speed (Double Pot Trick)"))
            //    {
            //        speedpotion = Buff.GetBuffByName("Potion of Speed (Double Pot Trick)");
            //        character.ActiveBuffs.Remove(speedpotion);
            //        speedpotion.Stats = new Stats();
            //        speedpotion.Stats.AddSpecialEffect(new SpecialEffect(Trigger.Use, new Stats { HasteRating = 500f }, (15f - 1f), fightLength));
            //        character.ActiveBuffs.Add(speedpotion);
            //    }
            //}

            Stats statsBuffs = GetBuffsStats(character.ActiveBuffs);

            Stats statsTalents = new Stats 
            {
                //Demonic Embrace: increases your stamina by 4/7/10%
                BonusStaminaMultiplier      = (talents.DemonicEmbrace == 1) ? 0.04f : (talents.DemonicEmbrace == 2) ? 0.07f : (talents.DemonicEmbrace == 3) ? 0.10f : 0f,
                
                //Fel Vitality: increases your maximum Health & Mana by 1/2/3%
                BonusHealthMultiplier       = (talents.FelVitality    * 0.01f),
                BonusManaMultiplier         = (talents.FelVitality    * 0.01f),
                
                //Suppression: increases your chance to hit with spells by 1/2/3%
                SpellHit                    = (talents.Suppression * 0.01f),

                //Demonic Tactics: increases your spell crit chance by 2/4/6/8/10%
                //Backlash: increases your spell crit chance by 1/2/3%
                BonusCritChance             = (talents.DemonicTactics * 0.02f)
                                            + (talents.Backlash * 0.01f),
            };
            
            Stats statsTotal = statsBase + statsItem + statsBuffs + statsTalents;

            //make sure that the bonus multipliers have been applied to each stat
            statsTotal.Stamina      = (float)Math.Floor(statsTotal.Stamina   * (1f + statsTotal.BonusStaminaMultiplier  ));
            statsTotal.Intellect    = (float)Math.Floor(statsTotal.Intellect * (1f + statsTotal.BonusIntellectMultiplier));
            statsTotal.Spirit       = (float)Math.Floor(statsTotal.Spirit    * (1f + statsTotal.BonusSpiritMultiplier   ));
            statsTotal.Strength     = (float)Math.Floor(statsTotal.Strength  * (1f + statsTotal.BonusStrengthMultiplier ));
            statsTotal.Agility      = (float)Math.Floor(statsTotal.Agility   * (1f + statsTotal.BonusAgilityMultiplier  ));
            statsTotal.Armor        = (float)Math.Floor(statsTotal.Armor     * (1f + statsTotal.BonusArmorMultiplier    ));
            
            // Agility increases Armor by 2 per point (http://www.wowwiki.com/Agility#Agility)
            statsTotal.BonusArmor  += (statsTotal.Agility * 2);
            statsTotal.Armor       += statsTotal.BonusArmor;

            //Health is calculated from stamina rating first, then its bonus multiplier (in this case, "Fel Vitality" talent) gets applied
            statsTotal.Health      += StatConversion.GetHealthFromStamina(statsTotal.Stamina);
            statsTotal.Health      *= (1 + statsTotal.BonusHealthMultiplier);

            //Mana is calculated from intellect rating first, then its bonus multiplier (in this case, "Expansive Mind" - Gnome racial) is applied
            statsTotal.Mana        += StatConversion.GetManaFromIntellect(statsTotal.Intellect);
            statsTotal.Mana        *= (1 + statsTotal.BonusManaMultiplier);
            
            //Crit rating - the MasterConjuror talent improves the firestone
            statsTotal.CritRating  += Spell.FirestoneSpellCritRating * (1f + (talents.MasterConjuror * 1.5f));
            statsTotal.SpellCrit   += StatConversion.GetSpellCritFromIntellect(statsTotal.Intellect);
            statsTotal.SpellCrit   += StatConversion.GetSpellCritFromRating(statsTotal.CritRating);
            statsTotal.SpellCrit   += statsTotal.BonusCritChance;
            statsTotal.SpellCrit   += statsTotal.SpellCritOnTarget;

            //Haste rating - the MasterConjuror talent improves the spellstone
            statsTotal.HasteRating += Spell.SpellstoneHasteRating * (1f + (talents.MasterConjuror * 1.5f));
            statsTotal.SpellHaste  += StatConversion.GetSpellHasteFromRating(statsTotal.HasteRating);
            
            //Hit rating 
            statsTotal.SpellHit    += StatConversion.GetSpellHitFromRating(statsTotal.HitRating);

            //Fel Armor is a DPS increase, so it is assumed that it will always be active
            statsTotal.SpellPower += 180 * (1 + talents.DemonicAegis * 0.10f);
            statsTotal.SpellDamageFromSpiritPercentage += 0.30f * (1 + talents.DemonicAegis * 0.10f);
            statsTotal.Hp5 += statsTotal.Health * 0.02f * (1 + talents.DemonicAegis * 0.10f);

            //Demon Armor is not a DPS increase and therefore not implemented, however it would be applied as follows:
            // statsTotal.Armor += 2100 * (1 + talents.DemonicAegis * 0.10f);
            // statsTotal.HealingReceivedMultiplier += 0.2f * (1 + talents.DemonicAegis * 0.10f);
            
            statsTotal.SpellPower += (float)Math.Round(statsTotal.SpellDamageFromSpiritPercentage * statsTotal.Spirit);
            
            if (talents.DemonicKnowledge > 0) 
            {
                //PetCalculations pet = new PetCalculations(statsTotal, character);
                //statsTotal.SpellPower += (pet.petStats.Intellect + pet.petStats.Stamina) * talents.DemonicKnowledge * 0.04f;
            }

            return statsTotal;
        }

        public override Stats GetRelevantStats(Stats stats) 
        {
            Stats s = new Stats 
            {
                //primary stats
                SpellPower = stats.SpellPower,
                Intellect = stats.Intellect,
                Spirit = stats.Spirit,
                HitRating = stats.HitRating,
                SpellHit = stats.SpellHit,
                HasteRating = stats.HasteRating,
                SpellHaste = stats.SpellHaste,
                CritRating = stats.CritRating,
                SpellCrit = stats.SpellCrit,
                SpellCritOnTarget = stats.SpellCritOnTarget,

                ShadowDamage = stats.ShadowDamage,
                SpellShadowDamageRating = stats.SpellShadowDamageRating,
                FireDamage = stats.FireDamage,
                SpellFireDamageRating = stats.SpellFireDamageRating,

                BonusIntellectMultiplier = stats.BonusIntellectMultiplier,
                BonusSpiritMultiplier = stats.BonusSpiritMultiplier,
                BonusSpellCritMultiplier = stats.BonusSpellCritMultiplier,
                BonusDamageMultiplier = stats.BonusDamageMultiplier,
                BonusShadowDamageMultiplier = stats.BonusShadowDamageMultiplier,
                BonusFireDamageMultiplier = stats.BonusFireDamageMultiplier,
                SpellDamageFromSpiritPercentage = stats.SpellDamageFromSpiritPercentage,

                //set bonuses
                Warlock2T7 = stats.Warlock2T7,
                Warlock4T7 = stats.Warlock4T7,
                Warlock2T8 = stats.Warlock2T8,
                Warlock4T8 = stats.Warlock4T8,
                Warlock2T9 = stats.Warlock2T9,
                Warlock4T9 = stats.Warlock4T9,
                Warlock2T10 = stats.Warlock2T10,
                Warlock4T10 = stats.Warlock4T10,

                //These stats can be used by warlocks, but they dont affect our dps calculations at all.
                //Included for display purposes only.
                Stamina = stats.Stamina,
                //Health = stats.Health,
                //Mana = stats.Mana,
                //Mp5 = stats.Mp5,

                //The following are custom stat properties belonging to buffs, items (or procs) that can be used/applied to warlocks.
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
                if (RelevantTrinket(effect))
                {
                    s.AddSpecialEffect(effect);
                }
            }
            return s;
        }

        protected bool RelevantTrinket(SpecialEffect effect)
        {
            if (effect.Trigger == Trigger.Use ||
                effect.Trigger == Trigger.DamageSpellCast ||
                effect.Trigger == Trigger.DamageSpellCrit ||
                effect.Trigger == Trigger.DamageSpellHit ||
                effect.Trigger == Trigger.SpellCast ||
                effect.Trigger == Trigger.SpellCrit ||
                effect.Trigger == Trigger.SpellHit ||
                effect.Trigger == Trigger.SpellMiss ||
                effect.Trigger == Trigger.DoTTick ||
                effect.Trigger == Trigger.DamageDone ||
                effect.Trigger == Trigger.DamageOrHealingDone)
            {
                return _HasRelevantStats(effect.Stats);
            }
            return false;
        }

        public override bool HasRelevantStats(Stats stats)
        {
            bool isRelevant = _HasRelevantStats(stats);

            foreach (SpecialEffect se in stats.SpecialEffects())
            {
                isRelevant |= RelevantTrinket(se);
            }
            return isRelevant;
        }

        protected bool _HasRelevantStats(Stats stats)
        {
            bool yes = (
                //our primary stats
                stats.SpellPower
                + stats.Intellect
                + stats.Spirit
                + stats.HitRating + stats.SpellHit
                + stats.HasteRating + stats.SpellHaste
                + stats.CritRating + stats.SpellCrit + stats.SpellCritOnTarget
                + stats.ShadowDamage + stats.SpellShadowDamageRating
                + stats.FireDamage + stats.SpellFireDamageRating

                //multipliers
                + stats.BonusIntellectMultiplier
                + stats.BonusSpiritMultiplier + stats.SpellDamageFromSpiritPercentage
                + stats.BonusSpellCritMultiplier
                + stats.BonusDamageMultiplier + stats.BonusShadowDamageMultiplier + stats.BonusFireDamageMultiplier

                //set bonuses
                + stats.Warlock2T7
                + stats.Warlock4T7
                + stats.Warlock2T8
                + stats.Warlock4T8
                + stats.Warlock2T9
                + stats.Warlock4T9
                + stats.Warlock2T10
                + stats.Warlock4T10
            ) > 0;

            bool maybe = (
                //can be used by warlocks, but it does not affect our DPS calculations
                stats.Stamina 

                //miscellaneous stats belonging to items (or trinket procs) that can be used/applied to warlocks
                //these stats are listed here so that those items (which supply them) can be listed
                //(these are terrible trinkets anyway - I should probably just remove them...)
                + stats.HighestStat                     //darkmoon card: greatness
                + stats.SpellsManaReduction             //spark of hope -> http://www.wowhead.com/?item=45703

                + stats.BonusManaPotion                 //triggered when a mana pot is consumed
                + stats.ManaRestoreFromBaseManaPPM      //judgement of wisdom
                + stats.ManaRestoreFromMaxManaPerSecond //replenishment sources
                + stats.ManaRestore                     //quite a few items that restore mana on spell cast or crit. Also used to model replenishment.
            ) > 0;

            bool no = (
                //ignore items with any of these stats
                stats.Health 
                + stats.Mana + stats.Mp5
                + stats.Resilience
                + stats.Armor + stats.BonusArmor + stats.Agility
                + stats.ArmorPenetration + stats.ArmorPenetrationRating
                + stats.Strength + stats.AttackPower
                + stats.ExpertiseRating
                + stats.Dodge + stats.DodgeRating
                + stats.Parry + stats.ParryRating
                + stats.Defense + stats.DefenseRating
                + stats.ArcaneResistance + stats.ArcaneResistanceBuff
                + stats.FireResistance + stats.FireResistanceBuff
                + stats.FrostResistance + stats.FrostResistanceBuff
                + stats.NatureResistance + stats.NatureResistanceBuff
                + stats.ShadowResistance + stats.ShadowResistanceBuff
                + stats.ThreatReductionMultiplier       //bracing earthsiege diamond (metagem) effect
            ) > 0;

            return yes || (maybe && !no);
        }

        private List<ItemType> _relevantItemTypes;
        public override List<ItemType> RelevantItemTypes
        {
            get
            {
                if (_relevantItemTypes == null)
                {
                    _relevantItemTypes = new List<ItemType>(new[] 
                    {
                        ItemType.None,
                        ItemType.Cloth,
                        ItemType.Dagger,
                        ItemType.Wand,
                        ItemType.OneHandSword,
                        ItemType.Staff
                    });
                }
                return _relevantItemTypes;
            }
        }


        #region RelevantGlyphs
        private static List<string> _relevantGlyphs;
        public override List<string> GetRelevantGlyphs() 
        {
            if (_relevantGlyphs == null) 
            {
                _relevantGlyphs = new List<string>
                                      {
                                          "Glyph of Chaos Bolt",
                                          "Glyph of Conflagrate",
                                          "Glyph of Corruption",
                                          "Glyph of Curse of Agony",
                                          "Glyph of Felguard",
                                          "Glyph of Haunt",
                                          "Glyph of Immolate",
                                          "Glyph of Imp",
                                          "Glyph of Incinerate",
                                          "Glyph of Life Tap",
                                          "Glyph of Metamorphosis",
                                          "Glyph of Quick Decay",
                                          "Glyph of Searing Pain",
                                          "Glyph of Shadowbolt",
                                          "Glyph of Shadowburn",
                                          "Glyph of Siphon Life",
                                          "Glyph of Unstable Affliction"
                                      };
            }
            return _relevantGlyphs;
        }
        #endregion

        public override ICalculationOptionBase DeserializeDataObject(string xml) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CalculationOptionsWarlock));
            StringReader reader = new StringReader(xml);
            CalculationOptionsWarlock calcOpts = serializer.Deserialize(reader) as CalculationOptionsWarlock;
            return calcOpts;
        }

        public override bool EnchantFitsInSlot(Enchant enchant, Character character, ItemSlot slot) 
        {
            if (slot == ItemSlot.OffHand || slot == ItemSlot.Ranged) { return false; }
            return base.EnchantFitsInSlot(enchant, character, slot);
        }

        public override bool ItemFitsInSlot(Item item, Character character, CharacterSlot slot, bool ignoreUnique) 
        {
            if (slot == CharacterSlot.OffHand && item.Slot == ItemSlot.OneHand) return false;
            return base.ItemFitsInSlot(item, character, slot, ignoreUnique);
        }
    }
}