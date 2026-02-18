using CorporateChaos.Models;
using System.Text.Json;

namespace CorporateChaos.Systems
{
    public class DialogueSystem
    {
        private ExtendedStoryModeData storyData;
        private Company company;
        private CharacterManager characterManager;
        private Random random = new Random();

        public DialogueSystem(ExtendedStoryModeData storyData, Company company, CharacterManager characterManager)
        {
            this.storyData = storyData;
            this.company = company;
            this.characterManager = characterManager;
        }

        public DialogueConversation CreateConversation(string characterId, string context)
        {
            var conversation = new DialogueConversation
            {
                ConversationId = $"{characterId}_{context}_{DateTime.Now.Ticks}",
                Title = $"Conversation with {StoryScript.Characters[characterId].Name}",
                Participants = new List<string> { "player", characterId },
                StartNodeId = "start",
                CurrentNodeId = "start"
            };

            // Generate dialogue nodes based on character and context
            GenerateDialogueNodes(conversation, characterId, context);

            return conversation;
        }

        private void GenerateDialogueNodes(DialogueConversation conversation, string characterId, string context)
        {
            var character = StoryScript.Characters[characterId];
            var relationship = storyData.CharacterRelationships[characterId];

            // Create start node with relationship-adapted dialogue
            var startNode = new DialogueNode
            {
                NodeId = "start",
                CharacterId = characterId,
                DialogueText = GetAdaptedDialogue(characterId, context, relationship),
                ContextTags = new List<string> { context, "introduction" },
                EmotionalTone = DetermineEmotionalTone(characterId, relationship, context),
                RelationshipContext = new Dictionary<string, int>
                {
                    ["trust"] = relationship.TrustLevel,
                    ["respect"] = relationship.ProfessionalRespect,
                    ["personal"] = relationship.PersonalConnection
                }
            };

            // Add adaptive text based on relationship levels and story flags
            AddAdaptiveTextVariations(startNode, characterId, relationship);

            // Generate choices based on relationship level and character personality
            startNode.Choices = GenerateChoicesForContext(characterId, context, relationship);

            conversation.Nodes["start"] = startNode;
        }

        private string GetAdaptedDialogue(string characterId, string context, CharacterRelationship relationship)
        {
            // Get base dialogue from character-specific dialogue system
            var baseDialogue = CharacterDialogue.GetCharacterDialogue(characterId, context, company, random);
            
            // Adapt dialogue based on relationship level and phase
            return AdaptDialogueForRelationship(characterId, baseDialogue, relationship, context);
        }

        private string AdaptDialogueForRelationship(string characterId, string baseDialogue, CharacterRelationship relationship, string context)
        {
            var adaptedDialogue = baseDialogue;
            
            // Apply relationship-based modifications
            switch (relationship.CurrentPhase)
            {
                case RelationshipPhase.FirstMeeting:
                    adaptedDialogue = AddFormalTone(characterId, adaptedDialogue);
                    break;
                    
                case RelationshipPhase.ProfessionalAcquaintance:
                    adaptedDialogue = AddProfessionalContext(characterId, adaptedDialogue, relationship);
                    break;
                    
                case RelationshipPhase.TrustedColleague:
                    adaptedDialogue = AddTrustedColleagueContext(characterId, adaptedDialogue, relationship);
                    break;
                    
                case RelationshipPhase.PersonalFriend:
                    adaptedDialogue = AddPersonalFriendContext(characterId, adaptedDialogue, relationship);
                    break;
                    
                case RelationshipPhase.LifelongBond:
                    adaptedDialogue = AddLifelongBondContext(characterId, adaptedDialogue, relationship);
                    break;
                    
                case RelationshipPhase.Strained:
                    adaptedDialogue = AddStrainedRelationshipContext(characterId, adaptedDialogue, relationship);
                    break;
                    
                case RelationshipPhase.Hostile:
                    adaptedDialogue = AddHostileContext(characterId, adaptedDialogue, relationship);
                    break;
            }
            
            // Apply context-specific adaptations
            adaptedDialogue = ApplyContextualAdaptations(characterId, adaptedDialogue, context, relationship);
            
            return adaptedDialogue;
        }

        private string AddFormalTone(string characterId, string dialogue)
        {
            // Add formal introductory elements for first meetings
            var character = StoryScript.Characters[characterId];
            return characterId switch
            {
                "joan" => $"Good morning! I'm {character.Name}, your {character.Role}. {dialogue}",
                "marcus_vey" => $"Mr. Vey here, your new {character.Role}. {dialogue}",
                "evelyn_cross" => $"Hello, I'm {character.Name} from {character.Role}. {dialogue}",
                _ => $"I'm {character.Name}. {dialogue}"
            };
        }

        private string AddProfessionalContext(string characterId, string dialogue, CharacterRelationship relationship)
        {
            // Add professional relationship context
            if (relationship.TrustLevel > 20)
            {
                return characterId switch
                {
                    "joan" => $"I've been observing your management style, and I think you should know: {dialogue}",
                    "marcus_vey" => $"Based on our previous discussions about financials: {dialogue}",
                    "evelyn_cross" => $"Given our ongoing work on HR matters: {dialogue}",
                    _ => $"As we've been working together: {dialogue}"
                };
            }
            return dialogue;
        }

        private string AddTrustedColleagueContext(string characterId, string dialogue, CharacterRelationship relationship)
        {
            // Add trusted colleague relationship context
            return characterId switch
            {
                "joan" => $"I feel comfortable being direct with you now: {dialogue}",
                "marcus_vey" => $"Since we've built a good working relationship: {dialogue}",
                "evelyn_cross" => $"I trust your judgment on people matters, so: {dialogue}",
                "vincent_duro" => $"I respect what you've accomplished, though: {dialogue}",
                _ => $"I've come to respect your leadership: {dialogue}"
            };
        }

        private string AddPersonalFriendContext(string characterId, string dialogue, CharacterRelationship relationship)
        {
            // Add personal friend relationship context
            return characterId switch
            {
                "joan" => $"You know, after all we've been through together: {dialogue}",
                "marcus_vey" => $"I'll be honest with you as a friend: {dialogue}",
                "evelyn_cross" => $"Speaking as someone who genuinely cares about you: {dialogue}",
                _ => $"As your friend, I need to tell you: {dialogue}"
            };
        }

        private string AddLifelongBondContext(string characterId, string dialogue, CharacterRelationship relationship)
        {
            // Add lifelong bond relationship context
            return characterId switch
            {
                "joan" => $"After all these years together, I can say with certainty: {dialogue}",
                "marcus_vey" => $"We've been through so much together, and I believe: {dialogue}",
                "evelyn_cross" => $"You're like family to me now, so I hope you understand: {dialogue}",
                _ => $"Our long partnership has taught me: {dialogue}"
            };
        }

        private string AddStrainedRelationshipContext(string characterId, string dialogue, CharacterRelationship relationship)
        {
            // Add strained relationship context
            return characterId switch
            {
                "joan" => $"I'm trying to remain professional despite our recent... difficulties: {dialogue}",
                "marcus_vey" => $"Look, I may not agree with your recent decisions, but: {dialogue}",
                "evelyn_cross" => $"I'm concerned about the direction we're heading, and: {dialogue}",
                "vincent_duro" => $"Your recent moves have been... interesting: {dialogue}",
                _ => $"Given our recent disagreements: {dialogue}"
            };
        }

        private string AddHostileContext(string characterId, string dialogue, CharacterRelationship relationship)
        {
            // Add hostile relationship context
            return characterId switch
            {
                "joan" => $"I'm obligated to inform you, though I question your judgment: {dialogue}",
                "marcus_vey" => $"Against my better judgment, I'm telling you: {dialogue}",
                "evelyn_cross" => $"I strongly disagree with your methods, but: {dialogue}",
                "vincent_duro" => $"You've made this personal, but business is business: {dialogue}",
                _ => $"Despite our conflicts: {dialogue}"
            };
        }

        private string ApplyContextualAdaptations(string characterId, string dialogue, string context, CharacterRelationship relationship)
        {
            // Apply context-specific adaptations based on current story state
            var currentQuarter = storyData.CurrentQuarter;
            var narrativeAct = StoryScript.GetNarrativeActForQuarter(currentQuarter);
            
            // Adapt based on narrative act
            switch (narrativeAct)
            {
                case NarrativeAct.Tutorial:
                    dialogue = AddTutorialContext(characterId, dialogue, context);
                    break;
                case NarrativeAct.RisingAction:
                    dialogue = AddRisingActionContext(characterId, dialogue, context, relationship);
                    break;
                case NarrativeAct.Climax:
                    dialogue = AddClimaxContext(characterId, dialogue, context, relationship);
                    break;
                case NarrativeAct.Resolution:
                    dialogue = AddResolutionContext(characterId, dialogue, context, relationship);
                    break;
            }
            
            // Apply company performance context
            dialogue = ApplyCompanyPerformanceContext(characterId, dialogue, context, relationship);
            
            return dialogue;
        }

        private string AddTutorialContext(string characterId, string dialogue, string context)
        {
            // Add tutorial-specific context for guidance
            if (characterId == "joan")
            {
                return context switch
                {
                    "crisis_management" => $"Don't worry, this is a learning experience. {dialogue} I'll guide you through this.",
                    "performance_review" => $"Let me help you understand what these numbers mean. {dialogue}",
                    _ => $"As we're still learning together: {dialogue}"
                };
            }
            return dialogue;
        }

        private string AddRisingActionContext(string characterId, string dialogue, string context, CharacterRelationship relationship)
        {
            // Add rising action context for increased stakes
            return characterId switch
            {
                "joan" => $"The stakes are getting higher now. {dialogue}",
                "marcus_vey" => $"This is where we separate the winners from the losers. {dialogue}",
                "vincent_duro" => $"The real competition is just beginning. {dialogue}",
                _ => $"Things are getting more complex now. {dialogue}"
            };
        }

        private string AddClimaxContext(string characterId, string dialogue, string context, CharacterRelationship relationship)
        {
            // Add climax context for high-stakes decisions
            return characterId switch
            {
                "joan" => $"This is a critical moment for our company. {dialogue}",
                "marcus_vey" => $"Everything we've built is on the line. {dialogue}",
                "evelyn_cross" => $"The team is looking to you for leadership. {dialogue}",
                _ => $"This decision could define our future. {dialogue}"
            };
        }

        private string AddResolutionContext(string characterId, string dialogue, string context, CharacterRelationship relationship)
        {
            // Add resolution context for legacy and reflection
            return characterId switch
            {
                "joan" => $"Looking back on our journey together: {dialogue}",
                "marcus_vey" => $"After all we've accomplished: {dialogue}",
                "evelyn_cross" => $"Reflecting on what we've built: {dialogue}",
                _ => $"As we near the end of this chapter: {dialogue}"
            };
        }

        private string ApplyCompanyPerformanceContext(string characterId, string dialogue, string context, CharacterRelationship relationship)
        {
            // Adapt dialogue based on current company performance
            if (company.MarketShare > 50)
            {
                dialogue = AddMarketLeaderContext(characterId, dialogue);
            }
            else if (company.ConsecutiveNegativeQuarters > 0)
            {
                dialogue = AddCrisisContext(characterId, dialogue);
            }
            else if (company.Capital > 500000000)
            {
                dialogue = AddWealthyCompanyContext(characterId, dialogue);
            }
            
            return dialogue;
        }

        private string AddMarketLeaderContext(string characterId, string dialogue)
        {
            return characterId switch
            {
                "joan" => $"Given our market leadership position: {dialogue}",
                "marcus_vey" => $"Now that we're market leaders: {dialogue}",
                "vincent_duro" => $"You've reached the top, but: {dialogue}",
                _ => $"As industry leaders: {dialogue}"
            };
        }

        private string AddCrisisContext(string characterId, string dialogue)
        {
            return characterId switch
            {
                "joan" => $"I know times are tough right now, but: {dialogue}",
                "marcus_vey" => $"We're in crisis mode, so: {dialogue}",
                "evelyn_cross" => $"The team is worried, and: {dialogue}",
                _ => $"During these difficult times: {dialogue}"
            };
        }

        private string AddWealthyCompanyContext(string characterId, string dialogue)
        {
            return characterId switch
            {
                "selena_park" => $"With your impressive valuation: {dialogue}",
                "marcus_vey" => $"Our financial success opens new doors: {dialogue}",
                _ => $"Given our strong financial position: {dialogue}"
            };
        }

        private EmotionalTone DetermineEmotionalTone(string characterId, CharacterRelationship relationship, string context)
        {
            // Determine emotional tone based on character, relationship, and context
            if (relationship.CurrentPhase == RelationshipPhase.Hostile)
                return EmotionalTone.Angry;
            
            if (relationship.CurrentPhase == RelationshipPhase.Strained)
                return EmotionalTone.Concerned;
            
            if (company.ConsecutiveNegativeQuarters > 0)
                return EmotionalTone.Worried;
            
            if (relationship.PersonalConnection > 60)
                return EmotionalTone.Warm;
            
            return characterId switch
            {
                "marcus_vey" => company.Risk > 50 ? EmotionalTone.Excited : EmotionalTone.Serious,
                "evelyn_cross" => company.Morale < 30 ? EmotionalTone.Concerned : EmotionalTone.Supportive,
                "vincent_duro" => company.MarketShare > 40 ? EmotionalTone.Competitive : EmotionalTone.Neutral,
                "lucinda_vale" => EmotionalTone.Enthusiastic,
                "sophie_kim" => EmotionalTone.Excited,
                _ => EmotionalTone.Professional
            };
        }

        private void AddAdaptiveTextVariations(DialogueNode node, string characterId, CharacterRelationship relationship)
        {
            // Add adaptive text variations based on relationship milestones and story flags
            
            // Trust-based variations
            if (relationship.TrustLevel >= 50)
            {
                node.AdaptiveText[$"relationship:{characterId}:trust:50"] = 
                    GetHighTrustDialogueVariation(characterId, node.DialogueText);
            }
            
            if (relationship.TrustLevel <= -30)
            {
                node.AdaptiveText[$"relationship:{characterId}:trust:-30"] = 
                    GetLowTrustDialogueVariation(characterId, node.DialogueText);
            }
            
            // Personal connection variations
            if (relationship.PersonalConnection >= 60)
            {
                node.AdaptiveText[$"relationship:{characterId}:personal:60"] = 
                    GetPersonalConnectionDialogueVariation(characterId, node.DialogueText);
            }
            
            // Professional respect variations
            if (relationship.ProfessionalRespect >= 70)
            {
                node.AdaptiveText[$"relationship:{characterId}:respect:70"] = 
                    GetHighRespectDialogueVariation(characterId, node.DialogueText);
            }
            
            // Story flag variations
            if (storyData.StoryFlags.Contains("first_crisis_handled"))
            {
                node.AdaptiveText["flag:first_crisis_handled"] = 
                    GetPostCrisisDialogueVariation(characterId, node.DialogueText);
            }
            
            if (storyData.StoryFlags.Contains("market_leader"))
            {
                node.AdaptiveText["flag:market_leader"] = 
                    GetMarketLeaderDialogueVariation(characterId, node.DialogueText);
            }
        }

        private string GetHighTrustDialogueVariation(string characterId, string baseDialogue)
        {
            return characterId switch
            {
                "joan" => "I trust you completely, so I'll be completely honest: " + baseDialogue,
                "marcus_vey" => "Since I trust your judgment: " + baseDialogue,
                "evelyn_cross" => "I have complete faith in your leadership: " + baseDialogue,
                _ => "I trust you with this information: " + baseDialogue
            };
        }

        private string GetLowTrustDialogueVariation(string characterId, string baseDialogue)
        {
            return characterId switch
            {
                "joan" => "I'm obligated to tell you, though I have concerns: " + baseDialogue,
                "marcus_vey" => "I question this approach, but: " + baseDialogue,
                "evelyn_cross" => "I'm worried about your decision-making: " + baseDialogue,
                _ => "I have reservations, but: " + baseDialogue
            };
        }

        private string GetPersonalConnectionDialogueVariation(string characterId, string baseDialogue)
        {
            return characterId switch
            {
                "joan" => "You know how much I care about you and this company: " + baseDialogue,
                "marcus_vey" => "As someone who's grown to care about you personally: " + baseDialogue,
                "evelyn_cross" => "Speaking as a friend who genuinely cares: " + baseDialogue,
                _ => "On a personal level: " + baseDialogue
            };
        }

        private string GetHighRespectDialogueVariation(string characterId, string baseDialogue)
        {
            return characterId switch
            {
                "joan" => "Your leadership has been exceptional: " + baseDialogue,
                "marcus_vey" => "I have tremendous respect for your business acumen: " + baseDialogue,
                "evelyn_cross" => "Your people skills are remarkable: " + baseDialogue,
                "vincent_duro" => "I respect what you've built here: " + baseDialogue,
                _ => "I have great respect for your abilities: " + baseDialogue
            };
        }

        private string GetPostCrisisDialogueVariation(string characterId, string baseDialogue)
        {
            return characterId switch
            {
                "joan" => "After how well you handled our last crisis: " + baseDialogue,
                "marcus_vey" => "Your crisis management was impressive: " + baseDialogue,
                "evelyn_cross" => "The way you protected the team during the crisis: " + baseDialogue,
                _ => "Given how you handled the last crisis: " + baseDialogue
            };
        }

        private string GetMarketLeaderDialogueVariation(string characterId, string baseDialogue)
        {
            return characterId switch
            {
                "joan" => "Now that we're market leaders: " + baseDialogue,
                "marcus_vey" => "Our market dominance changes everything: " + baseDialogue,
                "vincent_duro" => "You've beaten me to the top: " + baseDialogue,
                _ => "As the market leader: " + baseDialogue
            };
        }

        private List<DialogueChoice> GenerateChoicesForContext(string characterId, string context, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            // Generate relationship-appropriate choices
            choices.AddRange(GetRelationshipBasedChoices(characterId, relationship));
            
            // Add context-specific choices
            choices.AddRange(GetContextSpecificChoices(characterId, context, relationship));
            
            // Filter choices based on relationship level and story flags
            var filteredChoices = FilterChoicesForRelationship(choices, characterId, relationship);
            
            return filteredChoices;
        }

        private List<DialogueChoice> GetRelationshipBasedChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            // Always available professional choice
            choices.Add(CreateProfessionalChoice(characterId, relationship));
            
            // Trust-based choices
            if (relationship.TrustLevel >= 30)
            {
                choices.Add(CreateTrustBasedChoice(characterId, relationship));
            }
            
            // Personal connection choices
            if (relationship.PersonalConnection >= 40)
            {
                choices.Add(CreatePersonalChoice(characterId, relationship));
            }
            
            // Aggressive choices (available when relationship is strong or hostile)
            if (relationship.ProfessionalRespect >= 50 || relationship.CurrentPhase == RelationshipPhase.Hostile)
            {
                choices.Add(CreateAggressiveChoice(characterId, relationship));
            }
            
            // Diplomatic choices (always available but more effective with higher relationships)
            choices.Add(CreateDiplomaticChoice(characterId, relationship));
            
            return choices;
        }

        private List<DialogueChoice> GetContextSpecificChoices(string characterId, string context, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            switch (context)
            {
                case "crisis_management":
                    choices.AddRange(GetCrisisManagementChoices(characterId, relationship));
                    break;
                case "performance_review":
                    choices.AddRange(GetPerformanceReviewChoices(characterId, relationship));
                    break;
                case "employee_concerns":
                    choices.AddRange(GetEmployeeConcernChoices(characterId, relationship));
                    break;
                case "investment_opportunity":
                    choices.AddRange(GetInvestmentChoices(characterId, relationship));
                    break;
                default:
                    choices.AddRange(GetGeneralChoices(characterId, relationship));
                    break;
            }
            
            return choices;
        }

        private DialogueChoice CreateProfessionalChoice(string characterId, CharacterRelationship relationship)
        {
            return new DialogueChoice
            {
                ChoiceId = "professional_response",
                ChoiceText = "Let's approach this systematically and professionally.",
                Tone = ChoiceTone.Professional,
                ToneDescription = "Business-focused and methodical",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 3,
                    TrustChange = 1,
                    ImpactDescription = "Demonstrates professionalism and reliability"
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Maintains professional standards and reduces risk",
                        Type = ConsequenceType.Business,
                        Severity = ConsequenceRisk.Low
                    }
                }
            };
        }

        private DialogueChoice CreateTrustBasedChoice(string characterId, CharacterRelationship relationship)
        {
            return new DialogueChoice
            {
                ChoiceId = "trust_based_response",
                ChoiceText = "I value your expertise and want your honest opinion on this.",
                Tone = ChoiceTone.Supportive,
                ToneDescription = "Shows trust and values their input",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    TrustChange = 5,
                    PersonalConnectionChange = 3,
                    ImpactDescription = "Strengthens trust and shows respect for their expertise"
                },
                RequiresConditions = new List<string> { $"relationship:{characterId}:trust:30" }
            };
        }

        private DialogueChoice CreatePersonalChoice(string characterId, CharacterRelationship relationship)
        {
            return new DialogueChoice
            {
                ChoiceId = "personal_response",
                ChoiceText = "I appreciate you being here for me. What do you think we should do?",
                Tone = ChoiceTone.Personal,
                ToneDescription = "Personal and heartfelt approach",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    PersonalConnectionChange = 7,
                    TrustChange = 3,
                    ImpactDescription = "Deepens personal bond and shows vulnerability"
                },
                RequiresConditions = new List<string> { $"relationship:{characterId}:personal:40" }
            };
        }

        private DialogueChoice CreateAggressiveChoice(string characterId, CharacterRelationship relationship)
        {
            var choiceText = relationship.CurrentPhase == RelationshipPhase.Hostile 
                ? "We need to settle this once and for all."
                : "I'm taking decisive action on this matter.";
                
            return new DialogueChoice
            {
                ChoiceId = "aggressive_response",
                ChoiceText = choiceText,
                Tone = ChoiceTone.Aggressive,
                ToneDescription = "Direct and forceful approach",
                RiskLevel = ConsequenceRisk.Medium,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = characterId == "marcus_vey" ? 5 : -2,
                    TrustChange = -1,
                    ImpactDescription = characterId == "marcus_vey" 
                        ? "Marcus appreciates decisive leadership" 
                        : "May seem too aggressive for their personality"
                }
            };
        }

        private DialogueChoice CreateDiplomaticChoice(string characterId, CharacterRelationship relationship)
        {
            return new DialogueChoice
            {
                ChoiceId = "diplomatic_response",
                ChoiceText = "Let's find a solution that works for everyone involved.",
                Tone = ChoiceTone.Diplomatic,
                ToneDescription = "Collaborative and inclusive",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 4,
                    PersonalConnectionChange = 2,
                    ImpactDescription = "Shows wisdom and inclusive leadership",
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["evelyn_cross"] = 3, // HR appreciates collaboration
                        ["harold_finch"] = 2   // Legal likes careful approaches
                    }
                }
            };
        }

        private List<DialogueChoice> GetCrisisManagementChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "crisis_calm_approach",
                ChoiceText = "Let's stay calm and work through this step by step.",
                Tone = ChoiceTone.Professional,
                ToneDescription = "Calm and methodical crisis response",
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    TrustChange = 4,
                    RespectChange = 3
                }
            });
            
            if (relationship.PersonalConnection >= 30)
            {
                choices.Add(new DialogueChoice
                {
                    ChoiceId = "crisis_support_approach",
                    ChoiceText = "I know this is stressful. We'll get through this together.",
                    Tone = ChoiceTone.Supportive,
                    ToneDescription = "Empathetic and supportive",
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        PersonalConnectionChange = 6,
                        TrustChange = 2
                    },
                    RequiresConditions = new List<string> { $"relationship:{characterId}:personal:30" }
                });
            }
            
            return choices;
        }

        private List<DialogueChoice> GetPerformanceReviewChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "performance_analytical",
                ChoiceText = "Let's analyze the data and identify areas for improvement.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 4
                }
            });
            
            if (characterId == "sophie_kim")
            {
                choices.Add(new DialogueChoice
                {
                    ChoiceId = "performance_mentoring",
                    ChoiceText = "What insights have you discovered in the data, Sophie?",
                    Tone = ChoiceTone.Supportive,
                    ToneDescription = "Mentoring and encouraging",
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        PersonalConnectionChange = 5,
                        TrustChange = 3
                    }
                });
            }
            
            return choices;
        }

        private List<DialogueChoice> GetEmployeeConcernChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            if (characterId == "evelyn_cross")
            {
                choices.Add(new DialogueChoice
                {
                    ChoiceId = "employee_empathetic",
                    ChoiceText = "Employee wellbeing is our top priority. What do they need?",
                    Tone = ChoiceTone.Supportive,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        PersonalConnectionChange = 8,
                        TrustChange = 5,
                        SecondaryEffects = new Dictionary<string, int>
                        {
                            ["all_employees"] = 5
                        }
                    }
                });
            }
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "employee_balanced",
                ChoiceText = "We need to balance employee needs with business requirements.",
                Tone = ChoiceTone.Diplomatic,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 3,
                    TrustChange = 2
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> GetInvestmentChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            if (characterId == "marcus_vey")
            {
                choices.Add(new DialogueChoice
                {
                    ChoiceId = "investment_aggressive",
                    ChoiceText = "I like the high-risk, high-reward approach. Let's do it.",
                    Tone = ChoiceTone.Aggressive,
                    RiskLevel = ConsequenceRisk.High,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        RespectChange = 8,
                        TrustChange = 5,
                        PersonalConnectionChange = 3
                    }
                });
            }
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "investment_cautious",
                ChoiceText = "Let's analyze the risks more carefully before proceeding.",
                Tone = ChoiceTone.Professional,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = characterId == "harold_finch" ? 6 : 2,
                    TrustChange = 3
                }
            });
            
            return choices;
        }

        private List<DialogueChoice> GetGeneralChoices(string characterId, CharacterRelationship relationship)
        {
            var choices = new List<DialogueChoice>();
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "general_listen",
                ChoiceText = "I'm listening. Please tell me more.",
                Tone = ChoiceTone.Supportive,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    TrustChange = 3,
                    PersonalConnectionChange = 2
                }
            });
            
            choices.Add(new DialogueChoice
            {
                ChoiceId = "general_advice",
                ChoiceText = "What would you recommend in this situation?",
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

        private List<DialogueChoice> FilterChoicesForRelationship(List<DialogueChoice> choices, string characterId, CharacterRelationship relationship)
        {
            var filteredChoices = new List<DialogueChoice>();
            
            foreach (var choice in choices)
            {
                // Check if choice meets relationship requirements
                bool meetsRequirements = true;
                
                foreach (var condition in choice.RequiresConditions)
                {
                    if (!EvaluateChoiceCondition(condition, characterId, relationship))
                    {
                        meetsRequirements = false;
                        break;
                    }
                }
                
                if (meetsRequirements)
                {
                    filteredChoices.Add(choice);
                }
            }
            
            // Ensure we have at least 2 choices
            if (filteredChoices.Count < 2 && choices.Count >= 2)
            {
                // Add fallback choices
                var fallbackChoices = choices.Where(c => !filteredChoices.Contains(c))
                                           .Take(2 - filteredChoices.Count);
                filteredChoices.AddRange(fallbackChoices);
            }
            
            return filteredChoices.Take(4).ToList(); // Limit to 4 choices max
        }

        private bool EvaluateChoiceCondition(string condition, string characterId, CharacterRelationship relationship)
        {
            if (condition.StartsWith("relationship:"))
            {
                var parts = condition.Split(':');
                if (parts.Length == 4)
                {
                    var conditionCharacterId = parts[1];
                    var attribute = parts[2];
                    var threshold = int.Parse(parts[3]);
                    
                    if (conditionCharacterId == characterId)
                    {
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
            
            if (condition.StartsWith("flag:"))
            {
                var flagName = condition.Substring(5);
                return storyData.StoryFlags.Contains(flagName);
            }
            
            return true; // Default to true if condition can't be evaluated
        }

        public DialogueNode GetAdaptiveDialogueNode(string characterId, string context)
        {
            var relationship = storyData.CharacterRelationships[characterId];
            var activeFlags = storyData.StoryFlags;
            
            var node = new DialogueNode
            {
                NodeId = $"{characterId}_{context}_adaptive",
                CharacterId = characterId,
                DialogueText = GetAdaptedDialogue(characterId, context, relationship),
                EmotionalTone = DetermineEmotionalTone(characterId, relationship, context),
                ContextTags = new List<string> { context, "adaptive" }
            };
            
            // Add adaptive text variations
            AddAdaptiveTextVariations(node, characterId, relationship);
            
            // Get the final adapted dialogue text
            node.DialogueText = node.GetAdaptiveDialogueText(storyData.CharacterRelationships, activeFlags);
            
            // Generate adaptive choices
            node.Choices = GenerateChoicesForContext(characterId, context, relationship);
            
            return node;
        }
    }
}