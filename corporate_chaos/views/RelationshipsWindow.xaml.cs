using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class RelationshipsWindow : Window
    {
        private Dictionary<string, CharacterRelationship> relationships;
        private Dictionary<string, StoryCharacter> characters;

        public RelationshipsWindow(Dictionary<string, CharacterRelationship> relationships, Dictionary<string, StoryCharacter> characters)
        {
            InitializeComponent();
            this.relationships = relationships;
            this.characters = characters;
            
            LoadRelationships();
        }

        private void LoadRelationships()
        {
            RelationshipsPanel.Children.Clear();

            if (relationships == null || !relationships.Any())
            {
                var noRelationshipsText = new TextBlock
                {
                    Text = "You haven't met any characters yet.\nProgress through the story to meet new people!",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(20, 50, 20, 20)
                };
                RelationshipsPanel.Children.Add(noRelationshipsText);
                return;
            }

            foreach (var rel in relationships.OrderByDescending(r => CalculateOverallRelationship(r.Value)))
            {
                var characterId = rel.Key;
                var relationship = rel.Value;
                
                // Get character info
                StoryCharacter? character = null;
                if (characters != null && characters.ContainsKey(characterId))
                {
                    character = characters[characterId];
                }

                // Create relationship card
                var card = CreateRelationshipCard(characterId, relationship, character);
                RelationshipsPanel.Children.Add(card);
            }
        }

        private int CalculateOverallRelationship(CharacterRelationship rel)
        {
            // Average of the three relationship metrics
            return (rel.TrustLevel + rel.ProfessionalRespect + rel.PersonalConnection) / 3;
        }

        private Border CreateRelationshipCard(string characterId, CharacterRelationship relationship, StoryCharacter? character)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 62)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Avatar
            var avatarBorder = new Border
            {
                Width = 50,
                Height = 50,
                CornerRadius = new CornerRadius(25),
                Background = Brushes.White,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            if (character != null && !string.IsNullOrEmpty(character.ImagePath))
            {
                try
                {
                    var image = new Image
                    {
                        Width = 50,
                        Height = 50,
                        Stretch = Stretch.UniformToFill,
                        Source = new BitmapImage(new Uri($"pack://application:,,,/{character.ImagePath}"))
                    };
                    image.Clip = new EllipseGeometry(new Point(25, 25), 25, 25);
                    avatarBorder.Child = image;
                }
                catch
                {
                    // Fallback to text
                    var nameInitial = new TextBlock
                    {
                        Text = character.Name.Substring(0, 1).ToUpper(),
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(77, 42, 77)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    avatarBorder.Child = nameInitial;
                }
            }
            else
            {
                // Fallback initial
                var initial = character != null && !string.IsNullOrEmpty(character.Name) 
                    ? character.Name.Substring(0, 1).ToUpper() 
                    : "?";
                var nameInitial = new TextBlock
                {
                    Text = initial,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(77, 42, 77)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                avatarBorder.Child = nameInitial;
            }

            Grid.SetColumn(avatarBorder, 0);
            grid.Children.Add(avatarBorder);

            // Info panel
            var infoPanel = new StackPanel();
            Grid.SetColumn(infoPanel, 1);

            // Name and role
            var nameText = new TextBlock
            {
                Text = character?.Name ?? characterId,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            infoPanel.Children.Add(nameText);

            if (character != null && !string.IsNullOrEmpty(character.Role))
            {
                var roleText = new TextBlock
                {
                    Text = character.Role,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 200)),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                infoPanel.Children.Add(roleText);
            }

            // Relationship metrics
            var metricsPanel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            
            AddMetricBar(metricsPanel, "Trust", relationship.TrustLevel);
            AddMetricBar(metricsPanel, "Respect", relationship.ProfessionalRespect);
            AddMetricBar(metricsPanel, "Connection", relationship.PersonalConnection);
            
            infoPanel.Children.Add(metricsPanel);

            // Overall status
            int overallLevel = CalculateOverallRelationship(relationship);
            var statusText = new TextBlock
            {
                Text = GetRelationshipStatus(overallLevel),
                FontSize = 11,
                Foreground = GetRelationshipColor(overallLevel),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 3, 0, 0)
            };
            infoPanel.Children.Add(statusText);

            // Personality traits
            if (character != null && character.PersonalityTraits.Any())
            {
                var traitsPanel = new WrapPanel { Margin = new Thickness(0, 5, 0, 0) };
                foreach (var trait in character.PersonalityTraits.Take(3))
                {
                    var traitBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(77, 42, 77)),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8, 3, 8, 3),
                        Margin = new Thickness(0, 0, 5, 0)
                    };
                    var traitText = new TextBlock
                    {
                        Text = trait,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(200, 150, 255))
                    };
                    traitBorder.Child = traitText;
                    traitsPanel.Children.Add(traitBorder);
                }
                infoPanel.Children.Add(traitsPanel);
            }

            grid.Children.Add(infoPanel);
            card.Child = grid;

            return card;
        }

        private void AddMetricBar(StackPanel panel, string label, int value)
        {
            var metricGrid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });

            var labelText = new TextBlock
            {
                Text = label + ":",
                FontSize = 10,
                Foreground = Brushes.Gray
            };
            Grid.SetColumn(labelText, 0);
            metricGrid.Children.Add(labelText);

            // Progress bar background
            var barBackground = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 46)),
                CornerRadius = new CornerRadius(3),
                Height = 6,
                Margin = new Thickness(5, 0, 5, 0)
            };
            Grid.SetColumn(barBackground, 1);
            metricGrid.Children.Add(barBackground);

            // Progress bar fill
            var barFill = new Border
            {
                Background = GetMetricColor(value),
                CornerRadius = new CornerRadius(3),
                Height = 6,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = Math.Max(0, (value + 100) / 2.0) // Convert -100 to 100 range to 0-100%
            };
            Grid.SetColumn(barFill, 1);
            metricGrid.Children.Add(barFill);

            var valueText = new TextBlock
            {
                Text = value.ToString(),
                FontSize = 10,
                Foreground = GetMetricColor(value),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(valueText, 2);
            metricGrid.Children.Add(valueText);

            panel.Children.Add(metricGrid);
        }

        private Brush GetMetricColor(int value)
        {
            if (value >= 60) return new SolidColorBrush(Color.FromRgb(100, 255, 100)); // High - Green
            if (value >= 20) return new SolidColorBrush(Color.FromRgb(100, 200, 255)); // Medium - Blue
            if (value >= -20) return Brushes.Yellow; // Neutral - Yellow
            if (value >= -60) return Brushes.Orange; // Low - Orange
            return new SolidColorBrush(Color.FromRgb(255, 100, 100)); // Very Low - Red
        }

        private Brush GetRelationshipColor(int level)
        {
            if (level >= 60) return new SolidColorBrush(Color.FromRgb(100, 255, 100)); // Excellent - Green
            if (level >= 20) return new SolidColorBrush(Color.FromRgb(100, 200, 255)); // Good - Light Blue
            if (level >= -20) return Brushes.Yellow; // Neutral - Yellow
            if (level >= -60) return Brushes.Orange; // Poor - Orange
            return new SolidColorBrush(Color.FromRgb(255, 100, 100)); // Hostile - Red
        }

        private string GetRelationshipStatus(int level)
        {
            if (level >= 60) return "💚 Excellent - Trusted ally and close friend";
            if (level >= 20) return "💙 Good - Friendly and supportive";
            if (level >= -20) return "💛 Neutral - Professional relationship";
            if (level >= -60) return "🧡 Poor - Strained relationship";
            return "❤️ Hostile - Significant tension";
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
