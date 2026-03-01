using System.Windows;
using System.Windows.Controls;
using CorporateChaos.Models;
using CorporateChaos.Systems;

namespace CorporateChaos.Views
{
    public partial class OptionsWindow : Window
    {
        private GameSettings settings = null!;
        private BackgroundMusicManager? musicManager;
        private Window? mainWindow;

        public OptionsWindow(BackgroundMusicManager? musicMgr = null, Window? owner = null)
        {
            InitializeComponent();
            musicManager = musicMgr;
            mainWindow = owner;

            if (owner != null)
            {
                Owner = owner;
            }

            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            settings = SettingsManager.GetCurrentSettings();

            // Load audio settings
            VolumeSlider.Value = settings.Volume * 100;
            VolumePercentText.Text = $"{(int)(settings.Volume * 100)}%";
            MuteCheckBox.IsChecked = settings.IsMuted;

            // Load display settings
            if (settings.IsFullscreen)
            {
                FullscreenModeRadio.IsChecked = true;
            }
            else
            {
                WindowedModeRadio.IsChecked = true;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumePercentText != null)
            {
                int volumePercent = (int)VolumeSlider.Value;
                VolumePercentText.Text = $"{volumePercent}%";
                
                // Update volume in real-time
                double volume = VolumeSlider.Value / 100.0;
                settings.Volume = volume;
                musicManager?.SetVolume(volume);
            }
        }

        private void MuteCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (MuteCheckBox != null)
            {
                settings.IsMuted = MuteCheckBox.IsChecked ?? false;
                
                // Apply mute immediately
                if (settings.IsMuted)
                {
                    musicManager?.SetVolume(0);
                }
                else
                {
                    musicManager?.SetVolume(settings.Volume);
                }
            }
        }

        private void DisplayMode_Changed(object sender, RoutedEventArgs e)
        {
            if (FullscreenModeRadio == null || mainWindow == null) return;

            bool isFullscreen = FullscreenModeRadio.IsChecked ?? false;
            settings.IsFullscreen = isFullscreen;

            // Apply display mode immediately
            ApplyDisplayMode(isFullscreen);
        }

        private void ApplyDisplayMode(bool isFullscreen)
        {
            if (mainWindow == null) return;

            if (isFullscreen)
            {
                // Save current window size before going fullscreen
                if (mainWindow.WindowState != WindowState.Maximized)
                {
                    settings.WindowWidth = mainWindow.Width;
                    settings.WindowHeight = mainWindow.Height;
                }

                mainWindow.WindowStyle = WindowStyle.None;
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.ResizeMode = ResizeMode.CanResize;
                
                // Restore previous window size
                mainWindow.Width = settings.WindowWidth;
                mainWindow.Height = settings.WindowHeight;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            // Save settings to file
            SettingsManager.SaveSettings(settings);
            
            MessageBox.Show("Settings saved successfully!", "Settings", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            // Save settings before closing
            SettingsManager.SaveSettings(settings);
            Close();
        }
    }
}
