using System.Text.Json.Serialization;

namespace CorporateChaos.Models
{
    public class GameScore
    {
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }

        // Peak Performance Metrics (highest achieved during the run)
        [JsonPropertyName("peakCapital")]
        public double PeakCapital { get; set; }

        [JsonPropertyName("peakRevenue")]
        public double PeakRevenue { get; set; }

        [JsonPropertyName("peakProfit")]
        public double PeakProfit { get; set; }

        [JsonPropertyName("peakMarketShare")]
        public double PeakMarketShare { get; set; }

        [JsonPropertyName("peakEmployees")]
        public int PeakEmployees { get; set; }

        [JsonPropertyName("peakReputation")]
        public int PeakReputation { get; set; }

        // Final/End Performance (for reference)
        [JsonPropertyName("finalCapital")]
        public double FinalCapital { get; set; }

        [JsonPropertyName("finalMarketShare")]
        public double FinalMarketShare { get; set; }

        [JsonPropertyName("finalEmployees")]
        public int FinalEmployees { get; set; }

        [JsonPropertyName("quartersPlayed")]
        public int QuartersPlayed { get; set; }

        [JsonPropertyName("endReason")]
        public string EndReason { get; set; } = string.Empty;

        [JsonPropertyName("dateAchieved")]
        public DateTime DateAchieved { get; set; }

        [JsonPropertyName("peakQuarter")]
        public int PeakQuarter { get; set; } // Which quarter the peak was achieved

        public int CalculateScore()
        {
            // New score calculation based on PEAK performance
            double baseScore = 0;
            
            // Peak Capital (major component)
            baseScore += (PeakCapital / 1000) * 2; // 2 points per $1000 peak capital
            
            // Peak Revenue (quarterly achievement)
            baseScore += (PeakRevenue / 1000) * 3; // 3 points per $1000 peak revenue
            
            // Peak Profit (most important metric)
            baseScore += (PeakProfit / 1000) * 5; // 5 points per $1000 peak profit
            
            // Peak Market Share
            baseScore += PeakMarketShare * 50; // 50 points per % market share
            
            // Peak Employees (growth achievement)
            baseScore += PeakEmployees * 20; // 20 points per employee at peak
            
            // Peak Reputation
            baseScore += PeakReputation * 10; // 10 points per reputation point
            
            // Survival bonus (quarters played)
            baseScore += QuartersPlayed * 100; // 100 points per quarter survived
            
            // Peak achievement bonuses
            if (PeakMarketShare >= 50)
                baseScore *= 2.0; // Market dominance bonus
            
            if (PeakProfit >= 100000)
                baseScore *= 1.5; // High profitability bonus
            
            if (PeakCapital >= 1000000)
                baseScore *= 1.3; // Millionaire bonus
            
            if (QuartersPlayed >= 20)
                baseScore *= 1.2; // Long-term survival bonus

            Score = (int)Math.Max(0, baseScore);
            return Score;
        }

        public void UpdatePeakMetrics(Company company, int currentQuarter)
        {
            // Update peak metrics if current values are higher
            if (company.Capital > PeakCapital)
            {
                PeakCapital = company.Capital;
                PeakQuarter = currentQuarter;
            }

            if (company.QuarterlyRevenue > PeakRevenue)
                PeakRevenue = company.QuarterlyRevenue;

            double currentProfit = company.QuarterlyRevenue - company.QuarterlyExpenses;
            if (currentProfit > PeakProfit)
                PeakProfit = currentProfit;

            if (company.MarketShare > PeakMarketShare)
                PeakMarketShare = company.MarketShare;

            if (company.EmployeeCount > PeakEmployees)
                PeakEmployees = company.EmployeeCount;

            if (company.Reputation > PeakReputation)
                PeakReputation = company.Reputation;
        }
    }

    public class HighScoreData
    {
        [JsonPropertyName("scores")]
        public List<GameScore> Scores { get; set; } = new List<GameScore>();

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}