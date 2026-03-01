using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class StoryModeGuide : Window
    {
        private StoryEvent currentEvent;
        private int currentDialogueIndex = 0;
        private ExtendedStoryModeData? storyData;
        
        public bool IsCompleted { get; private set; } = false;

        public StoryModeGuide(StoryEvent storyEvent, int quarter, ExtendedStoryModeData? extendedStoryData = null)
        {
            InitializeComponent();
            currentEvent = storyEvent;
            storyData = extendedStoryData;
            LoadJoanAvatar();
            LoadStoryEvent(quarter);
            LoadStoryProgress(quarter);
            LoadCharacterRelationships();
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
                
                JoanAvatarImage.Source = assistantImage;
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
                    
                    JoanAvatarImage.Source = fallbackImage;
                }
                catch
                {
                    // If all else fails, leave it empty (will show white background)
                }
            }
        }

        private void LoadStoryEvent(int quarter)
        {
            // Update header info
            if (storyData != null)
            {
                var joanPhase = StoryScript.GetJoanPhaseForQuarter(quarter);
                RoleText.Text = GetJoanRoleText(joanPhase);
            }
            else
            {
                RoleText.Text = quarter <= 8 ? "Your Personal Corporate Assistant" : "Your Trusted Advisor";
            }
            
            QuarterText.Text = quarter <= 10 ? $"Quarter {quarter} - Tutorial Phase" : $"Quarter {quarter}";
            
            // Load event content
            EventTitleText.Text = currentEvent.Title;
            EventDescriptionText.Text = currentEvent.Description;
            ObjectiveText.Text = currentEvent.ObjectiveText;
            
            // Load first dialogue
            currentDialogueIndex = 0;
            UpdateDialogue();
        }

        private string GetJoanRoleText(RelationshipPhase phase)
        {
            return phase switch
            {
                RelationshipPhase.FirstMeeting => "Your New Corporate Assistant",
                RelationshipPhase.ProfessionalAcquaintance => "Your Corporate Assistant",
                RelationshipPhase.TrustedColleague => "Your Trusted Advisor",
                RelationshipPhase.PersonalFriend => "Your Personal Confidant",
                RelationshipPhase.LifelongBond => "Your Lifelong Friend",
                _ => "Your Personal Corporate Assistant"
            };
        }

        private void LoadStoryProgress(int quarter)
        {
            // Determine current act
            var currentAct = StoryScript.GetNarrativeActForQuarter(quarter);
            CurrentActText.Text = GetActDisplayText(currentAct);
            
            // Calculate act progress
            var (actStart, actEnd) = GetActQuarterRange(currentAct);
            var actProgress = ((double)(quarter - actStart) / (actEnd - actStart)) * 100;
            ActProgressText.Text = $"Progress: {actProgress:F0}%";
            ActProgressBar.Width = actProgress * 2.0; // Scale to fit the bar width
            
            // Update statistics
            if (storyData != null)
            {
                ChoiceCountText.Text = storyData.ChoiceHistory.Count.ToString();
                EventCountText.Text = storyData.StoryFlags.Count(f => f.StartsWith("event_")).ToString();
                
                var metCharacters = storyData.CharacterRelationships.Count;
                CharacterCountText.Text = $"{metCharacters}/9";
            }
            else
            {
                ChoiceCountText.Text = "0";
                EventCountText.Text = "0";
                CharacterCountText.Text = "1/9";
            }
        }

        private string GetActDisplayText(NarrativeAct act)
        {
            return act switch
            {
                NarrativeAct.Tutorial => "Act I: Tutorial",
                NarrativeAct.RisingAction => "Act II: Rising Action",
                NarrativeAct.Climax => "Act III: Climax",
                NarrativeAct.Resolution => "Act IV: Resolution",
                _ => "Story Mode"
            };
        }

        private (int start, int end) GetActQuarterRange(NarrativeAct act)
        {
            return act switch
            {
                NarrativeAct.Tutorial => (1, 10),
                NarrativeAct.RisingAction => (11, 60),
                NarrativeAct.Climax => (61, 100),
                NarrativeAct.Resolution => (101, 120),
                _ => (1, 120)
            };
        }

        private void LoadCharacterRelationships()
        {
            CharacterRelationshipsPanel.Children.Clear();
            
            if (storyData == null || storyData.CharacterRelationships.Count == 0)
            {
                // Show only Joan in tutorial mode
                AddCharacterRelationshipCard("joan", "Joan", null);
                return;
            }
            
            // Add all met characters
            foreach (var kvp in storyData.CharacterRelationships.OrderBy(c => c.Key))
            {
                var characterName = GetCharacterDisplayName(kvp.Key);
                AddCharacterRelationshipCard(kvp.Key, characterName, kvp.Value);
            }
        }

        private string GetCharacterDisplayName(string characterId)
        {
            return characterId switch
            {
                "joan" => "Joan",
                "marcus_vey" => "Marcus Vey",
                "evelyn_cross" => "Evelyn Cross",
                "vincent_duro" => "Vincent Duro",
                "lucinda_vale" => "Lucinda Vale",
                "gregory_shaw" => "Gregory Shaw",
                "selena_park" => "Selena Park",
                "harold_finch" => "Harold Finch",
                "sophie_kim" => "Sophie Kim",
                _ => characterId
            };
        }

        private void AddCharacterRelationshipCard(string characterId, string characterName, CharacterRelationship? relationship)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 77)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var mainPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            // Character avatar
            var avatarBorder = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            
            var avatarImage = new Image
            {
                Width = 38,
                Height = 38,
                Stretch = Stretch.UniformToFill,
                ClipToBounds = true
            };
            
            // Load character image
            try
            {
                var imagePath = GetCharacterImagePath(characterId);
                var imageUri = new Uri($"pack://application:,,,/{imagePath}");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                avatarImage.Source = bitmap;
                
                // Apply circular clip
                avatarImage.Clip = new EllipseGeometry(new Point(19, 19), 19, 19);
            }
            catch
            {
                // If image fails to load, leave white background
            }
            
            avatarBorder.Child = avatarImage;
            mainPanel.Children.Add(avatarBorder);

            var stackPanel = new StackPanel();
            
            // Character name
            var nameText = new TextBlock
            {
                Text = characterName,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(nameText);
            
            if (relationship != null)
            {
                // Relationship phase
                var phaseText = new TextBlock
                {
                    Text = GetRelationshipPhaseText(relationship.CurrentPhase),
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                stackPanel.Children.Add(phaseText);
                
                // Relationship bars
                AddRelationshipBar(stackPanel, "Trust", relationship.TrustLevel);
                AddRelationshipBar(stackPanel, "Respect", relationship.ProfessionalRespect);
                AddRelationshipBar(stackPanel, "Connection", relationship.PersonalConnection);
            }
            else
            {
                // Tutorial mode - show simple status
                var statusText = new TextBlock
                {
                    Text = "Professional Assistant",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(173, 216, 230))
                };
                stackPanel.Children.Add(statusText);
            }
            
            mainPanel.Children.Add(stackPanel);
            card.Child = mainPanel;
            CharacterRelationshipsPanel.Children.Add(card);
        }

        private string GetCharacterImagePath(string characterId)
        {
            return characterId switch
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
                _ => "images/assistant.png" // Fallback
            };
        }

        private string GetRelationshipPhaseText(RelationshipPhase phase)
        {
            return phase switch
            {
                RelationshipPhase.FirstMeeting => "First Meeting",
                RelationshipPhase.ProfessionalAcquaintance => "Professional",
                RelationshipPhase.TrustedColleague => "Trusted Colleague",
                RelationshipPhase.PersonalFriend => "Personal Friend",
                RelationshipPhase.LifelongBond => "Lifelong Bond",
                RelationshipPhase.Strained => "⚠️ Strained",
                RelationshipPhase.Hostile => "❌ Hostile",
                _ => "Unknown"
            };
        }

        private void AddRelationshipBar(StackPanel parent, string label, int value)
        {
            var container = new StackPanel { Margin = new Thickness(0, 0, 0, 5) };
            
            // Label and value
            var labelPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 3) };
            labelPanel.Children.Add(new TextBlock
            {
                Text = $"{label}: ",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(211, 211, 211))
            });
            labelPanel.Children.Add(new TextBlock
            {
                Text = value.ToString(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = GetRelationshipColor(value)
            });
            container.Children.Add(labelPanel);
            
            // Progress bar
            var barBackground = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 46)),
                CornerRadius = new CornerRadius(2),
                Height = 6
            };
            
            var barFill = new Border
            {
                Background = GetRelationshipColor(value),
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = Math.Max(0, (value + 100) / 2.0) // Scale -100 to 100 into 0 to 100
            };
            
            var grid = new Grid();
            grid.Children.Add(barBackground);
            grid.Children.Add(barFill);
            container.Children.Add(grid);
            
            parent.Children.Add(container);
        }

        private SolidColorBrush GetRelationshipColor(int value)
        {
            if (value >= 75) return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            if (value >= 50) return new SolidColorBrush(Color.FromRgb(139, 195, 74)); // Light green
            if (value >= 25) return new SolidColorBrush(Color.FromRgb(255, 235, 59)); // Yellow
            if (value >= 0) return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
            if (value >= -25) return new SolidColorBrush(Color.FromRgb(255, 87, 34)); // Deep orange
            return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
        }

        private void UpdateDialogue()
        {
            if (currentEvent.JoanDialogue.Count == 0)
            {
                DialogueText.Text = "I'm here to help you succeed! Good luck with this quarter.";
                NextDialogueBtn.IsEnabled = false;
                return;
            }

            if (currentDialogueIndex < currentEvent.JoanDialogue.Count)
            {
                DialogueText.Text = currentEvent.JoanDialogue[currentDialogueIndex];
            }

            // Update button states
            PrevDialogueBtn.IsEnabled = currentDialogueIndex > 0;
            NextDialogueBtn.IsEnabled = currentDialogueIndex < currentEvent.JoanDialogue.Count - 1;
            
            // Change "Next" to "Continue" on last dialogue
            if (currentDialogueIndex >= currentEvent.JoanDialogue.Count - 1)
            {
                NextDialogueBtn.Content = "Continue";
                NextDialogueBtn.IsEnabled = false;
            }
            else
            {
                NextDialogueBtn.Content = "Next →";
            }
        }

        private void PrevDialogueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentDialogueIndex > 0)
            {
                currentDialogueIndex--;
                UpdateDialogue();
            }
        }

        private void NextDialogueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentDialogueIndex < currentEvent.JoanDialogue.Count - 1)
            {
                currentDialogueIndex++;
                UpdateDialogue();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            IsCompleted = true;
            DialogResult = true;
            Close();
        }
    }
}