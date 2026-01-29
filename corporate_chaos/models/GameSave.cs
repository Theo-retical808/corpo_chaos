using System.Text.Json.Serialization;

namespace CorporateChaos.Models
{
    public class GameSave
    {
        [JsonPropertyName("saveName")]
        public string SaveName { get; set; } = string.Empty;

        [JsonPropertyName("playerNickname")]
        public string PlayerNickname { get; set; } = string.Empty;

        [JsonPropertyName("saveDate")]
        public DateTime SaveDate { get; set; } = DateTime.Now;

        [JsonPropertyName("currentQuarter")]
        public int CurrentQuarter { get; set; } = 1;

        [JsonPropertyName("company")]
        public Company Company { get; set; } = new Company();

        [JsonPropertyName("departments")]
        public Dictionary<Department, DepartmentStats> Departments { get; set; } = new Dictionary<Department, DepartmentStats>();

        [JsonPropertyName("availableEmployees")]
        public List<Employee> AvailableEmployees { get; set; } = new List<Employee>();

        [JsonPropertyName("gameEvents")]
        public List<string> GameEvents { get; set; } = new List<string>();

        [JsonPropertyName("quarterlyReports")]
        public List<QuarterlyReport> QuarterlyReports { get; set; } = new List<QuarterlyReport>();

        public string GetFileName()
        {
            return $"{SaveName}_{SaveDate:yyyyMMdd_HHmmss}.json";
        }
    }

    public class QuarterlyReport
    {
        [JsonPropertyName("quarter")]
        public int Quarter { get; set; }

        [JsonPropertyName("startingCapital")]
        public double StartingCapital { get; set; }

        [JsonPropertyName("endingCapital")]
        public double EndingCapital { get; set; }

        [JsonPropertyName("revenue")]
        public double Revenue { get; set; }

        [JsonPropertyName("expenses")]
        public double Expenses { get; set; }

        [JsonPropertyName("marketShareChange")]
        public double MarketShareChange { get; set; }

        [JsonPropertyName("employeeCount")]
        public int EmployeeCount { get; set; }

        [JsonPropertyName("majorEvents")]
        public List<string> MajorEvents { get; set; } = new List<string>();

        [JsonPropertyName("departmentPerformance")]
        public Dictionary<Department, double> DepartmentPerformance { get; set; } = new Dictionary<Department, double>();
    }

    public class GameRunRecord
    {
        [JsonPropertyName("runId")]
        public string RunId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("playerNickname")]
        public string PlayerNickname { get; set; } = string.Empty;

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("finalScore")]
        public int FinalScore { get; set; }

        [JsonPropertyName("quartersPlayed")]
        public int QuartersPlayed { get; set; }

        [JsonPropertyName("endReason")]
        public string EndReason { get; set; } = string.Empty;

        [JsonPropertyName("finalStats")]
        public Company FinalStats { get; set; } = new Company();

        [JsonPropertyName("quarterlyReports")]
        public List<QuarterlyReport> QuarterlyReports { get; set; } = new List<QuarterlyReport>();

        [JsonPropertyName("peakMarketShare")]
        public double PeakMarketShare { get; set; }

        [JsonPropertyName("maxEmployees")]
        public int MaxEmployees { get; set; }

        [JsonPropertyName("totalRevenue")]
        public double TotalRevenue { get; set; }
    }
}