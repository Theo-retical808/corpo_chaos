using CorporateChaos.Models;
using System.Text.Json;

namespace CorporateChaos.Systems
{
    public class NarrativeEngine
    {
        private ExtendedStoryModeData storyData;
        private Company company;
        private CharacterManager characterManager;
        private StoryBranchingSystem branchingSystem;
        private Random random = new Random();

        // Event generation parameters
        private const int MIN_QUARTERS_BETWEEN_MAJOR_EVENTS = 2;
        private const int MAX_QUARTERS_BETWEEN_MAJOR_EVENTS = 5;
        private const double CHARACTER_EVENT_PROBABILITY = 0.3;
        private const double CONFLICT_EVENT_PROBABILITY = 0.2;
        private const double MILESTONE_EVENT_PROBABILITY = 0.25;

        public NarrativeEngine(ExtendedStoryModeData storyData, Company company, CharacterManager characterManager)
        {
            this.storyData = storyData;
            this.company = company;
            this.characterManager = characterManager;
            this.branchingSystem = new StoryBranchingSystem(storyData, company, characterManager);
        }

        public StoryBranchingSystem BranchingSystem => branchingSystem;

        /// <summary>
        /// Generates narrative events for the current quarter based on company state and player actions
        /// </summary>
        public List<NarrativeEvent> GenerateEventsForQuarter(int quarter)
        {
            var events = new List<NarrativeEvent>();

            // Skip event generation during tutorial phase (handled by existing StoryScript)
            if (storyData.CurrentAct == NarrativeAct.Tutorial)
                return events;

            // Generate character development events
            events.AddRange(GenerateCharacterDevelopmentEvents(quarter));

            // Generate conflict events based on company performance
            events.AddRange(GenerateConflictEvents(quarter));

            // Generate milestone events for significant achievements
            events.AddRange(GenerateMilestoneEvents(quarter));

            // Generate choice consequence events from previous decisions
            events.AddRange(GenerateChoiceConsequenceEvents(quarter));

            // Generate branch-specific narrative content
            var activeBranches = branchingSystem.DeterminePrimaryBranches();
            if (activeBranches.Count > 0)
            {
                events.AddRange(branchingSystem.GenerateBranchSpecificContent(quarter, activeBranches));
            }

            // Generate act transition events
            if (IsActTransitionQuarter(quarter))
            {
                events.AddRange(GenerateActTransitionEvents(quarter));
            }

            // Limit events per quarter to avoid overwhelming the player
            return FilterAndPrioritizeEvents(events, quarter);
        }

        /// <summary>
        /// Generates character development events based on relationship progression and character arcs
        /// </summary>
        private List<NarrativeEvent> GenerateCharacterDevelopmentEvents(int quarter)
        {
            var events = new List<NarrativeEvent>();

            foreach (var character in StoryScript.Characters.Values)
            {
                // Skip if character hasn't been introduced yet
                if (quarter < character.IntroductionQuarter)
                    continue;

                // Check if character should be introduced this quarter
                if (quarter == character.IntroductionQuarter)
                {
                    events.Add(CreateCharacterIntroductionEvent(character, quarter));
                    continue;
                }

                // Generate relationship milestone events
                if (ShouldGenerateRelationshipMilestone(character.CharacterId, quarter))
                {
                    events.Add(CreateRelationshipMilestoneEvent(character, quarter));
                }

                // Generate personal challenge events for established characters
                if (ShouldGeneratePersonalChallenge(character.CharacterId, quarter))
                {
                    events.Add(CreatePersonalChallengeEvent(character, quarter));
                }
            }

            return events;
        }

        /// <summary>
        /// Generates conflict events based on company performance and external factors
        /// </summary>
        private List<NarrativeEvent> GenerateConflictEvents(int quarter)
        {
            var events = new List<NarrativeEvent>();

            // Business conflicts based on company performance
            if (company.ConsecutiveNegativeQuarters > 0)
            {
                events.Add(CreateFinancialCrisisConflict(quarter));
            }

            if (company.Morale < 30)
            {
                events.Add(CreateEmployeeMoraleConflict(quarter));
            }

            if (company.MarketShare > 40 && random.NextDouble() < CONFLICT_EVENT_PROBABILITY)
            {
                events.Add(CreateCompetitorChallengeConflict(quarter));
            }

            // Character-driven conflicts
            foreach (var characterId in storyData.CharacterRelationships.Keys)
            {
                var relationship = storyData.CharacterRelationships[characterId];
                if (relationship.CurrentPhase == RelationshipPhase.Strained && 
                    random.NextDouble() < 0.4)
                {
                    events.Add(CreateCharacterConflictEvent(characterId, quarter));
                }
            }

            return events;
        }

        /// <summary>
        /// Generates milestone events for significant company achievements
        /// </summary>
        private List<NarrativeEvent> GenerateMilestoneEvents(int quarter)
        {
            var events = new List<NarrativeEvent>();

            // Market share milestones
            if (company.MarketShare >= 25 && !HasTriggeredMilestone("market_share_25"))
            {
                events.Add(CreateMarketShareMilestone(quarter, 25));
                storyData.StoryFlags.Add("market_share_25");
            }

            if (company.MarketShare >= 50 && !HasTriggeredMilestone("market_share_50"))
            {
                events.Add(CreateMarketShareMilestone(quarter, 50));
                storyData.StoryFlags.Add("market_share_50");
            }

            // Capital milestones
            if (company.Capital >= 500000000 && !HasTriggeredMilestone("capital_500m"))
            {
                events.Add(CreateCapitalMilestone(quarter, 500000000));
                storyData.StoryFlags.Add("capital_500m");
            }

            if (company.Capital >= 1000000000 && !HasTriggeredMilestone("capital_1b"))
            {
                events.Add(CreateCapitalMilestone(quarter, 1000000000));
                storyData.StoryFlags.Add("capital_1b");
            }

            // Employee milestones
            if (company.EmployeeCount >= 50 && !HasTriggeredMilestone("employees_50"))
            {
                events.Add(CreateEmployeeMilestone(quarter, 50));
                storyData.StoryFlags.Add("employees_50");
            }

            // Time-based milestones
            if (quarter == 30 && !HasTriggeredMilestone("7_year_anniversary"))
            {
                events.Add(CreateAnniversaryMilestone(quarter, 7));
                storyData.StoryFlags.Add("7_year_anniversary");
            }

            if (quarter == 60 && !HasTriggeredMilestone("15_year_anniversary"))
            {
                events.Add(CreateAnniversaryMilestone(quarter, 15));
                storyData.StoryFlags.Add("15_year_anniversary");
            }

            return events;
        }

        /// <summary>
        /// Generates events based on consequences of previous player choices
        /// </summary>
        private List<NarrativeEvent> GenerateChoiceConsequenceEvents(int quarter)
        {
            var events = new List<NarrativeEvent>();

            // Look for choices with delayed consequences
            foreach (var choice in storyData.ChoiceHistory)
            {
                foreach (var flag in choice.ConsequenceFlags)
                {
                    if (ShouldTriggerConsequenceEvent(flag, quarter, choice.Quarter))
                    {
                        events.Add(CreateChoiceConsequenceEvent(choice, flag, quarter));
                    }
                }
            }

            return events;
        }

        /// <summary>
        /// Generates act transition events for major narrative shifts
        /// </summary>
        private List<NarrativeEvent> GenerateActTransitionEvents(int quarter)
        {
            var events = new List<NarrativeEvent>();
            var newAct = StoryScript.GetNarrativeActForQuarter(quarter);

            switch (newAct)
            {
                case NarrativeAct.RisingAction:
                    events.Add(CreateActTransitionEvent(quarter, "Tutorial Complete", 
                        "You've mastered the basics of corporate management. The real challenges begin now."));
                    break;

                case NarrativeAct.Climax:
                    events.Add(CreateActTransitionEvent(quarter, "The Stakes Rise", 
                        "Your company has reached a critical juncture. Every decision now carries enormous weight."));
                    break;

                case NarrativeAct.Resolution:
                    events.Add(CreateActTransitionEvent(quarter, "The Final Chapter", 
                        "As you approach the twilight of your career, it's time to consider your legacy."));
                    break;
            }

            return events;
        }

        #region Event Creation Methods

        private NarrativeEvent CreateCharacterIntroductionEvent(StoryCharacter character, int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"intro_{character.CharacterId}_Q{quarter}",
                EventType = NarrativeEventType.CharacterIntroduction,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { character.CharacterId },
                Title = $"Meet {character.Name}",
                Description = $"A new {character.Role} joins your corporate journey.",
                Dialogue = CharacterDialogue.GetIntroductionDialogue(character.CharacterId, company, random),
                Choices = CreateIntroductionChoices(character.CharacterId),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["character_introduced"] = character.CharacterId,
                    ["relationship_established"] = true
                }
            };
        }

        private NarrativeEvent CreateRelationshipMilestoneEvent(StoryCharacter character, int quarter)
        {
            var relationship = storyData.CharacterRelationships[character.CharacterId];
            
            return new NarrativeEvent
            {
                EventId = $"milestone_{character.CharacterId}_Q{quarter}",
                EventType = NarrativeEventType.RelationshipMilestone,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { character.CharacterId },
                Title = $"Growing Closer to {character.Name}",
                Description = GetRelationshipMilestoneDescription(character.CharacterId, relationship.CurrentPhase),
                Dialogue = CharacterDialogue.GetRelationshipMilestoneDialogue(character.CharacterId, relationship.CurrentPhase, company, random),
                Choices = CreateRelationshipMilestoneChoices(character.CharacterId, relationship),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["relationship_milestone"] = relationship.CurrentPhase.ToString(),
                    ["character_arc_progress"] = true
                }
            };
        }

        private NarrativeEvent CreatePersonalChallengeEvent(StoryCharacter character, int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"challenge_{character.CharacterId}_Q{quarter}",
                EventType = NarrativeEventType.PersonalChallenge,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { character.CharacterId },
                Title = $"{character.Name}'s Personal Challenge",
                Description = GetPersonalChallengeDescription(character.CharacterId),
                Dialogue = CharacterDialogue.GetPersonalChallengeDialogue(character.CharacterId, company, random),
                Choices = CreatePersonalChallengeChoices(character.CharacterId),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["personal_challenge"] = character.CharacterId,
                    ["emotional_investment"] = true
                }
            };
        }

        private NarrativeEvent CreateFinancialCrisisConflict(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"financial_crisis_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "marcus_vey", "evelyn_cross", "joan" },
                Title = "Financial Crisis Management",
                Description = "The company's financial struggles are creating tension among your leadership team.",
                Dialogue = new List<string>
                {
                    "The consecutive losses are putting enormous pressure on everyone.",
                    "Difficult decisions about cost-cutting and layoffs may be necessary.",
                    "How you handle this crisis will define your leadership."
                },
                Choices = CreateFinancialCrisisChoices(),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["crisis_type"] = "financial",
                    ["leadership_test"] = true
                }
            };
        }

        private NarrativeEvent CreateEmployeeMoraleConflict(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"morale_crisis_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "evelyn_cross", "joan" },
                Title = "Employee Morale Crisis",
                Description = "Low morale is threatening the stability of your workforce.",
                Dialogue = new List<string>
                {
                    "Employee satisfaction has reached critically low levels.",
                    "We're at risk of losing key talent if we don't act quickly.",
                    "The team needs to see that leadership cares about their wellbeing."
                },
                Choices = CreateMoraleConflictChoices(),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["morale_crisis"] = true,
                    ["employee_retention_risk"] = true
                }
            };
        }

        private NarrativeEvent CreateCompetitorChallengeConflict(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"competitor_challenge_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "vincent_duro", "lucinda_vale", "marcus_vey" },
                Title = "Competitive Pressure Intensifies",
                Description = "A major competitor is making aggressive moves to challenge your market position.",
                Dialogue = new List<string>
                {
                    "Vincent Duro's company is launching a direct assault on our market share.",
                    "They're using aggressive pricing and marketing tactics.",
                    "We need a strategic response to maintain our competitive advantage."
                },
                Choices = CreateCompetitorChallengeChoices(),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["competitor_challenge"] = "vincent_duro",
                    ["market_pressure"] = true
                }
            };
        }

        private NarrativeEvent CreateCharacterConflictEvent(string characterId, int quarter)
        {
            var character = StoryScript.Characters[characterId];
            
            return new NarrativeEvent
            {
                EventId = $"conflict_{characterId}_Q{quarter}",
                EventType = NarrativeEventType.BusinessConflict,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { characterId, "joan" },
                Title = $"Tension with {character.Name}",
                Description = GetCharacterConflictDescription(characterId),
                Dialogue = CharacterDialogue.GetConflictDialogue(characterId, company, random),
                Choices = CreateCharacterConflictChoices(characterId),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["character_conflict"] = characterId,
                    ["relationship_repair_opportunity"] = true
                }
            };
        }

        private NarrativeEvent CreateMarketShareMilestone(int quarter, int percentage)
        {
            return new NarrativeEvent
            {
                EventId = $"market_milestone_{percentage}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan", "lucinda_vale", "marcus_vey" },
                Title = $"Market Share Milestone: {percentage}%",
                Description = $"Your company has achieved {percentage}% market share - a significant milestone!",
                Dialogue = new List<string>
                {
                    $"Congratulations! Reaching {percentage}% market share is a remarkable achievement.",
                    "This milestone represents years of strategic decisions and hard work.",
                    "The entire team should be proud of what we've accomplished together."
                },
                Choices = CreateMilestoneChoices("market_share", percentage),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["milestone_type"] = "market_share",
                    ["milestone_value"] = percentage,
                    ["celebration_opportunity"] = true
                }
            };
        }

        private NarrativeEvent CreateCapitalMilestone(int quarter, double amount)
        {
            var amountText = amount >= 1000000000 ? "$1 Billion" : $"${amount / 1000000:F0} Million";
            
            return new NarrativeEvent
            {
                EventId = $"capital_milestone_{amount}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan", "marcus_vey", "selena_park" },
                Title = $"Capital Milestone: {amountText}",
                Description = $"Your company has reached {amountText} in capital - a financial triumph!",
                Dialogue = new List<string>
                {
                    $"Incredible! We've reached {amountText} in company capital.",
                    "This level of financial success opens up entirely new possibilities.",
                    "You've built something truly remarkable here."
                },
                Choices = CreateMilestoneChoices("capital", (int)(amount / 1000000)),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["milestone_type"] = "capital",
                    ["milestone_value"] = amount,
                    ["investment_opportunities"] = true
                }
            };
        }

        private NarrativeEvent CreateEmployeeMilestone(int quarter, int count)
        {
            return new NarrativeEvent
            {
                EventId = $"employee_milestone_{count}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan", "evelyn_cross" },
                Title = $"Team Growth Milestone: {count} Employees",
                Description = $"Your company now employs {count} people - a testament to your growth!",
                Dialogue = new List<string>
                {
                    $"We now have {count} employees working together toward our shared vision.",
                    "Each person represents a family supported by the opportunities you've created.",
                    "Building a team this size is a significant leadership achievement."
                },
                Choices = CreateMilestoneChoices("employees", count),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["milestone_type"] = "employees",
                    ["milestone_value"] = count,
                    ["team_celebration"] = true
                }
            };
        }

        private NarrativeEvent CreateAnniversaryMilestone(int quarter, int years)
        {
            return new NarrativeEvent
            {
                EventId = $"anniversary_{years}_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan" },
                Title = $"{years}-Year Anniversary",
                Description = $"Celebrating {years} years of corporate leadership and growth.",
                Dialogue = new List<string>
                {
                    $"Can you believe it's been {years} years since you took over this company?",
                    "Look at everything we've accomplished together during this time.",
                    "This anniversary is a moment to reflect on the journey and look toward the future."
                },
                Choices = CreateAnniversaryChoices(years),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["milestone_type"] = "anniversary",
                    ["milestone_value"] = years,
                    ["reflection_opportunity"] = true
                }
            };
        }

        private NarrativeEvent CreateChoiceConsequenceEvent(StoryChoiceRecord choice, string consequenceFlag, int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"consequence_{choice.ChoiceId}_{consequenceFlag}_Q{quarter}",
                EventType = NarrativeEventType.ChoiceConsequence,
                TriggerQuarter = quarter,
                InvolvedCharacters = GetInvolvedCharactersForConsequence(consequenceFlag),
                Title = "Consequences of Past Decisions",
                Description = GetConsequenceDescription(consequenceFlag, choice),
                Dialogue = GetConsequenceDialogue(consequenceFlag, choice),
                Choices = CreateConsequenceChoices(consequenceFlag),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["consequence_flag"] = consequenceFlag,
                    ["original_choice"] = choice.ChoiceId,
                    ["choice_quarter"] = choice.Quarter
                }
            };
        }

        private NarrativeEvent CreateActTransitionEvent(int quarter, string title, string description)
        {
            var newAct = StoryScript.GetNarrativeActForQuarter(quarter);
            
            return new NarrativeEvent
            {
                EventId = $"act_transition_{newAct}_Q{quarter}",
                EventType = NarrativeEventType.ActTransition,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan" },
                Title = title,
                Description = description,
                Dialogue = GetActTransitionDialogue(newAct, quarter),
                Choices = CreateActTransitionChoices(newAct),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["act_transition"] = newAct.ToString(),
                    ["narrative_shift"] = true
                }
            };
        }

        #endregion

        #region Choice Creation Methods

        private List<DialogueChoice> CreateIntroductionChoices(string characterId)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "intro_professional",
                ChoiceText = "Welcome to the team. I look forward to working with you.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 3,
                    TrustChange = 1
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "intro_enthusiastic",
                ChoiceText = "I'm excited to have you join us! Tell me about your background.",
                Tone = ChoiceTone.Supportive,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    PersonalConnectionChange = 4,
                    TrustChange = 2
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateRelationshipMilestoneChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "milestone_acknowledge",
                ChoiceText = "I value our working relationship and trust your judgment.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    TrustChange = 5,
                    RespectChange = 3
                }
            });
            
            if (relationship.PersonalConnection >= 30)
            {
                choices.Add(new DialogueChoice
                {
                    ChoiceId = "milestone_personal",
                    ChoiceText = "I'm grateful to have you as both a colleague and a friend.",
                    Tone = ChoiceTone.Personal,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        PersonalConnectionChange = 8,
                        TrustChange = 4
                    }
                });
            }
            
            return choices;
        }

        private List<DialogueChoice> CreatePersonalChallengeChoices(string characterId)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "challenge_support",
                ChoiceText = "I'm here to support you through this. What do you need?",
                Tone = ChoiceTone.Supportive,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    PersonalConnectionChange = 10,
                    TrustChange = 6
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "challenge_professional",
                ChoiceText = "Let's focus on how this affects our work and find solutions.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 4,
                    TrustChange = 2
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateFinancialCrisisChoices()
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "crisis_aggressive_cuts",
                ChoiceText = "We need immediate cost reductions. Prepare for layoffs.",
                Tone = ChoiceTone.Aggressive,
                RiskLevel = ConsequenceRisk.High,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "marcus_vey",
                    RespectChange = 5,
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["evelyn_cross"] = -8,
                        ["joan"] = -3
                    }
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "crisis_balanced_approach",
                ChoiceText = "Let's find cost savings that minimize impact on our people.",
                Tone = ChoiceTone.Diplomatic,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "evelyn_cross",
                    RespectChange = 6,
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["joan"] = 4,
                        ["marcus_vey"] = 1
                    }
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateMoraleConflictChoices()
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "morale_investment",
                ChoiceText = "Authorize employee bonuses and team-building initiatives.",
                Tone = ChoiceTone.Supportive,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "evelyn_cross",
                    PersonalConnectionChange = 8,
                    TrustChange = 5,
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["all_employees"] = 10
                    }
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "morale_communication",
                ChoiceText = "Schedule all-hands meetings to address concerns directly.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "evelyn_cross",
                    RespectChange = 5,
                    TrustChange = 3
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateCompetitorChallengeChoices()
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "competitor_aggressive_response",
                ChoiceText = "Launch a counter-offensive. Match their pricing and exceed their marketing.",
                Tone = ChoiceTone.Aggressive,
                RiskLevel = ConsequenceRisk.Medium,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "lucinda_vale",
                    RespectChange = 7,
                    TrustChange = 4
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "competitor_strategic_response",
                ChoiceText = "Focus on our unique strengths and differentiate our value proposition.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "marcus_vey",
                    RespectChange = 5,
                    TrustChange = 3
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateCharacterConflictChoices(string characterId)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "conflict_apologize",
                ChoiceText = "I apologize if my decisions have caused problems. Let's work this out.",
                Tone = ChoiceTone.Diplomatic,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    TrustChange = 8,
                    PersonalConnectionChange = 5,
                    ImpactDescription = "Humility and willingness to repair the relationship"
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "conflict_defend",
                ChoiceText = "I stand by my decisions. They were necessary for the company.",
                Tone = ChoiceTone.Aggressive,
                RiskLevel = ConsequenceRisk.Medium,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 3,
                    TrustChange = -2,
                    PersonalConnectionChange = -4
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateMilestoneChoices(string milestoneType, int value)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "milestone_celebrate",
                ChoiceText = "Let's celebrate this achievement with the entire team!",
                Tone = ChoiceTone.Supportive,
                RelationshipImpact = new RelationshipImpact
                {
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["all_employees"] = 5,
                        ["joan"] = 3,
                        ["evelyn_cross"] = 4
                    }
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "milestone_focus_forward",
                ChoiceText = "This is just the beginning. Let's set our sights even higher.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["marcus_vey"] = 4,
                        ["lucinda_vale"] = 3
                    }
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateAnniversaryChoices(int years)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "anniversary_grateful",
                ChoiceText = "I'm grateful for everyone who made this journey possible.",
                Tone = ChoiceTone.Personal,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    PersonalConnectionChange = 8,
                    TrustChange = 5,
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["all_characters"] = 3
                    }
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "anniversary_ambitious",
                ChoiceText = "The next phase will be even more ambitious than what we've achieved.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    RespectChange = 5,
                    TrustChange = 2
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateConsequenceChoices(string consequenceFlag)
        {
            var choices = new List<DialogueChoice>();
            
            // Generic consequence choices - can be expanded based on specific flags
            choices.Add(new DialogueChoice
            {
                ChoiceId = "consequence_accept",
                ChoiceText = "I accept responsibility for the consequences of my decisions.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["all_characters"] = 2
                    }
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "consequence_learn",
                ChoiceText = "This is a learning experience. How can we do better next time?",
                Tone = ChoiceTone.Supportive,
                RelationshipImpact = new RelationshipImpact
                {
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["joan"] = 4,
                        ["evelyn_cross"] = 3
                    }
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> CreateActTransitionChoices(NarrativeAct act)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "transition_ready",
                ChoiceText = "I'm ready for whatever challenges lie ahead.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    RespectChange = 4,
                    TrustChange = 3
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "transition_reflective",
                ChoiceText = "Let me take a moment to reflect on how far we've come.",
                Tone = ChoiceTone.Personal,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    PersonalConnectionChange = 5,
                    TrustChange = 2
                }
            });
            
            return choices;
        }

        #endregion

        #region Helper Methods

        private bool ShouldGenerateRelationshipMilestone(string characterId, int quarter)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return false;

            var relationship = storyData.CharacterRelationships[characterId];
            var character = StoryScript.Characters[characterId];
            
            // Check if enough time has passed since introduction
            if (quarter < character.IntroductionQuarter + 5)
                return false;

            // Check if relationship has progressed significantly
            var totalRelationship = relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection;
            
            // Generate milestone events at relationship thresholds
            return totalRelationship >= 60 && !HasTriggeredMilestone($"relationship_{characterId}_milestone") && 
                   random.NextDouble() < 0.3;
        }

        private bool ShouldGeneratePersonalChallenge(string characterId, int quarter)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return false;

            var relationship = storyData.CharacterRelationships[characterId];
            var character = StoryScript.Characters[characterId];
            
            // Only generate personal challenges for established relationships
            if (quarter < character.IntroductionQuarter + 10)
                return false;

            if (relationship.PersonalConnection < 40)
                return false;

            // Check if we haven't had a personal challenge recently
            var recentChallenges = storyData.CompletedStoryEvents
                .Where(e => e.Contains($"challenge_{characterId}") && 
                           int.Parse(e.Split('_').Last().Substring(1)) > quarter - 8)
                .Any();

            return !recentChallenges && random.NextDouble() < 0.2;
        }

        private bool IsActTransitionQuarter(int quarter)
        {
            return quarter == 11 || quarter == 61 || quarter == 101;
        }

        private bool HasTriggeredMilestone(string milestoneFlag)
        {
            return storyData.StoryFlags.Contains(milestoneFlag);
        }

        private bool ShouldTriggerConsequenceEvent(string consequenceFlag, int currentQuarter, int choiceQuarter)
        {
            // Trigger consequences 3-8 quarters after the original choice
            var quartersSince = currentQuarter - choiceQuarter;
            return quartersSince >= 3 && quartersSince <= 8 && 
                   !storyData.CompletedStoryEvents.Contains($"consequence_{consequenceFlag}");
        }

        private List<NarrativeEvent> FilterAndPrioritizeEvents(List<NarrativeEvent> events, int quarter)
        {
            // Limit to 2-3 events per quarter to avoid overwhelming the player
            var prioritizedEvents = events
                .OrderByDescending(e => GetEventPriority(e))
                .Take(3)
                .ToList();

            return prioritizedEvents;
        }

        private int GetEventPriority(NarrativeEvent narrativeEvent)
        {
            // Prioritize events based on type and importance
            return narrativeEvent.EventType switch
            {
                NarrativeEventType.ActTransition => 100,
                NarrativeEventType.CharacterIntroduction => 90,
                NarrativeEventType.EmotionalBeat => 80,
                NarrativeEventType.BusinessConflict => 70,
                NarrativeEventType.RelationshipMilestone => 60,
                NarrativeEventType.PersonalChallenge => 50,
                NarrativeEventType.ChoiceConsequence => 40,
                _ => 30
            };
        }

        #endregion

        #region Description and Dialogue Helper Methods

        private string GetRelationshipMilestoneDescription(string characterId, RelationshipPhase phase)
        {
            var character = StoryScript.Characters[characterId];
            return phase switch
            {
                RelationshipPhase.TrustedColleague => $"Your professional relationship with {character.Name} has deepened into mutual trust and respect.",
                RelationshipPhase.PersonalFriend => $"{character.Name} has become more than just a colleague - you've developed a genuine personal connection.",
                RelationshipPhase.LifelongBond => $"Your bond with {character.Name} has grown into a lifelong partnership built on shared experiences and mutual support.",
                _ => $"Your relationship with {character.Name} continues to evolve and strengthen."
            };
        }

        private string GetPersonalChallengeDescription(string characterId)
        {
            return characterId switch
            {
                "joan" => "Joan is facing a personal family crisis that's affecting her work performance.",
                "marcus_vey" => "Marcus is dealing with pressure from his previous firm and considering a lucrative job offer.",
                "evelyn_cross" => "Evelyn is struggling with work-life balance as her department grows more demanding.",
                "vincent_duro" => "Vincent's aggressive tactics are creating internal pressure within his own organization.",
                "lucinda_vale" => "Lucy is facing creative burnout and questioning her marketing strategies.",
                "gregory_shaw" => "Greg is dealing with family health issues that require his attention.",
                "selena_park" => "Selena is under pressure from her investment partners to deliver higher returns.",
                "harold_finch" => "Harold is facing ethical concerns about some of the company's business practices.",
                "sophie_kim" => "Sophie is feeling overwhelmed by the complexity of her analytical responsibilities.",
                _ => "This character is facing a significant personal challenge that affects their work."
            };
        }

        private string GetCharacterConflictDescription(string characterId)
        {
            var character = StoryScript.Characters[characterId];
            return $"Recent decisions have created tension with {character.Name}, straining your professional relationship.";
        }

        private List<string> GetInvolvedCharactersForConsequence(string consequenceFlag)
        {
            // Return characters involved based on consequence type
            return consequenceFlag switch
            {
                var flag when flag.Contains("financial") => new List<string> { "marcus_vey", "joan" },
                var flag when flag.Contains("employee") => new List<string> { "evelyn_cross", "joan" },
                var flag when flag.Contains("marketing") => new List<string> { "lucinda_vale", "joan" },
                var flag when flag.Contains("legal") => new List<string> { "harold_finch", "joan" },
                _ => new List<string> { "joan" }
            };
        }

        private string GetConsequenceDescription(string consequenceFlag, StoryChoiceRecord choice)
        {
            return $"The consequences of your decision in Q{choice.Quarter} are now becoming apparent.";
        }

        private List<string> GetConsequenceDialogue(string consequenceFlag, StoryChoiceRecord choice)
        {
            return new List<string>
            {
                $"Remember your decision back in Q{choice.Quarter}?",
                "The effects of that choice are starting to show.",
                "This is how decisions ripple through time in corporate leadership."
            };
        }

        private List<string> GetActTransitionDialogue(NarrativeAct act, int quarter)
        {
            return act switch
            {
                NarrativeAct.RisingAction => new List<string>
                {
                    "You've completed the tutorial phase and proven your basic competency.",
                    "Now the real challenges begin - the business world doesn't offer training wheels.",
                    "Every decision from here forward carries greater weight and consequence."
                },
                NarrativeAct.Climax => new List<string>
                {
                    "Your company has reached a critical juncture in its development.",
                    "The decisions you make in this phase will determine your ultimate legacy.",
                    "This is where true leadership is tested and proven."
                },
                NarrativeAct.Resolution => new List<string>
                {
                    "As we enter the final phase of your corporate journey, it's time to consider your legacy.",
                    "The choices you make now will determine how your story ends.",
                    "What kind of leader do you want to be remembered as?"
                },
                _ => new List<string> { "A new chapter in your corporate journey begins." }
            };
        }

        #endregion
    }
}