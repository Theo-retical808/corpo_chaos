using System.Windows;
using System.Windows.Input;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class NicknameDialog : Window
    {
        public string PlayerNickname { get; private set; } = string.Empty;
        public bool SaveScore { get; private set; } = false;
        
        private readonly GameConfig _config;
        private readonly int _playerScore;

        public NicknameDialog(int score, GameConfig config)
        {
            InitializeComponent();
            _playerScore = score;
            _config = config;
            
            ScoreText.Text = $"Your Score: {score:N0}";
            NicknameTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateNickname())
            {
                PlayerNickname = NicknameTextBox.Text.Trim();
                SaveScore = true;
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SaveScore = false;
            DialogResult = false;
            Close();
        }

        private void NicknameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OkButton_Click(sender, e);
            }
        }

        private bool ValidateNickname()
        {
            var nickname = NicknameTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(nickname))
            {
                ValidationText.Text = "Please enter a nickname.";
                return false;
            }
            
            if (nickname.Length < _config.MinimumNicknameLength)
            {
                ValidationText.Text = $"Nickname must be at least {_config.MinimumNicknameLength} characters long.";
                return false;
            }
            
            if (nickname.Length > _config.MaximumNicknameLength)
            {
                ValidationText.Text = $"Nickname must be no more than {_config.MaximumNicknameLength} characters long.";
                return false;
            }
            
            // Check for invalid characters
            if (nickname.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != ' '))
            {
                ValidationText.Text = "Nickname can only contain letters, numbers, spaces, hyphens, and underscores.";
                return false;
            }
            
            ValidationText.Text = "";
            return true;
        }
    }
}