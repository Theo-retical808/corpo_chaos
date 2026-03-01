using System.IO;
using System.Text.Json;
using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Manages game settings persistence and retrieval
    /// </summary>
    public class SettingsManager
    {
        private const string SETTINGS_FILE = "settings.json";
        private static GameSettings? _currentSettings;

        public static GameSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    string json = File.ReadAllText(SETTINGS_FILE);
                    _currentSettings = JsonSerializer.Deserialize<GameSettings>(json);
                    if (_currentSettings != null)
                    {
                        return _currentSettings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            // Return default settings if load fails
            _currentSettings = new GameSettings();
            return _currentSettings;
        }

        public static void SaveSettings(GameSettings settings)
        {
            try
            {
                _currentSettings = settings;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SETTINGS_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public static GameSettings GetCurrentSettings()
        {
            return _currentSettings ?? LoadSettings();
        }
    }
}
