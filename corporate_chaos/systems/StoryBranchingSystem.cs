using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Manages story branching based on player choice history and company performance.
    /// Creates different narrative experiences for different choice sequences.
    /// </summary>
    public class StoryBranchingSystem
    {
        private ExtendedStoryModeData storyData;
        private Company company;
        private CharacterManager characterManager;
        private Random random = new Random();

        // Branch path identifiers
        public const string BRANCH_AGGRESSIVE_GROWTH = "aggressive_growth";
        public const string BRANCH_CONSERVATIVE_MANAGEMENT = "conservative_management";
        public const string BRANCH_EMPLOYEE_FOCUSED = "employee_focused";
        public const string BRANCH_PROFIT_FOCUSED = "profit_focused";
        public const string BRANCH_ETHICAL_LEADERSHIP = "ethical_leadership";
        public const string BRANCH_RUTHLESS_EFFICIENCY = "ruthless_efficiency";
        public const string BRANCH_INNOVATION_DRIVEN = "innovation_driven";
        public const string BRANCH_MARKET_DOMINATION = "market_domination";

        // Branch tracking thresholds
        private const int BRANCH_DETERMINATION_THRESHOLD = 5; // Number of aligned choices to establish a branch
        private const double BRANCH_WEIGHT_THRESHOLD = 0.6; // Percentage of choices aligned with a branch

        public StoryBranchingSystem(ExtendedStoryModeData storyData, Company company, CharacterManager characterManager)
        {
            this.storyData = storyData;
            this.company = company;
            this.characterManager = characterManager;
        }

        /// <summary>
        /// Analyzes choice history to determine the player's primary story branch path
        /// </summary>
        public List<string> DeterminePrimaryBranches()
        {
            var branchScores = CalculateBranchScores();
            
            // Return branches that meet the threshold
            var primaryBranches = branchScores
                .Where(kvp => kvp.Value >= BRANCH_DETERMINATION_THRESHOLD)
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take(2) // Allow up to 2 primary branches
                .ToList();

            return primaryBranches;
        }

        /// <summary>
        /// Calculates scores for each potential story branch based on choice history
        /// </summary>
        private Dictionary<string, int> CalculateBranchScores()
        {
            var scores = new Dictionary<string, int>
            {
                [BRANCH_AGGRESSIVE_GROWTH] = 0,
                [BRANCH_CONSERVATIVE_MANAGEMENT] = 0,
                [BRANCH_EMPLOYEE_FOCUSED] = 0,
                [BRANCH_PROFIT_FOCUSED] = 0,
                [BRANCH_ETHICAL_LEADERSHIP] = 0,
                [BRANCH_RUTHLESS_EFFICIENCY] = 0,
                [BRANCH_INNOVATION_DRIVEN] = 0,
                [BRANCH_MARKET_DOMINATION] = 0
            };

            // Analyze each choice in history
            foreach (var choice in storyData.ChoiceHistory)
            {
                AnalyzeChoiceForBranches(choice, scores);
            }

            // Factor in company performance metrics
            AnalyzeCompanyPerformanceForBranches(scores);

            // Factor in character relationships
            AnalyzeRelationshipsForBranches(scores);

            return scores;
        }

        /// <summary>
        /// Analyzes a single choice to determine which branches it supports
        /// </summary>
        private void AnalyzeChoiceForBranches(StoryChoiceRecord choice, Dictionary<string, int> scores)
        {
            // Analyze choice tone and consequences
            if (choice.ChoiceId.Contains("aggressive") || choice.ChoiceId.Contains("attack"))
            {
                scores[BRANCH_AGGRESSIVE_GROWTH]++;
                scores[BRANCH_MARKET_DOMINATION]++;
            }

            if (choice.ChoiceId.Contains("conservative") || choice.ChoiceId.Contains("cautious"))
            {
                scores[BRANCH_CONSERVATIVE_MANAGEMENT]++;
            }

            if (choice.ChoiceId.Contains("employee") || choice.ChoiceId.Contains("support") || 
                choice.ChoiceId.Contains("morale"))
            {
                scores[BRANCH_EMPLOYEE_FOCUSED]++;
                scores[BRANCH_ETHICAL_LEADERSHIP]++;
            }

            if (choice.ChoiceId.Contains("profit") || choice.ChoiceId.Contains("cost") || 
                choice.ChoiceId.Contains("cuts"))
            {
                scores[BRANCH_PROFIT_FOCUSED]++;
            }

            if (choice.ChoiceId.Contains("ethical") || choice.ChoiceId.Contains("fair") || 
                choice.ChoiceId.Contains("responsible"))
            {
                scores[BRANCH_ETHICAL_LEADERSHIP]++;
            }

            if (choice.ChoiceId.Contains("efficiency") || choice.ChoiceId.Contains("ruthless") || 
                choice.ChoiceId.Contains("layoff"))
            {
                scores[BRANCH_RUTHLESS_EFFICIENCY]++;
            }

            if (choice.ChoiceId.Contains("innovation") || choice.ChoiceId.Contains("research") || 
                choice.ChoiceId.Contains("rd"))
            {
                scores[BRANCH_INNOVATION_DRIVEN]++;
            }

            // Analyze consequence flags
            foreach (var flag in choice.ConsequenceFlags)
            {
                if (flag.Contains("aggressive") || flag.Contains("expansion"))
                    scores[BRANCH_AGGRESSIVE_GROWTH]++;
                
                if (flag.Contains("employee_welfare") || flag.Contains("morale_boost"))
                    scores[BRANCH_EMPLOYEE_FOCUSED]++;
                
                if (flag.Contains("profit_maximization") || flag.Contains("cost_reduction"))
                    scores[BRANCH_PROFIT_FOCUSED]++;
                
                if (flag.Contains("ethical") || flag.Contains("responsible"))
                    scores[BRANCH_ETHICAL_LEADERSHIP]++;
                
                if (flag.Contains("innovation") || flag.Contains("rd_focus"))
                    scores[BRANCH_INNOVATION_DRIVEN]++;
            }
        }

        /// <summary>
        /// Analyzes company performance to determine branch alignment
        /// </summary>
        private void AnalyzeCompanyPerformanceForBranches(Dictionary<string, int> scores)
        {
            // Market share growth indicates aggressive or domination focus
            if (company.MarketShare > 30)
            {
                scores[BRANCH_AGGRESSIVE_GROWTH] += 2;
                scores[BRANCH_MARKET_DOMINATION] += 3;
            }

            // High morale indicates employee focus
            if (company.Morale > 70)
            {
                scores[BRANCH_EMPLOYEE_FOCUSED] += 2;
                scores[BRANCH_ETHICAL_LEADERSHIP] += 1;
            }

            // High capital with moderate growth indicates conservative management
            if (company.Capital > 1000000 && company.MarketShare < 20)
            {
                scores[BRANCH_CONSERVATIVE_MANAGEMENT] += 2;
            }

            // High capital with high market share indicates profit focus
            if (company.Capital > 2000000 && company.MarketShare > 25)
            {
                scores[BRANCH_PROFIT_FOCUSED] += 2;
            }

            // Low risk indicates conservative or ethical approach
            if (company.Risk < 20)
            {
                scores[BRANCH_CONSERVATIVE_MANAGEMENT] += 1;
                scores[BRANCH_ETHICAL_LEADERSHIP] += 1;
            }

            // High risk indicates aggressive approach
            if (company.Risk > 60)
            {
                scores[BRANCH_AGGRESSIVE_GROWTH] += 2;
                scores[BRANCH_RUTHLESS_EFFICIENCY] += 1;
            }
        }

        /// <summary>
        /// Analyzes character relationships to determine branch alignment
        /// </summary>
        private void AnalyzeRelationshipsForBranches(Dictionary<string, int> scores)
        {
            // Strong relationship with Evelyn Cross indicates employee focus
            if (storyData.CharacterRelationships.ContainsKey("evelyn_cross"))
            {
                var evelynRelationship = storyData.CharacterRelationships["evelyn_cross"];
                if (evelynRelationship.TrustLevel > 50)
                {
                    scores[BRANCH_EMPLOYEE_FOCUSED] += 2;
                    scores[BRANCH_ETHICAL_LEADERSHIP] += 1;
                }
            }

            // Strong relationship with Marcus Vey indicates profit/growth focus
            if (storyData.CharacterRelationships.ContainsKey("marcus_vey"))
            {
                var marcusRelationship = storyData.CharacterRelationships["marcus_vey"];
                if (marcusRelationship.TrustLevel > 50)
                {
                    scores[BRANCH_PROFIT_FOCUSED] += 2;
                    scores[BRANCH_AGGRESSIVE_GROWTH] += 1;
                }
            }

            // Strong relationship with Vincent Duro indicates competitive focus
            if (storyData.CharacterRelationships.ContainsKey("vincent_duro"))
            {
                var vincentRelationship = storyData.CharacterRelationships["vincent_duro"];
                if (vincentRelationship.ProfessionalRespect > 40)
                {
                    scores[BRANCH_MARKET_DOMINATION] += 2;
                }
            }

            // Strong relationship with Sophie Kim indicates innovation focus
            if (storyData.CharacterRelationships.ContainsKey("sophie_kim"))
            {
                var sophieRelationship = storyData.CharacterRelationships["sophie_kim"];
                if (sophieRelationship.PersonalConnection > 50)
                {
                    scores[BRANCH_INNOVATION_DRIVEN] += 2;
                }
            }
        }

        /// <summary>
        /// Generates branch-specific narrative content for the current quarter
        /// </summary>
        public List<NarrativeEvent> GenerateBranchSpecificContent(int quarter, List<string> activeBranches)
        {
            var events = new List<NarrativeEvent>();

            foreach (var branch in activeBranches)
            {
                var branchEvent = CreateBranchSpecificEvent(branch, quarter);
                if (branchEvent != null)
                {
                    events.Add(branchEvent);
                }
            }

            return events;
        }

        /// <summary>
        /// Creates a narrative event specific to a story branch
        /// </summary>
        private NarrativeEvent? CreateBranchSpecificEvent(string branch, int quarter)
        {
            // Only generate branch events after tutorial phase
            if (quarter <= 10) return null;

            // Check if we've already generated a recent event for this branch
            var recentBranchEvent = storyData.CompletedStoryEvents
                .Any(e => e.Contains($"branch_{branch}") && 
                         int.Parse(e.Split('_').Last().Substring(1)) > quarter - 10);

            if (recentBranchEvent) return null;

            return branch switch
            {
                BRANCH_AGGRESSIVE_GROWTH => CreateAggressiveGrowthEvent(quarter),
                BRANCH_CONSERVATIVE_MANAGEMENT => CreateConservativeManagementEvent(quarter),
                BRANCH_EMPLOYEE_FOCUSED => CreateEmployeeFocusedEvent(quarter),
                BRANCH_PROFIT_FOCUSED => CreateProfitFocusedEvent(quarter),
                BRANCH_ETHICAL_LEADERSHIP => CreateEthicalLeadershipEvent(quarter),
                BRANCH_RUTHLESS_EFFICIENCY => CreateRuthlessEfficiencyEvent(quarter),
                BRANCH_INNOVATION_DRIVEN => CreateInnovationDrivenEvent(quarter),
                BRANCH_MARKET_DOMINATION => CreateMarketDominationEvent(quarter),
                _ => null
            };
        }

        #region Branch-Specific Event Creation

        private NarrativeEvent CreateAggressiveGrowthEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_AGGRESSIVE_GROWTH}_Q{quarter}",
                EventType = NarrativeEventType.ChoiceConsequence,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "marcus_vey", "lucinda_vale", "joan" },
                Title = "The Aggressive Growth Path",
                Description = "Your aggressive expansion strategy is creating both opportunities and challenges.",
                Dialogue = new List<string>
                {
                    "Your bold growth strategy is paying off, but it's not without risks.",
                    "Marcus Vey: 'This aggressive approach is exactly what we need to dominate the market.'",
                    "Joan: 'Just remember, rapid growth can be difficult to sustain. Are we prepared for the challenges ahead?'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "aggressive_double_down",
                        ChoiceText = "Double down on expansion. We're just getting started.",
                        Tone = ChoiceTone.Aggressive,
                        RiskLevel = ConsequenceRisk.High,
                        ConsequenceFlags = new List<string> { "aggressive_expansion_accelerated", "high_risk_strategy" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_AGGRESSIVE_GROWTH] = 1.5,
                            [BRANCH_MARKET_DOMINATION] = 1.2
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "aggressive_consolidate",
                        ChoiceText = "Let's consolidate our gains before pushing further.",
                        Tone = ChoiceTone.Diplomatic,
                        RiskLevel = ConsequenceRisk.Medium,
                        ConsequenceFlags = new List<string> { "strategic_consolidation", "balanced_growth" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_AGGRESSIVE_GROWTH] = 0.8,
                            [BRANCH_CONSERVATIVE_MANAGEMENT] = 0.5
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_AGGRESSIVE_GROWTH,
                    ["narrative_divergence"] = true
                }
            };
        }

        private NarrativeEvent CreateConservativeManagementEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_CONSERVATIVE_MANAGEMENT}_Q{quarter}",
                EventType = NarrativeEventType.ChoiceConsequence,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "harold_finch", "gregory_shaw", "joan" },
                Title = "The Conservative Approach",
                Description = "Your cautious management style has built a stable, sustainable company.",
                Dialogue = new List<string>
                {
                    "Your conservative approach has created a remarkably stable organization.",
                    "Harold Finch: 'This prudent management style minimizes legal and financial risks.'",
                    "Joan: 'Stability is valuable, but are we missing opportunities for growth?'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "conservative_maintain",
                        ChoiceText = "Stability is our strength. We'll maintain this approach.",
                        Tone = ChoiceTone.Professional,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "conservative_strategy_reinforced", "stable_growth" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_CONSERVATIVE_MANAGEMENT] = 1.5,
                            [BRANCH_ETHICAL_LEADERSHIP] = 0.8
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "conservative_evolve",
                        ChoiceText = "Perhaps it's time to take some calculated risks.",
                        Tone = ChoiceTone.Diplomatic,
                        RiskLevel = ConsequenceRisk.Medium,
                        ConsequenceFlags = new List<string> { "strategic_evolution", "calculated_risk_taking" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_CONSERVATIVE_MANAGEMENT] = 0.7,
                            [BRANCH_AGGRESSIVE_GROWTH] = 0.6
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_CONSERVATIVE_MANAGEMENT,
                    ["narrative_divergence"] = true
                }
            };
        }

        private NarrativeEvent CreateEmployeeFocusedEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_EMPLOYEE_FOCUSED}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "evelyn_cross", "joan" },
                Title = "The People-First Philosophy",
                Description = "Your commitment to employee welfare has created a loyal, motivated workforce.",
                Dialogue = new List<string>
                {
                    "Your people-first approach has transformed our company culture.",
                    "Evelyn Cross: 'Employee satisfaction is at an all-time high. People genuinely want to work here.'",
                    "Joan: 'You've built something special - a company where people feel valued and supported.'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "employee_deepen",
                        ChoiceText = "Let's invest even more in our people's development and wellbeing.",
                        Tone = ChoiceTone.Supportive,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "employee_investment_increased", "culture_excellence" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_EMPLOYEE_FOCUSED] = 1.5,
                            [BRANCH_ETHICAL_LEADERSHIP] = 1.2
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "employee_balance",
                        ChoiceText = "We need to balance employee welfare with business performance.",
                        Tone = ChoiceTone.Professional,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "balanced_approach", "sustainable_culture" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_EMPLOYEE_FOCUSED] = 1.0,
                            [BRANCH_PROFIT_FOCUSED] = 0.5
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_EMPLOYEE_FOCUSED,
                    ["narrative_divergence"] = true,
                    ["morale_boost"] = 10
                }
            };
        }

        private NarrativeEvent CreateProfitFocusedEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_PROFIT_FOCUSED}_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "marcus_vey", "evelyn_cross", "joan" },
                Title = "The Bottom Line",
                Description = "Your relentless focus on profitability has delivered impressive financial results.",
                Dialogue = new List<string>
                {
                    "The numbers don't lie - your profit-focused strategy is working.",
                    "Marcus Vey: 'This is exactly the kind of financial discipline that builds empires.'",
                    "Evelyn Cross: 'But at what cost? Some employees feel like they're just numbers on a spreadsheet.'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "profit_maximize",
                        ChoiceText = "Profit is how we measure success. We stay the course.",
                        Tone = ChoiceTone.Aggressive,
                        RiskLevel = ConsequenceRisk.Medium,
                        ConsequenceFlags = new List<string> { "profit_maximization_priority", "financial_excellence" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_PROFIT_FOCUSED] = 1.5,
                            [BRANCH_RUTHLESS_EFFICIENCY] = 1.0
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "profit_humanize",
                        ChoiceText = "Perhaps we should consider the human element more carefully.",
                        Tone = ChoiceTone.Diplomatic,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "profit_with_purpose", "balanced_priorities" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_PROFIT_FOCUSED] = 0.8,
                            [BRANCH_EMPLOYEE_FOCUSED] = 0.7
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_PROFIT_FOCUSED,
                    ["narrative_divergence"] = true
                }
            };
        }

        private NarrativeEvent CreateEthicalLeadershipEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_ETHICAL_LEADERSHIP}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "harold_finch", "evelyn_cross", "joan" },
                Title = "Leading with Integrity",
                Description = "Your commitment to ethical leadership has earned respect throughout the industry.",
                Dialogue = new List<string>
                {
                    "Your ethical approach to business has set a new standard in the industry.",
                    "Harold Finch: 'Your integrity-first leadership is both rare and admirable.'",
                    "Joan: 'You've proven that doing the right thing and building a successful company aren't mutually exclusive.'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "ethical_champion",
                        ChoiceText = "Let's become industry leaders in corporate responsibility.",
                        Tone = ChoiceTone.Professional,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "ethical_leadership_champion", "industry_standard_setter" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_ETHICAL_LEADERSHIP] = 1.5,
                            [BRANCH_EMPLOYEE_FOCUSED] = 1.0
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "ethical_pragmatic",
                        ChoiceText = "Ethics are important, but we must remain competitive.",
                        Tone = ChoiceTone.Diplomatic,
                        RiskLevel = ConsequenceRisk.Medium,
                        ConsequenceFlags = new List<string> { "pragmatic_ethics", "competitive_balance" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_ETHICAL_LEADERSHIP] = 1.0,
                            [BRANCH_PROFIT_FOCUSED] = 0.6
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_ETHICAL_LEADERSHIP,
                    ["narrative_divergence"] = true,
                    ["reputation_boost"] = 15
                }
            };
        }

        private NarrativeEvent CreateRuthlessEfficiencyEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_RUTHLESS_EFFICIENCY}_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "gregory_shaw", "evelyn_cross", "joan" },
                Title = "Efficiency at All Costs",
                Description = "Your ruthless pursuit of efficiency has streamlined operations but created tension.",
                Dialogue = new List<string>
                {
                    "Your efficiency-first approach has eliminated waste and maximized productivity.",
                    "Gregory Shaw: 'The numbers are impressive. We're operating at peak efficiency.'",
                    "Evelyn Cross: 'But we've lost some of our best people. Efficiency isn't everything.'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "ruthless_continue",
                        ChoiceText = "Efficiency is survival. We can't afford sentimentality.",
                        Tone = ChoiceTone.Aggressive,
                        RiskLevel = ConsequenceRisk.High,
                        ConsequenceFlags = new List<string> { "ruthless_efficiency_maintained", "high_turnover_risk" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_RUTHLESS_EFFICIENCY] = 1.5,
                            [BRANCH_PROFIT_FOCUSED] = 1.2
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "ruthless_soften",
                        ChoiceText = "Maybe we've pushed too hard. Let's find a better balance.",
                        Tone = ChoiceTone.Supportive,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "efficiency_with_humanity", "culture_repair" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_RUTHLESS_EFFICIENCY] = 0.6,
                            [BRANCH_EMPLOYEE_FOCUSED] = 0.8
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_RUTHLESS_EFFICIENCY,
                    ["narrative_divergence"] = true,
                    ["morale_impact"] = -10
                }
            };
        }

        private NarrativeEvent CreateInnovationDrivenEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_INNOVATION_DRIVEN}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "sophie_kim", "joan" },
                Title = "Innovation as Strategy",
                Description = "Your commitment to innovation has positioned the company as an industry leader.",
                Dialogue = new List<string>
                {
                    "Your innovation-first strategy is transforming the industry.",
                    "Sophie Kim: 'Our R&D investments are paying off in breakthrough products and processes.'",
                    "Joan: 'You've created a culture where creativity and innovation thrive.'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "innovation_accelerate",
                        ChoiceText = "Let's increase R&D investment and push the boundaries further.",
                        Tone = ChoiceTone.Professional,
                        RiskLevel = ConsequenceRisk.Medium,
                        ConsequenceFlags = new List<string> { "innovation_acceleration", "rd_leadership" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_INNOVATION_DRIVEN] = 1.5,
                            [BRANCH_MARKET_DOMINATION] = 0.8
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "innovation_commercialize",
                        ChoiceText = "Focus on commercializing our innovations for profit.",
                        Tone = ChoiceTone.Professional,
                        RiskLevel = ConsequenceRisk.Low,
                        ConsequenceFlags = new List<string> { "innovation_commercialization", "profit_from_rd" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_INNOVATION_DRIVEN] = 1.0,
                            [BRANCH_PROFIT_FOCUSED] = 0.8
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_INNOVATION_DRIVEN,
                    ["narrative_divergence"] = true,
                    ["reputation_boost"] = 10
                }
            };
        }

        private NarrativeEvent CreateMarketDominationEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"branch_{BRANCH_MARKET_DOMINATION}_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "vincent_duro", "lucinda_vale", "joan" },
                Title = "The Path to Dominance",
                Description = "Your aggressive market strategy has positioned you as a dominant force in the industry.",
                Dialogue = new List<string>
                {
                    "Your company is becoming a dominant force that competitors fear.",
                    "Lucinda Vale: 'Our market presence is undeniable. We're setting the industry agenda.'",
                    "Joan: 'Vincent Duro and other competitors are watching our every move. Dominance comes with scrutiny.'"
                },
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceId = "domination_crush",
                        ChoiceText = "We eliminate competition. No mercy in business.",
                        Tone = ChoiceTone.Aggressive,
                        RiskLevel = ConsequenceRisk.High,
                        ConsequenceFlags = new List<string> { "market_dominance_aggressive", "competitor_elimination" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_MARKET_DOMINATION] = 1.5,
                            [BRANCH_RUTHLESS_EFFICIENCY] = 1.0
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceId = "domination_coexist",
                        ChoiceText = "We can dominate while allowing healthy competition.",
                        Tone = ChoiceTone.Diplomatic,
                        RiskLevel = ConsequenceRisk.Medium,
                        ConsequenceFlags = new List<string> { "market_leadership", "competitive_coexistence" },
                        StoryBranchInfluence = new Dictionary<string, double>
                        {
                            [BRANCH_MARKET_DOMINATION] = 1.2,
                            [BRANCH_ETHICAL_LEADERSHIP] = 0.7
                        }
                    }
                },
                GameplayEffects = new Dictionary<string, object>
                {
                    ["branch_event"] = BRANCH_MARKET_DOMINATION,
                    ["narrative_divergence"] = true,
                    ["market_share_boost"] = 2.0
                }
            };
        }

        #endregion

        /// <summary>
        /// Gets a narrative summary of the player's current story path
        /// </summary>
        public string GetBranchNarrativeSummary()
        {
            var primaryBranches = DeterminePrimaryBranches();
            
            if (primaryBranches.Count == 0)
            {
                return "Your leadership style is still taking shape. Your future choices will define your path.";
            }

            var summaries = new Dictionary<string, string>
            {
                [BRANCH_AGGRESSIVE_GROWTH] = "You've embraced aggressive expansion, pushing boundaries and taking bold risks to accelerate growth.",
                [BRANCH_CONSERVATIVE_MANAGEMENT] = "You've built a stable, sustainable company through careful planning and risk management.",
                [BRANCH_EMPLOYEE_FOCUSED] = "You've prioritized employee welfare, creating a culture where people feel valued and supported.",
                [BRANCH_PROFIT_FOCUSED] = "You've maintained relentless focus on profitability and financial performance.",
                [BRANCH_ETHICAL_LEADERSHIP] = "You've led with integrity, making ethical considerations central to every decision.",
                [BRANCH_RUTHLESS_EFFICIENCY] = "You've pursued maximum efficiency, streamlining operations even when it requires difficult choices.",
                [BRANCH_INNOVATION_DRIVEN] = "You've championed innovation, investing in R&D and creative solutions to industry challenges.",
                [BRANCH_MARKET_DOMINATION] = "You've pursued market dominance, positioning your company as an industry leader."
            };

            var narrativeParts = primaryBranches
                .Where(b => summaries.ContainsKey(b))
                .Select(b => summaries[b])
                .ToList();

            return string.Join(" ", narrativeParts);
        }

        /// <summary>
        /// Determines if a specific branch path is active for the player
        /// </summary>
        public bool IsBranchActive(string branchId)
        {
            var primaryBranches = DeterminePrimaryBranches();
            return primaryBranches.Contains(branchId);
        }

        /// <summary>
        /// Gets the strength/commitment level to a specific branch (0.0 to 1.0)
        /// </summary>
        public double GetBranchStrength(string branchId)
        {
            var scores = CalculateBranchScores();
            
            if (!scores.ContainsKey(branchId))
                return 0.0;

            var maxPossibleScore = storyData.ChoiceHistory.Count + 10; // Choices + performance factors
            return Math.Min(1.0, scores[branchId] / (double)maxPossibleScore);
        }

        /// <summary>
        /// Applies branch influence from a choice to update story flags
        /// </summary>
        public void ApplyBranchInfluence(DialogueChoice choice)
        {
            foreach (var influence in choice.StoryBranchInfluence)
            {
                var branchId = influence.Key;
                var weight = influence.Value;

                // Add or update branch influence flags
                var flagName = $"branch_influence_{branchId}";
                
                if (!storyData.StoryFlags.Contains(flagName))
                {
                    storyData.StoryFlags.Add(flagName);
                }

                // Track cumulative influence
                var cumulativeFlagName = $"branch_cumulative_{branchId}";
                if (!storyData.StoryFlags.Contains(cumulativeFlagName))
                {
                    storyData.StoryFlags.Add(cumulativeFlagName);
                }
            }
        }

        /// <summary>
        /// Gets recommended character interactions based on active branches
        /// </summary>
        public List<string> GetRecommendedCharacterInteractions()
        {
            var primaryBranches = DeterminePrimaryBranches();
            var recommendations = new List<string>();

            foreach (var branch in primaryBranches)
            {
                var characters = GetRelevantCharactersForBranch(branch);
                recommendations.AddRange(characters);
            }

            return recommendations.Distinct().ToList();
        }

        private List<string> GetRelevantCharactersForBranch(string branchId)
        {
            return branchId switch
            {
                BRANCH_AGGRESSIVE_GROWTH => new List<string> { "marcus_vey", "lucinda_vale" },
                BRANCH_CONSERVATIVE_MANAGEMENT => new List<string> { "harold_finch", "gregory_shaw" },
                BRANCH_EMPLOYEE_FOCUSED => new List<string> { "evelyn_cross", "joan" },
                BRANCH_PROFIT_FOCUSED => new List<string> { "marcus_vey", "selena_park" },
                BRANCH_ETHICAL_LEADERSHIP => new List<string> { "harold_finch", "evelyn_cross" },
                BRANCH_RUTHLESS_EFFICIENCY => new List<string> { "gregory_shaw", "marcus_vey" },
                BRANCH_INNOVATION_DRIVEN => new List<string> { "sophie_kim" },
                BRANCH_MARKET_DOMINATION => new List<string> { "vincent_duro", "lucinda_vale" },
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Checks if the player's choices create narrative coherence or contradictions
        /// </summary>
        public bool HasCoherentNarrativePath()
        {
            var primaryBranches = DeterminePrimaryBranches();
            
            // Check for contradictory branch combinations
            var contradictions = new Dictionary<string, List<string>>
            {
                [BRANCH_AGGRESSIVE_GROWTH] = new List<string> { BRANCH_CONSERVATIVE_MANAGEMENT },
                [BRANCH_EMPLOYEE_FOCUSED] = new List<string> { BRANCH_RUTHLESS_EFFICIENCY },
                [BRANCH_PROFIT_FOCUSED] = new List<string> { BRANCH_ETHICAL_LEADERSHIP },
                [BRANCH_CONSERVATIVE_MANAGEMENT] = new List<string> { BRANCH_AGGRESSIVE_GROWTH, BRANCH_MARKET_DOMINATION }
            };

            foreach (var branch in primaryBranches)
            {
                if (contradictions.ContainsKey(branch))
                {
                    var conflictingBranches = contradictions[branch];
                    if (primaryBranches.Any(b => conflictingBranches.Contains(b)))
                    {
                        return false; // Found contradictory branches
                    }
                }
            }

            return true; // No contradictions found
        }

        /// <summary>
        /// Gets a list of all available story branches
        /// </summary>
        public static List<string> GetAllBranches()
        {
            return new List<string>
            {
                BRANCH_AGGRESSIVE_GROWTH,
                BRANCH_CONSERVATIVE_MANAGEMENT,
                BRANCH_EMPLOYEE_FOCUSED,
                BRANCH_PROFIT_FOCUSED,
                BRANCH_ETHICAL_LEADERSHIP,
                BRANCH_RUTHLESS_EFFICIENCY,
                BRANCH_INNOVATION_DRIVEN,
                BRANCH_MARKET_DOMINATION
            };
        }
    }
}
