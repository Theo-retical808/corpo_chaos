using System.Text.Json.Serialization;

namespace CorporateChaos.Models
{
    /// <summary>
    /// Stores user preferences and game settings
    /// </summary>
    public class GameSettings
    {
        [JsonPropertyName("volume")]
        public double Volume { get; set; } = 0.3; // Default 30%

        [JsonPropertyName("isMuted")]
        public bool IsMuted { get; set; } = false;

        [JsonPropertyName("isFullscreen")]
        public bool IsFullscreen { get; set; } = false;

        [JsonPropertyName("windowWidth")]
        public double WindowWidth { get; set; } = 1400;

        [JsonPropertyName("windowHeight")]
        public double WindowHeight { get; set; } = 800;

        public GameSettings()
        {
        }
    }
}
