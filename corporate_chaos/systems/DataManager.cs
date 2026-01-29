using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    public class DataManager
    {
        private const string HIGH_SCORES_FILE = "highscores.json";
        private const string CONFIG_FILE = "config.xml";
        private GameConfig _config;
        private HighScoreData _highScoreData;

        public DataManager()
        {
            _config = LoadConfig();
            _highScoreData = LoadHighScores();
        }

        public GameConfig GetConfig() => _config;

        public GameConfig LoadConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    var serializer = new XmlSerializer(typeof(GameConfig));
                    using var reader = new FileStream(CONFIG_FILE, FileMode.Open);
                    return (GameConfig)serializer.Deserialize(reader)!;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
            }

            // Return default config and save it
            var defaultConfig = new GameConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        public void SaveConfig(GameConfig config)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(GameConfig));
                using var writer = new FileStream(CONFIG_FILE, FileMode.Create);
                serializer.Serialize(writer, config);
                _config = config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public HighScoreData LoadHighScores()
        {
            try
            {
                if (File.Exists(HIGH_SCORES_FILE))
                {
                    var json = File.ReadAllText(HIGH_SCORES_FILE);
                    var data = JsonSerializer.Deserialize<HighScoreData>(json);
                    return data ?? new HighScoreData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading high scores: {ex.Message}");
            }

            return new HighScoreData();
        }

        public void SaveHighScores()
        {
            try
            {
                _highScoreData.LastUpdated = DateTime.Now;
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(_highScoreData, options);
                File.WriteAllText(HIGH_SCORES_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving high scores: {ex.Message}");
            }
        }

        public void AddScore(GameScore score)
        {
            score.CalculateScore();
            score.DateAchieved = DateTime.Now;
            
            _highScoreData.Scores.Add(score);
            
            // Sort by score descending and keep only top scores
            _highScoreData.Scores = _highScoreData.Scores
                .OrderByDescending(s => s.Score)
                .Take(_config.MaxHighScores)
                .ToList();
            
            SaveHighScores();
        }

        public List<GameScore> GetTopScores(int count = 10)
        {
            return _highScoreData.Scores
                .OrderByDescending(s => s.Score)
                .Take(count)
                .ToList();
        }

        public bool IsHighScore(int score)
        {
            if (_highScoreData.Scores.Count < _config.MaxHighScores)
                return true;
            
            return score > _highScoreData.Scores.Min(s => s.Score);
        }

        public int GetPlayerRank(string nickname)
        {
            var playerScores = _highScoreData.Scores
                .Where(s => s.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.Score)
                .ToList();

            if (!playerScores.Any())
                return -1;

            var bestScore = playerScores.First().Score;
            var allScores = _highScoreData.Scores.OrderByDescending(s => s.Score).ToList();
            
            return allScores.FindIndex(s => s.Score == bestScore) + 1;
        }
    }
}