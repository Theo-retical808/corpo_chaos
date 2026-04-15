using System.Windows;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class QuarterlySummary : Window
    {
        public QuarterlySummary(int quarter, Company company, Dictionary<Department, DepartmentStats> departments, List<string> events)
        {
            InitializeComponent();
            
            // Set quarter title
            QuarterTitleText.Text = $"📊 QUARTERLY SUMMARY - Q{quarter}";
            
            // Update financial data - expenses include operational + crisis + decisions
            double totalExpenses = company.QuarterlyExpenses + company.NetLoss + company.DecisionExpenses;
            RevenueText.Text = $"${company.QuarterlyRevenue:N0}";
            ExpensesText.Text = $"${totalExpenses:N0}";
            double netResult = company.QuarterlyRevenue - totalExpenses;
            if (netResult >= 0)
            {
                NetResultLabel.Text = "Net Profit";
                NetResultText.Text = $"${netResult:N0}";
                NetResultText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                NetResultLabel.Text = "Net Loss";
                NetResultText.Text = $"-${Math.Abs(netResult):N0}";
                NetResultText.Foreground = System.Windows.Media.Brushes.LightCoral;
            }
            
            // Update company metrics
            CapitalText.Text = $"${company.Capital:N0}";
            ReputationText.Text = company.Reputation.ToString();
            MoraleText.Text = company.Morale.ToString();
            RiskText.Text = company.Risk.ToString();
            
            // Color code metrics based on values
            ReputationText.Foreground = GetMetricColor(company.Reputation);
            MoraleText.Foreground = GetMetricColor(company.Morale);
            RiskText.Foreground = GetRiskColor(company.Risk);
            
            // Update events
            if (events.Count > 0)
            {
                EventsText.Text = string.Join("\n\n", events);
            }
            else
            {
                EventsText.Text = "No significant events occurred this quarter.";
            }
        }

        private System.Windows.Media.Brush GetMetricColor(int value)
        {
            return value switch
            {
                >= 50 => System.Windows.Media.Brushes.LightGreen,
                >= 20 => System.Windows.Media.Brushes.Yellow,
                >= 0 => System.Windows.Media.Brushes.White,
                >= -20 => System.Windows.Media.Brushes.Orange,
                _ => System.Windows.Media.Brushes.LightCoral
            };
        }

        private System.Windows.Media.Brush GetRiskColor(int risk)
        {
            return risk switch
            {
                >= 50 => System.Windows.Media.Brushes.Red,
                >= 20 => System.Windows.Media.Brushes.Orange,
                >= 0 => System.Windows.Media.Brushes.Yellow,
                >= -20 => System.Windows.Media.Brushes.LightGreen,
                _ => System.Windows.Media.Brushes.Cyan
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}