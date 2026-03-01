using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CorporateChaos.Views
{
    public partial class ModernMessageBox : Window
    {
        public enum MessageBoxType
        {
            Information,
            Question,
            Warning,
            Error,
            Success
        }

        public enum MessageBoxButtons
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel
        }

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        private ModernMessageBox(string message, string title, MessageBoxType type, MessageBoxButtons buttons)
        {
            InitializeComponent();
            
            TitleText.Text = title;
            MessageText.Text = message;
            
            // Set icon and header color based on type
            SetMessageBoxStyle(type);
            
            // Add buttons based on button type
            AddButtons(buttons);
        }

        private void SetMessageBoxStyle(MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.Information:
                    IconText.Text = "ℹ️";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a4a6e"));
                    break;
                case MessageBoxType.Question:
                    IconText.Text = "❓";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4a6e"));
                    break;
                case MessageBoxType.Warning:
                    IconText.Text = "⚠️";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6e5a2a"));
                    break;
                case MessageBoxType.Error:
                    IconText.Text = "❌";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6e2a2a"));
                    break;
                case MessageBoxType.Success:
                    IconText.Text = "✅";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a6e4a"));
                    break;
            }
        }

        private void AddButtons(MessageBoxButtons buttons)
        {
            ButtonPanel.Children.Clear();

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    AddButton("OK", MessageBoxResult.OK, "PrimaryButton");
                    break;
                    
                case MessageBoxButtons.OKCancel:
                    AddButton("Cancel", MessageBoxResult.Cancel, "GhostButton", 10);
                    AddButton("OK", MessageBoxResult.OK, "PrimaryButton");
                    break;
                    
                case MessageBoxButtons.YesNo:
                    AddButton("No", MessageBoxResult.No, "GhostButton", 10);
                    AddButton("Yes", MessageBoxResult.Yes, "SuccessButton");
                    break;
                    
                case MessageBoxButtons.YesNoCancel:
                    AddButton("Cancel", MessageBoxResult.Cancel, "GhostButton", 10);
                    AddButton("No", MessageBoxResult.No, "SecondaryButton", 10);
                    AddButton("Yes", MessageBoxResult.Yes, "SuccessButton");
                    break;
            }
        }

        private void AddButton(string content, MessageBoxResult result, string styleName, int leftMargin = 0)
        {
            var button = new Button
            {
                Content = content,
                MinWidth = 100,
                Margin = new Thickness(leftMargin, 0, 0, 0),
                Style = (Style)FindResource(styleName)
            };
            
            button.Click += (s, e) =>
            {
                Result = result;
                DialogResult = true;
                Close();
            };
            
            ButtonPanel.Children.Add(button);
        }

        // Static Show methods
        public static MessageBoxResult Show(string message, string title = "Message", 
            MessageBoxType type = MessageBoxType.Information, 
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            Window? owner = null)
        {
            var dialog = new ModernMessageBox(message, title, type, buttons);
            
            if (owner != null)
            {
                dialog.Owner = owner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            
            dialog.ShowDialog();
            return dialog.Result;
        }

        // Convenience methods
        public static MessageBoxResult ShowInformation(string message, string title = "Information", Window? owner = null)
        {
            return Show(message, title, MessageBoxType.Information, MessageBoxButtons.OK, owner);
        }

        public static MessageBoxResult ShowQuestion(string message, string title = "Question", Window? owner = null)
        {
            return Show(message, title, MessageBoxType.Question, MessageBoxButtons.YesNo, owner);
        }

        public static MessageBoxResult ShowWarning(string message, string title = "Warning", Window? owner = null)
        {
            return Show(message, title, MessageBoxType.Warning, MessageBoxButtons.OK, owner);
        }

        public static MessageBoxResult ShowError(string message, string title = "Error", Window? owner = null)
        {
            return Show(message, title, MessageBoxType.Error, MessageBoxButtons.OK, owner);
        }

        public static MessageBoxResult ShowSuccess(string message, string title = "Success", Window? owner = null)
        {
            return Show(message, title, MessageBoxType.Success, MessageBoxButtons.OK, owner);
        }
    }
}
