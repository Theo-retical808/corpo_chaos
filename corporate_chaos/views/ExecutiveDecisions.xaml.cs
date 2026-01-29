using System.Windows;
using System.Windows.Controls;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class ExecutiveDecisions : Window
    {
        private Company company;
        private Dictionary<Department, DepartmentStats> departments;
        private Random random = new Random();
        
        public event Action<string>? DecisionMade;

        public ExecutiveDecisions(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            InitializeComponent();
            this.company = company;
            this.departments = departments;
            
            InitializeBudgetSliders();
            UpdateCompanyStatus();
        }

        private void InitializeBudgetSliders()
        {
            // Set current budget allocations
            MarketingBudgetSlider.Value = company.MarketingBudget;
            OperationsBudgetSlider.Value = company.OperationsBudget;
            FinanceBudgetSlider.Value = company.FinanceBudget;
            HRBudgetSlider.Value = company.HRBudget;
            ITBudgetSlider.Value = company.ITBudget;
            ResearchBudgetSlider.Value = company.ResearchBudget;
            
            // Set up event handlers for budget sliders
            MarketingBudgetSlider.ValueChanged += BudgetSlider_ValueChanged;
            OperationsBudgetSlider.ValueChanged += BudgetSlider_ValueChanged;
            FinanceBudgetSlider.ValueChanged += BudgetSlider_ValueChanged;
            HRBudgetSlider.ValueChanged += BudgetSlider_ValueChanged;
            ITBudgetSlider.ValueChanged += BudgetSlider_ValueChanged;
            ResearchBudgetSlider.ValueChanged += BudgetSlider_ValueChanged;
            
            UpdateBudgetDisplay();
        }

        private void BudgetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBudgetDisplay();
        }

        private void UpdateBudgetDisplay()
        {
            if (MarketingBudgetText == null) return; // Not fully initialized yet
            
            MarketingBudgetText.Text = $"{MarketingBudgetSlider.Value:F0}%";
            OperationsBudgetText.Text = $"{OperationsBudgetSlider.Value:F0}%";
            FinanceBudgetText.Text = $"{FinanceBudgetSlider.Value:F0}%";
            HRBudgetText.Text = $"{HRBudgetSlider.Value:F0}%";
            ITBudgetText.Text = $"{ITBudgetSlider.Value:F0}%";
            ResearchBudgetText.Text = $"{ResearchBudgetSlider.Value:F0}%";
            
            double total = MarketingBudgetSlider.Value + OperationsBudgetSlider.Value + 
                          FinanceBudgetSlider.Value + HRBudgetSlider.Value + 
                          ITBudgetSlider.Value + ResearchBudgetSlider.Value;
            
            TotalBudgetText.Text = $"Total: {total:F0}%";
            TotalBudgetText.Foreground = total == 100 ? System.Windows.Media.Brushes.LightGreen : System.Windows.Media.Brushes.Orange;
        }

        private void UpdateCompanyStatus()
        {
            CompanyStatusText.Text = $"Capital: ${company.Capital:N0} | Reputation: {company.Reputation} | Morale: {company.Morale} | Risk: {company.Risk}";
        }

        // Calculate market share gain with diminishing returns (same logic as Company class)
        private double GetMarketShareGain(double baseGain)
        {
            // Diminishing returns formula: gain decreases as market share increases
            double diminishingFactor = 1.0 - (company.MarketShare / 100.0);
            
            // Additional competitive pressure at higher market shares
            double competitivePressure = 1.0;
            if (company.MarketShare >= 50) competitivePressure = 0.5; // 50% harder above 50%
            if (company.MarketShare >= 60) competitivePressure = 0.3; // 70% harder above 60%
            if (company.MarketShare >= 65) competitivePressure = 0.2; // 80% harder above 65%
            
            return baseGain * diminishingFactor * competitivePressure;
        }

        // Financial Decisions
        private void CostCuttingLight_Click(object sender, RoutedEventArgs e)
        {
            if (company.Capital < 10000)
            {
                MessageBox.Show("Not enough capital to implement cost cutting measures!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double savings = company.QuarterlyExpenses * 0.05; // 5% savings
            company.Capital += savings;
            company.Morale -= 5;
            company.Risk += 3;
            
            DecisionMade?.Invoke($"✂️ Light cost cutting implemented! Saved ${savings:N0}, but morale decreased by 5 and risk increased by 3.");
            UpdateCompanyStatus();
        }

        private void CostCuttingMedium_Click(object sender, RoutedEventArgs e)
        {
            if (company.Capital < 25000)
            {
                MessageBox.Show("Not enough capital to implement cost cutting measures!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double savings = company.QuarterlyExpenses * 0.15; // 15% savings
            company.Capital += savings;
            company.Morale -= 12;
            company.Risk += 8;
            
            DecisionMade?.Invoke($"✂️ Medium cost cutting implemented! Saved ${savings:N0}, but morale decreased by 12 and risk increased by 8.");
            UpdateCompanyStatus();
        }

        private void CostCuttingHeavy_Click(object sender, RoutedEventArgs e)
        {
            if (company.Capital < 50000)
            {
                MessageBox.Show("Not enough capital to implement heavy cost cutting!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double savings = company.QuarterlyExpenses * 0.25; // 25% savings
            company.Capital += savings;
            company.Morale -= 20;
            company.Risk += 15;
            
            // Risk of employee exodus
            if (random.NextDouble() < 0.3) // 30% chance
            {
                DecisionMade?.Invoke($"✂️ Heavy cost cutting implemented! Saved ${savings:N0}, but caused major employee dissatisfaction. Some employees may quit!");
            }
            else
            {
                DecisionMade?.Invoke($"✂️ Heavy cost cutting implemented! Saved ${savings:N0}, but morale decreased by 20 and risk increased by 15.");
            }
            
            UpdateCompanyStatus();
        }

        private void BonusSmall_Click(object sender, RoutedEventArgs e)
        {
            double cost = 50000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for employee bonuses!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Morale += 15;
            
            // Boost employee productivity
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.Morale = Math.Min(100, employee.Morale + 10);
                    employee.Productivity = Math.Min(100, employee.Productivity + 3);
                }
            }
            
            DecisionMade?.Invoke($"🎁 Small employee bonuses distributed! Cost ${cost:N0}, morale increased by 15, employee productivity boosted!");
            UpdateCompanyStatus();
        }

        private void BonusLarge_Click(object sender, RoutedEventArgs e)
        {
            double cost = 150000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for large employee bonuses!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Morale += 25;
            
            // Significant boost to employee productivity
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.Morale = Math.Min(100, employee.Morale + 20);
                    employee.Productivity = Math.Min(100, employee.Productivity + 8);
                }
            }
            
            DecisionMade?.Invoke($"🎁 Large employee bonuses distributed! Cost ${cost:N0}, morale increased by 25, significant productivity boost!");
            UpdateCompanyStatus();
        }

        private void EmergencyLoan_Click(object sender, RoutedEventArgs e)
        {
            double loanAmount = 200000;
            company.Capital += loanAmount;
            company.Risk += 20;
            company.Reputation -= 10;
            
            DecisionMade?.Invoke($"🏦 Emergency loan of ${loanAmount:N0} secured! Risk increased by 20, reputation decreased by 10.");
            UpdateCompanyStatus();
        }

        // Strategic Decisions
        private void MarketingLocal_Click(object sender, RoutedEventArgs e)
        {
            double cost = 75000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for marketing campaign!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Reputation += random.Next(8, 15);
            
            // Reduced market share gain with diminishing returns
            double baseGain = random.NextDouble() * 1.0 + 0.5; // 0.5-1.5% instead of 1-3%
            double actualGain = GetMarketShareGain(baseGain);
            company.MarketShare += actualGain;
            company.Risk += 5;
            
            DecisionMade?.Invoke($"📢 Local marketing campaign launched! Cost ${cost:N0}, reputation increased, market share +{actualGain:F2}%, risk +5.");
            UpdateCompanyStatus();
        }

        private void MarketingNational_Click(object sender, RoutedEventArgs e)
        {
            double cost = 200000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for national marketing campaign!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Reputation += random.Next(15, 25);
            
            // Reduced market share gain with diminishing returns
            double baseGain = random.NextDouble() * 2.0 + 1.0; // 1-3% instead of 2-6%
            double actualGain = GetMarketShareGain(baseGain);
            company.MarketShare += actualGain;
            company.Risk += 12;
            
            DecisionMade?.Invoke($"📢 National marketing campaign launched! Cost ${cost:N0}, significant reputation boost, market share +{actualGain:F2}%, risk +12.");
            UpdateCompanyStatus();
        }

        private void RetreatWeekend_Click(object sender, RoutedEventArgs e)
        {
            double cost = 30000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for company retreat!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Morale += 12;
            company.Risk -= 5;
            
            // Boost employee morale and productivity
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.Morale = Math.Min(100, employee.Morale + 15);
                    employee.Productivity = Math.Min(100, employee.Productivity + 5);
                }
            }
            
            DecisionMade?.Invoke($"🏖️ Weekend company retreat organized! Cost ${cost:N0}, morale boosted, risk reduced, team productivity improved!");
            UpdateCompanyStatus();
        }

        private void RetreatWeek_Click(object sender, RoutedEventArgs e)
        {
            double cost = 80000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for week-long retreat!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Morale += 20;
            company.Risk -= 10;
            
            // Major boost to employee morale and productivity
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.Morale = Math.Min(100, employee.Morale + 25);
                    employee.Productivity = Math.Min(100, employee.Productivity + 10);
                }
            }
            
            DecisionMade?.Invoke($"🏖️ Week-long company retreat organized! Cost ${cost:N0}, major morale boost, risk significantly reduced, excellent team building!");
            UpdateCompanyStatus();
        }

        private void RDInvestment_Click(object sender, RoutedEventArgs e)
        {
            double cost = 120000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for R&D investment!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            
            // Reduced market share gain with diminishing returns
            double baseGain = random.NextDouble() * 1.5 + 1.0; // 1-2.5% instead of 2-5%
            double actualGain = GetMarketShareGain(baseGain);
            company.MarketShare += actualGain;
            company.Reputation += random.Next(10, 18);
            company.Risk += 8;
            
            DecisionMade?.Invoke($"🔬 Major R&D investment made! Cost ${cost:N0}, market share +{actualGain:F2}%, reputation increased, innovation risk +8.");
            UpdateCompanyStatus();
        }

        private void CrisisManagement_Click(object sender, RoutedEventArgs e)
        {
            double cost = 100000;
            if (company.Capital < cost)
            {
                MessageBox.Show("Not enough capital for crisis management consultants!", "Insufficient Funds", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            company.Capital -= cost;
            company.Risk -= 15;
            company.Reputation += random.Next(5, 12);
            
            DecisionMade?.Invoke($"🚨 Crisis management consultants hired! Cost ${cost:N0}, risk reduced by 15, reputation improved through better crisis handling.");
            UpdateCompanyStatus();
        }

        // Department Budget Allocation
        private void ApplyBudget_Click(object sender, RoutedEventArgs e)
        {
            double total = MarketingBudgetSlider.Value + OperationsBudgetSlider.Value + 
                          FinanceBudgetSlider.Value + HRBudgetSlider.Value + 
                          ITBudgetSlider.Value + ResearchBudgetSlider.Value;
            
            if (Math.Abs(total - 100) > 0.1)
            {
                MessageBox.Show("Budget allocation must total exactly 100%!", "Invalid Budget", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Save budget allocations to company
            company.MarketingBudget = MarketingBudgetSlider.Value;
            company.OperationsBudget = OperationsBudgetSlider.Value;
            company.FinanceBudget = FinanceBudgetSlider.Value;
            company.HRBudget = HRBudgetSlider.Value;
            company.ITBudget = ITBudgetSlider.Value;
            company.ResearchBudget = ResearchBudgetSlider.Value;

            // Apply immediate budget effects
            ApplyBudgetEffects();
            
            DecisionMade?.Invoke($"📊 Department budget allocation updated! New focus areas will affect department performance next quarter.");
            
            MessageBox.Show("Budget allocation applied successfully! Effects will be visible next quarter.", 
                "Budget Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyBudgetEffects()
        {
            // Marketing budget affects reputation and market share growth
            double marketingBudget = MarketingBudgetSlider.Value;
            if (marketingBudget >= 25)
            {
                company.Reputation += 3;
                company.MarketShare += 0.5;
            }
            else if (marketingBudget <= 5)
            {
                company.Reputation -= 2;
            }

            // Operations budget affects efficiency and risk
            double operationsBudget = OperationsBudgetSlider.Value;
            if (operationsBudget >= 25)
            {
                company.Risk -= 3;
                // Boost operations department efficiency
                if (departments.ContainsKey(Department.Operations))
                {
                    departments[Department.Operations].Efficiency += 5;
                }
            }
            else if (operationsBudget <= 10)
            {
                company.Risk += 5;
            }

            // Finance budget affects capital management
            double financeBudget = FinanceBudgetSlider.Value;
            if (financeBudget >= 20)
            {
                // Better financial management reduces quarterly expenses
                company.QuarterlyExpenses *= 0.95;
            }

            // HR budget affects employee morale and hiring quality
            double hrBudget = HRBudgetSlider.Value;
            if (hrBudget >= 20)
            {
                company.Morale += 5;
                // Boost HR department efficiency for better hiring
                if (departments.ContainsKey(Department.HR))
                {
                    departments[Department.HR].Efficiency += 10;
                }
            }
            else if (hrBudget <= 5)
            {
                company.Morale -= 3;
            }

            // IT budget affects risk and productivity
            double itBudget = ITBudgetSlider.Value;
            if (itBudget >= 25)
            {
                company.Risk -= 5;
                // Boost all employee productivity through better IT
                foreach (var dept in departments.Values)
                {
                    foreach (var employee in dept.Employees)
                    {
                        employee.Productivity = Math.Min(100, employee.Productivity + 2);
                    }
                }
            }
            else if (itBudget <= 10)
            {
                company.Risk += 8;
            }

            // Research budget affects innovation and market share
            double researchBudget = ResearchBudgetSlider.Value;
            if (researchBudget >= 25)
            {
                company.MarketShare += 1;
                company.Reputation += 2;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}