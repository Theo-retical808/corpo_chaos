using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CorporateChaos.Models;
using CorporateChaos.Systems;

namespace CorporateChaos.Views
{
    public partial class JoanDialogue : Window
    {
        private Company company;
        private Dictionary<Department, DepartmentStats> departments;
        private bool isStoryMode;
        private int currentQuarter;
        private Random random = new Random();
        private StoryModeManager? storyModeManager;
        
        // Enhanced dialogue system properties
        private DialogueConversation? currentConversation;
        private DialogueNode? currentDialogueNode;
        private bool isBranchingMode = false;
        private Dictionary<string, CharacterRelationship> characterRelationships = new Dictionary<string, CharacterRelationship>();
        private List<string> activeStoryFlags = new List<string>();
        private string currentCharacterId = "joan"; // Track which character is speaking

        public JoanDialogue(Company company, Dictionary<Department, DepartmentStats> departments, bool isStoryMode = false, int quarter = 1, StoryModeManager? storyModeManager = null)
        {
            InitializeComponent();
            this.company = company;
            this.departments = departments;
            this.isStoryMode = isStoryMode;
            this.currentQuarter = quarter;
            this.storyModeManager = storyModeManager;
            
            InitializeDialogueSystem();
            LoadJoanAvatar();
            GenerateDialogue();
        }

        // Constructor for branching conversation mode
        public JoanDialogue(Company company, Dictionary<Department, DepartmentStats> departments, DialogueConversation? conversation, Dictionary<string, CharacterRelationship> relationships, List<string> storyFlags, bool isStoryMode = false, int quarter = 1, StoryModeManager? storyModeManager = null)
        {
            InitializeComponent();
            this.company = company;
            this.departments = departments;
            this.isStoryMode = isStoryMode;
            this.currentQuarter = quarter;
            this.storyModeManager = storyModeManager;
            this.currentConversation = conversation;
            this.characterRelationships = relationships;
            this.activeStoryFlags = storyFlags;
            this.isBranchingMode = conversation != null; // Only use branching mode if conversation is provided
            
            InitializeDialogueSystem();
            LoadJoanAvatar();
            
            if (isBranchingMode)
            {
                LoadBranchingConversation();
            }
            else
            {
                // Use adaptive dialogue generation
                GenerateAdaptiveDialogue();
            }
        }

        private void InitializeDialogueSystem()
        {
            // Initialize character relationships if not provided
            if (!characterRelationships.ContainsKey("joan"))
            {
                characterRelationships["joan"] = new CharacterRelationship
                {
                    TrustLevel = 50,
                    ProfessionalRespect = 60,
                    PersonalConnection = 30,
                    CurrentPhase = StoryScript.GetJoanPhaseForQuarter(currentQuarter)
                };
            }
            
            // Initialize story flags based on current game state
            if (activeStoryFlags.Count == 0)
            {
                activeStoryFlags.Add($"quarter_{currentQuarter}");
                if (isStoryMode) activeStoryFlags.Add("story_mode");
                if (currentQuarter <= 10) activeStoryFlags.Add("tutorial_phase");
            }
        }

        private void LoadJoanAvatar()
        {
            try
            {
                // Try to load assistant.png first
                var assistantUri = new Uri("pack://application:,,,/images/assistant.png");
                var assistantImage = new BitmapImage();
                assistantImage.BeginInit();
                assistantImage.UriSource = assistantUri;
                assistantImage.CacheOption = BitmapCacheOption.OnLoad;
                assistantImage.EndInit();
                
                JoanDialogueAvatar.Source = assistantImage;
            }
            catch
            {
                try
                {
                    // Fallback to human_resources.png
                    var fallbackUri = new Uri("pack://application:,,,/images/human_resources.png");
                    var fallbackImage = new BitmapImage();
                    fallbackImage.BeginInit();
                    fallbackImage.UriSource = fallbackUri;
                    fallbackImage.CacheOption = BitmapCacheOption.OnLoad;
                    fallbackImage.EndInit();
                    
                    JoanDialogueAvatar.Source = fallbackImage;
                }
                catch
                {
                    // If all else fails, leave it empty
                }
            }
        }

        private void GenerateDialogue()
        {
            if (isBranchingMode && currentConversation != null)
            {
                LoadBranchingConversation();
                return;
            }

            var dialogue = GenerateContextualDialogue();
            JoanDialogueText.Text = dialogue.MainMessage;
            SituationAnalysisText.Text = dialogue.SituationAnalysis;
            RecommendationsText.Text = dialogue.Recommendations;
            
            // Update Joan's role text based on story progression
            if (isStoryMode && storyModeManager != null)
            {
                var joanPhase = StoryScript.GetJoanPhaseForQuarter(currentQuarter);
                JoanRoleText.Text = joanPhase switch
                {
                    RelationshipPhase.ProfessionalAcquaintance => "Your Professional Corporate Assistant",
                    RelationshipPhase.TrustedColleague => "Your Trusted Corporate Advisor",
                    RelationshipPhase.PersonalFriend => "Your Personal Confidant & Advisor",
                    RelationshipPhase.LifelongBond => "Your Lifelong Friend & Trusted Partner",
                    _ => "Your Personal Corporate Assistant"
                };
            }
            
            // Update status based on company situation
            if (company.Capital < 100000)
                JoanStatusText.Text = "Concerned about finances";
            else if (company.Morale < -20)
                JoanStatusText.Text = "Worried about employee morale";
            else if (company.Risk > 50)
                JoanStatusText.Text = "Monitoring risk levels";
            else
                JoanStatusText.Text = "Ready to help!";
                
            // Show traditional buttons for legacy mode
            TraditionalButtonsPanel.Visibility = Visibility.Visible;
            BranchingChoicesPanel.Visibility = Visibility.Collapsed;
        }

        private void GenerateAdaptiveDialogue()
        {
            // Generate adaptive dialogue based on relationships and story context
            var adaptedDialogue = GenerateRelationshipAwareDialogue();
            JoanDialogueText.Text = adaptedDialogue.MainMessage;
            SituationAnalysisText.Text = adaptedDialogue.SituationAnalysis;
            RecommendationsText.Text = adaptedDialogue.Recommendations;
            
            // Update Joan's role text based on relationship
            UpdateCharacterInfoForAdaptive();
            
            // Show traditional buttons for now (can be enhanced later with choices)
            TraditionalButtonsPanel.Visibility = Visibility.Visible;
            BranchingChoicesPanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateCharacterInfoForAdaptive()
        {
            if (!characterRelationships.ContainsKey("joan")) return;
            
            var relationship = characterRelationships["joan"];
            
            // Update role text based on relationship phase
            JoanRoleText.Text = relationship.CurrentPhase switch
            {
                RelationshipPhase.ProfessionalAcquaintance => "Your Professional Corporate Assistant",
                RelationshipPhase.TrustedColleague => "Your Trusted Corporate Advisor", 
                RelationshipPhase.PersonalFriend => "Your Personal Confidant & Advisor",
                RelationshipPhase.LifelongBond => "Your Lifelong Friend & Trusted Partner",
                RelationshipPhase.Strained => "Your Assistant (relationship strained)",
                RelationshipPhase.Hostile => "Your Assistant (relationship hostile)",
                _ => "Your Personal Corporate Assistant"
            };
            
            // Update status based on relationship levels
            if (relationship.TrustLevel >= 80)
                JoanStatusText.Text = "Completely trusts you";
            else if (relationship.TrustLevel >= 60)
                JoanStatusText.Text = "Has strong confidence in you";
            else if (relationship.TrustLevel >= 40)
                JoanStatusText.Text = "Building trust with you";
            else if (relationship.TrustLevel >= 20)
                JoanStatusText.Text = "Still getting to know you";
            else if (relationship.TrustLevel >= 0)
                JoanStatusText.Text = "Cautious about your decisions";
            else
                JoanStatusText.Text = "Concerned about your leadership";
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GenerateRelationshipAwareDialogue()
        {
            // Get base dialogue
            var baseDialogue = GenerateContextualDialogue();
            
            // Adapt based on relationship if available
            if (characterRelationships.ContainsKey("joan"))
            {
                var relationship = characterRelationships["joan"];
                var adaptedDialogue = AdaptDialogueForRelationship(baseDialogue, relationship);
                return adaptedDialogue;
            }
            
            return baseDialogue;
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) AdaptDialogueForRelationship(
            (string MainMessage, string SituationAnalysis, string Recommendations) baseDialogue, 
            CharacterRelationship relationship)
        {
            var adaptedMessage = baseDialogue.MainMessage;
            var adaptedAnalysis = baseDialogue.SituationAnalysis;
            var adaptedRecommendations = baseDialogue.Recommendations;
            
            // Apply relationship-based modifications to the main message
            switch (relationship.CurrentPhase)
            {
                case RelationshipPhase.FirstMeeting:
                    adaptedMessage = $"Good morning! I'm Secretary Joan, your personal assistant. {adaptedMessage}";
                    break;
                    
                case RelationshipPhase.ProfessionalAcquaintance:
                    if (relationship.TrustLevel > 20)
                    {
                        adaptedMessage = $"I've been observing your management style, and I think you should know: {adaptedMessage}";
                    }
                    break;
                    
                case RelationshipPhase.TrustedColleague:
                    adaptedMessage = $"I feel comfortable being direct with you now: {adaptedMessage}";
                    break;
                    
                case RelationshipPhase.PersonalFriend:
                    adaptedMessage = $"You know, after all we've been through together: {adaptedMessage}";
                    break;
                    
                case RelationshipPhase.LifelongBond:
                    adaptedMessage = $"After all these years together, I can say with certainty: {adaptedMessage}";
                    break;
                    
                case RelationshipPhase.Strained:
                    adaptedMessage = $"I'm trying to remain professional despite our recent... difficulties: {adaptedMessage}";
                    break;
                    
                case RelationshipPhase.Hostile:
                    adaptedMessage = $"I'm obligated to inform you, though I question your judgment: {adaptedMessage}";
                    break;
            }
            
            // Apply context-specific adaptations based on story flags
            adaptedMessage = ApplyStoryFlagAdaptations(adaptedMessage, relationship);
            
            // Enhance situation analysis with relationship context
            adaptedAnalysis = EnhanceSituationAnalysisWithRelationship(adaptedAnalysis, relationship);
            
            return (adaptedMessage, adaptedAnalysis, adaptedRecommendations);
        }

        private string ApplyStoryFlagAdaptations(string message, CharacterRelationship relationship)
        {
            // Apply adaptations based on active story flags
            if (activeStoryFlags.Contains("first_crisis_handled"))
            {
                message = $"After how well you handled our last crisis: {message}";
            }
            
            if (activeStoryFlags.Contains("market_leader"))
            {
                message = $"Now that we're market leaders: {message}";
            }
            
            // Apply narrative act context
            var narrativeAct = StoryScript.GetNarrativeActForQuarter(currentQuarter);
            switch (narrativeAct)
            {
                case NarrativeAct.Tutorial:
                    if (currentQuarter <= 5)
                    {
                        message = $"As we're still learning together: {message}";
                    }
                    break;
                case NarrativeAct.RisingAction:
                    message = $"The stakes are getting higher now. {message}";
                    break;
                case NarrativeAct.Climax:
                    message = $"This is a critical moment for our company. {message}";
                    break;
                case NarrativeAct.Resolution:
                    message = $"Looking back on our journey together: {message}";
                    break;
            }
            
            // Apply company performance context
            if (company.MarketShare > 50)
            {
                message = $"Given our market leadership position: {message}";
            }
            else if (company.ConsecutiveNegativeQuarters > 0)
            {
                message = $"I know times are tough right now, but: {message}";
            }
            else if (company.Capital > 500000000)
            {
                message = $"With our impressive financial position: {message}";
            }
            
            return message;
        }

        private string EnhanceSituationAnalysisWithRelationship(string baseAnalysis, CharacterRelationship relationship)
        {
            var enhancedAnalysis = new List<string> { baseAnalysis };
            
            // Add relationship context
            enhancedAnalysis.Add($"\n💭 Relationship Status:");
            enhancedAnalysis.Add($"🤝 Trust Level: {GetRelationshipDescription(relationship.TrustLevel)} ({relationship.TrustLevel})");
            enhancedAnalysis.Add($"💼 Professional Respect: {GetRelationshipDescription(relationship.ProfessionalRespect)} ({relationship.ProfessionalRespect})");
            enhancedAnalysis.Add($"💗 Personal Connection: {GetRelationshipDescription(relationship.PersonalConnection)} ({relationship.PersonalConnection})");
            
            // Add shared experiences if any
            if (relationship.SharedExperiences.Any())
            {
                enhancedAnalysis.Add($"📚 Recent Experiences: {relationship.SharedExperiences.Count} shared moments");
            }
            
            return string.Join("\n", enhancedAnalysis);
        }

        private void LoadBranchingConversation()
        {
            if (currentConversation == null) return;

            // Detect which character is speaking from the conversation
            if (currentConversation.Participants != null && currentConversation.Participants.Count > 0)
            {
                // Find the non-player character
                currentCharacterId = currentConversation.Participants.FirstOrDefault(p => p != "player") ?? "joan";
            }

            // Get the current dialogue node
            if (currentConversation.Nodes.ContainsKey(currentConversation.CurrentNodeId))
            {
                currentDialogueNode = currentConversation.Nodes[currentConversation.CurrentNodeId];
                
                // Display adaptive dialogue text
                var dialogueText = currentDialogueNode.GetAdaptiveDialogueText(characterRelationships, activeStoryFlags);
                JoanDialogueText.Text = dialogueText;
                
                // Update character info based on relationship
                UpdateCharacterInfoForBranching();
                
                // Generate situation analysis for branching mode
                SituationAnalysisText.Text = GenerateBranchingSituationAnalysis();
                RecommendationsText.Text = "Choose your response carefully - your choice will affect your relationship and future interactions.";
                
                // Show branching choices
                DisplayBranchingChoices();
                
                // Hide traditional buttons and show branching panel
                TraditionalButtonsPanel.Visibility = Visibility.Collapsed;
                BranchingChoicesPanel.Visibility = Visibility.Visible;
            }
        }

        private void UpdateCharacterInfoForBranching()
        {
            // Get character info from StoryScript
            if (StoryScript.Characters.ContainsKey(currentCharacterId))
            {
                var character = StoryScript.Characters[currentCharacterId];
                CharacterNameText.Text = character.Name;
                Title = $"{character.Name} - Conversation";
                
                // Load character avatar
                LoadCharacterAvatar(currentCharacterId);
            }
            
            if (!characterRelationships.ContainsKey(currentCharacterId)) return;
            
            var relationship = characterRelationships[currentCharacterId];
            
            // Update role text based on character
            if (currentCharacterId == "joan")
            {
                JoanRoleText.Text = relationship.CurrentPhase switch
                {
                    RelationshipPhase.ProfessionalAcquaintance => "Your Professional Corporate Assistant",
                    RelationshipPhase.TrustedColleague => "Your Trusted Corporate Advisor", 
                    RelationshipPhase.PersonalFriend => "Your Personal Confidant & Advisor",
                    RelationshipPhase.LifelongBond => "Your Lifelong Friend & Trusted Partner",
                    _ => "Your Personal Corporate Assistant"
                };
            }
            else if (StoryScript.Characters.ContainsKey(currentCharacterId))
            {
                var character = StoryScript.Characters[currentCharacterId];
                JoanRoleText.Text = character.Role;
            }
            
            // Update status based on relationship levels
            if (relationship.TrustLevel >= 80)
                JoanStatusText.Text = "Completely trusts you";
            else if (relationship.TrustLevel >= 60)
                JoanStatusText.Text = "Has strong confidence in you";
            else if (relationship.TrustLevel >= 40)
                JoanStatusText.Text = "Building trust with you";
            else if (relationship.TrustLevel >= 20)
                JoanStatusText.Text = "Still getting to know you";
            else
                JoanStatusText.Text = "Cautious about your decisions";
        }
        
        private void LoadCharacterAvatar(string characterId)
        {
            try
            {
                string imagePath = characterId switch
                {
                    "joan" => "images/assistant.png",
                    "marcus_vey" => "images/char/marcus_vey.png",
                    "evelyn_cross" => "images/char/evelyn_cross.png",
                    "vincent_duro" => "images/char/vincent_duro.png",
                    "lucinda_vale" => "images/char/lucinda_vale.png",
                    "gregory_shaw" => "images/char/gregory_shaw.png",
                    "selena_park" => "images/char/selena_park.png",
                    "harold_finch" => "images/char/harold_finch.png",
                    "sophie_kim" => "images/char/sophie_kim.png",
                    _ => "images/assistant.png"
                };
                
                var uri = new Uri($"pack://application:,,,/{imagePath}");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                JoanDialogueAvatar.Source = bitmap;
            }
            catch
            {
                // Fallback to default
            }
        }

        private string GenerateBranchingSituationAnalysis()
        {
            if (currentDialogueNode == null) return "Analyzing conversation context...";
            
            var analysis = new List<string>();
            
            // Add emotional tone context
            analysis.Add($"💭 Conversation Tone: {GetEmotionalToneDescription(currentDialogueNode.EmotionalTone)}");
            
            // Add relationship context
            if (characterRelationships.ContainsKey("joan"))
            {
                var relationship = characterRelationships["joan"];
                analysis.Add($"🤝 Trust Level: {GetRelationshipDescription(relationship.TrustLevel)}");
                analysis.Add($"💼 Professional Respect: {GetRelationshipDescription(relationship.ProfessionalRespect)}");
                analysis.Add($"💗 Personal Connection: {GetRelationshipDescription(relationship.PersonalConnection)}");
            }
            
            // Add context tags
            if (currentDialogueNode.ContextTags.Any())
            {
                analysis.Add($"📋 Context: {string.Join(", ", currentDialogueNode.ContextTags)}");
            }
            
            return string.Join("\n", analysis);
        }

        private string GetEmotionalToneDescription(EmotionalTone tone)
        {
            return tone switch
            {
                EmotionalTone.Positive => "Optimistic and encouraging",
                EmotionalTone.Negative => "Concerned or disappointed", 
                EmotionalTone.Tense => "Stressful situation requiring care",
                EmotionalTone.Warm => "Friendly and supportive",
                EmotionalTone.Serious => "Important matter needing attention",
                EmotionalTone.Playful => "Light-hearted and casual",
                EmotionalTone.Concerned => "Worried about current situation",
                EmotionalTone.Excited => "Enthusiastic about opportunities",
                EmotionalTone.Disappointed => "Let down by recent events",
                _ => "Professional and neutral"
            };
        }

        private string GetRelationshipDescription(int level)
        {
            return level switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good", 
                >= 40 => "Moderate",
                >= 20 => "Developing",
                _ => "Needs Work"
            };
        }

        private void DisplayBranchingChoices()
        {
            if (currentDialogueNode == null) return;
            
            // Clear existing choice buttons
            ChoiceButtonsContainer.Children.Clear();
            
            // Get available choices based on relationship and story flags
            var availableChoices = currentDialogueNode.GetAvailableChoices(characterRelationships, activeStoryFlags);
            
            foreach (var choice in availableChoices)
            {
                CreateChoiceButton(choice);
            }
        }

        private void CreateChoiceButton(DialogueChoice choice)
        {
            var button = new Button
            {
                Height = 50,
                Margin = new Thickness(0, 0, 0, 8),
                Background = GetToneColor(choice.Tone),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 8, 15, 8),
                Tag = choice,
                Cursor = System.Windows.Input.Cursors.Hand,
                IsHitTestVisible = true,
                Focusable = true
            };
            
            // Create button content with tone indicator and risk level
            var content = new StackPanel { Orientation = Orientation.Horizontal, IsHitTestVisible = false };
            
            // Tone indicator
            var toneIndicator = new TextBlock
            {
                Text = choice.GetToneIndicator(),
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            content.Children.Add(toneIndicator);
            
            // Risk indicator
            var riskIndicator = new TextBlock
            {
                Text = choice.GetRiskIndicator(),
                FontSize = 14,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            content.Children.Add(riskIndicator);
            
            // Choice text
            var choiceText = new TextBlock
            {
                Text = choice.ChoiceText,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 450,
                IsHitTestVisible = false
            };
            content.Children.Add(choiceText);
            
            button.Content = content;
            
            // Add click handler
            button.Click += (s, e) => HandleChoiceSelection(choice);
            
            // Add hover style with proper template
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.BackgroundProperty, GetToneColor(choice.Tone)));
            style.Setters.Add(new Setter(Button.CursorProperty, System.Windows.Input.Cursors.Hand));
            
            // Create control template for better button rendering
            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            borderFactory.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));
            
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            borderFactory.AppendChild(contentPresenterFactory);
            template.VisualTree = borderFactory;
            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            
            var trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Button.BackgroundProperty, GetToneHoverColor(choice.Tone)));
            style.Triggers.Add(trigger);
            
            button.Style = style;
            
            ChoiceButtonsContainer.Children.Add(button);
        }

        private Brush GetToneColor(ChoiceTone tone)
        {
            return tone switch
            {
                ChoiceTone.Professional => new SolidColorBrush(Color.FromRgb(42, 77, 58)),  // #2a4d3a
                ChoiceTone.Supportive => new SolidColorBrush(Color.FromRgb(42, 77, 77)),    // #2a4d4d
                ChoiceTone.Aggressive => new SolidColorBrush(Color.FromRgb(77, 42, 42)),    // #4d2a2a
                ChoiceTone.Diplomatic => new SolidColorBrush(Color.FromRgb(77, 58, 42)),    // #4d3a2a
                ChoiceTone.Personal => new SolidColorBrush(Color.FromRgb(77, 42, 77)),      // #4d2a4d
                ChoiceTone.Humorous => new SolidColorBrush(Color.FromRgb(58, 42, 77)),     // #3a2a4d
                _ => new SolidColorBrush(Color.FromRgb(42, 42, 77))                        // #2a2a4d
            };
        }

        private Brush GetToneHoverColor(ChoiceTone tone)
        {
            return tone switch
            {
                ChoiceTone.Professional => new SolidColorBrush(Color.FromRgb(58, 109, 74)), // #3a6d4a
                ChoiceTone.Supportive => new SolidColorBrush(Color.FromRgb(58, 109, 109)),   // #3a6d6d
                ChoiceTone.Aggressive => new SolidColorBrush(Color.FromRgb(109, 58, 58)),    // #6d3a3a
                ChoiceTone.Diplomatic => new SolidColorBrush(Color.FromRgb(109, 90, 58)),    // #6d5a3a
                ChoiceTone.Personal => new SolidColorBrush(Color.FromRgb(109, 58, 109)),     // #6d3a6d
                ChoiceTone.Humorous => new SolidColorBrush(Color.FromRgb(90, 58, 109)),     // #5a3a6d
                _ => new SolidColorBrush(Color.FromRgb(58, 58, 109))                        // #3a3a6d
            };
        }

        private void HandleChoiceSelection(DialogueChoice choice)
        {
            // Apply relationship changes
            ApplyChoiceConsequences(choice);
            
            // Record the choice in story mode
            if (storyModeManager != null && storyModeManager.IsStoryMode)
            {
                RecordStoryChoice(choice);
            }
            
            // Add choice to conversation history
            if (currentConversation != null)
            {
                currentConversation.ConversationHistory.Add($"Player: {choice.ChoiceText}");
            }
            
            // Show character reaction if available
            if (!string.IsNullOrEmpty(choice.CharacterReaction))
            {
                ShowCharacterReaction(choice.CharacterReaction);
            }
            
            // Close dialogue or continue to next node
            if (string.IsNullOrEmpty(choice.NextNodeId) || choice.NextNodeId == "end")
            {
                Close();
            }
            else
            {
                // Navigate to next dialogue node (for future implementation)
                NavigateToNextNode(choice.NextNodeId);
            }
        }

        private void RecordStoryChoice(DialogueChoice choice)
        {
            if (storyModeManager == null) return;
            
            // Determine the character ID (default to "joan" for now)
            string characterId = currentDialogueNode?.CharacterId ?? "joan";
            
            // Determine the event ID
            string eventId = currentConversation?.ConversationId ?? $"dialogue_q{currentQuarter}";
            
            // Create the choice record
            var choiceRecord = new StoryChoiceRecord
            {
                Quarter = currentQuarter,
                EventId = eventId,
                ChoiceId = choice.ChoiceId,
                ChoiceText = choice.ChoiceText,
                RelationshipImpacts = new Dictionary<string, int>(choice.RelationshipChanges),
                ConsequenceFlags = new List<string>(choice.ConsequenceFlags)
            };
            
            // Record the choice through the story mode manager
            storyModeManager.RecordPlayerChoice(choiceRecord);
        }

        private void ApplyChoiceConsequences(DialogueChoice choice)
        {
            // Apply relationship changes
            foreach (var relationshipChange in choice.RelationshipChanges)
            {
                if (characterRelationships.ContainsKey(relationshipChange.Key))
                {
                    var relationship = characterRelationships[relationshipChange.Key];
                    relationship.TrustLevel = Math.Clamp(relationship.TrustLevel + relationshipChange.Value, -100, 100);
                }
            }
            
            // Apply enhanced relationship impact
            if (!string.IsNullOrEmpty(choice.RelationshipImpact.PrimaryCharacter) && 
                characterRelationships.ContainsKey(choice.RelationshipImpact.PrimaryCharacter))
            {
                var relationship = characterRelationships[choice.RelationshipImpact.PrimaryCharacter];
                relationship.TrustLevel = Math.Clamp(relationship.TrustLevel + choice.RelationshipImpact.TrustChange, -100, 100);
                relationship.ProfessionalRespect = Math.Clamp(relationship.ProfessionalRespect + choice.RelationshipImpact.RespectChange, -100, 100);
                relationship.PersonalConnection = Math.Clamp(relationship.PersonalConnection + choice.RelationshipImpact.PersonalConnectionChange, -100, 100);
            }
            
            // Add consequence flags to active story flags
            activeStoryFlags.AddRange(choice.ConsequenceFlags);
            
            // Apply gameplay effects (for future integration with game systems)
            foreach (var effect in choice.GameplayEffects)
            {
                // Handle advice-specific effects
                if (effect.Key == "advice_object" && effect.Value is CharacterAdvice advice)
                {
                    bool followed = choice.GameplayEffects.ContainsKey("advice_followed") && 
                                   (bool)choice.GameplayEffects["advice_followed"];
                    ApplyAdviceEffect(advice, followed);
                }
                
                // This would integrate with the main game systems
                // For now, we'll just track the effects
                Console.WriteLine($"Gameplay effect: {effect.Key} = {effect.Value}");
            }
        }

        private void ShowCharacterReaction(string reaction)
        {
            // Update the dialogue text to show character reaction
            JoanDialogueText.Text = reaction;
            
            // Update recommendations to show the impact
            RecommendationsText.Text = "Your choice has been noted. Joan will remember this interaction.";
        }

        private void ApplyAdviceEffect(CharacterAdvice advice, bool followed)
        {
            // Get the advice system from the narrative engine
            if (storyModeManager?.NarrativeEngine?.AdviceSystem != null)
            {
                storyModeManager.NarrativeEngine.AdviceSystem.ApplyAdviceEffect(advice, followed);
            }
        }

        private void NavigateToNextNode(string nextNodeId)
        {
            if (currentConversation != null && currentConversation.Nodes.ContainsKey(nextNodeId))
            {
                currentConversation.CurrentNodeId = nextNodeId;
                LoadBranchingConversation();
            }
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GenerateContextualDialogue()
        {
            // Enhanced dialogue with character relationship awareness
            if (isStoryMode && storyModeManager != null)
            {
                var joanPhase = StoryScript.GetJoanPhaseForQuarter(currentQuarter);
                var narrativeAct = StoryScript.GetNarrativeActForQuarter(currentQuarter);
                
                // Generate phase-appropriate dialogue
                var phaseDialogue = GeneratePhaseSpecificDialogue(joanPhase, narrativeAct);
                if (phaseDialogue.HasValue)
                {
                    return phaseDialogue.Value;
                }
            }

            // Special retirement health dialogue around quarter 110
            if (!isStoryMode && currentQuarter >= 110 && currentQuarter <= 115)
            {
                var healthDialogues = new[]
                {
                    "You know, I've been thinking... you've been at this for nearly 30 years now. How are you feeling? The stress of running a company can take its toll on one's health.",
                    "I hope you're taking care of yourself. All these years of corporate leadership... it's important to think about your well-being as retirement approaches.",
                    "You're getting close to the traditional retirement age. Have you been considering what comes next? Your health and happiness matter too.",
                    "I've noticed you've been working tirelessly for decades. Perhaps it's time to start thinking about slowing down? Your health is irreplaceable.",
                    "After all these years together, I feel I should mention - you're not getting any younger. Have you thought about your long-term health and retirement plans?"
                };
                
                return (
                    healthDialogues[new Random().Next(healthDialogues.Length)],
                    "After nearly three decades of corporate leadership, it's natural to reflect on health and legacy.",
                    "💡 Consider your long-term well-being\n💡 Retirement at quarter 120 is approaching\n💡 Think about what kind of legacy you want to leave"
                );
            }

            // Rest of the existing dialogue generation logic...
            return GenerateStandardDialogue();
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations)? GeneratePhaseSpecificDialogue(RelationshipPhase joanPhase, NarrativeAct narrativeAct)
        {
            // Generate dialogue based on Joan's relationship phase and narrative act
            switch (joanPhase)
            {
                case RelationshipPhase.ProfessionalAcquaintance:
                    return GenerateProfessionalDialogue();
                    
                case RelationshipPhase.TrustedColleague:
                    return GenerateTrustedDialogue();
                    
                case RelationshipPhase.PersonalFriend:
                    return GeneratePersonalDialogue();
                    
                case RelationshipPhase.LifelongBond:
                    return GenerateLifelongDialogue();
            }
            
            return null;
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GenerateProfessionalDialogue()
        {
            var messages = new[]
            {
                "Good day! Let me provide you with a professional assessment of our current business situation.",
                "I've prepared a comprehensive analysis of our company's performance this quarter.",
                "As your assistant, I've compiled the key metrics and strategic recommendations for your review."
            };
            
            return (
                messages[random.Next(messages.Length)],
                "Professional analysis of current company metrics and performance indicators.",
                "Strategic recommendations based on industry best practices and current market conditions."
            );
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GenerateTrustedDialogue()
        {
            var messages = new[]
            {
                "I've been thinking about our strategic direction, and I have some insights to share with you.",
                "Based on our working relationship, I feel comfortable sharing some honest observations about our progress.",
                "You've shown excellent leadership so far. Let me share what I'm seeing from my perspective."
            };
            
            return (
                messages[random.Next(messages.Length)],
                "Trusted advisor perspective on company trajectory and leadership effectiveness.",
                "Candid recommendations based on our established working relationship and mutual trust."
            );
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GeneratePersonalDialogue()
        {
            var messages = new[]
            {
                "I hope you don't mind me saying, but I've grown quite invested in our company's success - and your well-being.",
                "After all this time working together, I feel I can speak more personally about what I'm observing.",
                "You know, I've come to really care about both the business and how you're handling the pressures of leadership."
            };
            
            return (
                messages[random.Next(messages.Length)],
                "Personal observations about leadership challenges and company culture from someone who cares.",
                "Heartfelt advice balancing business success with personal well-being and sustainable practices."
            );
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GenerateLifelongDialogue()
        {
            var messages = new[]
            {
                "My dear friend, after all these years together, I want to share what's truly on my mind about our journey.",
                "You know, looking back on everything we've built together, I have some thoughts about where we're headed.",
                "As someone who's been by your side through thick and thin, let me share my deepest insights about our path forward."
            };
            
            return (
                messages[random.Next(messages.Length)],
                "Lifelong perspective on the journey we've shared and the legacy we're building together.",
                "Wisdom from a trusted friend who's witnessed your entire corporate journey and cares deeply about your future."
            );
        }

        private (string MainMessage, string SituationAnalysis, string Recommendations) GenerateStandardDialogue()
        {
            // Special retirement health dialogue around quarter 110
            if (!isStoryMode && currentQuarter >= 110 && currentQuarter <= 115)
            {
                var healthDialogues = new[]
                {
                    "You know, I've been thinking... you've been at this for nearly 30 years now. How are you feeling? The stress of running a company can take its toll on one's health.",
                    "I hope you're taking care of yourself. All these years of corporate leadership... it's important to think about your well-being as retirement approaches.",
                    "You're getting close to the traditional retirement age. Have you been considering what comes next? Your health and happiness matter too.",
                    "I've noticed you've been working tirelessly for decades. Perhaps it's time to start thinking about slowing down? Your health is irreplaceable.",
                    "After all these years together, I feel I should mention - you're not getting any younger. Have you thought about your long-term health and retirement plans?"
                };
                
                return (
                    healthDialogues[new Random().Next(healthDialogues.Length)],
                    "After nearly three decades of corporate leadership, it's natural to reflect on health and legacy.",
                    "💡 Consider your long-term well-being\n💡 Retirement at quarter 120 is approaching\n💡 Think about what kind of legacy you want to leave"
                );
            }

            // End of quarter dialogues
            var endQuarterDialogues = new[]
            {
                "Another quarter completed! Let's review how we performed and plan for the next one.",
                "Time flies when you're running a company! Here's my assessment of this quarter.",
                "Quarter end is always a good time to reflect and strategize. Here's what I see.",
                "Great work this quarter! Let me share some insights for moving forward.",
                "Every quarter brings new challenges and opportunities. Here's my analysis."
            };

            // Situation-based dialogues with hints
            string mainMessage;
            string situationAnalysis;
            string recommendations;

            // Determine main message based on company state
            if (company.ConsecutiveNegativeQuarters >= 1)
            {
                mainMessage = "I'm very concerned about our financial situation. We're operating in the red, and if this continues for another quarter, we might face... serious consequences.";
            }
            else if (company.Capital < 50000)
            {
                mainMessage = "Our cash flow is extremely tight. We need to be very careful about our spending and focus on revenue generation.";
            }
            else if (company.EmployeeCount <= 2)
            {
                mainMessage = "I'm worried about our workforce situation. A company can't function without people. If we lose our remaining employees, we might not be able to continue operations.";
            }
            else if (company.Morale < -30)
            {
                mainMessage = "Our employees are really struggling with morale. This could lead to major problems if we don't address it soon.";
            }
            else if (company.Risk > 70)
            {
                mainMessage = "We're operating at very high risk levels. One wrong move could be catastrophic for the company.";
            }
            else if (company.MarketShare > 60)
            {
                mainMessage = "Incredible progress! We're becoming a major player in the market. I wonder what would happen if we could capture even more market share...";
            }
            else if (company.Capital > 500000000) // $500 million hint
            {
                mainMessage = "Wow! Our capital reserves are extraordinary. I've heard that companies with this kind of wealth sometimes attract acquisition offers from larger corporations...";
            }
            else if (company.MarketShare > 50)
            {
                mainMessage = "Excellent work! We're dominating the market. Now we need to maintain this position strategically.";
            }
            else
            {
                mainMessage = endQuarterDialogues[new Random().Next(endQuarterDialogues.Length)];
            }

            // Generate situation analysis with subtle hints
            var analysisPoints = new List<string>();
            
            if (company.Capital > 1000000000) // $1 billion
                analysisPoints.Add($"💰 Exceptional capital reserves: ${company.Capital:N0} - you're in the big leagues now!");
            else if (company.Capital > 750000000) // $750 million
                analysisPoints.Add($"💰 Outstanding financial position: ${company.Capital:N0} - major corporations notice this kind of success");
            else if (company.Capital > 500000)
                analysisPoints.Add($"💰 Strong financial position with ${company.Capital:N0} in capital");
            else if (company.Capital < 100000)
                analysisPoints.Add($"⚠️ Low capital reserves: ${company.Capital:N0} - financial risk is high");
            else
                analysisPoints.Add($"💰 Moderate capital: ${company.Capital:N0}");

            if (company.ConsecutiveNegativeQuarters > 0)
                analysisPoints.Add($"🚨 CRITICAL: {company.ConsecutiveNegativeQuarters} quarter(s) of negative capital - bankruptcy risk!");

            if (company.Morale > 50)
                analysisPoints.Add($"😊 High employee morale ({company.Morale}) - team is motivated");
            else if (company.Morale < -20)
                analysisPoints.Add($"😟 Low morale ({company.Morale}) - employees are unhappy");
            else
                analysisPoints.Add($"😐 Average morale ({company.Morale})");

            if (company.MarketShare > 65)
                analysisPoints.Add($"📈 Dominant market position ({company.MarketShare:F1}%) - you're so close to something special...");
            else if (company.MarketShare > 40)
                analysisPoints.Add($"📈 Strong market position ({company.MarketShare:F1}% market share)");
            else if (company.MarketShare < 10)
                analysisPoints.Add($"📉 Weak market position ({company.MarketShare:F1}% market share)");
            else
                analysisPoints.Add($"📊 Moderate market share ({company.MarketShare:F1}%)");

            int totalEmployees = company.EmployeeCount;
            if (totalEmployees <= 1)
                analysisPoints.Add($"🚨 CRITICAL: Only {totalEmployees} employee(s) - companies need people to survive!");
            else if (totalEmployees < 5)
                analysisPoints.Add($"👥 Small team ({totalEmployees} employees) - consider hiring more");
            else if (totalEmployees > 20)
                analysisPoints.Add($"👥 Large team ({totalEmployees} employees) - good workforce");
            else
                analysisPoints.Add($"👥 {totalEmployees} employees across departments");

            situationAnalysis = string.Join("\n", analysisPoints);

            // Generate recommendations with subtle hints
            var recommendationsList = new List<string>();

            if (company.ConsecutiveNegativeQuarters >= 1)
                recommendationsList.Add("🚨 URGENT: Take immediate action to avoid... permanent consequences");
            else if (company.Capital < 100000)
                recommendationsList.Add("💡 Consider taking an emergency loan or cutting costs immediately");
            
            if (company.EmployeeCount <= 2)
                recommendationsList.Add("🚨 URGENT: Hire employees immediately - companies can't survive without people");
            
            if (company.Morale < -20)
                recommendationsList.Add("💡 Organize a company retreat or give employee bonuses to boost morale");
            
            if (company.Risk > 50)
                recommendationsList.Add("💡 Hire crisis management consultants to reduce operational risk");
            
            if (company.MarketShare < 20)
                recommendationsList.Add("💡 Launch marketing campaigns to increase market presence");
            else if (company.MarketShare > 65)
                recommendationsList.Add("💡 You're so close to market dominance - push for that final stretch!");
            
            if (totalEmployees < 8)
                recommendationsList.Add("💡 Consider hiring more employees to strengthen departments");

            // Story mode specific reminders
            if (isStoryMode)
            {
                if (currentQuarter <= 10)
                {
                    recommendationsList.Add($"📚 Tutorial Quarter {currentQuarter}: Focus on learning the mechanics");
                }
                
                // Check for forgotten mechanics in story mode
                if (currentQuarter > 3 && !HasUsedExecutiveDecisions())
                    recommendationsList.Add("⏰ Reminder: Don't forget to use Executive Decisions for strategic moves!");
                
                if (currentQuarter > 2 && departments.Values.All(d => d.GetEmployeeCount() < 2))
                    recommendationsList.Add("⏰ Reminder: Consider hiring more employees through the Hiring Panel!");
            }

            if (recommendationsList.Count == 0)
                recommendationsList.Add("💡 Keep up the good work! Monitor your metrics and stay strategic.");

            recommendations = string.Join("\n", recommendationsList);

            return (mainMessage, situationAnalysis, recommendations);
        }

        private bool HasUsedExecutiveDecisions()
        {
            // This would need to be tracked in the game state
            // For now, assume they haven't if they have default budget allocations
            return !(company.MarketingBudget == 15.0 && company.OperationsBudget == 20.0 && 
                    company.FinanceBudget == 15.0 && company.HRBudget == 10.0 && 
                    company.ITBudget == 20.0 && company.ResearchBudget == 20.0);
        }

        private void GetAdviceBtn_Click(object sender, RoutedEventArgs e)
        {
            // Regenerate advice with different focus
            var strategicAdvice = GenerateStrategicAdvice();
            JoanDialogueText.Text = strategicAdvice;
            SituationAnalysisText.Text = "Here's my strategic assessment based on current market conditions and company performance.";
            RecommendationsText.Text = GenerateStrategicRecommendations();
        }

        private string GenerateStrategicAdvice()
        {
            var adviceOptions = new[]
            {
                "Based on your current position, I recommend focusing on sustainable growth rather than rapid expansion.",
                "Your company is at a critical juncture. The decisions you make this quarter will shape your future.",
                "I've analyzed the market trends and your competition. Here's what I think you should prioritize.",
                "Looking at your financial health and employee satisfaction, I have some strategic insights to share.",
                "The key to success is balancing risk and reward. Let me help you find that balance."
            };

            return adviceOptions[random.Next(adviceOptions.Length)];
        }

        private string GenerateStrategicRecommendations()
        {
            var recommendations = new List<string>();

            // Financial strategy
            if (company.Capital > 300000)
                recommendations.Add("💰 With strong capital, consider investing in R&D or marketing for growth");
            else
                recommendations.Add("💰 Focus on cost management and revenue generation");

            // Market strategy
            if (company.MarketShare < 30)
                recommendations.Add("📈 Aggressive marketing campaigns could help capture market share");
            else
                recommendations.Add("📈 Defend your market position with quality improvements");

            // Employee strategy
            if (company.Morale > 30)
                recommendations.Add("👥 High morale gives you flexibility for strategic initiatives");
            else
                recommendations.Add("👥 Invest in employee satisfaction before pursuing aggressive growth");

            return string.Join("\n", recommendations);
        }

        private void CheckRemindersBtn_Click(object sender, RoutedEventArgs e)
        {
            var reminders = GenerateReminders();
            JoanDialogueText.Text = "Here are some important reminders and tips for managing your company effectively.";
            SituationAnalysisText.Text = "I've noticed some areas that might need your attention.";
            RecommendationsText.Text = reminders;
        }

        private string GenerateReminders()
        {
            var reminders = new List<string>();

            // Check various game mechanics usage
            if (isStoryMode)
            {
                reminders.Add("📚 Story Mode Tips:");
                
                if (currentQuarter <= 10)
                    reminders.Add("• Take your time to learn each mechanic as it's introduced");
                
                reminders.Add("• Don't forget to check the hiring panel regularly for new talent");
                reminders.Add("• Executive decisions can dramatically change your company's trajectory");
                reminders.Add("• Department budget allocation affects long-term performance");
            }

            // General reminders with subtle hints
            reminders.Add("\n🎯 Success & Survival Tips:");
            reminders.Add("• Monitor your cash flow carefully - consecutive losses can be... final");
            reminders.Add("• Employee morale affects productivity and turnover rates");
            reminders.Add("• High risk levels increase the chance of catastrophic events");
            reminders.Add("• Companies need people to function - zero employees means game over");
            reminders.Add("• Market dominance brings great rewards for those who achieve it");
            reminders.Add("• Exceptional financial success sometimes attracts... interesting opportunities");
            
            if (!isStoryMode && currentQuarter >= 100)
            {
                reminders.Add("• Consider your health and well-being as you approach traditional retirement age");
            }

            return string.Join("\n", reminders);
        }

        private void CloseDialogueBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DismissBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Method to create a sample branching dialogue for testing
        public static DialogueConversation CreateSampleBranchingDialogue(Company company, int quarter)
        {
            var conversation = new DialogueConversation
            {
                ConversationId = $"joan_sample_{DateTime.Now.Ticks}",
                Title = "Important Discussion with Joan",
                Participants = new List<string> { "player", "joan" },
                StartNodeId = "start",
                CurrentNodeId = "start"
            };

            // Create the main dialogue node
            var startNode = new DialogueNode
            {
                NodeId = "start",
                CharacterId = "joan",
                DialogueText = GetSampleDialogueText(company, quarter),
                EmotionalTone = GetEmotionalToneForSituation(company),
                ContextTags = new List<string> { "quarterly_review", "important_decision" },
                MinimumChoices = 2,
                MaximumChoices = 4
            };

            // Add adaptive text based on relationship
            startNode.AdaptiveText["relationship:joan:trust:70"] = "I feel comfortable being completely honest with you about this situation. We've built a strong working relationship.";
            startNode.AdaptiveText["relationship:joan:personal:50"] = "You know, after working together for a while, I think we need to address this directly.";

            // Create response choices
            startNode.Choices = CreateSampleChoices(company, quarter);

            conversation.Nodes["start"] = startNode;
            return conversation;
        }

        private static string GetSampleDialogueText(Company company, int quarter)
        {
            if (company.ConsecutiveNegativeQuarters >= 1)
            {
                return "I'm very concerned about our financial situation. We're in a critical position and need to make some difficult decisions. How do you want to handle this crisis?";
            }
            else if (company.Morale < -20)
            {
                return "I've been hearing a lot of complaints from the employees lately. Morale is really suffering, and it's starting to affect productivity. We need to address this situation carefully.";
            }
            else if (company.MarketShare > 60)
            {
                return "This is incredible! We're really dominating the market now. But with great success comes great responsibility. How do you want to manage our position moving forward?";
            }
            else if (quarter <= 10)
            {
                return "As we continue learning the ropes together, I want to make sure you're comfortable with the decisions we're making. What's your leadership philosophy as we build this company?";
            }
            else
            {
                return "We're at an important crossroads in our company's development. I have some thoughts on our direction, but I'd like to hear your perspective first. How do you see our future?";
            }
        }

        private static EmotionalTone GetEmotionalToneForSituation(Company company)
        {
            if (company.ConsecutiveNegativeQuarters >= 1) return EmotionalTone.Concerned;
            if (company.Morale < -20) return EmotionalTone.Disappointed;
            if (company.MarketShare > 60) return EmotionalTone.Excited;
            if (company.Capital > 500000000) return EmotionalTone.Positive;
            return EmotionalTone.Serious;
        }

        private static List<DialogueChoice> CreateSampleChoices(Company company, int quarter)
        {
            var choices = new List<DialogueChoice>();

            // Professional approach
            choices.Add(new DialogueChoice
            {
                ChoiceId = "professional",
                ChoiceText = "Let's analyze this systematically and follow established business protocols.",
                Tone = ChoiceTone.Professional,
                ToneDescription = "Methodical and business-focused approach",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipChanges = { ["joan"] = 3 },
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    RespectChange = 5,
                    TrustChange = 2,
                    ImpactDescription = "Joan appreciates your professional approach"
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Maintains stability and reduces immediate risk",
                        Type = ConsequenceType.Business,
                        Severity = ConsequenceRisk.Low
                    }
                },
                ConsequenceFlags = new List<string> { "professional_leadership", "systematic_approach" },
                CharacterReaction = "I appreciate your methodical approach. Let's work through this step by step."
            });

            // Supportive approach
            choices.Add(new DialogueChoice
            {
                ChoiceId = "supportive",
                ChoiceText = "I want to make sure everyone feels heard and supported through this situation.",
                Tone = ChoiceTone.Supportive,
                ToneDescription = "Empathetic and team-focused",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipChanges = { ["joan"] = 5 },
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    PersonalConnectionChange = 8,
                    TrustChange = 5,
                    ImpactDescription = "Joan is touched by your concern for others"
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Improves team morale and employee satisfaction",
                        Type = ConsequenceType.Relationship,
                        Severity = ConsequenceRisk.Low
                    }
                },
                ConsequenceFlags = new List<string> { "supportive_leadership", "employee_focused" },
                CharacterReaction = "That's exactly what I was hoping you'd say. Your compassion for the team really shows."
            });

            // Aggressive approach (only if company is in crisis)
            if (company.ConsecutiveNegativeQuarters >= 1 || company.Morale < -30)
            {
                choices.Add(new DialogueChoice
                {
                    ChoiceId = "aggressive",
                    ChoiceText = "We need to take decisive action immediately, even if it's uncomfortable.",
                    Tone = ChoiceTone.Aggressive,
                    ToneDescription = "Bold and action-oriented",
                    RiskLevel = ConsequenceRisk.High,
                    RelationshipChanges = { ["joan"] = -2 },
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = "joan",
                        RespectChange = 3,
                        TrustChange = -3,
                        ImpactDescription = "Joan respects your decisiveness but worries about the approach"
                    },
                    ImmediateConsequences = new List<ConsequencePreview>
                    {
                        new ConsequencePreview
                        {
                            Description = "Quick resolution but potential for unintended consequences",
                            Type = ConsequenceType.Business,
                            Severity = ConsequenceRisk.Medium
                        }
                    },
                    LongTermConsequences = new List<ConsequencePreview>
                    {
                        new ConsequencePreview
                        {
                            Description = "May create resistance among team members",
                            Type = ConsequenceType.Relationship,
                            Severity = ConsequenceRisk.Medium
                        }
                    },
                    ConsequenceFlags = new List<string> { "aggressive_leadership", "crisis_management" },
                    RequiresConditions = new List<string> { "relationship:joan:trust:20" },
                    CharacterReaction = "I understand the urgency, but I hope we can be thoughtful about how we implement these changes."
                });
            }

            // Diplomatic approach
            choices.Add(new DialogueChoice
            {
                ChoiceId = "diplomatic",
                ChoiceText = "Let's bring together the key people and find a solution that works for everyone.",
                Tone = ChoiceTone.Diplomatic,
                ToneDescription = "Collaborative and inclusive",
                RiskLevel = ConsequenceRisk.Medium,
                RelationshipChanges = { ["joan"] = 4 },
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = "joan",
                    RespectChange = 7,
                    PersonalConnectionChange = 3,
                    ImpactDescription = "Joan admires your inclusive leadership style"
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Takes more time but builds consensus and buy-in",
                        Type = ConsequenceType.Story,
                        Severity = ConsequenceRisk.Low
                    }
                },
                ConsequenceFlags = new List<string> { "diplomatic_leadership", "collaborative_approach" },
                UnlocksFutureOptions = new List<string> { "team_leadership_path", "consensus_builder" },
                CharacterReaction = "That's a wonderful approach. I think bringing everyone together will really strengthen our team."
            });

            return choices;
        }

        // Method to test branching dialogue (can be called from main game)
        public static void ShowBranchingDialogueExample(Company company, Dictionary<Department, DepartmentStats> departments, int quarter, Window owner)
        {
            var conversation = CreateSampleBranchingDialogue(company, quarter);
            var relationships = new Dictionary<string, CharacterRelationship>
            {
                ["joan"] = new CharacterRelationship
                {
                    TrustLevel = 50,
                    ProfessionalRespect = 60,
                    PersonalConnection = 30,
                    CurrentPhase = StoryScript.GetJoanPhaseForQuarter(quarter)
                }
            };
            var storyFlags = new List<string> { $"quarter_{quarter}", "example_dialogue" };

            var dialogue = new JoanDialogue(company, departments, conversation, relationships, storyFlags, true, quarter);
            dialogue.Owner = owner;
            dialogue.ShowDialog();
        }
    }
}