using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class FinancialReport : Window
    {
        public FinancialReport(int quarter, Company company, Dictionary<Department, DepartmentStats> departments, double startingCapital)
        {
            InitializeComponent();
            PopulateReport(quarter, company, departments, startingCapital);
        }

        private void PopulateReport(int quarter, Company company, Dictionary<Department, DepartmentStats> departments, double startingCapital)
        {
            TitleText.Text = $"📑 FINANCIAL REPORT - Q{quarter}";

            // Revenue breakdown
            double baseRevenue = company.MarketShare * 15000;
            double deptProductivity = departments.Values.Sum(d => d.GetTotalProductivity()) / 80.0;
            double reputationMod = company.GetReputationRevenueModifier();
            double revenueBeforeReputation = baseRevenue * (1 + deptProductivity);

            BaseRevenueText.Text = $"${baseRevenue:N0}";
            DeptBonusText.Text = $"+${(revenueBeforeReputation - baseRevenue):N0}";
            ReputationModText.Text = $"x{reputationMod:F2}";
            TotalRevenueText.Text = $"${company.QuarterlyRevenue:N0}";

            // Expenses breakdown
            double salaries = departments.Values.Sum(d => d.GetQuarterlyCost());
            double operational = company.QuarterlyExpenses - salaries;
            double decisionExp = company.DecisionExpenses;
            double crisisLoss = company.NetLoss;
            double totalExpenses = company.QuarterlyExpenses + decisionExp + crisisLoss;

            SalariesText.Text = $"${salaries:N0}";
            OperationalText.Text = $"${operational:N0}";
            DecisionExpText.Text = decisionExp > 0 ? $"${decisionExp:N0}" : "$0";
            CrisisLossText.Text = crisisLoss > 0 ? $"${crisisLoss:N0}" : "$0";
            TotalExpensesText.Text = $"${totalExpenses:N0}";

            // Net result
            double netResult = company.QuarterlyRevenue - totalExpenses;
            if (netResult >= 0)
            {
                NetResultLabel.Text = "📈 NET PROFIT";
                NetResultText.Text = $"+${netResult:N0}";
                NetResultText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7dcea0"));
                NetResultBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a3a2a"));
            }
            else
            {
                NetResultLabel.Text = "📉 NET LOSS";
                NetResultText.Text = $"-${Math.Abs(netResult):N0}";
                NetResultText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8a0a0"));
                NetResultBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a1a1a"));
            }

            // Capital summary
            StartCapitalText.Text = $"${startingCapital:N0}";
            EndCapitalText.Text = $"${company.Capital:N0}";
            double capitalChange = company.Capital - startingCapital;
            if (capitalChange >= 0)
            {
                CapitalChangeLabel.Text = "Gained";
                CapitalChangeText.Text = $"+${capitalChange:N0}";
                CapitalChangeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7dcea0"));
            }
            else
            {
                CapitalChangeLabel.Text = "Lost";
                CapitalChangeText.Text = $"-${Math.Abs(capitalChange):N0}";
                CapitalChangeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8a0a0"));
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
