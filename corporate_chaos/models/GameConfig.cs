using System.Xml.Serialization;

namespace CorporateChaos.Models
{
    [XmlRoot("GameConfiguration")]
    public class GameConfig
    {
        [XmlElement("MaxHighScores")]
        public int MaxHighScores { get; set; } = 10;

        [XmlElement("DefaultStartingCapital")]
        public double DefaultStartingCapital { get; set; } = 100000;

        [XmlElement("DefaultStartingEmployees")]
        public int DefaultStartingEmployees { get; set; } = 5;

        [XmlElement("ScoreMultiplier")]
        public double ScoreMultiplier { get; set; } = 1.0;

        [XmlElement("AutoSaveEnabled")]
        public bool AutoSaveEnabled { get; set; } = true;

        [XmlElement("ShowScoreCalculation")]
        public bool ShowScoreCalculation { get; set; } = true;

        [XmlElement("MinimumNicknameLength")]
        public int MinimumNicknameLength { get; set; } = 2;

        [XmlElement("MaximumNicknameLength")]
        public int MaximumNicknameLength { get; set; } = 20;
    }
}