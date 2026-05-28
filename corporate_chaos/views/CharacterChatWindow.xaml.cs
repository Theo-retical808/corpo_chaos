using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CorporateChaos.Models;
using CorporateChaos.Systems;

namespace CorporateChaos.Views
{
    /// <summary>
    /// Chat-bubble style dialogue window for character interactions.
    /// Character lines appear on the left; player choices appear on the right after selection.
    /// </summary>
    public partial class CharacterChatWindow : Window
    {
        // ── Dependencies ────────────────────────────────────────────────────
        private readonly Company company;
        private readonly StoryModeManager storyModeManager;
        private readonly int currentQuarter;
        private readonly string characterId;

        // ── Conversation state ───────────────────────────────────────────────
        private DialogueConversation conversation;
        private DialogueNode? currentNode;
        private Dictionary<string, CharacterRelationship> relationships;
        private List<string> storyFlags;

        // ── Tone colours (bubble backgrounds) ───────────────────────────────
        private static readonly Color CharBubbleColor   = Color.FromRgb(30, 45, 74);   // #1e2d4a
        private static readonly Color PlayerBubbleColor = Color.FromRgb(42, 77, 58);   // #2a4d3a

        private static readonly Dictionary<ChoiceTone, Color> ToneColors = new()
        {
            [ChoiceTone.Professional] = Color.FromRgb(30, 60, 90),
            [ChoiceTone.Supportive]   = Color.FromRgb(30, 70, 70),
            [ChoiceTone.Aggressive]   = Color.FromRgb(80, 35, 35),
            [ChoiceTone.Diplomatic]   = Color.FromRgb(75, 55, 30),
            [ChoiceTone.Personal]     = Color.FromRgb(70, 35, 80),
            [ChoiceTone.Humorous]     = Color.FromRgb(50, 35, 80),
        };

        private static readonly Dictionary<ChoiceTone, Color> ToneHoverColors = new()
        {
            [ChoiceTone.Professional] = Color.FromRgb(45, 85, 120),
            [ChoiceTone.Supportive]   = Color.FromRgb(45, 100, 100),
            [ChoiceTone.Aggressive]   = Color.FromRgb(110, 50, 50),
            [ChoiceTone.Diplomatic]   = Color.FromRgb(105, 80, 45),
            [ChoiceTone.Personal]     = Color.FromRgb(100, 50, 110),
            [ChoiceTone.Humorous]     = Color.FromRgb(75, 50, 110),
        };

        // ────────────────────────────────────────────────────────────────────
        public CharacterChatWindow(
            Company company,
            StoryModeManager storyModeManager,
            int currentQuarter,
            string characterId,
            DialogueConversation conversation,
            Dictionary<string, CharacterRelationship> relationships,
            List<string> storyFlags)
        {
            InitializeComponent();

            this.company          = company;
            this.storyModeManager = storyModeManager;
            this.currentQuarter   = currentQuarter;
            this.characterId      = characterId;
            this.conversation     = conversation;
            this.relationships    = relationships;
            this.storyFlags       = storyFlags;

            SetupHeader();
            StartConversation();
        }

        // ── Header setup ────────────────────────────────────────────────────
        private void SetupHeader()
        {
            // Name & role
            if (StoryScript.Characters.TryGetValue(characterId, out var character))
            {
                CharacterNameText.Text = character.Name;
                CharacterRoleText.Text = character.Role;
                Title = $"Conversation with {character.Name}";
            }

            // Avatar
            LoadAvatar(characterId);

            // Relationship bar + label
            UpdateRelationshipDisplay();
        }

        private void LoadAvatar(string charId)
        {
            string path = charId switch
            {
                "joan"          => "images/assistant.png",
                "marcus_vey"    => "images/char/marcus_vey.png",
                "evelyn_cross"  => "images/char/evelyn_cross.png",
                "vincent_duro"  => "images/char/vincent_duro.png",
                "lucinda_vale"  => "images/char/lucinda_vale.png",
                "gregory_shaw"  => "images/char/gregory_shaw.png",
                "selena_park"   => "images/char/selena_park.png",
                "harold_finch"  => "images/char/harold_finch.png",
                "sophie_kim"    => "images/char/sophie_kim.png",
                _               => "images/assistant.png"
            };

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri($"pack://application:,,,/{path}");
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                CharacterAvatar.Source = bmp;
            }
            catch { /* leave blank */ }
        }

        private void UpdateRelationshipDisplay()
        {
            if (!relationships.TryGetValue(characterId, out var rel)) return;

            int avg = (rel.TrustLevel + rel.ProfessionalRespect + rel.PersonalConnection) / 3;

            // Bar width: map -100..100 → 0..160 px
            double barWidth = Math.Clamp((avg + 100) / 200.0 * 160.0, 0, 160);
            RelationshipBar.Width = barWidth;

            // Bar colour
            Color barColor = avg switch
            {
                >= 60  => Color.FromRgb(76, 175, 80),   // green
                >= 30  => Color.FromRgb(74, 158, 255),  // blue
                >= 0   => Color.FromRgb(180, 180, 60),  // yellow
                >= -30 => Color.FromRgb(220, 120, 40),  // orange
                _      => Color.FromRgb(200, 60, 60),   // red
            };
            RelationshipBar.Background = new SolidColorBrush(barColor);

            string label = avg switch
            {
                >= 60  => "Excellent",
                >= 30  => "Good",
                >= 0   => "Neutral",
                >= -30 => "Strained",
                _      => "Poor"
            };
            RelationshipText.Text = $"Relationship: {label}  ({avg:+0;-0;0})";
        }

        // ── Conversation flow ────────────────────────────────────────────────
        private void StartConversation()
        {
            if (!conversation.Nodes.TryGetValue(conversation.CurrentNodeId, out currentNode))
                return;

            ShowCharacterBubble(currentNode.GetAdaptiveDialogueText(relationships, storyFlags));
            ShowChoices(currentNode);
        }

        private void AdvanceToNode(string nodeId)
        {
            if (!conversation.Nodes.TryGetValue(nodeId, out currentNode))
            {
                // No more nodes — close after a short pause
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(800);
                    Close();
                });
                return;
            }

            conversation.CurrentNodeId = nodeId;
            ShowCharacterBubble(currentNode.GetAdaptiveDialogueText(relationships, storyFlags));
            ShowChoices(currentNode);
        }

        // ── Bubble rendering ─────────────────────────────────────────────────

        /// <summary>Adds a character speech bubble (left-aligned).</summary>
        private void ShowCharacterBubble(string text)
        {
            var bubble = BuildBubble(
                text,
                CharBubbleColor,
                HorizontalAlignment.Left,
                isCharacter: true);

            BubbleContainer.Children.Add(bubble);
            ScrollToBottom();
        }

        /// <summary>Adds a player reply bubble (right-aligned).</summary>
        private void ShowPlayerBubble(string text, ChoiceTone tone)
        {
            Color bg = ToneColors.TryGetValue(tone, out var c) ? c : PlayerBubbleColor;

            var bubble = BuildBubble(
                text,
                bg,
                HorizontalAlignment.Right,
                isCharacter: false);

            BubbleContainer.Children.Add(bubble);
            ScrollToBottom();
        }

        private UIElement BuildBubble(string text, Color bgColor, HorizontalAlignment align, bool isCharacter)
        {
            // Outer row — controls left/right alignment
            var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };

            // Bubble border
            var bubble = new Border
            {
                Background        = new SolidColorBrush(bgColor),
                CornerRadius      = isCharacter
                    ? new CornerRadius(4, 16, 16, 16)   // tail top-left
                    : new CornerRadius(16, 4, 16, 16),  // tail top-right
                Padding           = new Thickness(14, 10, 14, 10),
                MaxWidth          = 360,
                HorizontalAlignment = align,
                Margin            = isCharacter
                    ? new Thickness(0, 0, 60, 0)
                    : new Thickness(60, 0, 0, 0),
            };

            bubble.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color       = Colors.Black,
                Opacity     = 0.25,
                BlurRadius  = 6,
                ShadowDepth = 1
            };

            var tb = new TextBlock
            {
                Text            = text,
                TextWrapping    = TextWrapping.Wrap,
                FontSize        = 13,
                LineHeight      = 20,
                Foreground      = isCharacter
                    ? new SolidColorBrush(Color.FromRgb(220, 230, 245))
                    : new SolidColorBrush(Color.FromRgb(210, 240, 220)),
            };

            bubble.Child = tb;
            row.Children.Add(bubble);

            // Fade-in animation
            bubble.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            bubble.BeginAnimation(OpacityProperty, fadeIn);

            return row;
        }

        // ── Choice buttons ───────────────────────────────────────────────────
        private void ShowChoices(DialogueNode node)
        {
            ChoiceButtonsPanel.Children.Clear();

            var choices = node.GetAvailableChoices(relationships, storyFlags);

            if (choices.Count == 0)
            {
                // No choices — auto-close
                ChoicesPanel.Visibility = Visibility.Collapsed;
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(1200);
                    Close();
                });
                return;
            }

            ChoicesPanel.Visibility = Visibility.Visible;

            // Prompt text
            ChoicesPromptText.Text = node.EmotionalTone switch
            {
                EmotionalTone.Tense       => "Choose your words carefully...",
                EmotionalTone.Warm        => "How do you respond?",
                EmotionalTone.Serious     => "What do you say?",
                EmotionalTone.Playful     => "How do you reply?",
                EmotionalTone.Concerned   => "What do you tell them?",
                EmotionalTone.Excited     => "How do you respond?",
                _                         => "How do you respond?"
            };

            foreach (var choice in choices)
                ChoiceButtonsPanel.Children.Add(BuildChoiceButton(choice));
        }

        private UIElement BuildChoiceButton(DialogueChoice choice)
        {
            Color bg    = ToneColors.TryGetValue(choice.Tone, out var c)      ? c      : Color.FromRgb(42, 42, 62);
            Color hover = ToneHoverColors.TryGetValue(choice.Tone, out var hc) ? hc : Color.FromRgb(60, 60, 90);

            var btn = new Button
            {
                Margin                   = new Thickness(0, 0, 0, 6),
                Background               = new SolidColorBrush(bg),
                Foreground               = Brushes.White,
                BorderThickness          = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding                  = new Thickness(12, 9, 12, 9),
                Cursor                   = System.Windows.Input.Cursors.Hand,
                Tag                      = choice,
            };

            // Custom template — rounded corners, no default chrome
            var template = new ControlTemplate(typeof(Button));
            var borderFef = new FrameworkElementFactory(typeof(Border));
            borderFef.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFef.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            borderFef.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Button.PaddingProperty));
            var cpFef = new FrameworkElementFactory(typeof(ContentPresenter));
            cpFef.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            cpFef.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFef.AppendChild(cpFef);
            template.VisualTree = borderFef;

            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            style.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(bg)));
            var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(hover)));
            style.Triggers.Add(hoverTrigger);
            btn.Style = style;

            // Content: tone icon + text
            var row = new StackPanel { Orientation = Orientation.Horizontal };

            row.Children.Add(new TextBlock
            {
                Text               = GetToneIcon(choice.Tone),
                FontSize           = 15,
                Margin             = new Thickness(0, 0, 8, 0),
                VerticalAlignment  = VerticalAlignment.Center,
            });

            row.Children.Add(new TextBlock
            {
                Text             = choice.ChoiceText,
                TextWrapping     = TextWrapping.Wrap,
                FontSize         = 12,
                MaxWidth         = 380,
                VerticalAlignment = VerticalAlignment.Center,
            });

            btn.Content = row;
            btn.Click  += (_, _) => OnChoiceSelected(choice);

            return btn;
        }

        private static string GetToneIcon(ChoiceTone tone) => tone switch
        {
            ChoiceTone.Professional => "💼",
            ChoiceTone.Supportive   => "🤝",
            ChoiceTone.Aggressive   => "⚡",
            ChoiceTone.Diplomatic   => "🕊️",
            ChoiceTone.Personal     => "💬",
            ChoiceTone.Humorous     => "😄",
            _                       => "▶",
        };

        // ── Choice handling ──────────────────────────────────────────────────
        private void OnChoiceSelected(DialogueChoice choice)
        {
            // Disable all choice buttons immediately
            foreach (UIElement el in ChoiceButtonsPanel.Children)
                if (el is Button b) b.IsEnabled = false;

            // Show player bubble
            ShowPlayerBubble(choice.ChoiceText, choice.Tone);

            // Apply consequences
            ApplyConsequences(choice);

            // Record choice
            if (storyModeManager?.IsStoryMode == true)
                RecordChoice(choice);

            // Update relationship display
            UpdateRelationshipDisplay();

            // Show reaction bubble then advance
            if (!string.IsNullOrWhiteSpace(choice.CharacterReaction))
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(350);
                    ShowCharacterBubble(choice.CharacterReaction);

                    await System.Threading.Tasks.Task.Delay(400);
                    NavigateNext(choice.NextNodeId);
                });
            }
            else
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(350);
                    NavigateNext(choice.NextNodeId);
                });
            }
        }

        private void NavigateNext(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId) || nextNodeId == "end")
            {
                ChoicesPanel.Visibility = Visibility.Collapsed;
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(900);
                    Close();
                });
            }
            else
            {
                AdvanceToNode(nextNodeId);
            }
        }

        private void ApplyConsequences(DialogueChoice choice)
        {
            // Legacy RelationshipChanges dict
            foreach (var kv in choice.RelationshipChanges)
            {
                if (relationships.TryGetValue(kv.Key, out var rel))
                    rel.TrustLevel = Math.Clamp(rel.TrustLevel + kv.Value, -100, 100);
            }

            // Rich RelationshipImpact
            var impact = choice.RelationshipImpact;
            if (!string.IsNullOrEmpty(impact.PrimaryCharacter) &&
                relationships.TryGetValue(impact.PrimaryCharacter, out var r))
            {
                r.TrustLevel           = Math.Clamp(r.TrustLevel           + impact.TrustChange,              -100, 100);
                r.ProfessionalRespect  = Math.Clamp(r.ProfessionalRespect  + impact.RespectChange,             -100, 100);
                r.PersonalConnection   = Math.Clamp(r.PersonalConnection   + impact.PersonalConnectionChange,  -100, 100);
            }

            storyFlags.AddRange(choice.ConsequenceFlags);
        }

        private void RecordChoice(DialogueChoice choice)
        {
            if (storyModeManager == null) return;

            var record = new StoryChoiceRecord
            {
                Quarter              = currentQuarter,
                EventId              = conversation.ConversationId,
                ChoiceId             = choice.ChoiceId,
                ChoiceText           = choice.ChoiceText,
                RelationshipImpacts  = new Dictionary<string, int>(choice.RelationshipChanges),
                ConsequenceFlags     = new List<string>(choice.ConsequenceFlags),
            };

            storyModeManager.RecordPlayerChoice(record);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private void ScrollToBottom()
        {
            Dispatcher.InvokeAsync(() =>
                ChatScrollViewer.ScrollToEnd(),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    }
}
