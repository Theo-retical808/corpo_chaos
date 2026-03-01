using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CorporateChaos.Models;
using CorporateChaos.Systems;

namespace CorporateChaos.Views
{
    public partial class CharacterInteractionWindow : Window
    {
        private readonly Company company;
        private readonly StoryModeManager storyModeManager;
        private readonly int currentQuarter;

        public CharacterInteractionWindow(Company company, StoryModeManager storyModeManager, int currentQuarter)
        {
            InitializeComponent();
            this.company = company;
            this.storyModeManager = storyModeManager;
            this.currentQuarter = currentQuarter;

            PopulateCharacterList();
        }

        private void PopulateCharacterList()
        {
            CharacterListPanel.Children.Clear();

            // Get all introduced characters
            var introducedCharacters = StoryScript.Characters.Values
                .Where(c => storyModeManager.IsCharacterIntroduced(c.CharacterId))
                .OrderBy(c => c.IntroductionQuarter)
                .ToList();

            if (introducedCharacters.Count == 0)
            {
                var noCharactersText = new TextBlock
                {
                    Text = "No characters have been introduced yet.\nContinue playing to meet new people!",
                    FontSize = 14,
                    Foreground = Brushes.LightGray,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(20, 50, 20, 20)
                };
                CharacterListPanel.Children.Add(noCharactersText);
                return;
            }

            foreach (var character in introducedCharacters)
            {
                var characterButton = CreateCharacterButton(character);
                CharacterListPanel.Children.Add(characterButton);
            }
        }

        private Button CreateCharacterButton(StoryCharacter character)
        {
            var button = new Button
            {
                Height = 80,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 62)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(74, 74, 106)),
                BorderThickness = new Thickness(2),
                Cursor = System.Windows.Input.Cursors.Hand,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            // Create button content
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Character icon/image
            var iconBorder = new Border
            {
                Width = 50,
                Height = 50,
                CornerRadius = new CornerRadius(25),
                Background = Brushes.White,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var characterImage = new System.Windows.Controls.Image
            {
                Width = 48,
                Height = 48,
                Stretch = Stretch.UniformToFill,
                ClipToBounds = true
            };

            // Load character image
            try
            {
                var imagePath = GetCharacterImagePath(character.CharacterId);
                var imageUri = new Uri($"pack://application:,,,/{imagePath}");
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                characterImage.Source = bitmap;
                
                // Apply circular clip
                characterImage.Clip = new EllipseGeometry(new Point(24, 24), 24, 24);
            }
            catch
            {
                // Fallback to emoji icon if image fails to load
                var iconText = new TextBlock
                {
                    Text = GetCharacterIcon(character.CharacterId),
                    FontSize = 24,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                iconBorder.Child = iconText;
                iconBorder.Background = new SolidColorBrush(Color.FromRgb(74, 74, 106));
            }

            if (characterImage.Source != null)
            {
                iconBorder.Child = characterImage;
            }

            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            // Character info
            var infoPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var nameText = new TextBlock
            {
                Text = character.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            var roleText = new TextBlock
            {
                Text = character.Role,
                FontSize = 12,
                Foreground = Brushes.LightGray
            };

            var relationshipText = new TextBlock
            {
                Text = GetRelationshipSummary(character.CharacterId),
                FontSize = 11,
                Foreground = Brushes.LightBlue,
                Margin = new Thickness(0, 5, 0, 0)
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(roleText);
            infoPanel.Children.Add(relationshipText);

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            // Talk button indicator
            var talkText = new TextBlock
            {
                Text = "💬 Talk",
                FontSize = 14,
                Foreground = Brushes.LightGreen,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };

            Grid.SetColumn(talkText, 2);
            grid.Children.Add(talkText);

            button.Content = grid;

            // Add click handler
            button.Click += (s, e) => OnCharacterSelected(character);

            // Add hover effect
            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(58, 58, 78));
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(90, 90, 122));
            };

            button.MouseLeave += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(42, 42, 62));
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(74, 74, 106));
            };

            return button;
        }

        private string GetCharacterIcon(string characterId)
        {
            return characterId switch
            {
                "joan" => "👩‍💼",
                "marcus_vey" => "💼",
                "evelyn_cross" => "🤝",
                "vincent_duro" => "🎯",
                "lucinda_vale" => "📢",
                "gregory_shaw" => "⚙️",
                "selena_park" => "💰",
                "harold_finch" => "⚖️",
                "sophie_kim" => "📊",
                _ => "👤"
            };
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
                _ => "images/assistant.png"
            };
        }

        private string GetRelationshipSummary(string characterId)
        {
            if (!storyModeManager.StoryData.CharacterRelationships.ContainsKey(characterId))
                return "Relationship: New";

            var relationship = storyModeManager.StoryData.CharacterRelationships[characterId];
            var avgRelationship = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;

            string status = avgRelationship switch
            {
                >= 60 => "Excellent",
                >= 30 => "Good",
                >= 0 => "Neutral",
                >= -30 => "Strained",
                _ => "Poor"
            };

            return $"Relationship: {status} ({avgRelationship:+0;-0;0})";
        }

        private void OnCharacterSelected(StoryCharacter character)
        {
            // Generate a check-in conversation with the character
            var conversation = storyModeManager.CharacterManager.GenerateCharacterCheckIn(
                character.CharacterId,
                company,
                currentQuarter
            );

            if (conversation != null)
            {
                // Show the dialogue
                var dialogue = new JoanDialogue(
                    company,
                    null!, // departments not needed for character conversations
                    conversation,
                    storyModeManager.StoryData.CharacterRelationships,
                    storyModeManager.StoryData.StoryFlags,
                    true,
                    currentQuarter,
                    storyModeManager
                );

                dialogue.Owner = this;
                dialogue.Title = $"Conversation with {character.Name}";
                dialogue.ShowDialog();

                // Refresh the character list to update relationship status
                PopulateCharacterList();
            }
            else
            {
                MessageBox.Show(
                    $"{character.Name} is not available to talk right now. Try again later!",
                    "Character Busy",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
