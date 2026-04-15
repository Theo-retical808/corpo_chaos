using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CorporateChaos.Systems
{
    public class NameData
    {
        [JsonPropertyName("maleFirstNames")]
        public List<string> MaleFirstNames { get; set; } = new();

        [JsonPropertyName("femaleFirstNames")]
        public List<string> FemaleFirstNames { get; set; } = new();

        [JsonPropertyName("lastNames")]
        public List<string> LastNames { get; set; } = new();
    }

    public class DepartmentPositionData
    {
        [JsonPropertyName("descriptions")]
        public List<string> Descriptions { get; set; } = new();

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();
    }

    public class PositionData
    {
        [JsonPropertyName("departments")]
        public Dictionary<string, DepartmentPositionData> Departments { get; set; } = new();
    }

    public class CrisisTypeData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class EventData
    {
        [JsonPropertyName("marketDisruptions")]
        public List<string> MarketDisruptions { get; set; } = new();

        [JsonPropertyName("competitorActions")]
        public List<string> CompetitorActions { get; set; } = new();

        [JsonPropertyName("financialCrises")]
        public List<string> FinancialCrises { get; set; } = new();

        [JsonPropertyName("scandals")]
        public List<string> Scandals { get; set; } = new();

        [JsonPropertyName("mismanagements")]
        public List<string> Mismanagements { get; set; } = new();

        [JsonPropertyName("positivePR")]
        public List<string> PositivePR { get; set; } = new();

        [JsonPropertyName("miscommunications")]
        public List<string> Miscommunications { get; set; } = new();

        [JsonPropertyName("teamBuildingSuccesses")]
        public List<string> TeamBuildingSuccesses { get; set; } = new();

        [JsonPropertyName("productDefects")]
        public List<string> ProductDefects { get; set; } = new();

        [JsonPropertyName("qualitySuccesses")]
        public List<string> QualitySuccesses { get; set; } = new();

        [JsonPropertyName("catastrophicEvents")]
        public List<string> CatastrophicEvents { get; set; } = new();

        [JsonPropertyName("randomChaos")]
        public List<string> RandomChaos { get; set; } = new();

        [JsonPropertyName("crisisTypes")]
        public List<CrisisTypeData> CrisisTypes { get; set; } = new();
    }

    public static class GameDataLoader
    {
        private static NameData? _nameData;
        private static PositionData? _positionData;
        private static EventData? _eventData;

        private static string GetDataPath(string filename)
        {
            // Try multiple paths for data files
            var paths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", filename),
                Path.Combine(Directory.GetCurrentDirectory(), "data", filename),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "data", filename)
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }

            return paths[0]; // Return first path as default
        }

        public static NameData LoadNames()
        {
            if (_nameData != null) return _nameData;

            try
            {
                var path = GetDataPath("names.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    _nameData = JsonSerializer.Deserialize<NameData>(json) ?? new NameData();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Names data file not found at: {path}");
                    _nameData = GetDefaultNames();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading names: {ex.Message}");
                _nameData = GetDefaultNames();
            }

            return _nameData;
        }

        public static PositionData LoadPositions()
        {
            if (_positionData != null) return _positionData;

            try
            {
                var path = GetDataPath("positions.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    _positionData = JsonSerializer.Deserialize<PositionData>(json) ?? new PositionData();
                }
                else
                {
                    _positionData = new PositionData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading positions: {ex.Message}");
                _positionData = new PositionData();
            }

            return _positionData;
        }

        public static EventData LoadEvents()
        {
            if (_eventData != null) return _eventData;

            try
            {
                var path = GetDataPath("events.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    _eventData = JsonSerializer.Deserialize<EventData>(json) ?? new EventData();
                }
                else
                {
                    _eventData = GetDefaultEvents();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading events: {ex.Message}");
                _eventData = GetDefaultEvents();
            }

            return _eventData;
        }

        /// <summary>
        /// Clears cached data so it will be reloaded from disk on next access.
        /// </summary>
        public static void ReloadAll()
        {
            _nameData = null;
            _positionData = null;
            _eventData = null;
        }

        private static NameData GetDefaultNames()
        {
            return new NameData
            {
                MaleFirstNames = new List<string> { "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles" },
                FemaleFirstNames = new List<string> { "Mary", "Patricia", "Jennifer", "Linda", "Barbara", "Elizabeth", "Susan", "Jessica", "Sarah", "Karen" },
                LastNames = new List<string> { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" }
            };
        }

        private static EventData GetDefaultEvents()
        {
            return new EventData
            {
                MarketDisruptions = new List<string> { "Market disruption affects your business" },
                CompetitorActions = new List<string> { "launches aggressive campaign" },
                FinancialCrises = new List<string> { "Financial crisis impacts operations" },
                Scandals = new List<string> { "internal issue becomes public" },
                Mismanagements = new List<string> { "management error causes problems" },
                PositivePR = new List<string> { "Company receives positive coverage" },
                Miscommunications = new List<string> { "Communication breakdown occurs" },
                TeamBuildingSuccesses = new List<string> { "Team building event succeeds" },
                ProductDefects = new List<string> { "product issue discovered" },
                QualitySuccesses = new List<string> { "Quality milestone achieved" },
                CatastrophicEvents = new List<string> { "major incident affects company" },
                RandomChaos = new List<string> { "Something unexpected happens at the office" },
                CrisisTypes = new List<CrisisTypeData> { new() { Title = "Crisis", Description = "A crisis has occurred" } }
            };
        }
    }
}
