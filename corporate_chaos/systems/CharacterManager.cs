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
                        advice.Add("ðŸ’° Marcus suggests: 'With this capital, we could pursue aggressive expansion or high-yield investments.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "aggressive_expansion", currentQuarter);
                    }
                    if (company.Risk < 20)
                    {
                        advice.Add("ðŸ“ˆ Marcus suggests: 'We're playing it too safe. Higher risk could mean higher rewards.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "increase_risk", currentQuarter);
                    }
                    break;

                case "evelyn_cross":
                    if (company.Morale < 30)
                    {
                        advice.Add("ðŸ˜Ÿ Evelyn warns: 'Employee morale is critically low. We need immediate action to prevent turnover.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "improve_morale", currentQuarter);
                    }
                    if (company.EmployeeCount < 5)
                    {
                        advice.Add("ðŸ‘¥ Evelyn suggests: 'We're understaffed. Consider hiring to improve productivity and reduce burnout.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "hire_employees", currentQuarter);
                    }
                    break;

                case "vincent_duro":
                    if (company.MarketShare > 40)
                    {
                        advice.Add("ðŸ¢ Vincent challenges: 'Impressive market share, but can you maintain it against real competition?'");
                        endingTracker?.RecordCharacterAdvice(characterId, "competitive_warning", currentQuarter);
                    }
                    break;

                case "lucinda_vale":
                    if (company.Reputation < 20)
                    {
                        advice.Add("ðŸ“¢ Lucy suggests: 'Our public image needs work. A strategic PR campaign could transform our reputation.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "pr_campaign", currentQuarter);
                    }
                    break;

                case "gregory_shaw":
                    if (company.Risk > 60)
                    {
                        advice.Add("âš™ï¸ Greg warns: 'Operations are becoming unstable. We need to focus on efficiency and risk reduction.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "reduce_risk", currentQuarter);
                    }
                    if (company.EmployeeCount > 15)
                    {
                        advice.Add("ðŸ“Š Greg suggests: 'With this workforce size, we need better operational systems and processes.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "improve_operations", currentQuarter);
                    }
                    break;

                case "selena_park":
                    if (company.Capital > 750000000)
                    {
                        advice.Add("ðŸ’¼ Selena hints: 'Companies with your financial profile often attract acquisition interest from major conglomerates...'");
                        endingTracker?.RecordCharacterAdvice(characterId, "buyout_opportunity", currentQuarter);
                    }
                    if (company.MarketShare > 50)
                    {
                        advice.Add("ðŸ“ˆ Selena suggests: 'Strong market position creates excellent opportunities for strategic partnerships.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "strategic_partnership", currentQuarter);
                    }
                    break;

                case "harold_finch":
                    if (company.Risk > 70)
                    {
                        advice.Add("âš–ï¸ Harold warns: 'Current risk levels expose us to potential legal and regulatory issues.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "legal_risk_warning", currentQuarter);
                    }
                    if (company.ConsecutiveNegativeQuarters > 0)
                    {
                        advice.Add("ðŸ“‹ Harold advises: 'Financial distress increases legal vulnerabilities. We need careful crisis management.'");
                        endingTracker?.RecordCharacterAdvice(characterId, "crisis_management", currentQuarter);
                    }
                    break;

                case "sophie_kim":
                    var efficiency = Math.Max(50, 100 - company.Risk);
                    advice.Add($"ðŸ“Š Sophie reports: 'Data shows our efficiency is at {efficiency}%. I found some optimization opportunities!'");
                    endingTracker?.RecordCharacterAdvice(characterId, "efficiency_insights", currentQuarter);
                    if (company.MarketShare > 30)
                    {
                        advice.Add("ðŸ“ˆ Sophie suggests: 'Our market share growth pattern suggests we could capture even more with targeted strategies!'");
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
                suggestions.Add($"ðŸ’” Your relationship with {character.Name} is {relationship.CurrentPhase}");
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
                "joan" => "ðŸ’¡ Joan values honesty and appreciation. Acknowledge her contributions and show genuine care.",
                "marcus_vey" => "ðŸ’¡ Marcus respects results and decisiveness. Show him you value his financial expertise.",
                "evelyn_cross" => "ðŸ’¡ Evelyn needs to see you care about employees. Demonstrate empathy and support for the team.",
                "vincent_duro" => "ðŸ’¡ Vincent respects strength but appreciates respect. Find common ground without showing weakness.",
                "lucinda_vale" => "ðŸ’¡ Lucy needs creative validation. Acknowledge her vision and give her space to innovate.",
                "gregory_shaw" => "ðŸ’¡ Greg values reliability and competence. Show him you respect his operational expertise.",
                "selena_park" => "ðŸ’¡ Selena needs to see ROI and strategic thinking. Demonstrate your business acumen.",
                "harold_finch" => "ðŸ’¡ Harold needs to see you take legal concerns seriously. Show respect for proper procedures.",
                "sophie_kim" => "ðŸ’¡ Sophie needs encouragement and mentorship. Show her you believe in her potential.",
                _ => "ðŸ’¡ Show genuine care and respect for their perspective to begin repairing the relationship."
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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // RICH DIALOGUE GENERATION â€” unique, context-aware, non-repetitive
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        private static readonly Random _rng = new Random();

        /// <summary>
        /// Picks a random item from a list, seeded by quarter so the same quarter
        /// always returns the same line (prevents re-roll spam), but varies each quarter.
        /// </summary>
        private static T Pick<T>(IList<T> list, int seed) => list[(seed * 7 + 13) % list.Count];

        /// <summary>
        /// Generates a rich, context-aware, multi-node check-in conversation.
        /// Each call produces dialogue unique to the character, relationship level,
        /// company state, and current quarter.
        /// </summary>
        public DialogueConversation? GenerateCharacterCheckIn(string characterId, Company company, int currentQuarter)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId)) return null;
            var character = StoryScript.Characters.GetValueOrDefault(characterId);
            if (character == null) return null;

            var relationship = storyData.CharacterRelationships[characterId];
            int avg = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;
            int seed = currentQuarter * 31 + characterId.Length * 7;

            var conversation = new DialogueConversation
            {
                ConversationId = $"checkin_{characterId}_Q{currentQuarter}",
                Title = $"Conversation with {character.Name}",
                Participants = new System.Collections.Generic.List<string> { "player", characterId },
                StartNodeId = "greeting",
                CurrentNodeId = "greeting"
            };

            // Build the full multi-node conversation
            var greetingNode = BuildGreetingNode(characterId, relationship, avg, seed, company, currentQuarter);
            var depthNode    = BuildDepthNode(characterId, relationship, avg, seed, company, currentQuarter);
            var endNode      = BuildEndNode(characterId, relationship, avg, seed);

            conversation.Nodes["greeting"] = greetingNode;
            conversation.Nodes["depth"]    = depthNode;
            conversation.Nodes["end"]      = endNode;

            return conversation;
        }

        // ── GREETING NODE ────────────────────────────────────────────────────
        private DialogueNode BuildGreetingNode(string characterId, CharacterRelationship rel, int avg, int seed, Company company, int quarter)
        {
            string opening = PickGreeting(characterId, avg, seed, company, quarter);
            string context = BuildContextLine(characterId, company, quarter, avg);

            var choices = new System.Collections.Generic.List<DialogueChoice>();

            // Choice set varies by character personality and relationship tier
            switch (characterId)
            {
                case "joan":
                    choices.Add(MakeChoice("Ask for a full situation briefing", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 2, connection: 1,
                        "Requested a thorough briefing — Joan appreciates your diligence."));
                    choices.Add(MakeChoice("Tell her she's doing a great job", "depth", ChoiceTone.Supportive,
                        characterId, trust: 2, respect: 1, connection: 5,
                        "Acknowledged Joan's hard work — she feels genuinely valued."));
                    choices.Add(MakeChoice("Ask if she has any personal concerns", "depth", ChoiceTone.Personal,
                        characterId, trust: 1, respect: 0, connection: 6,
                        "Showed personal interest in Joan's wellbeing — deepens the bond."));
                    if (avg >= 40)
                        choices.Add(MakeChoice("Joke about the chaos this quarter", "depth", ChoiceTone.Humorous,
                            characterId, trust: 2, respect: 0, connection: 4,
                            "Shared a laugh with Joan — lightens the mood between you."));
                    break;

                case "marcus_vey":
                    choices.Add(MakeChoice("Ask for his honest financial assessment", "depth", ChoiceTone.Professional,
                        characterId, trust: 4, respect: 5, connection: 1,
                        "Sought Marcus's expertise directly — he respects that."));
                    choices.Add(MakeChoice("Challenge his conservative projections", "depth", ChoiceTone.Aggressive,
                        characterId, trust: -2, respect: 3, connection: -1,
                        "Pushed back on Marcus's numbers — he's irritated but intrigued."));
                    choices.Add(MakeChoice("Ask what keeps him up at night about the company", "depth", ChoiceTone.Diplomatic,
                        characterId, trust: 3, respect: 2, connection: 3,
                        "Invited Marcus to share his deeper worries — he opens up slightly."));
                    if (avg >= 50)
                        choices.Add(MakeChoice("Offer to share your long-term vision with him", "depth", ChoiceTone.Personal,
                            characterId, trust: 5, respect: 3, connection: 4,
                            "Shared your vision with Marcus — he's genuinely invested now."));
                    break;

                case "evelyn_cross":
                    choices.Add(MakeChoice("Ask how the team is really feeling", "depth", ChoiceTone.Supportive,
                        characterId, trust: 3, respect: 2, connection: 5,
                        "Showed genuine care for the team — Evelyn is moved."));
                    choices.Add(MakeChoice("Discuss a recent tough HR decision", "depth", ChoiceTone.Professional,
                        characterId, trust: 2, respect: 4, connection: 2,
                        "Engaged Evelyn on a difficult topic — she appreciates the honesty."));
                    choices.Add(MakeChoice("Ask her what she would do differently", "depth", ChoiceTone.Diplomatic,
                        characterId, trust: 4, respect: 3, connection: 3,
                        "Invited Evelyn's perspective — she feels heard and respected."));
                    if (avg < 20)
                        choices.Add(MakeChoice("Apologize for a recent decision that hurt morale", "depth", ChoiceTone.Personal,
                            characterId, trust: 6, respect: 2, connection: 7,
                            "Offered a genuine apology — Evelyn's guard comes down."));
                    break;

                case "vincent_duro":
                    choices.Add(MakeChoice("Acknowledge his competitive edge", "depth", ChoiceTone.Diplomatic,
                        characterId, trust: 3, respect: 4, connection: 2,
                        "Showed respect for Vincent's abilities — he's pleasantly surprised."));
                    choices.Add(MakeChoice("Propose a temporary truce on market tactics", "depth", ChoiceTone.Professional,
                        characterId, trust: 2, respect: 3, connection: 1,
                        "Suggested cooperation — Vincent is cautiously interested."));
                    choices.Add(MakeChoice("Challenge him to a friendly wager on next quarter", "depth", ChoiceTone.Aggressive,
                        characterId, trust: -1, respect: 4, connection: 3,
                        "Threw down a challenge — Vincent loves the competition."));
                    if (avg >= 30)
                        choices.Add(MakeChoice("Ask what he actually thinks of your leadership", "depth", ChoiceTone.Personal,
                            characterId, trust: 4, respect: 2, connection: 5,
                            "Asked for Vincent's honest opinion — he gives a rare genuine answer."));
                    break;

                case "lucinda_vale":
                    choices.Add(MakeChoice("Ask for her read on public perception right now", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 4, connection: 2,
                        "Sought Lucinda's PR expertise — she's in her element."));
                    choices.Add(MakeChoice("Pitch a bold new brand direction", "depth", ChoiceTone.Aggressive,
                        characterId, trust: 1, respect: 2, connection: 3,
                        "Proposed something bold — Lucinda is excited by the ambition."));
                    choices.Add(MakeChoice("Ask about her creative process", "depth", ChoiceTone.Personal,
                        characterId, trust: 2, respect: 1, connection: 6,
                        "Showed interest in Lucinda as a person — she opens up warmly."));
                    if (avg >= 40)
                        choices.Add(MakeChoice("Compliment a recent campaign she ran", "depth", ChoiceTone.Supportive,
                            characterId, trust: 2, respect: 3, connection: 5,
                            "Recognized Lucinda's work specifically — she's genuinely touched."));
                    break;

                case "gregory_shaw":
                    choices.Add(MakeChoice("Ask for an operational efficiency report", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 5, connection: 1,
                        "Requested Greg's analysis — he appreciates the structured approach."));
                    choices.Add(MakeChoice("Ask what single change would have the biggest impact", "depth", ChoiceTone.Diplomatic,
                        characterId, trust: 4, respect: 4, connection: 2,
                        "Asked Greg for his top priority — he gives a precise, useful answer."));
                    choices.Add(MakeChoice("Admit you've been neglecting operations", "depth", ChoiceTone.Personal,
                        characterId, trust: 5, respect: 2, connection: 4,
                        "Showed vulnerability with Greg — he respects the honesty."));
                    if (avg < 20)
                        choices.Add(MakeChoice("Ask why he seems frustrated lately", "depth", ChoiceTone.Supportive,
                            characterId, trust: 4, respect: 1, connection: 5,
                            "Noticed Greg's frustration and addressed it — he's relieved."));
                    break;

                case "selena_park":
                    choices.Add(MakeChoice("Ask for her investment outlook this quarter", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 4, connection: 1,
                        "Sought Selena's financial insight — she's impressed you came prepared."));
                    choices.Add(MakeChoice("Ask if she'd personally invest in this company", "depth", ChoiceTone.Personal,
                        characterId, trust: 4, respect: 3, connection: 4,
                        "Asked Selena a pointed personal question — she gives a candid answer."));
                    choices.Add(MakeChoice("Discuss a risky growth opportunity", "depth", ChoiceTone.Aggressive,
                        characterId, trust: 2, respect: 3, connection: 2,
                        "Proposed a bold move to Selena — she's intrigued by the risk appetite."));
                    if (avg >= 50)
                        choices.Add(MakeChoice("Ask her to be a confidential advisor", "depth", ChoiceTone.Diplomatic,
                            characterId, trust: 6, respect: 4, connection: 5,
                            "Invited Selena into your inner circle — she's honored and committed."));
                    break;

                case "harold_finch":
                    choices.Add(MakeChoice("Ask for a legal risk assessment", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 5, connection: 1,
                        "Requested Harold's legal review — he's glad you're being proactive."));
                    choices.Add(MakeChoice("Ask if there's anything he's worried you're overlooking", "depth", ChoiceTone.Diplomatic,
                        characterId, trust: 4, respect: 4, connection: 3,
                        "Invited Harold's concerns — he opens up about a real risk."));
                    choices.Add(MakeChoice("Push back on one of his cautious recommendations", "depth", ChoiceTone.Aggressive,
                        characterId, trust: -3, respect: 1, connection: -2,
                        "Challenged Harold's advice — he's professionally offended."));
                    if (avg >= 40)
                        choices.Add(MakeChoice("Thank him for protecting the company quietly", "depth", ChoiceTone.Supportive,
                            characterId, trust: 3, respect: 2, connection: 6,
                            "Acknowledged Harold's behind-the-scenes work — he's visibly moved."));
                    break;

                case "sophie_kim":
                    choices.Add(MakeChoice("Ask what the data says about our biggest risk", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 3, connection: 2,
                        "Asked Sophie a direct analytical question — she lights up."));
                    choices.Add(MakeChoice("Ask her to walk you through her latest finding", "depth", ChoiceTone.Supportive,
                        characterId, trust: 2, respect: 2, connection: 5,
                        "Gave Sophie space to explain her work — she feels genuinely valued."));
                    choices.Add(MakeChoice("Challenge one of her data conclusions", "depth", ChoiceTone.Aggressive,
                        characterId, trust: -1, respect: 2, connection: -1,
                        "Questioned Sophie's analysis — she's flustered but rises to it."));
                    if (avg >= 30)
                        choices.Add(MakeChoice("Ask about her career goals", "depth", ChoiceTone.Personal,
                            characterId, trust: 2, respect: 1, connection: 7,
                            "Showed interest in Sophie's future — she's touched you asked."));
                    break;

                default:
                    choices.Add(MakeChoice("Ask for their honest assessment", "depth", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 3, connection: 1, "Sought honest feedback."));
                    choices.Add(MakeChoice("Express appreciation for their work", "depth", ChoiceTone.Supportive,
                        characterId, trust: 2, respect: 2, connection: 4, "Showed appreciation."));
                    break;
            }

            return new DialogueNode
            {
                NodeId = "greeting",
                CharacterId = characterId,
                DialogueText = $"{opening}\n\n{context}",
                EmotionalTone = GetEmotionalToneForRelationship(rel),
                Choices = choices
            };
        }

        // ── DEPTH NODE (second turn) ─────────────────────────────────────────
        private DialogueNode BuildDepthNode(string characterId, CharacterRelationship rel, int avg, int seed, Company company, int quarter)
        {
            string body = BuildDepthDialogue(characterId, avg, seed, company, quarter);
            var choices = new System.Collections.Generic.List<DialogueChoice>();

            switch (characterId)
            {
                case "joan":
                    choices.Add(MakeChoice("Ask her to keep a closer eye on things next quarter", "end", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 2, connection: 1, "Delegated watchful oversight to Joan."));
                    choices.Add(MakeChoice("Tell her you couldn't do this without her", "end", ChoiceTone.Personal,
                        characterId, trust: 2, respect: 1, connection: 7, "Expressed genuine reliance on Joan."));
                    choices.Add(MakeChoice("Ask her to flag anything unusual immediately", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 4, respect: 3, connection: 2, "Set up a direct alert channel with Joan."));
                    break;

                case "marcus_vey":
                    choices.Add(MakeChoice("Ask him to model three financial scenarios", "end", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 6, connection: 1, "Gave Marcus a concrete analytical task."));
                    choices.Add(MakeChoice("Tell him you trust his judgment on the numbers", "end", ChoiceTone.Supportive,
                        characterId, trust: 4, respect: 5, connection: 3, "Affirmed Marcus's financial authority."));
                    choices.Add(MakeChoice("Disagree with his risk tolerance", "end", ChoiceTone.Aggressive,
                        characterId, trust: -3, respect: 2, connection: -2, "Clashed with Marcus on risk — tension rises."));
                    choices.Add(MakeChoice("Ask him to mentor a junior finance employee", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 3, respect: 3, connection: 4, "Invited Marcus to invest in the team."));
                    break;

                case "evelyn_cross":
                    choices.Add(MakeChoice("Ask her to run a team morale initiative", "end", ChoiceTone.Supportive,
                        characterId, trust: 3, respect: 4, connection: 4, "Empowered Evelyn to lead a morale effort."));
                    choices.Add(MakeChoice("Ask her to identify your top three at-risk employees", "end", ChoiceTone.Professional,
                        characterId, trust: 4, respect: 4, connection: 2, "Tasked Evelyn with proactive HR monitoring."));
                    choices.Add(MakeChoice("Tell her the team is lucky to have her", "end", ChoiceTone.Personal,
                        characterId, trust: 2, respect: 2, connection: 7, "Gave Evelyn heartfelt recognition."));
                    break;

                case "vincent_duro":
                    choices.Add(MakeChoice("Respect his position and end on good terms", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 3, respect: 4, connection: 2, "Parted with Vincent on respectful terms."));
                    choices.Add(MakeChoice("Warn him you're coming for his market share", "end", ChoiceTone.Aggressive,
                        characterId, trust: -2, respect: 5, connection: 1, "Issued a competitive warning — Vincent is energized."));
                    choices.Add(MakeChoice("Suggest you both have more in common than you think", "end", ChoiceTone.Personal,
                        characterId, trust: 4, respect: 3, connection: 5, "Found common ground with Vincent — unexpected warmth."));
                    break;

                case "lucinda_vale":
                    choices.Add(MakeChoice("Give her full creative control on the next campaign", "end", ChoiceTone.Supportive,
                        characterId, trust: 3, respect: 3, connection: 5, "Trusted Lucinda's creative vision completely."));
                    choices.Add(MakeChoice("Ask her to focus on reputation repair this quarter", "end", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 4, connection: 2, "Directed Lucinda toward a specific PR goal."));
                    choices.Add(MakeChoice("Ask what she'd do if she were CEO for a day", "end", ChoiceTone.Humorous,
                        characterId, trust: 2, respect: 2, connection: 6, "Invited Lucinda's playful leadership fantasy."));
                    break;

                case "gregory_shaw":
                    choices.Add(MakeChoice("Ask him to implement his top efficiency recommendation", "end", ChoiceTone.Professional,
                        characterId, trust: 4, respect: 6, connection: 2, "Acted on Greg's advice — he's satisfied."));
                    choices.Add(MakeChoice("Ask him to train the operations team on new processes", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 3, respect: 4, connection: 3, "Invested in Greg's expertise through training."));
                    choices.Add(MakeChoice("Tell him his work is the backbone of this company", "end", ChoiceTone.Supportive,
                        characterId, trust: 2, respect: 3, connection: 6, "Gave Greg rare but meaningful recognition."));
                    break;

                case "selena_park":
                    choices.Add(MakeChoice("Ask her to evaluate a specific investment opportunity", "end", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 5, connection: 2, "Gave Selena a concrete task — she's engaged."));
                    choices.Add(MakeChoice("Ask her to keep this conversation confidential", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 5, respect: 3, connection: 4, "Built a private trust channel with Selena."));
                    choices.Add(MakeChoice("Tell her you value her perspective above all others", "end", ChoiceTone.Personal,
                        characterId, trust: 4, respect: 3, connection: 7, "Made Selena feel uniquely valued."));
                    break;

                case "harold_finch":
                    choices.Add(MakeChoice("Ask him to draft a risk mitigation memo", "end", ChoiceTone.Professional,
                        characterId, trust: 4, respect: 6, connection: 1, "Gave Harold a formal task — he's in his element."));
                    choices.Add(MakeChoice("Promise to run major decisions by him first", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 5, respect: 5, connection: 3, "Committed to Harold's oversight — he's reassured."));
                    choices.Add(MakeChoice("Ask him what he'd do if he weren't a lawyer", "end", ChoiceTone.Personal,
                        characterId, trust: 3, respect: 1, connection: 6, "Saw Harold as a person, not just a function."));
                    break;

                case "sophie_kim":
                    choices.Add(MakeChoice("Ask her to build a dashboard for key metrics", "end", ChoiceTone.Professional,
                        characterId, trust: 3, respect: 4, connection: 2, "Gave Sophie an exciting analytical project."));
                    choices.Add(MakeChoice("Tell her she's one of the sharpest minds here", "end", ChoiceTone.Supportive,
                        characterId, trust: 2, respect: 3, connection: 7, "Gave Sophie a confidence-boosting compliment."));
                    choices.Add(MakeChoice("Ask her to present her findings to the whole team", "end", ChoiceTone.Diplomatic,
                        characterId, trust: 3, respect: 4, connection: 4, "Gave Sophie visibility and recognition."));
                    break;

                default:
                    choices.Add(MakeChoice("Thank them and wrap up", "end", ChoiceTone.Professional,
                        characterId, trust: 2, respect: 2, connection: 1, "Ended professionally."));
                    choices.Add(MakeChoice("Express genuine appreciation", "end", ChoiceTone.Supportive,
                        characterId, trust: 2, respect: 1, connection: 4, "Showed warmth."));
                    break;
            }

            return new DialogueNode
            {
                NodeId = "depth",
                CharacterId = characterId,
                DialogueText = body,
                EmotionalTone = avg >= 40 ? EmotionalTone.Warm : EmotionalTone.Serious,
                Choices = choices
            };
        }

        // ── END NODE ─────────────────────────────────────────────────────────
        private DialogueNode BuildEndNode(string characterId, CharacterRelationship rel, int avg, int seed)
        {
            string farewell = PickFarewell(characterId, avg, seed);
            return new DialogueNode
            {
                NodeId = "end",
                CharacterId = characterId,
                DialogueText = farewell,
                EmotionalTone = avg >= 30 ? EmotionalTone.Positive : EmotionalTone.Neutral,
                Choices = new System.Collections.Generic.List<DialogueChoice>()
            };
        }


        #endregion

        #region Dialogue Helper Methods

        /// <summary>
        /// Creates a DialogueChoice with relationship impact pre-populated.
        /// </summary>
        private static DialogueChoice MakeChoice(
            string text,
            string nextNodeId,
            ChoiceTone tone,
            string characterId,
            int trust,
            int respect,
            int connection,
            string reaction)
        {
            return new DialogueChoice
            {
                ChoiceId = $"{characterId}_{nextNodeId}_{tone}_{System.Math.Abs(text.GetHashCode()) % 10000}",
                ChoiceText = text,
                NextNodeId = nextNodeId,
                Tone = tone,
                CharacterReaction = reaction,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    TrustChange = trust,
                    RespectChange = respect,
                    PersonalConnectionChange = connection
                },
                IsAvailable = true
            };
        }

        /// <summary>
        /// Maps a character's relationship average to an appropriate EmotionalTone.
        /// </summary>
        private static EmotionalTone GetEmotionalToneForRelationship(CharacterRelationship rel)
        {
            int avg = (rel.TrustLevel + rel.ProfessionalRespect + rel.PersonalConnection) / 3;
            return avg switch
            {
                >= 60 => EmotionalTone.Warm,
                >= 30 => EmotionalTone.Professional,
                >= 0  => EmotionalTone.Neutral,
                >= -30 => EmotionalTone.Tense,
                _ => EmotionalTone.Negative
            };
        }

        /// <summary>
        /// Returns the body text for the depth (second-turn) dialogue node.
        /// </summary>
        private static string BuildDepthDialogue(string characterId, int avg, int seed, Company company, int quarter)
        {
            var rng = new System.Random(seed ^ (characterId.GetHashCode() * 17));
            string tier = avg >= 60 ? "high" : avg >= 30 ? "mid" : "low";

            var lines = new System.Collections.Generic.Dictionary<string, string[]>
            {
                ["joan_high"]  = new[] {
                    "I've been thinking about how far we've come. It hasn't been easy, but I'm proud of what we've built together.",
                    "Honestly? I feel like we're finally hitting our stride. The team trusts you — and so do I.",
                    "There's something different about this quarter. I can feel it. Things are clicking." },
                ["joan_mid"]   = new[] {
                    "I'll be honest — it's been a tough stretch. But we're managing.",
                    "The team is holding up. I'm keeping an eye on a few things, but nothing critical yet.",
                    "I've seen worse. We'll get through this quarter." },
                ["joan_low"]   = new[] {
                    "I won't sugarcoat it. Morale is shaky and people are talking.",
                    "I'm doing my best, but I need more direction from the top.",
                    "Things feel uncertain right now. I hope that changes soon." },

                ["marcus_vey_high"]  = new[] {
                    "The numbers are telling an interesting story this quarter. I think we're positioned better than most realise.",
                    "I've been running some models. If we stay disciplined, the upside is significant.",
                    "I don't say this often, but I think we're making the right calls." },
                ["marcus_vey_mid"]   = new[] {
                    "The margins are tighter than I'd like. We need to watch our burn rate.",
                    "I have concerns about the current trajectory, but nothing we can't course-correct.",
                    "The financials are stable. Not exciting, but stable." },
                ["marcus_vey_low"]   = new[] {
                    "I'm going to be blunt — the numbers are bad and getting worse.",
                    "We're burning through capital faster than I projected. This needs to change.",
                    "I've flagged this before. I hope someone is listening." },

                ["evelyn_cross_high"]  = new[] {
                    "The team is really rallying. I've seen a genuine shift in energy this quarter.",
                    "People feel seen right now. That's rare, and it matters more than any bonus.",
                    "I've been doing this a long time. This team has something special." },
                ["evelyn_cross_mid"]   = new[] {
                    "Morale is okay. Not great, but okay. A few people are struggling quietly.",
                    "I'm keeping tabs on a couple of situations. Nothing urgent, but worth watching.",
                    "The team is functional. I'd like to see more investment in people, though." },
                ["evelyn_cross_low"]   = new[] {
                    "I'm worried about burnout. People are stretched thin and it's starting to show.",
                    "There's a lot of quiet frustration right now. I'm trying to hold things together.",
                    "Honestly? People are scared. They need to hear something reassuring from leadership." },

                ["vincent_duro_high"]  = new[] {
                    "You've earned a seat at the table. I don't say that lightly.",
                    "I've been watching your moves. You're playing a longer game than I expected.",
                    "Respect is rare in this industry. You've got mine — for now." },
                ["vincent_duro_mid"]   = new[] {
                    "You're holding your own. I'll give you that.",
                    "The market is shifting. Let's see if you can keep up.",
                    "I've seen companies like yours plateau. Don't let that happen." },
                ["vincent_duro_low"]   = new[] {
                    "You're making mistakes. I'm watching.",
                    "The market doesn't forgive weakness. Remember that.",
                    "I've seen this before. It doesn't end well." },

                ["lucinda_vale_high"]  = new[] {
                    "I've been sketching out some ideas that I think could really move the needle on brand perception.",
                    "The campaign last quarter got people talking. I want to build on that energy.",
                    "I love what we're doing right now. It feels authentic." },
                ["lucinda_vale_mid"]   = new[] {
                    "I have some ideas, but I need more runway to execute properly.",
                    "The brand is okay. I think we can do better, though.",
                    "I'm working on something. Give me a bit more time." },
                ["lucinda_vale_low"]   = new[] {
                    "I feel like my ideas keep getting watered down. It's frustrating.",
                    "The brand is suffering and I don't think anyone's listening to me.",
                    "I need more support if you want real results." },

                ["gregory_shaw_high"]  = new[] {
                    "Operations are running at 94% efficiency. I have a plan to get to 97%.",
                    "I've identified three process bottlenecks. I'd like your sign-off to fix them.",
                    "The systems are solid. I'm proud of what this team has built." },
                ["gregory_shaw_mid"]   = new[] {
                    "We're functional, but there's room for improvement. I have notes.",
                    "A few processes need attention. Nothing critical, but it adds up.",
                    "Operations are holding. I'd like to discuss some optimisations." },
                ["gregory_shaw_low"]   = new[] {
                    "We're running inefficiently and it's costing us. I've said this before.",
                    "The systems are strained. We need investment or things will break.",
                    "I can't keep patching problems without proper resources." },

                ["selena_park_high"]  = new[] {
                    "I've been tracking three opportunities that I think are worth serious consideration.",
                    "The data is pointing somewhere interesting. I'd like to walk you through it.",
                    "I don't get excited easily, but this quarter's numbers have my attention." },
                ["selena_park_mid"]   = new[] {
                    "There are some patterns worth watching. Nothing definitive yet.",
                    "I have analysis ready if you want to go through it.",
                    "The picture is mixed. I'll give you the full breakdown." },
                ["selena_park_low"]   = new[] {
                    "The data isn't good. I'd rather tell you now than later.",
                    "I've been flagging concerns for a while. I hope this conversation changes something.",
                    "The numbers don't lie. We need to talk about what they're saying." },

                ["harold_finch_high"]  = new[] {
                    "I've reviewed the contracts and I'm satisfied with our legal position.",
                    "There are a few clauses I want to revisit, but overall we're well-protected.",
                    "I sleep better when the legal framework is solid. Right now, it is." },
                ["harold_finch_mid"]   = new[] {
                    "There are some areas of exposure I want to flag. Nothing critical, but worth noting.",
                    "I've been reviewing the compliance documentation. A few things need updating.",
                    "We're legally sound, but I have some recommendations." },
                ["harold_finch_low"]   = new[] {
                    "I have serious concerns about our current legal exposure.",
                    "Some of the recent decisions have created risk I'm not comfortable with.",
                    "I need you to take the legal implications more seriously." },

                ["sophie_kim_high"]  = new[] {
                    "I've been running a predictive model on next quarter's performance. The results are fascinating.",
                    "The data tells a story — and it's a good one if we play it right.",
                    "I found a correlation in the employee data that I think could change how we approach hiring." },
                ["sophie_kim_mid"]   = new[] {
                    "I have some analysis ready. There are a few things worth discussing.",
                    "The metrics are mixed, but there are some bright spots.",
                    "I've been digging into the numbers. Want to see what I found?" },
                ["sophie_kim_low"]   = new[] {
                    "The data is concerning. I've been trying to flag this for a while.",
                    "Some of the trends I'm seeing aren't good. We should talk about them.",
                    "I have analysis that I think leadership needs to see." },
            };

            string key = $"{characterId}_{tier}";
            if (lines.TryGetValue(key, out var options))
                return options[rng.Next(options.Length)];

            return avg >= 50
                ? "Things are moving in the right direction. I wanted to share some thoughts."
                : "There are things we need to address. I'll be direct with you.";
        }

        /// <summary>
        /// Returns an opening line for the character based on relationship average and context.
        /// </summary>
        private static string PickGreeting(string characterId, int avg, int seed, Company company, int quarter)
        {
            var rng = new System.Random(seed ^ characterId.GetHashCode());
            string tier = avg >= 60 ? "high" : avg >= 30 ? "mid" : "low";

            var greetings = new System.Collections.Generic.Dictionary<string, string[]>
            {
                ["joan_high"]  = new[] { "Good morning! I've already pulled the reports you'll need.", "Always a pleasure — I've got everything ready for you.", "Right on time. I had a feeling you'd want to talk today." },
                ["joan_mid"]   = new[] { "Good morning. What can I help you with?", "I was just finishing up the quarterly notes. Come in.", "Morning. I'll get you up to speed." },
                ["joan_low"]   = new[] { "Oh — I didn't expect you. One moment.", "I'm a bit behind today, but I'll do my best.", "Yes? I'm in the middle of something, but go ahead." },

                ["marcus_vey_high"]  = new[] { "I was hoping you'd stop by. The numbers are interesting this quarter.", "Good timing — I've been running some projections I think you'll want to see.", "Always good to talk strategy with someone who actually reads the reports." },
                ["marcus_vey_mid"]   = new[] { "Come in. I have about fifteen minutes.", "The financials are on my desk if you want to go through them.", "What's on your mind? I'll be direct." },
                ["marcus_vey_low"]   = new[] { "I'm busy. Make it quick.", "I hope this is important.", "Fine. What do you need?" },

                ["evelyn_cross_high"]  = new[] { "I'm so glad you came by — I've been thinking about the team.", "Perfect timing. I just finished the morale survey results.", "You always seem to know when I need to talk." },
                ["evelyn_cross_mid"]   = new[] { "Hi! Come in. I was just reviewing some HR notes.", "Good to see you. What brings you by?", "I have a few things on my mind too, actually." },
                ["evelyn_cross_low"]   = new[] { "Oh. Hi. I wasn't expecting you.", "I'm a little swamped right now, but okay.", "Sure. What is it?" },

                ["vincent_duro_high"]  = new[] { "Well, well. The competition comes to visit.", "I respect that you came in person. Sit down.", "I was wondering when you'd show up. Let's talk." },
                ["vincent_duro_mid"]   = new[] { "Interesting. What do you want?", "I'll hear you out. Don't waste my time.", "You've got my attention. For now." },
                ["vincent_duro_low"]   = new[] { "I'm surprised you have the nerve.", "Make it fast.", "What do you want?" },

                ["lucinda_vale_high"]  = new[] { "Oh good, I was hoping to bounce some ideas off you!", "You have impeccable timing — I just finished a new concept.", "Come in! I've been dying to show you something." },
                ["lucinda_vale_mid"]   = new[] { "Hey! What's up?", "Good timing, I'm between projects.", "Come in. What's on your mind?" },
                ["lucinda_vale_low"]   = new[] { "Oh. Hey.", "I'm kind of in the zone right now, but sure.", "Yeah, what is it?" },

                ["gregory_shaw_high"]  = new[] { "Good. I was about to send you a memo anyway.", "Efficiency is up this quarter — I have data to show you.", "I appreciate you making time. I have recommendations." },
                ["gregory_shaw_mid"]   = new[] { "Come in. I'll keep it brief.", "What do you need? I'm in the middle of a process review.", "Alright. What is it?" },
                ["gregory_shaw_low"]   = new[] { "I'm busy.", "This better be worth interrupting me.", "Fine. Quickly." },

                ["selena_park_high"]  = new[] { "I was just thinking about you — I have some interesting data.", "Perfect. I've been wanting to share some analysis.", "Good timing. Sit down, this is worth your attention." },
                ["selena_park_mid"]   = new[] { "Hello. What brings you by?", "I have a few minutes. What's on your mind?", "Come in. I'll hear you out." },
                ["selena_park_low"]   = new[] { "I'm in the middle of something.", "What do you need?", "Make it brief." },

                ["harold_finch_high"]  = new[] { "I'm glad you came. I have some legal considerations to discuss.", "Good. I've been reviewing the contracts — we should talk.", "I was about to call you. There are a few things I want to flag." },
                ["harold_finch_mid"]   = new[] { "Come in. I'll need a moment to pull up the relevant documents.", "What can I do for you?", "I have time. What is it?" },
                ["harold_finch_low"]   = new[] { "I'm reviewing a contract. This will have to be brief.", "What is it?", "I hope this is a legal matter." },

                ["sophie_kim_high"]  = new[] { "Oh! I was just running a model I think you'll love.", "Great timing — I have some insights to share.", "I was hoping you'd stop by. I found something interesting." },
                ["sophie_kim_mid"]   = new[] { "Hi! What's up?", "Come in. I'm between analyses.", "Hey, what do you need?" },
                ["sophie_kim_low"]   = new[] { "Oh. Hi.", "I'm kind of busy, but okay.", "Yeah?" },
            };

            string key = $"{characterId}_{tier}";
            if (greetings.TryGetValue(key, out var lines))
                return lines[rng.Next(lines.Length)];

            // Fallback
            return avg >= 50
                ? "Good to see you. What's on your mind?"
                : "What do you need?";
        }

        /// <summary>
        /// Returns a short contextual line reflecting the company's current state.
        /// </summary>
        private static string BuildContextLine(string characterId, Company company, int quarter, int avg)
        {
            int year = (quarter / 4) + 1;
            string qLabel = $"Q{(quarter % 4) + 1} Year {year}";

            if (company.Capital < 0)
                return $"[{qLabel}] The company is in the red — every conversation carries weight right now.";
            if (company.Morale < -30)
                return $"[{qLabel}] Morale is low across the board. People are watching leadership closely.";
            if (company.Reputation > 70)
                return $"[{qLabel}] The company's reputation is strong — there's real momentum here.";
            if (company.MarketShare > 50)
                return $"[{qLabel}] You're leading the market. The pressure to stay on top is real.";

            return $"[{qLabel}] Things are moving. Every decision shapes what comes next.";
        }

        /// <summary>
        /// Returns a farewell line based on character, relationship average, and seed.
        /// </summary>
        private static string PickFarewell(string characterId, int avg, int seed)
        {
            var rng = new System.Random(seed ^ (characterId.GetHashCode() * 31));
            string tier = avg >= 60 ? "high" : avg >= 30 ? "mid" : "low";

            var farewells = new System.Collections.Generic.Dictionary<string, string[]>
            {
                ["joan_high"]  = new[] { "Take care of yourself too, not just the company.", "I'll have everything ready for next quarter. Count on it.", "It's always good when we get a chance to actually talk." },
                ["joan_mid"]   = new[] { "I'll keep you posted on anything that comes up.", "Good luck this quarter.", "Let me know if you need anything." },
                ["joan_low"]   = new[] { "I'll get back to work then.", "Okay. Goodbye.", "Right. I'll be here." },

                ["marcus_vey_high"]  = new[] { "Good talk. I'll have updated projections on your desk by Friday.", "You think differently than most. I appreciate that.", "Let's do this again before the quarter closes." },
                ["marcus_vey_mid"]   = new[] { "I'll send you the summary.", "We'll see how the numbers play out.", "Good enough. I'll be in touch." },
                ["marcus_vey_low"]   = new[] { "We're done here.", "I have work to do.", "Fine." },

                ["evelyn_cross_high"]  = new[] { "Thank you for checking in — it really does matter.", "I feel better about things after talking with you.", "You're one of the good ones. Don't forget that." },
                ["evelyn_cross_mid"]   = new[] { "Thanks for stopping by.", "I'll keep an eye on things.", "Take care." },
                ["evelyn_cross_low"]   = new[] { "Okay. Bye.", "I'll get back to it.", "Sure." },

                ["vincent_duro_high"]  = new[] { "Respect. See you on the other side.", "I didn't expect to enjoy this. I did.", "You're worth watching. I mean that." },
                ["vincent_duro_mid"]   = new[] { "We'll see.", "Don't get comfortable.", "Goodbye." },
                ["vincent_duro_low"]   = new[] { "We're done.", "Don't waste my time again.", "Leave." },

                ["lucinda_vale_high"]  = new[] { "This was fun! Let's do it again soon.", "You always leave me with something to think about.", "I'm going to go sketch something. You inspired me." },
                ["lucinda_vale_mid"]   = new[] { "Cool. Talk later!", "Alright, back to it.", "See you around." },
                ["lucinda_vale_low"]   = new[] { "Okay, bye.", "Sure.", "Later." },

                ["gregory_shaw_high"]  = new[] { "Efficient conversation. I appreciate that.", "I'll have the process report updated by end of week.", "Good. We're aligned. That matters." },
                ["gregory_shaw_mid"]   = new[] { "I'll follow up with a memo.", "Noted. Goodbye.", "Fine. I'll get back to work." },
                ["gregory_shaw_low"]   = new[] { "We're done.", "I have things to do.", "Goodbye." },

                ["selena_park_high"]  = new[] { "I'll run the numbers and send you a summary.", "This was a good use of time. Rare.", "I'll be in touch when I have something concrete." },
                ["selena_park_mid"]   = new[] { "I'll think about what you said.", "Noted. Goodbye.", "I'll follow up if anything changes." },
                ["selena_park_low"]   = new[] { "We're done here.", "Goodbye.", "I have work to do." },

                ["harold_finch_high"]  = new[] { "I'll draft a memo summarising our discussion.", "Good. I feel better knowing we're on the same page legally.", "I'll flag anything that needs your attention." },
                ["harold_finch_mid"]   = new[] { "I'll review the relevant clauses and get back to you.", "Noted. I'll be in touch.", "Goodbye." },
                ["harold_finch_low"]   = new[] { "I'll document this conversation.", "Goodbye.", "I have contracts to review." },

                ["sophie_kim_high"]  = new[] { "I'll send you the full analysis tonight!", "This was great — I love when we get to dig into the data together.", "I'll have something new for you by next quarter." },
                ["sophie_kim_mid"]   = new[] { "I'll send you the summary.", "Cool. Talk later.", "Okay, bye!" },
                ["sophie_kim_low"]   = new[] { "Okay. Bye.", "Sure.", "Later." },
            };

            string key = $"{characterId}_{tier}";
            if (farewells.TryGetValue(key, out var lines))
                return lines[rng.Next(lines.Length)];

            return avg >= 50 ? "Good talk. Until next time." : "Goodbye.";
        }

        #endregion
    }
}
