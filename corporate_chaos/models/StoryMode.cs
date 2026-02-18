using System.Text.Json.Serialization;

namespace CorporateChaos.Models
{
    public enum StoryPhase
    {
        Tutorial,
        FullMode
    }

    public enum NarrativeAct
    {
        Tutorial,       // Q1-10
        RisingAction,   // Q11-60
        Climax,         // Q61-100
        Resolution      // Q101-120
    }

    public enum RelationshipPhase
    {
        FirstMeeting,
        ProfessionalAcquaintance,
        TrustedColleague,
        PersonalFriend,
        LifelongBond,
        Strained,
        Hostile
    }

    public enum CharacterArcPhase
    {
        Introduction,
        Development,
        Conflict,
        Resolution,
        Legacy
    }

    public enum NarrativeEventType
    {
        CharacterIntroduction,
        RelationshipMilestone,
        PersonalChallenge,
        BusinessConflict,
        EmotionalBeat,
        ChoiceConsequence,
        ActTransition,
        EndingSetup
    }

    public enum ChoiceTone
    {
        Professional,
        Supportive,
        Aggressive,
        Diplomatic,
        Personal,
        Humorous
    }

    public enum EmotionalTone
    {
        Neutral,
        Positive,
        Negative,
        Tense,
        Warm,
        Serious,
        Playful,
        Concerned,
        Excited,
        Disappointed,
        Angry,
        Worried,
        Supportive,
        Competitive,
        Enthusiastic,
        Professional
    }

    public enum ConsequenceRisk
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class ConsequencePreview
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public ConsequenceType Type { get; set; } = ConsequenceType.Relationship;
        
        [JsonPropertyName("severity")]
        public ConsequenceRisk Severity { get; set; } = ConsequenceRisk.Low;
        
        [JsonPropertyName("affectedCharacters")]
        public List<string> AffectedCharacters { get; set; } = new List<string>();
        
        [JsonPropertyName("gameplayImpact")]
        public Dictionary<string, object> GameplayImpact { get; set; } = new Dictionary<string, object>();
        
        [JsonPropertyName("triggerQuarter")]
        public int? TriggerQuarter { get; set; }
    }

    public class RelationshipImpact
    {
        [JsonPropertyName("primaryCharacter")]
        public string PrimaryCharacter { get; set; } = string.Empty;
        
        [JsonPropertyName("trustChange")]
        public int TrustChange { get; set; } = 0;
        
        [JsonPropertyName("respectChange")]
        public int RespectChange { get; set; } = 0;
        
        [JsonPropertyName("personalConnectionChange")]
        public int PersonalConnectionChange { get; set; } = 0;
        
        [JsonPropertyName("secondaryEffects")]
        public Dictionary<string, int> SecondaryEffects { get; set; } = new Dictionary<string, int>();
        
        [JsonPropertyName("impactDescription")]
        public string ImpactDescription { get; set; } = string.Empty;
        
        [JsonPropertyName("phaseTransitionPotential")]
        public bool PhaseTransitionPotential { get; set; } = false;
    }

    public enum ConsequenceType
    {
        Relationship,
        Gameplay,
        Story,
        Character,
        Business,
        Emotional
    }

    public enum EndingType
    {
        MarketDominance,        // 70% market share
        ConglomerateBuyout,     // $1B+ capital
        GracefulRetirement,     // Reach Q120 with stable company
        BankruptcyFailure,      // Consecutive negative quarters
        LostManpowerFailure,    // Zero employees
        HealthRetirement        // Early retirement due to health
    }

    public enum MechanicType
    {
        BasicOperations,      // Q1 - Company stats, end quarter
        EmployeeHiring,       // Q2 - Hiring system
        DepartmentManagement, // Q3 - Department assignments
        ExecutiveDecisions,   // Q4 - Strategic decisions
        FinancialManagement,  // Q5 - Budget allocation
        CrisisManagement,     // Q6 - Handling chaos events
        AdvancedHR,          // Q7 - Employee firing, performance management
        MarketAnalysis,      // Q8 - Market competition, strategic positioning
        RiskManagement,      // Q9 - Risk assessment, mitigation strategies
        AdvancedStrategy     // Q10 - Complex decision making, long-term planning
    }

    public class StoryModeData
    {
        [JsonPropertyName("currentQuarter")]
        public int CurrentQuarter { get; set; } = 1;

        [JsonPropertyName("currentPhase")]
        public StoryPhase CurrentPhase { get; set; } = StoryPhase.Tutorial;

        [JsonPropertyName("unlockedMechanics")]
        public HashSet<MechanicType> UnlockedMechanics { get; set; } = new HashSet<MechanicType>();

        [JsonPropertyName("completedTutorials")]
        public HashSet<MechanicType> CompletedTutorials { get; set; } = new HashSet<MechanicType>();

        [JsonPropertyName("storyEvents")]
        public List<string> CompletedStoryEvents { get; set; } = new List<string>();

        [JsonPropertyName("isStoryMode")]
        public bool IsStoryMode { get; set; } = true;
    }

    // Extended story mode data for the expanded narrative system
    public class ExtendedStoryModeData : StoryModeData
    {
        // Character relationship tracking
        [JsonPropertyName("characterRelationships")]
        public Dictionary<string, CharacterRelationship> CharacterRelationships { get; set; } = new Dictionary<string, CharacterRelationship>();
        
        // Story choice history
        [JsonPropertyName("choiceHistory")]
        public List<StoryChoiceRecord> ChoiceHistory { get; set; } = new List<StoryChoiceRecord>();
        
        // Current narrative state
        [JsonPropertyName("currentAct")]
        public NarrativeAct CurrentAct { get; set; } = NarrativeAct.Tutorial;
        
        [JsonPropertyName("activeStoryArcs")]
        public List<string> ActiveStoryArcs { get; set; } = new List<string>();
        
        [JsonPropertyName("storyFlags")]
        public List<string> StoryFlags { get; set; } = new List<string>();
        
        // Character arc progression
        [JsonPropertyName("characterArcs")]
        public Dictionary<string, CharacterArcState> CharacterArcs { get; set; } = new Dictionary<string, CharacterArcState>();
        
        // Ending path tracking
        [JsonPropertyName("endingProgression")]
        public EndingPathData EndingProgression { get; set; } = new EndingPathData();
    }

    public class CharacterRelationship
    {
        [JsonPropertyName("trustLevel")]
        public int TrustLevel { get; set; } = 0; // -100 to 100
        
        [JsonPropertyName("professionalRespect")]
        public int ProfessionalRespect { get; set; } = 0; // -100 to 100
        
        [JsonPropertyName("personalConnection")]
        public int PersonalConnection { get; set; } = 0; // -100 to 100
        
        [JsonPropertyName("sharedExperiences")]
        public List<string> SharedExperiences { get; set; } = new List<string>();
        
        [JsonPropertyName("conflictHistory")]
        public List<string> ConflictHistory { get; set; } = new List<string>();
        
        [JsonPropertyName("currentPhase")]
        public RelationshipPhase CurrentPhase { get; set; } = RelationshipPhase.FirstMeeting;
    }

    public class StoryChoiceRecord
    {
        [JsonPropertyName("quarter")]
        public int Quarter { get; set; }
        
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = string.Empty;
        
        [JsonPropertyName("choiceId")]
        public string ChoiceId { get; set; } = string.Empty;
        
        [JsonPropertyName("choiceText")]
        public string ChoiceText { get; set; } = string.Empty;
        
        [JsonPropertyName("relationshipImpacts")]
        public Dictionary<string, int> RelationshipImpacts { get; set; } = new Dictionary<string, int>();
        
        [JsonPropertyName("consequenceFlags")]
        public List<string> ConsequenceFlags { get; set; } = new List<string>();
    }

    public class CharacterArcState
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; } = string.Empty;
        
        [JsonPropertyName("currentPhase")]
        public CharacterArcPhase CurrentPhase { get; set; } = CharacterArcPhase.Introduction;
        
        [JsonPropertyName("completedMilestones")]
        public List<string> CompletedMilestones { get; set; } = new List<string>();
        
        [JsonPropertyName("availableMilestones")]
        public List<string> AvailableMilestones { get; set; } = new List<string>();
        
        [JsonPropertyName("arcSpecificData")]
        public Dictionary<string, object> ArcSpecificData { get; set; } = new Dictionary<string, object>();
        
        [JsonPropertyName("nextMilestoneQuarter")]
        public int NextMilestoneQuarter { get; set; } = 1;
    }

    public class EndingPathData
    {
        [JsonPropertyName("viableEndings")]
        public List<EndingType> ViableEndings { get; set; } = new List<EndingType>();
        
        [JsonPropertyName("endingProbabilities")]
        public Dictionary<EndingType, double> EndingProbabilities { get; set; } = new Dictionary<EndingType, double>();
        
        [JsonPropertyName("endingRequirementsMet")]
        public List<string> EndingRequirementsMet { get; set; } = new List<string>();
        
        [JsonPropertyName("endingBlockers")]
        public List<string> EndingBlockers { get; set; } = new List<string>();
    }

    public class StoryCharacter
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        
        [JsonPropertyName("personalityTraits")]
        public List<string> PersonalityTraits { get; set; } = new List<string>();
        
        [JsonPropertyName("relationshipWithPlayer")]
        public CharacterRelationship RelationshipWithPlayer { get; set; } = new CharacterRelationship();
        
        [JsonPropertyName("characterArcMilestones")]
        public List<string> CharacterArcMilestones { get; set; } = new List<string>();
        
        [JsonPropertyName("characterState")]
        public Dictionary<string, object> CharacterState { get; set; } = new Dictionary<string, object>();
        
        [JsonPropertyName("availableDialogueTopics")]
        public List<string> AvailableDialogueTopics { get; set; } = new List<string>();
        
        [JsonPropertyName("introductionQuarter")]
        public int IntroductionQuarter { get; set; } = 1;
    }

    public class DialogueConversation
    {
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("participants")]
        public List<string> Participants { get; set; } = new List<string>();
        
        [JsonPropertyName("nodes")]
        public Dictionary<string, DialogueNode> Nodes { get; set; } = new Dictionary<string, DialogueNode>();
        
        [JsonPropertyName("startNodeId")]
        public string StartNodeId { get; set; } = string.Empty;
        
        [JsonPropertyName("currentNodeId")]
        public string CurrentNodeId { get; set; } = string.Empty;
        
        [JsonPropertyName("conversationHistory")]
        public List<string> ConversationHistory { get; set; } = new List<string>();
        
        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; } = false;
    }

    public class DialogueNode
    {
        [JsonPropertyName("nodeId")]
        public string NodeId { get; set; } = string.Empty;
        
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; } = string.Empty;
        
        [JsonPropertyName("dialogueText")]
        public string DialogueText { get; set; } = string.Empty;
        
        [JsonPropertyName("contextTags")]
        public List<string> ContextTags { get; set; } = new List<string>();
        
        [JsonPropertyName("choices")]
        public List<DialogueChoice> Choices { get; set; } = new List<DialogueChoice>();
        
        [JsonPropertyName("conditions")]
        public List<string> Conditions { get; set; } = new List<string>();
        
        [JsonPropertyName("effects")]
        public Dictionary<string, object> Effects { get; set; } = new Dictionary<string, object>();
        
        // Enhanced properties for branching conversations
        [JsonPropertyName("emotionalTone")]
        public EmotionalTone EmotionalTone { get; set; } = EmotionalTone.Neutral;
        
        [JsonPropertyName("relationshipContext")]
        public Dictionary<string, int> RelationshipContext { get; set; } = new Dictionary<string, int>();
        
        [JsonPropertyName("storyFlags")]
        public List<string> StoryFlags { get; set; } = new List<string>();
        
        [JsonPropertyName("minimumChoices")]
        public int MinimumChoices { get; set; } = 2;
        
        [JsonPropertyName("maximumChoices")]
        public int MaximumChoices { get; set; } = 4;
        
        [JsonPropertyName("adaptiveText")]
        public Dictionary<string, string> AdaptiveText { get; set; } = new Dictionary<string, string>();

        // Helper methods for managing multiple response options
        public List<DialogueChoice> GetAvailableChoices(Dictionary<string, CharacterRelationship> relationships, List<string> activeStoryFlags)
        {
            var availableChoices = new List<DialogueChoice>();
            
            foreach (var choice in Choices)
            {
                if (IsChoiceAvailable(choice, relationships, activeStoryFlags))
                {
                    availableChoices.Add(choice);
                }
            }
            
            // Ensure we have at least the minimum number of choices
            if (availableChoices.Count < MinimumChoices && Choices.Count >= MinimumChoices)
            {
                // Add fallback choices if needed
                var fallbackChoices = Choices.Where(c => !availableChoices.Contains(c))
                                           .Take(MinimumChoices - availableChoices.Count);
                availableChoices.AddRange(fallbackChoices);
            }
            
            // Limit to maximum choices
            if (availableChoices.Count > MaximumChoices)
            {
                availableChoices = availableChoices.Take(MaximumChoices).ToList();
            }
            
            return availableChoices;
        }

        private bool IsChoiceAvailable(DialogueChoice choice, Dictionary<string, CharacterRelationship> relationships, List<string> activeStoryFlags)
        {
            if (!choice.IsAvailable)
                return false;
            
            // Check required conditions
            foreach (var condition in choice.RequiresConditions)
            {
                if (!EvaluateCondition(condition, relationships, activeStoryFlags))
                    return false;
            }
            
            return true;
        }

        private bool EvaluateCondition(string condition, Dictionary<string, CharacterRelationship> relationships, List<string> activeStoryFlags)
        {
            // Simple condition evaluation - can be expanded
            if (condition.StartsWith("flag:"))
            {
                var flagName = condition.Substring(5);
                return activeStoryFlags.Contains(flagName);
            }
            
            if (condition.StartsWith("relationship:"))
            {
                var parts = condition.Substring(13).Split(':');
                if (parts.Length == 3)
                {
                    var characterId = parts[0];
                    var attribute = parts[1]; // trust, respect, personal
                    var threshold = int.Parse(parts[2]);
                    
                    if (relationships.ContainsKey(characterId))
                    {
                        var relationship = relationships[characterId];
                        return attribute switch
                        {
                            "trust" => relationship.TrustLevel >= threshold,
                            "respect" => relationship.ProfessionalRespect >= threshold,
                            "personal" => relationship.PersonalConnection >= threshold,
                            _ => false
                        };
                    }
                }
            }
            
            return true; // Default to available if condition can't be evaluated
        }

        public string GetAdaptiveDialogueText(Dictionary<string, CharacterRelationship> relationships, List<string> activeStoryFlags)
        {
            // Check for adaptive text based on relationship levels or story flags
            foreach (var adaptiveEntry in AdaptiveText)
            {
                if (EvaluateCondition(adaptiveEntry.Key, relationships, activeStoryFlags))
                {
                    return adaptiveEntry.Value;
                }
            }
            
            return DialogueText; // Return default text if no adaptive text matches
        }

        public bool HasMultipleResponseOptions()
        {
            return Choices.Count >= MinimumChoices;
        }

        public Dictionary<ChoiceTone, int> GetToneDistribution()
        {
            var distribution = new Dictionary<ChoiceTone, int>();
            
            foreach (var choice in Choices)
            {
                if (distribution.ContainsKey(choice.Tone))
                    distribution[choice.Tone]++;
                else
                    distribution[choice.Tone] = 1;
            }
            
            return distribution;
        }
    }

    public class DialogueChoice
    {
        [JsonPropertyName("choiceId")]
        public string ChoiceId { get; set; } = string.Empty;
        
        [JsonPropertyName("choiceText")]
        public string ChoiceText { get; set; } = string.Empty;
        
        [JsonPropertyName("nextNodeId")]
        public string NextNodeId { get; set; } = string.Empty;
        
        [JsonPropertyName("relationshipChanges")]
        public Dictionary<string, int> RelationshipChanges { get; set; } = new Dictionary<string, int>();
        
        [JsonPropertyName("consequenceFlags")]
        public List<string> ConsequenceFlags { get; set; } = new List<string>();
        
        [JsonPropertyName("tone")]
        public ChoiceTone Tone { get; set; } = ChoiceTone.Professional;
        
        [JsonPropertyName("previewText")]
        public string PreviewText { get; set; } = string.Empty;
        
        [JsonPropertyName("requiresConditions")]
        public List<string> RequiresConditions { get; set; } = new List<string>();
        
        [JsonPropertyName("gameplayEffects")]
        public Dictionary<string, object> GameplayEffects { get; set; } = new Dictionary<string, object>();
        
        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; } = true;
        
        // Enhanced properties for consequence tracking and relationship impact
        [JsonPropertyName("immediateConsequences")]
        public List<ConsequencePreview> ImmediateConsequences { get; set; } = new List<ConsequencePreview>();
        
        [JsonPropertyName("longTermConsequences")]
        public List<ConsequencePreview> LongTermConsequences { get; set; } = new List<ConsequencePreview>();
        
        [JsonPropertyName("relationshipImpact")]
        public RelationshipImpact RelationshipImpact { get; set; } = new RelationshipImpact();
        
        [JsonPropertyName("toneDescription")]
        public string ToneDescription { get; set; } = string.Empty;
        
        [JsonPropertyName("riskLevel")]
        public ConsequenceRisk RiskLevel { get; set; } = ConsequenceRisk.Low;
        
        [JsonPropertyName("characterReaction")]
        public string CharacterReaction { get; set; } = string.Empty;
        
        [JsonPropertyName("unlocksFutureOptions")]
        public List<string> UnlocksFutureOptions { get; set; } = new List<string>();
        
        [JsonPropertyName("blocksFutureOptions")]
        public List<string> BlocksFutureOptions { get; set; } = new List<string>();
        
        [JsonPropertyName("storyBranchInfluence")]
        public Dictionary<string, double> StoryBranchInfluence { get; set; } = new Dictionary<string, double>();

        // Helper methods for consequence tracking and relationship impact calculation
        public string GetToneIndicator()
        {
            return Tone switch
            {
                ChoiceTone.Professional => "💼",
                ChoiceTone.Supportive => "🤝",
                ChoiceTone.Aggressive => "⚡",
                ChoiceTone.Diplomatic => "🕊️",
                ChoiceTone.Personal => "💭",
                ChoiceTone.Humorous => "😄",
                _ => "💬"
            };
        }

        public string GetRiskIndicator()
        {
            return RiskLevel switch
            {
                ConsequenceRisk.Low => "🟢",
                ConsequenceRisk.Medium => "🟡",
                ConsequenceRisk.High => "🟠",
                ConsequenceRisk.Critical => "🔴",
                _ => "⚪"
            };
        }

        public int CalculateTotalRelationshipImpact(string characterId)
        {
            int total = 0;
            
            // Add direct relationship changes
            if (RelationshipChanges.ContainsKey(characterId))
            {
                total += RelationshipChanges[characterId];
            }
            
            // Add relationship impact calculations
            if (RelationshipImpact.PrimaryCharacter == characterId)
            {
                total += RelationshipImpact.TrustChange + RelationshipImpact.RespectChange + RelationshipImpact.PersonalConnectionChange;
            }
            
            // Add secondary effects
            if (RelationshipImpact.SecondaryEffects.ContainsKey(characterId))
            {
                total += RelationshipImpact.SecondaryEffects[characterId];
            }
            
            return total;
        }

        public List<string> GetConsequencePreview()
        {
            var preview = new List<string>();
            
            // Add immediate consequences
            foreach (var consequence in ImmediateConsequences)
            {
                preview.Add($"Immediate: {consequence.Description}");
            }
            
            // Add long-term consequences
            foreach (var consequence in LongTermConsequences)
            {
                var timing = consequence.TriggerQuarter.HasValue ? $"Q{consequence.TriggerQuarter}" : "Later";
                preview.Add($"{timing}: {consequence.Description}");
            }
            
            // Add relationship impact summary
            if (!string.IsNullOrEmpty(RelationshipImpact.ImpactDescription))
            {
                preview.Add($"Relationship: {RelationshipImpact.ImpactDescription}");
            }
            
            return preview;
        }

        public bool HasSignificantConsequences()
        {
            return RiskLevel >= ConsequenceRisk.Medium ||
                   ImmediateConsequences.Any(c => c.Severity >= ConsequenceRisk.Medium) ||
                   LongTermConsequences.Any(c => c.Severity >= ConsequenceRisk.Medium) ||
                   RelationshipImpact.PhaseTransitionPotential ||
                   Math.Abs(RelationshipImpact.TrustChange) >= 10 ||
                   Math.Abs(RelationshipImpact.RespectChange) >= 10;
        }

        public Dictionary<string, object> CalculateAllEffects(Company company, Dictionary<string, CharacterRelationship> relationships)
        {
            var effects = new Dictionary<string, object>(GameplayEffects);
            
            // Add relationship changes to effects
            foreach (var change in RelationshipChanges)
            {
                effects[$"relationship_{change.Key}"] = change.Value;
            }
            
            // Add consequence flags to effects
            if (ConsequenceFlags.Any())
            {
                effects["consequence_flags"] = ConsequenceFlags;
            }
            
            // Add story branch influences
            foreach (var influence in StoryBranchInfluence)
            {
                effects[$"story_branch_{influence.Key}"] = influence.Value;
            }
            
            return effects;
        }
    }

    public class NarrativeEvent
    {
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = string.Empty;
        
        [JsonPropertyName("eventType")]
        public NarrativeEventType EventType { get; set; } = NarrativeEventType.CharacterIntroduction;
        
        [JsonPropertyName("triggerQuarter")]
        public int TriggerQuarter { get; set; } = 1;
        
        [JsonPropertyName("triggerConditions")]
        public List<string> TriggerConditions { get; set; } = new List<string>();
        
        [JsonPropertyName("involvedCharacters")]
        public List<string> InvolvedCharacters { get; set; } = new List<string>();
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("dialogue")]
        public List<string> Dialogue { get; set; } = new List<string>();
        
        [JsonPropertyName("choices")]
        public List<DialogueChoice> Choices { get; set; } = new List<DialogueChoice>();
        
        [JsonPropertyName("gameplayEffects")]
        public Dictionary<string, object> GameplayEffects { get; set; } = new Dictionary<string, object>();
    }

    public class StoryEvent
    {
        public int Quarter { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MechanicType IntroducedMechanic { get; set; }
        public List<string> JoanDialogue { get; set; } = new List<string>();
        public string ObjectiveText { get; set; } = string.Empty;
        public Dictionary<string, object> EventData { get; set; } = new Dictionary<string, object>();
    }

    public class StoryScript
    {
        public static readonly Dictionary<int, StoryEvent> StoryEvents = new Dictionary<int, StoryEvent>
        {
            [1] = new StoryEvent
            {
                Quarter = 1,
                EventId = "company_takeover",
                Title = "Welcome to Your New Company!",
                Description = "You've just taken over MidCorp Industries, a mid-sized manufacturing company with experienced staff in key departments.",
                IntroducedMechanic = MechanicType.BasicOperations,
                JoanDialogue = new List<string>
                {
                    "Hello! I'm Secretary Joan, your personal assistant. Congratulations on your new position as CEO!",
                    "I'll be here to guide you through your first few quarters as you learn the ropes of corporate management.",
                    "Let's start with the basics. This is your executive dashboard where you can monitor company performance.",
                    "Your main stats are Capital (money), Market Share (%), Reputation, Morale, and Risk levels.",
                    "I've ensured we have key staff in Research, Marketing, and HR to keep the company running smoothly.",
                    "⚠️ IMPORTANT: Never let your employee count reach zero! Without human capital, the business cannot operate and you'll face immediate failure.",
                    "For now, focus on understanding these numbers and our current team. Click 'End Quarter' when you're ready to proceed."
                },
                ObjectiveText = "Learn the basic company stats and end your first quarter",
                EventData = new Dictionary<string, object>
                {
                    ["startingCapital"] = 750000,
                    ["startingEmployees"] = 3,
                    ["startingMarketShare"] = 8.5
                }
            },

            [2] = new StoryEvent
            {
                Quarter = 2,
                EventId = "first_hiring",
                Title = "Expanding the Team",
                Description = "The board wants you to grow the company. It's time to hire new talent to support expansion.",
                IntroducedMechanic = MechanicType.EmployeeHiring,
                JoanDialogue = new List<string>
                {
                    "Great job completing your first quarter! Now let's talk about growing your team.",
                    "The 'Hire New Employees' button opens our recruitment panel where you can review candidates.",
                    "Each candidate has different skills, experience levels, and salary requirements.",
                    "Your HR department's performance affects the quality of candidates you attract.",
                    "Try hiring 2-3 new employees this quarter. Remember, you can only refresh the candidate list 3 times per quarter!"
                },
                ObjectiveText = "Hire 2-3 new employees using the hiring panel",
                EventData = new Dictionary<string, object>
                {
                    ["targetHires"] = 3,
                    ["budgetIncrease"] = 50000
                }
            },

            [3] = new StoryEvent
            {
                Quarter = 3,
                EventId = "department_organization",
                Title = "Organizing Your Workforce",
                Description = "Your new hires need proper department assignments to be effective. Time to organize your workforce strategically.",
                IntroducedMechanic = MechanicType.DepartmentManagement,
                JoanDialogue = new List<string>
                {
                    "Excellent hiring! Now we need to assign these employees to departments where they'll be most effective.",
                    "Click on any department button to open the Department Panel and manage employee assignments.",
                    "Each employee has skills that make them better suited for certain departments.",
                    "Look at their position descriptions and skill keywords to make smart assignments.",
                    "Remember: Your workforce is your most valuable asset. Losing all employees means immediate business failure!",
                    "A well-organized workforce is the foundation of a successful company!"
                },
                ObjectiveText = "Assign all unassigned employees to appropriate departments",
                EventData = new Dictionary<string, object>
                {
                    ["focusDepartments"] = new[] { "Marketing", "Operations", "HR" }
                }
            },

            [4] = new StoryEvent
            {
                Quarter = 4,
                EventId = "strategic_decisions",
                Title = "Executive Decision Making",
                Description = "The company needs strategic direction. As CEO, you must make key decisions about investments and company direction.",
                IntroducedMechanic = MechanicType.ExecutiveDecisions,
                JoanDialogue = new List<string>
                {
                    "Now that your team is organized, it's time to make some strategic decisions!",
                    "The 'Executive Decisions' panel gives you powerful tools to shape your company's future.",
                    "You can cut costs, give bonuses, launch marketing campaigns, and allocate budgets.",
                    "Each decision has trade-offs - bonuses improve morale but cost money, marketing increases reputation but raises risk.",
                    "Try launching a marketing campaign this quarter to boost your company's reputation!"
                },
                ObjectiveText = "Use Executive Decisions to launch a marketing campaign",
                EventData = new Dictionary<string, object>
                {
                    ["recommendedAction"] = "marketing_campaign",
                    ["budgetBonus"] = 25000
                }
            },

            [5] = new StoryEvent
            {
                Quarter = 5,
                EventId = "budget_management",
                Title = "Financial Planning",
                Description = "Proper budget allocation across departments is crucial for balanced growth and operational efficiency.",
                IntroducedMechanic = MechanicType.FinancialManagement,
                JoanDialogue = new List<string>
                {
                    "Let's talk about financial management - the backbone of any successful company.",
                    "In the Executive Decisions panel, you'll find budget allocation sliders for each department.",
                    "Different departments need different levels of investment based on your strategy.",
                    "Marketing drives reputation, Operations improves efficiency, Finance manages costs, HR attracts talent.",
                    "Try adjusting your budget allocation to support your company's growth strategy!"
                },
                ObjectiveText = "Adjust department budget allocations in Executive Decisions",
                EventData = new Dictionary<string, object>
                {
                    ["recommendedAllocation"] = new Dictionary<string, double>
                    {
                        ["Marketing"] = 20.0,
                        ["Operations"] = 25.0,
                        ["Finance"] = 15.0,
                        ["HR"] = 15.0,
                        ["IT"] = 15.0,
                        ["Research"] = 10.0
                    }
                }
            },

            [6] = new StoryEvent
            {
                Quarter = 6,
                EventId = "first_crisis",
                Title = "Crisis Management",
                Description = "A major supplier has failed to deliver critical components. How you handle this crisis will define your leadership.",
                IntroducedMechanic = MechanicType.CrisisManagement,
                JoanDialogue = new List<string>
                {
                    "Oh no! We're facing our first major crisis. A key supplier has let us down.",
                    "This is where crisis management skills become crucial. Notice how your Risk level affects these situations.",
                    "The Quarterly Summary will show you what happened and how it impacted the company.",
                    "Your crisis response strategy (in the control knobs) determines how you handle these situations.",
                    "⚠️ Be careful during crises - they can sometimes lead to employee layoffs or resignations!",
                    "Don't panic - every CEO faces crises. It's how you respond that matters!"
                },
                ObjectiveText = "Navigate the supply chain crisis and maintain company stability",
                EventData = new Dictionary<string, object>
                {
                    ["crisisType"] = "supply_chain",
                    ["impactLevel"] = "moderate",
                    ["responseOptions"] = new[] { "immediate", "control", "absorb" }
                }
            },

            [7] = new StoryEvent
            {
                Quarter = 7,
                EventId = "advanced_hr_management",
                Title = "Advanced Human Resources",
                Description = "Managing employee performance becomes critical as your company grows. Learn to make tough decisions about underperforming staff.",
                IntroducedMechanic = MechanicType.AdvancedHR,
                JoanDialogue = new List<string>
                {
                    "Now that you've mastered the basics, let's talk about advanced HR management.",
                    "Sometimes, despite our best efforts, employees don't meet expectations or cause problems.",
                    "You now have the ability to fire employees through the Department Management panels.",
                    "⚠️ Be very careful! Firing employees affects team morale and you must never have zero employees.",
                    "Use this power wisely - fire only those who are truly underperforming or causing issues.",
                    "Remember: A well-managed team is more valuable than a large team with poor performers."
                },
                ObjectiveText = "Learn about employee firing system and maintain team performance",
                EventData = new Dictionary<string, object>
                {
                    ["focusArea"] = "employee_management",
                    ["newFeature"] = "employee_firing"
                }
            },

            [8] = new StoryEvent
            {
                Quarter = 8,
                EventId = "market_analysis_mastery",
                Title = "Market Analysis & Competition",
                Description = "A major competitor threatens your market position. Master advanced market analysis and competitive strategy.",
                IntroducedMechanic = MechanicType.MarketAnalysis,
                JoanDialogue = new List<string>
                {
                    "Alert! A major competitor has launched an aggressive campaign against us.",
                    "This is where advanced market analysis becomes crucial for survival.",
                    "Study your control knobs carefully - each setting affects how you compete.",
                    "Market Strategy, Risk Appetite, and Budget Allocation all work together.",
                    "Use your Executive Decisions panel to launch counter-strategies.",
                    "Remember: it's not just about reacting, it's about strategic positioning!"
                },
                ObjectiveText = "Defend against competitor threat and maintain market position",
                EventData = new Dictionary<string, object>
                {
                    ["competitorName"] = "MegaCorp Industries",
                    ["marketShareThreat"] = 3.0,
                    ["recommendedActions"] = new[] { "marketing_campaign", "rd_investment", "strategic_positioning" }
                }
            },

            [9] = new StoryEvent
            {
                Quarter = 9,
                EventId = "risk_management_systems",
                Title = "Risk Management Systems",
                Description = "Multiple challenges emerge simultaneously. Learn to assess and mitigate various types of business risks.",
                IntroducedMechanic = MechanicType.RiskManagement,
                JoanDialogue = new List<string>
                {
                    "This quarter brings multiple challenges - a true test of risk management skills.",
                    "Notice how your Risk Appetite setting affects every aspect of your business.",
                    "Conservative approaches reduce risk but may limit growth opportunities.",
                    "Aggressive strategies can accelerate growth but increase vulnerability.",
                    "Your Crisis Response setting determines how you handle unexpected events.",
                    "Master the balance between risk and reward - that's the key to sustainable success!"
                },
                ObjectiveText = "Navigate multiple business risks while maintaining growth",
                EventData = new Dictionary<string, object>
                {
                    ["riskTypes"] = new[] { "financial", "operational", "market", "regulatory" },
                    ["challengeLevel"] = "high",
                    ["learningFocus"] = "risk_assessment"
                }
            },

            [10] = new StoryEvent
            {
                Quarter = 10,
                EventId = "strategic_mastery",
                Title = "Strategic Mastery & Graduation",
                Description = "Your final tutorial challenge: demonstrate mastery of all corporate management concepts through complex strategic decision-making.",
                IntroducedMechanic = MechanicType.AdvancedStrategy,
                JoanDialogue = new List<string>
                {
                    "Congratulations on reaching your final tutorial quarter!",
                    "This is your graduation exam - you'll face complex, interconnected challenges.",
                    "Everything you've learned comes together: hiring, firing, budgeting, crisis management, and strategic thinking.",
                    "Your goal is to achieve sustainable growth while maintaining high employee satisfaction.",
                    "After this quarter, the full ChaosEngine activates - no more training wheels!",
                    "I believe you're ready to become a true corporate leader. Show me what you've learned!"
                },
                ObjectiveText = "Achieve 15% market share, 70+ morale, and positive capital growth",
                EventData = new Dictionary<string, object>
                {
                    ["targetMarketShare"] = 15.0,
                    ["targetMorale"] = 70,
                    ["targetCapitalGrowth"] = true,
                    ["graduationThreshold"] = true,
                    ["finalChallenge"] = true
                }
            }
        };

        public static readonly Dictionary<MechanicType, string> MechanicDescriptions = new Dictionary<MechanicType, string>
        {
            [MechanicType.BasicOperations] = "Understanding company stats and basic operations",
            [MechanicType.EmployeeHiring] = "Recruiting and hiring new talent",
            [MechanicType.DepartmentManagement] = "Organizing employees into effective departments",
            [MechanicType.ExecutiveDecisions] = "Making strategic business decisions",
            [MechanicType.FinancialManagement] = "Managing budgets and financial allocation",
            [MechanicType.CrisisManagement] = "Handling unexpected challenges and crises",
            [MechanicType.AdvancedHR] = "Employee performance management and firing decisions",
            [MechanicType.MarketAnalysis] = "Competitive analysis and market positioning",
            [MechanicType.RiskManagement] = "Risk assessment and mitigation strategies",
            [MechanicType.AdvancedStrategy] = "Complex strategic thinking and long-term planning"
        };

        // Character definitions for the expanded story system
        public static readonly Dictionary<string, StoryCharacter> Characters = new Dictionary<string, StoryCharacter>
        {
            ["joan"] = new StoryCharacter
            {
                CharacterId = "joan",
                Name = "Secretary Joan",
                Role = "Personal Corporate Assistant",
                PersonalityTraits = new List<string> { "professional", "supportive", "knowledgeable", "gradually_personal" },
                IntroductionQuarter = 1,
                CharacterArcMilestones = new List<string> 
                { 
                    "professional_assistant", "trusted_advisor", "personal_confidant", "lifelong_friend" 
                }
            },
            ["marcus_vey"] = new StoryCharacter
            {
                CharacterId = "marcus_vey",
                Name = "Marcus Vey",
                Role = "Chief Financial Officer",
                PersonalityTraits = new List<string> { "shrewd", "numbers_driven", "impatient", "risk_loving" },
                IntroductionQuarter = 15,
                CharacterArcMilestones = new List<string> 
                { 
                    "ambitious_newcomer", "trusted_advisor", "strategic_partner_or_rival" 
                }
            },
            ["evelyn_cross"] = new StoryCharacter
            {
                CharacterId = "evelyn_cross",
                Name = "Evelyn Cross",
                Role = "Head of Human Resources",
                PersonalityTraits = new List<string> { "empathetic", "organized", "protective_of_employees" },
                IntroductionQuarter = 20,
                CharacterArcMilestones = new List<string> 
                { 
                    "cautious_professional", "employee_advocate", "cultural_guardian" 
                }
            },
            ["vincent_duro"] = new StoryCharacter
            {
                CharacterId = "vincent_duro",
                Name = "Vincent Duro",
                Role = "Rival CEO",
                PersonalityTraits = new List<string> { "aggressive", "cunning", "publicly_charming", "privately_cutthroat" },
                IntroductionQuarter = 25,
                CharacterArcMilestones = new List<string> 
                { 
                    "distant_competitor", "direct_rival", "nemesis_or_respected_opponent" 
                }
            },
            ["lucinda_vale"] = new StoryCharacter
            {
                CharacterId = "lucinda_vale",
                Name = "Lucinda Vale",
                Role = "PR & Marketing Head",
                PersonalityTraits = new List<string> { "creative", "persuasive", "flamboyant", "headline_focused" },
                IntroductionQuarter = 30,
                CharacterArcMilestones = new List<string> 
                { 
                    "enthusiastic_marketer", "brand_strategist", "public_face" 
                }
            },
            ["gregory_shaw"] = new StoryCharacter
            {
                CharacterId = "gregory_shaw",
                Name = "Gregory Shaw",
                Role = "Operations Manager",
                PersonalityTraits = new List<string> { "calm", "methodical", "numbers_focused", "cynical" },
                IntroductionQuarter = 35,
                CharacterArcMilestones = new List<string> 
                { 
                    "steady_manager", "efficiency_expert", "operational_backbone" 
                }
            },
            ["selena_park"] = new StoryCharacter
            {
                CharacterId = "selena_park",
                Name = "Selena Park",
                Role = "Venture Capitalist",
                PersonalityTraits = new List<string> { "persuasive", "strategic", "roi_focused" },
                IntroductionQuarter = 40,
                CharacterArcMilestones = new List<string> 
                { 
                    "potential_investor", "financial_partner", "buyout_opportunity" 
                }
            },
            ["harold_finch"] = new StoryCharacter
            {
                CharacterId = "harold_finch",
                Name = "Harold Finch",
                Role = "Legal Counsel",
                PersonalityTraits = new List<string> { "precise", "pedantic", "highly_cautious" },
                IntroductionQuarter = 45,
                CharacterArcMilestones = new List<string> 
                { 
                    "risk_averse_lawyer", "trusted_advisor", "strategic_protector" 
                }
            },
            ["sophie_kim"] = new StoryCharacter
            {
                CharacterId = "sophie_kim",
                Name = "Sophie Kim",
                Role = "Junior Analyst",
                PersonalityTraits = new List<string> { "enthusiastic", "naive", "data_loving" },
                IntroductionQuarter = 12,
                CharacterArcMilestones = new List<string> 
                { 
                    "eager_intern", "valuable_analyst", "protege_or_successor" 
                }
            }
        };

        // Helper methods for narrative act management
        public static NarrativeAct GetNarrativeActForQuarter(int quarter)
        {
            return quarter switch
            {
                <= 10 => NarrativeAct.Tutorial,
                <= 60 => NarrativeAct.RisingAction,
                <= 100 => NarrativeAct.Climax,
                _ => NarrativeAct.Resolution
            };
        }

        public static RelationshipPhase GetJoanPhaseForQuarter(int quarter)
        {
            return quarter switch
            {
                <= 10 => RelationshipPhase.ProfessionalAcquaintance,
                <= 40 => RelationshipPhase.TrustedColleague,
                <= 80 => RelationshipPhase.PersonalFriend,
                _ => RelationshipPhase.LifelongBond
            };
        }
    }
}