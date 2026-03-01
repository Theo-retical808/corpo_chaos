using CorporateChaos.Models;
using System.Text.Json;

namespace CorporateChaos.Systems
{
    public class CharacterManager
    {
        private ExtendedStoryModeData storyData;
        private Company company;
        private EndingProbabilityTracker? endingTracker;

        public CharacterManager(ExtendedStoryModeData storyData, Company company)
        {
            this.storyData = storyData;
            this.company = company;
            InitializeCharacterRelationships();
            endingTracker = new EndingProbabilityTracker(storyData, company);
        }

        /// <summary>
        /// Gets the ending probability tracker for this character manager
        /// </summary>
        public EndingProbabilityTracker EndingTracker => endingTracker ?? new EndingProbabilityTracker(storyData, company);

        private void InitializeCharacterRelationships()
        {
            // Initialize relationships for all characters if they don't exist
            foreach (var character in StoryScript.Characters.Values)
            {
                if (!storyData.CharacterRelationships.ContainsKey(character.CharacterId))
                {
                    storyData.CharacterRelationships[character.CharacterId] = new CharacterRelationship
                    {
                        TrustLevel = 0,
                        ProfessionalRespect = 0,
                        PersonalConnection = 0,
                        CurrentPhase = RelationshipPhase.FirstMeeting
                    };
                }

                if (!storyData.CharacterArcs.ContainsKey(character.CharacterId))
                {
                    storyData.CharacterArcs[character.CharacterId] = new CharacterArcState
                    {
                        CharacterId = character.CharacterId,
                        CurrentPhase = CharacterArcPhase.Introduction,
                        NextMilestoneQuarter = character.IntroductionQuarter
                    };
                }
            }
        }

        public void UpdateCharacterRelationship(string characterId, int trustChange, int respectChange, int connectionChange, string experience = "")
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return;

            var relationship = storyData.CharacterRelationships[characterId];
            
            // Apply changes with bounds checking
            relationship.TrustLevel = Math.Max(-100, Math.Min(100, relationship.TrustLevel + trustChange));
            relationship.ProfessionalRespect = Math.Max(-100, Math.Min(100, relationship.ProfessionalRespect + respectChange));
            relationship.PersonalConnection = Math.Max(-100, Math.Min(100, relationship.PersonalConnection + connectionChange));

            // Add experience if provided
            if (!string.IsNullOrEmpty(experience))
            {
                relationship.SharedExperiences.Add($"Q{storyData.CurrentQuarter}: {experience}");
            }

            // Update relationship phase based on overall relationship strength
            UpdateRelationshipPhase(characterId);
        }

        private void UpdateRelationshipPhase(string characterId)
        {
            var relationship = storyData.CharacterRelationships[characterId];
            var averageRelationship = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;

            // Special handling for Joan based on quarter progression
            if (characterId == "joan")
            {
                var joanPhase = StoryScript.GetJoanPhaseForQuarter(storyData.CurrentQuarter);
                relationship.CurrentPhase = joanPhase;
                return;
            }

            // General relationship phase progression for other characters
            relationship.CurrentPhase = averageRelationship switch
            {
                < -50 => RelationshipPhase.Hostile,
                < -20 => RelationshipPhase.Strained,
                < 20 => RelationshipPhase.ProfessionalAcquaintance,
                < 50 => RelationshipPhase.TrustedColleague,
                < 80 => RelationshipPhase.PersonalFriend,
                _ => RelationshipPhase.LifelongBond
            };
        }

        public void UpdateJoanPhaseForQuarter(int quarter)
        {
            // Update Joan's relationship phase based on the current quarter
            if (storyData.CharacterRelationships.ContainsKey("joan"))
            {
                var joanRelationship = storyData.CharacterRelationships["joan"];
                var newPhase = StoryScript.GetJoanPhaseForQuarter(quarter);
                
                // Only update if the phase has changed
                if (joanRelationship.CurrentPhase != newPhase)
                {
                    var oldPhase = joanRelationship.CurrentPhase;
                    joanRelationship.CurrentPhase = newPhase;
                    
                    // Add a shared experience to mark the phase transition
                    string transitionMessage = newPhase switch
                    {
                        RelationshipPhase.TrustedColleague => "Joan has become a trusted advisor after working together through the early challenges",
                        RelationshipPhase.PersonalFriend => "Your relationship with Joan has deepened into a personal friendship",
                        RelationshipPhase.LifelongBond => "Joan has become a lifelong friend and trusted partner after years of collaboration",
                        _ => $"Relationship with Joan evolved to {newPhase}"
                    };
                    
                    joanRelationship.SharedExperiences.Add($"Q{quarter}: {transitionMessage}");
                    
                    System.Diagnostics.Debug.WriteLine($"Joan's relationship phase updated from {oldPhase} to {newPhase} at Q{quarter}");
                }
            }
        }

        public void ProcessBusinessDecisionImpact(string decisionType, Dictionary<string, object> decisionData)
        {
            // Update character relationships based on business decisions
            switch (decisionType)
            {
                case "employee_bonus":
                    UpdateCharacterRelationship("evelyn_cross", 5, 3, 2, "Approved employee bonuses");
                    UpdateCharacterRelationship("marcus_vey", -2, 0, 0, "Concerned about bonus costs");
                    break;

                case "cost_cutting":
                    UpdateCharacterRelationship("marcus_vey", 3, 5, 1, "Supported cost reduction measures");
                    UpdateCharacterRelationship("evelyn_cross", -3, -2, -1, "Worried about employee impact");
                    break;

                case "marketing_campaign":
                    UpdateCharacterRelationship("lucinda_vale", 4, 3, 2, "Launched marketing campaign");
                    UpdateCharacterRelationship("harold_finch", -1, 0, 0, "Cautious about marketing risks");
                    break;

                case "employee_firing":
                    UpdateCharacterRelationship("evelyn_cross", -5, -3, -2, "Fired employees");
                    UpdateCharacterRelationship("marcus_vey", 2, 1, 0, "Supported workforce optimization");
                    break;

                case "high_risk_investment":
                    UpdateCharacterRelationship("marcus_vey", 5, 4, 2, "Pursued high-risk investment");
                    UpdateCharacterRelationship("harold_finch", -4, -2, -1, "Concerned about legal risks");
                    break;
            }
        }

        public List<string> GetCharacterAdvice(string characterId, Company company, int currentQuarter)
        {
            if (!StoryScript.Characters.ContainsKey(characterId))
                return new List<string>();

            var character = StoryScript.Characters[characterId];
            var relationship = storyData.CharacterRelationships[characterId];
            var advice = new List<string>();

            // Generate character-specific advice based on personality and company state
            switch (characterId)
            {
                case "marcus_vey":
                    if (company.Capital > 1000000)
                    {
                        advice.Add("💰 Marcus suggests: 'With this capital, we could pursue aggressive expansion or high-yield investments.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "aggressive_expansion", currentQuarter);
                    }
                    if (company.Risk < 20)
                    {
                        advice.Add("📈 Marcus suggests: 'We're playing it too safe. Higher risk could mean higher rewards.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "increase_risk", currentQuarter);
                    }
                    break;

                case "evelyn_cross":
                    if (company.Morale < 30)
                    {
                        advice.Add("😟 Evelyn warns: 'Employee morale is critically low. We need immediate action to prevent turnover.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "improve_morale", currentQuarter);
                    }
                    if (company.EmployeeCount < 5)
                    {
                        advice.Add("👥 Evelyn suggests: 'We're understaffed. Consider hiring to improve productivity and reduce burnout.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "hire_employees", currentQuarter);
                    }
                    break;

                case "vincent_duro":
                    if (company.MarketShare > 40)
                    {
                        advice.Add("🏢 Vincent challenges: 'Impressive market share, but can you maintain it against real competition?'");
                        endingTracker?.RecordCharacterAdvice(characterId, "competitive_warning", currentQuarter);
                    }
                    break;

                case "lucinda_vale":
                    if (company.Reputation < 20)
                    {
                        advice.Add("📢 Lucy suggests: 'Our public image needs work. A strategic PR campaign could transform our reputation.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "pr_campaign", currentQuarter);
                    }
                    break;

                case "gregory_shaw":
                    if (company.Risk > 60)
                    {
                        advice.Add("⚙️ Greg warns: 'Operations are becoming unstable. We need to focus on efficiency and risk reduction.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "reduce_risk", currentQuarter);
                    }
                    if (company.EmployeeCount > 15)
                    {
                        advice.Add("📊 Greg suggests: 'With this workforce size, we need better operational systems and processes.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "improve_operations", currentQuarter);
                    }
                    break;

                case "selena_park":
                    if (company.Capital > 750000000)
                    {
                        advice.Add("💼 Selena hints: 'Companies with your financial profile often attract acquisition interest from major conglomerates...'");
                        endingTracker?.RecordCharacterAdvice(characterId, "buyout_opportunity", currentQuarter);
                    }
                    if (company.MarketShare > 50)
                    {
                        advice.Add("📈 Selena suggests: 'Strong market position creates excellent opportunities for strategic partnerships.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "strategic_partnership", currentQuarter);
                    }
                    break;

                case "harold_finch":
                    if (company.Risk > 70)
                    {
                        advice.Add("⚖️ Harold warns: 'Current risk levels expose us to potential legal and regulatory issues.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "legal_risk_warning", currentQuarter);
                    }
                    if (company.ConsecutiveNegativeQuarters > 0)
                    {
                        advice.Add("📋 Harold advises: 'Financial distress increases legal vulnerabilities. We need careful crisis management.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "crisis_management", currentQuarter);
                    }
                    break;

                case "sophie_kim":
                    var efficiency = Math.Max(50, 100 - company.Risk);
                    advice.Add($"📊 Sophie reports: 'Data shows our efficiency is at {efficiency}%. I found some optimization opportunities!'");
                    endingTracker?.RecordCharacterAdvice(characterId, "efficiency_insights", currentQuarter);
                    if (company.MarketShare > 30)
                    {
                        advice.Add("📈 Sophie suggests: 'Our market share growth pattern suggests we could capture even more with targeted strategies!'");
                        endingTracker?.RecordCharacterAdvice(characterId, "market_growth_strategy", currentQuarter);
                    }
                    break;
            }

            return advice;
        }


        public bool ShouldIntroduceCharacter(string characterId, int currentQuarter)
        {
            if (!StoryScript.Characters.ContainsKey(characterId))
                return false;

            var character = StoryScript.Characters[characterId];
            var arcState = storyData.CharacterArcs[characterId];

            return currentQuarter >= character.IntroductionQuarter && 
                   arcState.CurrentPhase == CharacterArcPhase.Introduction;
        }

        public void IntroduceCharacter(string characterId)
        {
            if (!storyData.CharacterArcs.ContainsKey(characterId))
                return;

            var arcState = storyData.CharacterArcs[characterId];
            arcState.CurrentPhase = CharacterArcPhase.Development;
            arcState.CompletedMilestones.Add("character_introduction");
        }

        public Dictionary<EndingType, double> CalculateEndingProbabilities()
        {
            // Use the comprehensive ending probability tracker
            if (endingTracker != null)
            {
                return endingTracker.CalculateEndingProbabilities();
            }

            // Fallback to basic calculation if tracker is not available
            var probabilities = new Dictionary<EndingType, double>();

            // Market Dominance ending
            probabilities[EndingType.MarketDominance] = company.MarketShare > 65 ? 0.8 : 0.0;

            // Conglomerate Buyout ending
            probabilities[EndingType.ConglomerateBuyout] = company.Capital > 1000000000 ? 0.7 : 0.0;

            // Bankruptcy ending
            probabilities[EndingType.BankruptcyFailure] = company.ConsecutiveNegativeQuarters >= 1 ? 0.9 : 0.0;

            // Lost Manpower ending
            probabilities[EndingType.LostManpowerFailure] = company.EmployeeCount <= 1 ? 1.0 : 0.0;

            // Graceful Retirement ending (default if no other conditions met)
            if (probabilities.Values.All(p => p < 0.1))
                probabilities[EndingType.GracefulRetirement] = 0.6;

            return probabilities;
        }

        #region Relationship Repair System

        /// <summary>
        /// Checks if a character relationship needs repair (is strained or hostile)
        /// </summary>
        public bool NeedsRelationshipRepair(string characterId)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return false;

            var relationship = storyData.CharacterRelationships[characterId];
            return relationship.CurrentPhase == RelationshipPhase.Strained || 
                   relationship.CurrentPhase == RelationshipPhase.Hostile;
        }

        /// <summary>
        /// Gets a list of all characters with strained or hostile relationships
        /// </summary>
        public List<string> GetCharactersNeedingRepair()
        {
            var needingRepair = new List<string>();
            
            foreach (var kvp in storyData.CharacterRelationships)
            {
                if (NeedsRelationshipRepair(kvp.Key))
                {
                    needingRepair.Add(kvp.Key);
                }
            }
            
            return needingRepair;
        }

        /// <summary>
        /// Applies relationship repair from an empathetic response
        /// </summary>
        public void ApplyRelationshipRepair(string characterId, int trustIncrease, int respectIncrease, int connectionIncrease, string repairContext)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return;

            var relationship = storyData.CharacterRelationships[characterId];
            var oldPhase = relationship.CurrentPhase;
            
            // Apply the relationship improvements
            UpdateCharacterRelationship(characterId, trustIncrease, respectIncrease, connectionIncrease, repairContext);
            
            // Clear recent conflicts if repair is significant
            if (trustIncrease >= 10 || connectionIncrease >= 10)
            {
                // Mark conflicts as resolved rather than removing them (history matters)
                var resolvedConflicts = relationship.ConflictHistory
                    .Select(c => $"[RESOLVED] {c}")
                    .ToList();
                relationship.ConflictHistory = resolvedConflicts;
                
                // Add repair milestone
                relationship.SharedExperiences.Add($"Q{storyData.CurrentQuarter}: Relationship repaired through {repairContext}");
            }
            
            var newPhase = relationship.CurrentPhase;
            
            // Log phase transition if it occurred
            if (oldPhase != newPhase)
            {
                System.Diagnostics.Debug.WriteLine($"Relationship with {characterId} improved from {oldPhase} to {newPhase} through repair");
            }
        }

        /// <summary>
        /// Processes empathetic choice impact on character relationship
        /// </summary>
        public void ProcessEmpatheticChoice(string characterId, RelationshipImpact impact, string choiceContext)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return;

            var relationship = storyData.CharacterRelationships[characterId];
            
            // Apply primary relationship changes
            ApplyRelationshipRepair(
                characterId, 
                impact.TrustChange, 
                impact.RespectChange, 
                impact.PersonalConnectionChange, 
                choiceContext
            );
            
            // Apply secondary effects to other characters
            foreach (var secondaryEffect in impact.SecondaryEffects)
            {
                if (storyData.CharacterRelationships.ContainsKey(secondaryEffect.Key))
                {
                    UpdateCharacterRelationship(
                        secondaryEffect.Key, 
                        secondaryEffect.Value / 3,  // Trust
                        secondaryEffect.Value / 3,  // Respect
                        secondaryEffect.Value / 3,  // Connection
                        $"Indirect effect from support shown to {StoryScript.Characters[characterId].Name}"
                    );
                }
            }
            
            // Check for phase transition potential
            if (impact.PhaseTransitionPotential)
            {
                CheckAndTriggerPhaseTransition(characterId);
            }
        }

        /// <summary>
        /// Checks if a relationship should transition to a new phase
        /// </summary>
        private void CheckAndTriggerPhaseTransition(string characterId)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return;

            var relationship = storyData.CharacterRelationships[characterId];
            var oldPhase = relationship.CurrentPhase;
            
            UpdateRelationshipPhase(characterId);
            
            var newPhase = relationship.CurrentPhase;
            
            if (oldPhase != newPhase)
            {
                // Add milestone for phase transition
                string transitionMessage = GetPhaseTransitionMessage(characterId, oldPhase, newPhase);
                relationship.SharedExperiences.Add($"Q{storyData.CurrentQuarter}: {transitionMessage}");
                
                System.Diagnostics.Debug.WriteLine($"Phase transition for {characterId}: {oldPhase} -> {newPhase}");
            }
        }

        /// <summary>
        /// Gets a descriptive message for relationship phase transitions
        /// </summary>
        private string GetPhaseTransitionMessage(string characterId, RelationshipPhase oldPhase, RelationshipPhase newPhase)
        {
            var character = StoryScript.Characters[characterId];
            
            // Positive transitions
            if (newPhase > oldPhase)
            {
                return newPhase switch
                {
                    RelationshipPhase.ProfessionalAcquaintance => $"Established professional relationship with {character.Name}",
                    RelationshipPhase.TrustedColleague => $"Developed mutual trust and respect with {character.Name}",
                    RelationshipPhase.PersonalFriend => $"Deepened friendship with {character.Name} beyond professional boundaries",
                    RelationshipPhase.LifelongBond => $"Formed a lifelong bond with {character.Name} through shared experiences",
                    _ => $"Relationship with {character.Name} improved"
                };
            }
            
            // Negative transitions (repair scenarios)
            if (newPhase < oldPhase)
            {
                return newPhase switch
                {
                    RelationshipPhase.Hostile => $"Relationship with {character.Name} became hostile",
                    RelationshipPhase.Strained => $"Relationship with {character.Name} became strained",
                    RelationshipPhase.ProfessionalAcquaintance => $"Relationship with {character.Name} cooled to professional distance",
                    _ => $"Relationship with {character.Name} changed"
                };
            }
            
            return $"Relationship with {character.Name} evolved";
        }

        /// <summary>
        /// Gets relationship repair suggestions for a character
        /// </summary>
        public List<string> GetRelationshipRepairSuggestions(string characterId)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return new List<string>();

            var relationship = storyData.CharacterRelationships[characterId];
            var character = StoryScript.Characters[characterId];
            var suggestions = new List<string>();
            
            if (!NeedsRelationshipRepair(characterId))
                return suggestions;

            // Analyze what caused the strain
            var recentConflicts = relationship.ConflictHistory
                .Where(c => !c.StartsWith("[RESOLVED]"))
                .ToList();

            if (recentConflicts.Any())
            {
                suggestions.Add($"💔 Your relationship with {character.Name} is {relationship.CurrentPhase}");
                suggestions.Add($"Recent conflicts: {recentConflicts.Count} unresolved issues");
            }

            // Provide character-specific repair suggestions
            suggestions.Add(GetCharacterSpecificRepairSuggestion(characterId, relationship));
            
            return suggestions;
        }

        /// <summary>
        /// Gets character-specific repair suggestions based on personality
        /// </summary>
        private string GetCharacterSpecificRepairSuggestion(string characterId, CharacterRelationship relationship)
        {
            return characterId switch
            {
                "joan" => "💡 Joan values honesty and appreciation. Acknowledge her contributions and show genuine care.",
                "marcus_vey" => "💡 Marcus respects results and decisiveness. Show him you value his financial expertise.",
                "evelyn_cross" => "💡 Evelyn needs to see you care about employees. Demonstrate empathy and support for the team.",
                "vincent_duro" => "💡 Vincent respects strength but appreciates respect. Find common ground without showing weakness.",
                "lucinda_vale" => "💡 Lucy needs creative validation. Acknowledge her vision and give her space to innovate.",
                "gregory_shaw" => "💡 Greg values reliability and competence. Show him you respect his operational expertise.",
                "selena_park" => "💡 Selena needs to see ROI and strategic thinking. Demonstrate your business acumen.",
                "harold_finch" => "💡 Harold needs to see you take legal concerns seriously. Show respect for proper procedures.",
                "sophie_kim" => "💡 Sophie needs encouragement and mentorship. Show her you believe in her potential.",
                _ => "💡 Show genuine care and respect for their perspective to begin repairing the relationship."
            };
        }

        /// <summary>
        /// Tracks a conflict in the relationship history
        /// </summary>
        public void RecordRelationshipConflict(string characterId, string conflictDescription)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return;

            var relationship = storyData.CharacterRelationships[characterId];
            relationship.ConflictHistory.Add($"Q{storyData.CurrentQuarter}: {conflictDescription}");
            
            System.Diagnostics.Debug.WriteLine($"Conflict recorded with {characterId}: {conflictDescription}");
        }

        /// <summary>
        /// Gets the overall relationship health score (0-100)
        /// </summary>
        public int GetRelationshipHealthScore(string characterId)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return 0;

            var relationship = storyData.CharacterRelationships[characterId];
            
            // Calculate average of the three relationship dimensions
            var averageScore = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;
            
            // Convert from -100/100 scale to 0-100 scale
            return (int)((averageScore + 100) / 2);
        }

        #endregion

        #region Ending Probability and Advice Tracking

        /// <summary>
        /// Records when the player follows or ignores character advice
        /// This affects ending probabilities and character relationships
        /// </summary>
        public void RecordAdviceResponse(string characterId, string adviceType, bool followed, int quarter)
        {
            // Record in the ending tracker
            endingTracker?.RecordAdviceResponse(characterId, adviceType, followed, quarter);

            // Update character relationship based on whether advice was followed
            if (followed)
            {
                // Following advice improves trust and respect
                UpdateCharacterRelationship(characterId, 3, 5, 1, $"Followed advice: {adviceType}");
            }
            else
            {
                // Ignoring advice may strain the relationship, especially if it was important
                UpdateCharacterRelationship(characterId, -2, -3, 0, $"Ignored advice: {adviceType}");
            }
        }

        /// <summary>
        /// Gets the most likely ending based on current probabilities
        /// </summary>
        public EndingType GetMostLikelyEnding()
        {
            return endingTracker?.GetMostLikelyEnding() ?? EndingType.GracefulRetirement;
        }

        /// <summary>
        /// Gets the advice follow rate for a specific character
        /// </summary>
        public double GetAdviceFollowRate(string characterId)
        {
            return endingTracker?.GetAdviceFollowRate(characterId) ?? 0.0;
        }

        /// <summary>
        /// Generates a check-in conversation with a character for player-initiated interactions
        /// </summary>
        public DialogueConversation? GenerateCharacterCheckIn(string characterId, Company company, int currentQuarter)
        {
            // Verify character has been introduced
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return null;

            var character = StoryScript.Characters.GetValueOrDefault(characterId);
            if (character == null)
                return null;

            var relationship = storyData.CharacterRelationships[characterId];

            // Create a conversation
            var conversation = new DialogueConversation
            {
                ConversationId = $"checkin_{characterId}_Q{currentQuarter}",
                Title = $"Conversation with {character.Name}",
                Participants = new List<string> { "player", characterId },
                StartNodeId = "greeting",
                CurrentNodeId = "greeting"
            };

            // Generate greeting based on relationship
            string greetingText = GenerateGreeting(characterId, relationship);

            // Get character advice
            var adviceLines = GetCharacterAdvice(characterId, company, currentQuarter);
            string adviceText = adviceLines.Count > 0 ? string.Join("\n\n", adviceLines) : "Everything seems to be running smoothly right now.";

            // Create greeting node
            var greetingNode = new DialogueNode
            {
                NodeId = "greeting",
                CharacterId = characterId,
                DialogueText = $"{greetingText}\n\n{adviceText}",
                EmotionalTone = GetEmotionalToneForRelationship(relationship),
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        ChoiceText = "Thanks for the advice",
                        NextNodeId = "end",
                        Tone = ChoiceTone.Professional,
                        RelationshipChanges = new Dictionary<string, int>
                        {
                            [characterId] = 2 // Small positive boost for listening
                        }
                    },
                    new DialogueChoice
                    {
                        ChoiceText = "I appreciate your perspective",
                        NextNodeId = "end",
                        Tone = ChoiceTone.Supportive,
                        RelationshipChanges = new Dictionary<string, int>
                        {
                            [characterId] = 5 // Larger boost for showing appreciation
                        }
                    }
                }
            };

            // Create end node
            var endNode = new DialogueNode
            {
                NodeId = "end",
                CharacterId = characterId,
                DialogueText = GetFarewellMessage(characterId, relationship),
                EmotionalTone = EmotionalTone.Positive,
                Choices = new List<DialogueChoice>()
            };

            conversation.Nodes.Add("greeting", greetingNode);
            conversation.Nodes.Add("end", endNode);

            return conversation;
        }

        private string GenerateGreeting(string characterId, CharacterRelationship relationship)
        {
            var avgRelationship = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;

            return characterId switch
            {
                "joan" when avgRelationship >= 60 => "It's always good to see you! How can I help?",
                "joan" when avgRelationship >= 30 => "Hello! What can I do for you today?",
                "joan" => "Yes? What do you need?",

                "marcus_vey" when avgRelationship >= 60 => "Good to see you. Let's talk numbers.",
                "marcus_vey" when avgRelationship >= 30 => "What's on your mind? Make it quick.",
                "marcus_vey" => "I'm busy. What is it?",

                "evelyn_cross" when avgRelationship >= 60 => "I'm so glad you stopped by! Let's chat.",
                "evelyn_cross" when avgRelationship >= 30 => "Hello! How are things going?",
                "evelyn_cross" => "Yes? Is there something you need?",

                "vincent_duro" when avgRelationship >= 60 => "Well, well. To what do I owe the pleasure?",
                "vincent_duro" when avgRelationship >= 30 => "Checking in on the competition?",
                "vincent_duro" => "What do you want?",

                "lucinda_vale" when avgRelationship >= 60 => "Darling! Perfect timing. Let's talk strategy.",
                "lucinda_vale" when avgRelationship >= 30 => "Oh, hello! Looking for some PR magic?",
                "lucinda_vale" => "Yes? I'm in the middle of something.",

                "gregory_shaw" when avgRelationship >= 60 => "Good timing. I have some operational insights for you.",
                "gregory_shaw" when avgRelationship >= 30 => "What brings you by?",
                "gregory_shaw" => "I'm analyzing data. What do you need?",

                "selena_park" when avgRelationship >= 60 => "Always a pleasure. Let's discuss opportunities.",
                "selena_park" when avgRelationship >= 30 => "Hello. Looking for investment advice?",
                "selena_park" => "I hope this is worth my time.",

                "harold_finch" when avgRelationship >= 60 => "Ah, good. I wanted to discuss some legal matters with you.",
                "harold_finch" when avgRelationship >= 30 => "Yes? Do you have legal questions?",
                "harold_finch" => "I'm reviewing contracts. What is it?",

                "sophie_kim" when avgRelationship >= 60 => "Oh! I'm so excited to share what I found in the data!",
                "sophie_kim" when avgRelationship >= 30 => "Hi! Want to see some interesting analytics?",
                "sophie_kim" => "Um, hi. Did you need something?",

                _ => "Hello. What can I do for you?"
            };
        }

        private string GetFarewellMessage(string characterId, CharacterRelationship relationship)
        {
            var avgRelationship = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;

            return characterId switch
            {
                "joan" when avgRelationship >= 60 => "Anytime! My door is always open for you.",
                "joan" => "Good luck out there.",

                "marcus_vey" when avgRelationship >= 60 => "Let's make some money together.",
                "marcus_vey" => "Don't waste this opportunity.",

                "evelyn_cross" when avgRelationship >= 60 => "Take care! Remember, our people come first.",
                "evelyn_cross" => "Good luck with your decisions.",

                "vincent_duro" when avgRelationship >= 60 => "May the best CEO win... but we both know who that is.",
                "vincent_duro" => "See you in the market.",

                "lucinda_vale" when avgRelationship >= 60 => "Go make some headlines, darling!",
                "lucinda_vale" => "Keep your brand strong.",

                "gregory_shaw" when avgRelationship >= 60 => "Efficiency is everything. Don't forget that.",
                "gregory_shaw" => "Back to work.",

                "selena_park" when avgRelationship >= 60 => "Smart investments lead to great returns. Remember that.",
                "selena_park" => "Think about what I said.",

                "harold_finch" when avgRelationship >= 60 => "Stay compliant, stay successful.",
                "harold_finch" => "Don't do anything legally questionable.",

                "sophie_kim" when avgRelationship >= 60 => "The data never lies! Good luck!",
                "sophie_kim" => "Hope that helps!",

                _ => "Goodbye."
            };
        }

        private EmotionalTone GetEmotionalToneForRelationship(CharacterRelationship relationship)
        {
            var avgRelationship = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;

            return avgRelationship switch
            {
                >= 60 => EmotionalTone.Positive,
                >= 30 => EmotionalTone.Neutral,
                >= 0 => EmotionalTone.Concerned,
                _ => EmotionalTone.Tense
            };
        }


        #endregion
    }
}
