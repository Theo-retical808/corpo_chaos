using System.Windows;
using CorporateChaos.Models;
using CorporateChaos.Systems;

namespace CorporateChaos.Views
{
    public partial class HighScoresWindow : Window
    {
        public class HighScoreDisplay
        {
            public int Rank { get; set; }
            public string Nickname { get; set; } = string.Empty;
            public int Score { get; set; }
            public double PeakCapital { get; set; }
            public double PeakRevenue { get; set; }
            public double PeakProfit { get; set; }
            public double PeakMarketShare { get; set; }
            public int PeakEmployees { get; set; }
            public int QuartersPlayed { get; set; }
            public string EndReason { get; set; } = string.Empty;
        }

        public HighScoresWindow(DataManager dataManager)
        {
            InitializeComponent();
            LoadHighScores(dataManager);
        }

        private void LoadHighScores(DataManager dataManager)
        {
            var scores = dataManager.GetTopScores(15); // Show more scores with wider display
            var displayScores = scores.Select((score, index) => new HighScoreDisplay
            {
                Rank = index + 1,
                Nickname = score.Nickname,
                Score = score.Score,
                PeakCapital = score.PeakCapital,
                PeakRevenue = score.PeakRevenue,
                PeakProfit = score.PeakProfit,
                PeakMarketShare = score.PeakMarketShare,
                PeakEmployees = score.PeakEmployees,
                QuartersPlayed = score.QuartersPlayed,
                EndReason = score.EndReason
            }).ToList();

            HighScoresDataGrid.ItemsSource = displayScores;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}