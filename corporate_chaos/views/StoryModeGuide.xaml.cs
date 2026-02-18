using System.Windows;
using System.Windows.Media.Imaging;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class StoryModeGuide : Window
    {
        private StoryEvent currentEvent;
        private int currentDialogueIndex = 0;
        
        public bool IsCompleted { get; private set; } = false;

        public StoryModeGuide(StoryEvent storyEvent, int quarter)
        {
            InitializeComponent();
            currentEvent = storyEvent;
            LoadJoanAvatar();
            LoadStoryEvent(quarter);
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
            QuarterText.Text = quarter <= 8 ? $"Quarter {quarter} - Tutorial Phase" : $"Quarter {quarter} - Full Mode";
            
            // Load event content
            EventTitleText.Text = currentEvent.Title;
            EventDescriptionText.Text = currentEvent.Description;
            ObjectiveText.Text = currentEvent.ObjectiveText;
            
            // Load first dialogue
            currentDialogueIndex = 0;
            UpdateDialogue();
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