using System.IO;
using System.Text.Json;
using System.Windows;
using CorporateChaos.Models;
using CorporateChaos.Views;

namespace CorporateChaos.Systems
{
    public class StoryModeManager
    {
        private const string STORY_SAVE_FILE = "story_progress.json";
        private ExtendedStoryModeData storyData = null!;
        private Company company;
        private CharacterManager? characterManager;
        private NarrativeEngine? narrativeEngine;
        private EndingProbabilityTracker? endingTracker;
        
        public ExtendedStoryModeData StoryData => storyData;
        public bool IsStoryMode => storyData.IsStoryMode;
        public bool IsInTutorial => storyData.CurrentPhase == StoryPhase.Tutorial;
        public CharacterManager? CharacterManager => characterManager;
        public NarrativeEngine? NarrativeEngine => narrativeEngine;
        public EndingProbabilityTracker? EndingTracker => endingTracker;

        public StoryModeManager(Company company)
        {
            this.company = company;
            LoadStoryProgress();
        }

        public void StartNewStoryMode()
        {
            storyData = new ExtendedStoryModeData
            {
                CurrentQuarter = 1,
                CurrentPhase = StoryPhase.Tutorial,
                CurrentAct = NarrativeAct.Tutorial,
                IsStoryMode = true
            };
            
            // Initialize with basic operations unlocked from the start
            storyData.UnlockedMechanics.Add(MechanicType.BasicOperations);
            
            // Set up starting company state for story mode
            SetupStoryModeCompany();
            
            // Initialize character manager
            characterManager = new CharacterManager(storyData, company);
            
            // Initialize narrative engine
            narrativeEngine = new NarrativeEngine(storyData, company, characterManager);
            narrativeEngine.InitializeContentDistributor();
            
            // Initialize ending tracker
            endingTracker = new EndingProbabilityTracker(storyData, company);
            
            // Initialize advice system
            narrativeEngine.InitializeAdviceSystem(endingTracker);
            
            SaveStoryProgress();
        }

        private void SetupStoryModeCompany()
        {
            // Story mode starts with a more guided setup
            company.Capital = 750000; // More starting capital for story
            company.MarketShare = 8.5; // Slightly higher starting position
            company.Reputation = 10; // Start with slight positive reputation
            company.Morale = 20; // Start with decent morale
            company.Risk = -10; // Start with low risk
        }

        public void SetupStartingEmployees(Dictionary<Department, DepartmentStats> departments, List<Employee> hiredEmployees)
        {
            if (!IsStoryMode) return;

            // Create starting employees for key departments
            var startingEmployees = new List<Employee>
            {
                // Research Department - Senior researcher
                new Employee
                {
                    Name = "Dr. Sarah Mitchell",
                    OverallSkill = SkillLevel.Senior,
                    Specialization = Department.Research,
                    Experience = 8,
                    Productivity = 85,
                    Morale = 75,
                    Salary = 7500,
                    RiskLevel = RiskLevel.Low,
                    QuarterHired = 0,
                    IsAssigned = true,
                    AssignedDepartment = Department.Research,
                    PositionDescription = "Research scientist with experimental design expertise",
                    SkillKeywords = new List<string> { "research", "innovation", "analysis", "development" }
                },

                // Marketing Department - Mid-level marketer
                new Employee
                {
                    Name = "Alex Rodriguez",
                    OverallSkill = SkillLevel.Mid,
                    Specialization = Department.Marketing,
                    Experience = 5,
                    Productivity = 72,
                    Morale = 80,
                    Salary = 5200,
                    RiskLevel = RiskLevel.Low,
                    QuarterHired = 0,
                    IsAssigned = true,
                    AssignedDepartment = Department.Marketing,
                    PositionDescription = "Digital marketing specialist focused on social media growth",
                    SkillKeywords = new List<string> { "campaigns", "social media", "branding", "analytics" }
                },

                // HR Department - Experienced HR manager
                new Employee
                {
                    Name = "Jennifer Chen",
                    OverallSkill = SkillLevel.Senior,
                    Specialization = Department.HR,
                    Experience = 7,
                    Productivity = 78,
                    Morale = 85,
                    Salary = 6800,
                    RiskLevel = RiskLevel.VeryLow,
                    QuarterHired = 0,
                    IsAssigned = true,
                    AssignedDepartment = Department.HR,
                    PositionDescription = "HR generalist with policy development experience",
                    SkillKeywords = new List<string> { "recruitment", "policies", "culture", "training" }
                }
            };

            // Add employees to hired list
            hiredEmployees.AddRange(startingEmployees);

            // Assign employees to their departments
            foreach (var employee in startingEmployees)
            {
                if (employee.AssignedDepartment.HasValue)
                {
                    departments[employee.AssignedDepartment.Value].Employees.Add(employee);
                }
            }

            // Update company employee count
            company.EmployeeCount = hiredEmployees.Count;
        }

        public bool ShouldShowStoryEvent(int quarter)
        {
            return IsStoryMode && IsInTutorial && StoryScript.StoryEvents.ContainsKey(quarter) 
                   && !storyData.CompletedStoryEvents.Contains($"quarter_{quarter}");
        }

        public StoryEvent? GetStoryEventForQuarter(int quarter)
        {
            if (StoryScript.StoryEvents.TryGetValue(quarter, out StoryEvent? storyEvent))
            {
                return storyEvent;
            }
            return null;
        }

        public void ShowStoryGuide(int quarter, Window owner)
        {
            var storyEvent = GetStoryEventForQuarter(quarter);
            if (storyEvent != null)
            {
                // Unlock the mechanic BEFORE showing the dialog so the user can interact with it
                if (!storyData.UnlockedMechanics.Contains(storyEvent.IntroducedMechanic))
                {
                    System.Diagnostics.Debug.WriteLine($"Unlocking mechanic: {storyEvent.IntroducedMechanic} for Quarter {quarter}");
                    storyData.UnlockedMechanics.Add(storyEvent.IntroducedMechanic);
                    SaveStoryProgress();
                    
                    // Update the main window UI to reflect the newly unlocked mechanic
                    if (owner is MainWindow mainWindow)
                    {
                        System.Diagnostics.Debug.WriteLine("Refreshing MainWindow UI after unlocking mechanic");
                        mainWindow.RefreshUI();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Mechanic {storyEvent.IntroducedMechanic} already unlocked for Quarter {quarter}");
                }
                
                var guideWindow = new StoryModeGuide(storyEvent, quarter, storyData);
                guideWindow.Owner = owner;
                
                if (guideWindow.ShowDialog() == true)
                {
                    // Mark event as completed
                    storyData.CompletedStoryEvents.Add($"quarter_{quarter}");
                    
                    SaveStoryProgress();
                }
            }
        }

        public bool IsMechanicUnlocked(MechanicType mechanic)
        {
            if (!IsStoryMode) return true; // All mechanics available in sandbox
            return storyData.UnlockedMechanics.Contains(mechanic);
        }

        public void CompleteQuarter(int quarter)
        {
            storyData.CurrentQuarter = quarter;
            
            // Update narrative act based on quarter
            storyData.CurrentAct = StoryScript.GetNarrativeActForQuarter(quarter);
            
            // Check if tutorial phase is complete (after Q10)
            if (quarter > 10 && storyData.CurrentPhase == StoryPhase.Tutorial)
            {
                storyData.CurrentPhase = StoryPhase.FullMode;
                ShowGraduationMessage();
            }
            
            // Process character introductions and relationship updates
            if (characterManager != null)
            {
                ProcessCharacterEvents(quarter);
            }
            
            SaveStoryProgress();
        }

        private void ProcessCharacterEvents(int quarter)
        {
            if (characterManager == null) return;

            // Update Joan's relationship phase based on quarter progression
            characterManager.UpdateJoanPhaseForQuarter(quarter);

            // Check for character introductions
            foreach (var character in StoryScript.Characters.Values)
            {
                if (characterManager.ShouldIntroduceCharacter(character.CharacterId, quarter))
                {
                    characterManager.IntroduceCharacter(character.CharacterId);
                    // Add introduction event to story events
                    storyData.CompletedStoryEvents.Add($"character_intro_{character.CharacterId}_Q{quarter}");
                }
            }

            // Update ending probabilities
            storyData.EndingProgression.EndingProbabilities = characterManager.CalculateEndingProbabilities();
        }

        public void ProcessBusinessDecision(string decisionType, Dictionary<string, object> decisionData)
        {
            characterManager?.ProcessBusinessDecisionImpact(decisionType, decisionData);
            SaveStoryProgress();
        }

        public List<string> GetCharacterAdvice(string characterId)
        {
            return characterManager?.GetCharacterAdvice(characterId, company, storyData.CurrentQuarter) ?? new List<string>();
        }

        public bool IsCharacterAvailable(string characterId)
        {
            if (!IsStoryMode || characterManager == null) return false;
            
            var character = StoryScript.Characters.GetValueOrDefault(characterId);
            if (character == null) return false;
            
            return storyData.CurrentQuarter >= character.IntroductionQuarter;
        }

        /// <summary>
        /// Checks if a character has been introduced (alias for IsCharacterAvailable)
        /// </summary>
        public bool IsCharacterIntroduced(string characterId)
        {
            return IsCharacterAvailable(characterId);
        }

        private void ShowGraduationMessage()
        {
            System.Windows.MessageBox.Show(
                "🎓 Congratulations! You've completed the extended tutorial phase!\n\n" +
                "Secretary Joan says:\n" +
                "\"You've mastered both the fundamentals and advanced concepts of corporate management! " +
                "From basic operations to complex strategic thinking, you've shown excellent progress. " +
                "The full chaos system is now active - you're ready for the real corporate world!\"\n\n" +
                "You now have access to all game mechanics including the full ChaosEngine. " +
                "Good luck building your corporate empire!",
                "Tutorial Complete!",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        public List<string> GetAvailableFeatures()
        {
            var features = new List<string>();
            
            foreach (var mechanic in storyData.UnlockedMechanics)
            {
                if (StoryScript.MechanicDescriptions.TryGetValue(mechanic, out string? description))
                {
                    features.Add($"✅ {description}");
                }
            }
            
            // Add locked features
            foreach (MechanicType mechanic in Enum.GetValues<MechanicType>())
            {
                if (!storyData.UnlockedMechanics.Contains(mechanic))
                {
                    if (StoryScript.MechanicDescriptions.TryGetValue(mechanic, out string? description))
                    {
                        features.Add($"🔒 {description} (Unlocks in Q{GetUnlockQuarter(mechanic)})");
                    }
                }
            }
            
            return features;
        }

        private int GetUnlockQuarter(MechanicType mechanic)
        {
            // Find which quarter unlocks this mechanic
            foreach (var kvp in StoryScript.StoryEvents)
            {
                if (kvp.Value.IntroducedMechanic == mechanic)
                {
                    return kvp.Key;
                }
            }
            return 1;
        }

        public List<string> ProcessStoryModeEvents(int quarter, Dictionary<Department, DepartmentStats> departments)
        {
            var storyEvents = new List<string>();
            
            if (!IsStoryMode || !IsInTutorial) return storyEvents;

            // Apply story-specific modifications based on quarter
            switch (quarter)
            {
                case 2:
                    // Boost hiring quality for tutorial
                    int oldReputation = company.Reputation;
                    company.Reputation = Math.Max(company.Reputation, 15);
                    if (company.Reputation > oldReputation)
                    {
                        storyEvents.Add("📈 Company reputation improved due to initial success and market positioning");
                    }
                    break;
                    
                case 3:
                    // Ensure stable operations for department tutorial
                    int oldMorale = company.Morale;
                    company.Morale = Math.Max(company.Morale, 25);
                    if (company.Morale > oldMorale)
                    {
                        storyEvents.Add("😊 Employee morale boosted by effective department organization and clear leadership");
                    }
                    break;
                    
                case 4:
                    // Prepare for executive decisions tutorial
                    company.Capital += 25000;
                    storyEvents.Add("💰 Received $25,000 strategic investment fund for executive decision-making initiatives");
                    break;
                    
                case 5:
                    // Financial management tutorial - ensure good position
                    int oldRisk = company.Risk;
                    company.Risk = Math.Max(-20, company.Risk - 10);
                    if (company.Risk < oldRisk)
                    {
                        storyEvents.Add("🛡️ Risk levels reduced through improved financial planning and budget allocation");
                    }
                    break;
                    
                case 6:
                    // Trigger controlled supply chain crisis for tutorial - REDUCED for balance
                    company.Risk = Math.Min(25, company.Risk + 10); // Reduced from 35 and +15
                    double capitalLoss = Math.Min(25000, company.Capital * 0.04); // Reduced from 50000 and 0.08
                    company.Capital -= capitalLoss;
                    storyEvents.Add($"⚠️ Supply chain disruption caused ${capitalLoss:N0} in losses and increased operational risk");
                    storyEvents.Add("📋 Crisis management protocols activated - this is a learning opportunity for handling challenges");
                    break;
                    
                case 7:
                    // Advanced HR tutorial - create performance issues
                    // Slightly reduce some employee morale to demonstrate firing mechanics
                    foreach (var dept in departments.Values)
                    {
                        if (dept.Employees.Count > 0)
                        {
                            var randomEmployee = dept.Employees[new Random().Next(dept.Employees.Count)];
                            randomEmployee.Morale = Math.Max(30, randomEmployee.Morale - 15);
                            randomEmployee.Productivity = Math.Max(40, randomEmployee.Productivity - 10);
                            storyEvents.Add($"👤 {randomEmployee.Name} in {dept.Department} showing performance concerns - may require management attention");
                            storyEvents.Add("🎯 Advanced HR management now required - consider employee performance reviews");
                            break; // Only affect one employee
                        }
                    }
                    break;
                    
                case 8:
                    // Market analysis tutorial - competitor pressure - REDUCED for balance
                    double marketLoss = Math.Max(company.MarketShare - 1.0, 8.0) - company.MarketShare; // Reduced from -2.0 and 7.0
                    company.MarketShare = Math.Max(company.MarketShare - 1.0, 8.0);
                    company.Reputation -= 4; // Reduced from 8
                    company.Risk += 6; // Reduced from 12
                    storyEvents.Add($"🏢 Major competitor launched aggressive campaign - lost {Math.Abs(marketLoss):F1}% market share");
                    storyEvents.Add("📉 Reputation and risk levels affected by increased market competition");
                    storyEvents.Add("🎯 Strategic market analysis and competitive response now critical");
                    break;
                    
                case 9:
                    // Risk management tutorial - multiple challenges - REDUCED for balance
                    company.Risk += 10; // Reduced from 20
                    double riskCapitalLoss = Math.Min(35000, company.Capital * 0.06); // Reduced from 75000 and 0.12
                    company.Capital -= riskCapitalLoss;
                    company.Morale -= 5; // Reduced from 10
                    storyEvents.Add($"🌪️ Multiple business challenges emerged simultaneously - ${riskCapitalLoss:N0} impact");
                    storyEvents.Add("⚠️ Risk levels elevated across all departments - comprehensive risk management needed");
                    storyEvents.Add("😰 Employee morale affected by uncertainty - leadership response crucial");
                    break;
                    
                case 10:
                    // Final challenge - test all skills with support
                    if (company.MarketShare < 12)
                    {
                        // Give a boost if struggling for final challenge
                        company.MarketShare += 1.5;
                        storyEvents.Add("📈 Strategic market positioning improved through focused efforts");
                    }
                    if (company.Morale < 60)
                    {
                        // Morale boost if needed
                        company.Morale += 10;
                        storyEvents.Add("😊 Employee confidence restored through effective leadership");
                    }
                    // Add some capital for final strategic decisions
                    company.Capital += 50000;
                    storyEvents.Add("💰 Received $50,000 strategic development fund for final tutorial challenges");
                    storyEvents.Add("🎓 Final tutorial phase - demonstrate mastery of all corporate management skills");
                    break;
            }
            
            return storyEvents;
        }

        private void LoadStoryProgress()
        {
            try
            {
                if (File.Exists(STORY_SAVE_FILE))
                {
                    var json = File.ReadAllText(STORY_SAVE_FILE);
                    
                    // Try to deserialize as ExtendedStoryModeData first
                    try
                    {
                        storyData = JsonSerializer.Deserialize<ExtendedStoryModeData>(json) ?? new ExtendedStoryModeData();
                    }
                    catch
                    {
                        // Fallback to legacy StoryModeData and migrate
                        var legacyData = JsonSerializer.Deserialize<StoryModeData>(json);
                        if (legacyData != null)
                        {
                            storyData = MigrateLegacyData(legacyData);
                        }
                        else
                        {
                            storyData = new ExtendedStoryModeData { IsStoryMode = false };
                        }
                    }
                }
                else
                {
                    storyData = new ExtendedStoryModeData { IsStoryMode = false };
                }

                // Initialize character manager if in story mode
                if (storyData.IsStoryMode)
                {
                    characterManager = new CharacterManager(storyData, company);
                    
                    // Initialize narrative engine
                    narrativeEngine = new NarrativeEngine(storyData, company, characterManager);
                    narrativeEngine.InitializeContentDistributor();
                    
                    // Initialize ending tracker
                    endingTracker = new EndingProbabilityTracker(storyData, company);
                    
                    // Initialize advice system
                    narrativeEngine.InitializeAdviceSystem(endingTracker);
                }
            }
            catch
            {
                storyData = new ExtendedStoryModeData { IsStoryMode = false };
            }
        }

        private ExtendedStoryModeData MigrateLegacyData(StoryModeData legacyData)
        {
            var extendedData = new ExtendedStoryModeData
            {
                CurrentQuarter = legacyData.CurrentQuarter,
                CurrentPhase = legacyData.CurrentPhase,
                UnlockedMechanics = legacyData.UnlockedMechanics,
                CompletedTutorials = legacyData.CompletedTutorials,
                CompletedStoryEvents = legacyData.CompletedStoryEvents,
                IsStoryMode = legacyData.IsStoryMode,
                CurrentAct = StoryScript.GetNarrativeActForQuarter(legacyData.CurrentQuarter)
            };

            return extendedData;
        }

        private void SaveStoryProgress()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(storyData, options);
                File.WriteAllText(STORY_SAVE_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving story progress: {ex.Message}");
            }
        }

        public void ResetStoryMode()
        {
            if (File.Exists(STORY_SAVE_FILE))
            {
                File.Delete(STORY_SAVE_FILE);
            }
            storyData = new ExtendedStoryModeData { IsStoryMode = false };
            characterManager = null;
        }

        // Dialogue adaptation methods for enhanced story experience
        public DialogueConversation? CreateAdaptiveDialogue(string characterId, string context)
        {
            if (!IsStoryMode || characterManager == null) return null;
            
            // TODO: Implement DialogueSystem integration
            // var dialogueSystem = new CorporateChaos.Systems.DialogueSystem(storyData, company, characterManager);
            // return dialogueSystem.CreateConversation(characterId, context);
            return null;
        }

        public DialogueNode? GetAdaptiveDialogueNode(string characterId, string context)
        {
            if (!IsStoryMode || characterManager == null) return null;
            
            // TODO: Implement DialogueSystem integration
            // var dialogueSystem = new CorporateChaos.Systems.DialogueSystem(storyData, company, characterManager);
            // return dialogueSystem.GetAdaptiveDialogueNode(characterId, context);
            return null;
        }

        public void ShowAdaptiveCharacterDialogue(string characterId, string context, Dictionary<Department, DepartmentStats> departments, Window owner)
        {
            if (!IsStoryMode || characterManager == null) return;
            
            var conversation = CreateAdaptiveDialogue(characterId, context);
            if (conversation == null) return;
            
            var dialogue = new JoanDialogue(
                company, 
                departments, 
                conversation, 
                storyData.CharacterRelationships, 
                storyData.StoryFlags, 
                true, 
                storyData.CurrentQuarter, 
                this
            );
            
            dialogue.Owner = owner;
            dialogue.ShowDialog();
        }

        public void ShowJoanAdaptiveDialogue(Dictionary<Department, DepartmentStats> departments, Window owner, string context = "quarterly_review")
        {
            if (!IsStoryMode || characterManager == null) 
            {
                // Fallback to traditional dialogue
                var traditionalDialogue = new JoanDialogue(company, departments, true, storyData.CurrentQuarter, this);
                traditionalDialogue.Owner = owner;
                traditionalDialogue.ShowDialog();
                return;
            }
            
            // Create adaptive dialogue using the enhanced constructor
            var dialogue = new JoanDialogue(
                company, 
                departments, 
                null, // No pre-built conversation - let JoanDialogue create it
                storyData.CharacterRelationships, 
                storyData.StoryFlags, 
                true, 
                storyData.CurrentQuarter, 
                this
            );
            
            dialogue.Owner = owner;
            dialogue.ShowDialog();
        }

        public void ApplyChoiceConsequences(string characterId, DialogueChoice choice)
        {
            if (!IsStoryMode || characterManager == null) return;
            
            // Apply relationship changes
            characterManager.UpdateCharacterRelationship(
                characterId,
                choice.RelationshipImpact.TrustChange,
                choice.RelationshipImpact.RespectChange,
                choice.RelationshipImpact.PersonalConnectionChange,
                choice.RelationshipImpact.ImpactDescription
            );
            
            // Add consequence flags to story flags
            foreach (var flag in choice.ConsequenceFlags)
            {
                if (!storyData.StoryFlags.Contains(flag))
                {
                    storyData.StoryFlags.Add(flag);
                }
            }
            
            // Apply secondary relationship effects
            foreach (var secondaryEffect in choice.RelationshipImpact.SecondaryEffects)
            {
                if (storyData.CharacterRelationships.ContainsKey(secondaryEffect.Key))
                {
                    characterManager.UpdateCharacterRelationship(
                        secondaryEffect.Key,
                        secondaryEffect.Value / 3, // Distribute the effect across trust/respect/personal
                        secondaryEffect.Value / 3,
                        secondaryEffect.Value / 3,
                        $"Secondary effect from interaction with {characterId}"
                    );
                }
            }
            
            SaveStoryProgress();
        }

        public void RecordPlayerChoice(StoryChoiceRecord choiceRecord)
        {
            if (!IsStoryMode) return;
            
            // Add the choice to the history
            storyData.ChoiceHistory.Add(choiceRecord);
            
            // Save the updated story progress
            SaveStoryProgress();
        }

        public void TriggerChoiceConsequences(int currentQuarter)
        {
            if (!IsStoryMode) return;
            
            // Find choices that have consequences scheduled for this quarter
            var relevantChoices = storyData.ChoiceHistory
                .Where(c => c.ConsequenceFlags.Any(f => f.StartsWith($"trigger_q{currentQuarter}:")))
                .ToList();
            
            foreach (var choice in relevantChoices)
            {
                // Process each consequence flag that's scheduled for this quarter
                var quarterFlags = choice.ConsequenceFlags
                    .Where(f => f.StartsWith($"trigger_q{currentQuarter}:"))
                    .ToList();
                
                foreach (var flag in quarterFlags)
                {
                    // Extract the consequence type from the flag
                    // Format: "trigger_q{quarter}:{consequence_type}"
                    var parts = flag.Split(':');
                    if (parts.Length >= 2)
                    {
                        var consequenceType = parts[1];
                        ProcessChoiceConsequence(choice, consequenceType, currentQuarter);
                    }
                }
            }
        }

        private void ProcessChoiceConsequence(StoryChoiceRecord choice, string consequenceType, int currentQuarter)
        {
            // Process different types of consequences
            switch (consequenceType)
            {
                case "relationship_change":
                    // Apply delayed relationship changes
                    foreach (var impact in choice.RelationshipImpacts)
                    {
                        if (storyData.CharacterRelationships.ContainsKey(impact.Key))
                        {
                            var relationship = storyData.CharacterRelationships[impact.Key];
                            relationship.TrustLevel = Math.Clamp(relationship.TrustLevel + impact.Value, -100, 100);
                        }
                    }
                    break;
                
                case "story_event":
                    // Trigger a story event based on the choice
                    AddStoryFlag($"choice_consequence_{choice.ChoiceId}_q{currentQuarter}");
                    break;
                
                case "character_reaction":
                    // Mark that a character will react to this choice
                    AddStoryFlag($"character_reaction_{choice.EventId}");
                    break;
                
                default:
                    // Generic consequence - just add a flag
                    AddStoryFlag($"consequence_{consequenceType}_q{currentQuarter}");
                    break;
            }
            
            SaveStoryProgress();
        }

        public bool ShouldUseAdaptiveDialogue()
        {
            // Use adaptive dialogue for story mode after tutorial phase
            return IsStoryMode && storyData.CurrentQuarter > 5;
        }

        public string GetCharacterRelationshipSummary(string characterId)
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return "No relationship established";
            
            var relationship = storyData.CharacterRelationships[characterId];
            var phase = relationship.CurrentPhase;
            
            return phase switch
            {
                RelationshipPhase.FirstMeeting => "Just met",
                RelationshipPhase.ProfessionalAcquaintance => "Professional colleague",
                RelationshipPhase.TrustedColleague => "Trusted advisor",
                RelationshipPhase.PersonalFriend => "Personal friend",
                RelationshipPhase.LifelongBond => "Lifelong partner",
                RelationshipPhase.Strained => "Relationship strained",
                RelationshipPhase.Hostile => "Hostile relationship",
                _ => "Unknown relationship"
            };
        }

        public List<string> GetActiveStoryFlags()
        {
            return new List<string>(storyData.StoryFlags);
        }

        public void AddStoryFlag(string flag)
        {
            if (!storyData.StoryFlags.Contains(flag))
            {
                storyData.StoryFlags.Add(flag);
                SaveStoryProgress();
            }
        }

        public void RemoveStoryFlag(string flag)
        {
            if (storyData.StoryFlags.Remove(flag))
            {
                SaveStoryProgress();
            }
        }
    }
}