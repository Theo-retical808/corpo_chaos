using System.Windows;
using System.Windows.Controls;
using CorporateChaos.Models;
using CorporateChaos.Systems;

namespace CorporateChaos.Views
{
    public partial class SaveFileManager : Window
    {
        private SaveLoadManager saveLoadManager;
        private List<GameSave> saveFiles;
        
        public GameSave? SelectedSave { get; private set; }
        public bool ShouldLoadGame { get; private set; } = false;

        public SaveFileManager(SaveLoadManager saveLoadManager)
        {
            InitializeComponent();
            this.saveLoadManager = saveLoadManager;
            this.saveFiles = new List<GameSave>();
            
            LoadSaveFiles();
        }

        private void LoadSaveFiles()
        {
            saveFiles.Clear();
            var saveFileNames = saveLoadManager.GetSaveFiles();
            
            foreach (var fileName in saveFileNames)
            {
                var gameSave = saveLoadManager.LoadGame(fileName);
                if (gameSave != null)
                {
                    saveFiles.Add(gameSave);
                }
            }
            
            RefreshSaveFilesList();
        }

        private void RefreshSaveFilesList()
        {
            if (saveFiles.Count == 0)
            {
                SaveFilesItemsControl.ItemsSource = null;
                NoSavesText.Visibility = Visibility.Visible;
            }
            else
            {
                SaveFilesItemsControl.ItemsSource = saveFiles.OrderByDescending(s => GetSaveFileDate(s)).ToList();
                NoSavesText.Visibility = Visibility.Collapsed;
            }
        }

        private DateTime GetSaveFileDate(GameSave save)
        {
            // Try to extract date from save name or use a default
            try
            {
                // Save names are typically in format: "Corporate_Q{quarter}_{MMdd_HHmm}"
                var parts = save.SaveName.Split('_');
                if (parts.Length >= 3)
                {
                    var datePart = parts[2]; // MMdd
                    var timePart = parts.Length > 3 ? parts[3] : "0000"; // HHmm
                    
                    if (datePart.Length == 4 && timePart.Length == 4)
                    {
                        int month = int.Parse(datePart.Substring(0, 2));
                        int day = int.Parse(datePart.Substring(2, 2));
                        int hour = int.Parse(timePart.Substring(0, 2));
                        int minute = int.Parse(timePart.Substring(2, 2));
                        
                        return new DateTime(DateTime.Now.Year, month, day, hour, minute, 0);
                    }
                }
            }
            catch
            {
                // If parsing fails, use current time
            }
            
            return DateTime.Now;
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GameSave gameSave)
            {
                SelectedSave = gameSave;
                ShouldLoadGame = true;
                DialogResult = true;
                Close();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GameSave gameSave)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the save file:\n\n{gameSave.SaveName}\n\nThis action cannot be undone!",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    string fileName = gameSave.GetFileName();
                    if (saveLoadManager.DeleteSave(fileName))
                    {
                        MessageBox.Show($"Save file '{gameSave.SaveName}' has been deleted.", 
                            "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Refresh the list
                        LoadSaveFiles();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to delete save file '{gameSave.SaveName}'.", 
                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadSaveFiles();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}