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
            UpdateRetreatCosts();
            UpdateDynamicPricing(); // Add dynamic pricing updates
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

        private void UpdateRetreatCosts()
        {
            int totalEmployees = departments.Values.Sum(d => d.GetEmployeeCount());
            
            // Calculate weekend retreat cost
            double weekendCost = 15000 + (totalEmployees * 800);
            RetreatWeekendBtn.Content = $"Weekend (${weekendCost:N0})";
            RetreatWeekendBtn.ToolTip = $"Cost: ${weekendCost:N0} (${15000:N0} base + ${800:N0} × {totalEmployees} employees)";
            
            // Calculate week-long retreat cost
            double weekCost = 35000 + (totalEmployees * 1500);
            RetreatWeekBtn.Content = $"Week (${weekCost:N0})";
            RetreatWeekBtn.ToolTip = $"Cost: ${weekCost:N0} (${35000:N0} base + ${1500:N0} × {totalEmployees} employees)";
        }

        private void UpdateDynamicPricing()
        {
            // Update Crisis Management pricing based on risk level
            double consultantCost = CalculateConsultantCost();
            CrisisManagementBtn.Content = $"Hire Consultants (${consultantCost:N0})";
            CrisisManagementBtn.ToolTip = $"Cost scales with risk level. Current risk: {company.Risk}";

            // Update Employee Bonus pricing based on employee count and positions
            var bonusCosts = CalculateBonusCosts();
            BonusSmallBtn.Content = $"Small (${bonusCosts.Small:N0})";
            BonusLargeBtn.Content = $"Large (${bonusCosts.Large:N0})";
            BonusSmallBtn.ToolTip = $"Cost based on {GetTotalEmployeeCount()} employees and their positions";
            BonusLargeBtn.ToolTip = $"Cost based on {GetTotalEmployeeCount()} employees and their positions";

            // Update Marketing pricing based on reputation
            var marketingCosts = CalculateMarketingCosts();
            MarketingLocalBtn.Content = $"Local (${marketingCosts.Local:N0})";
            MarketingNationalBtn.Content = $"National (${marketingCosts.National:N0})";
            MarketingLocalBtn.ToolTip = $"Cost affected by reputation ({company.Reputation}). Lower reputation = higher cost";
            MarketingNationalBtn.ToolTip = $"Cost affected by reputation ({company.Reputation}). Lower reputation = higher cost";
        }

        private double CalculateConsultantCost()
        {
            double baseCost = 100000;
            
            // Risk-based pricing: higher risk = more expensive consultants
            // Risk ranges from -100 to 100, but we focus on positive risk
            double riskMultiplier = 1.0;
            
            if (company.Risk <= 0)
                riskMultiplier = 0.7; // 30% discount for low/negative risk
            else if (company.Risk <= 25)
                riskMultiplier = 1.0; // Base price for moderate risk
            else if (company.Risk <= 50)
                riskMultiplier = 1.5; // 50% more expensive for high risk
            else if (company.Risk <= 75)
                riskMultiplier = 2.0; // 100% more expensive for very high risk
            else
                riskMultiplier = 3.0; // 200% more expensive for extreme risk
            
            return baseCost * riskMultiplier;
        }

        private (double Small, double Large) CalculateBonusCosts()
        {
            int totalEmployees = GetTotalEmployeeCount();
            double positionMultiplier = CalculatePositionMultiplier();
            
            // Base costs scale with employee count and position levels
            double baseSmall = 25000 + (totalEmployees * 2000); // $2K per employee base
            double baseLarge = 75000 + (totalEmployees * 5000); // $5K per employee base
            
            // Apply position multiplier (higher for senior employees)
            double smallCost = baseSmall * positionMultiplier;
            double largeCost = baseLarge * positionMultiplier;
            
            return (smallCost, largeCost);
        }

        private (double Local, double National) CalculateMarketingCosts()
        {
            double baseLocal = 100000;
            double baseNational = 275000;
            
            // Reputation-based pricing: lower reputation = higher marketing costs
            // Reputation ranges from -100 to 100
            double reputationMultiplier = 1.0;
            
            if (company.Reputation >= 50)
                reputationMultiplier = 0.7; // 30% discount for excellent reputation
            else if (company.Reputation >= 20)
                reputationMultiplier = 0.85; // 15% discount for good reputation
            else if (company.Reputation >= 0)
                reputationMultiplier = 1.0; // Base price for neutral reputation
            else if (company.Reputation >= -25)
                reputationMultiplier = 1.3; // 30% more expensive for poor reputation
            else if (company.Reputation >= -50)
                reputationMultiplier = 1.6; // 60% more expensive for bad reputation
            else
                reputationMultiplier = 2.0; // 100% more expensive for terrible reputation
            
            return (baseLocal * reputationMultiplier, baseNational * reputationMultiplier);
        }

        private int GetTotalEmployeeCount()
        {
            return departments.Values.Sum(d => d.GetEmployeeCount());
        }

        private double CalculatePositionMultiplier()
        {
            int totalEmployees = GetTotalEmployeeCount();
            if (totalEmployees == 0) return 1.0;
            
            double totalMultiplier = 0;
            int employeeCount = 0;
            
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    // Position-based multiplier
                    double positionMultiplier = employee.OverallSkill switch
                    {
                        SkillLevel.Trainee => 0.8,
                        SkillLevel.Junior => 1.0,
                        SkillLevel.Mid => 1.3,
                        SkillLevel.Senior => 1.6,
                        SkillLevel.Expert => 2.0,
                        _ => 1.0
                    };
                    
                    totalMultiplier += positionMultiplier;
                    employeeCount++;
                }
            }
            
            return employeeCount > 0 ? totalMultiplier / employeeCount : 1.0;
        }

        // Calculate market share gain with diminishing returns (same logic as Company class)
        private double GetMarketShareGain(double baseGain)
        {
            // Hard cap at 60% market share for marketing/R&D actions
            if (company.MarketShare >= 60.0)
            {
                return 0.0; // No market share gain from marketing/R&D above 60%
            }
            
            // Diminishing returns formula: gain decreases as market share increases
            double diminishingFactor = 1.0 - (company.MarketShare / 100.0);
            
            // Additional competitive pressure at higher market shares
            double competitivePressure = 1.0;
            if (company.MarketShare >= 30) competitivePressure = 0.8; // 20% harder above 30%
            if (company.MarketShare >= 40) competitivePressure = 0.6; // 40% harder above 40%
            if (company.MarketShare >= 50) competitivePressure = 0.4; // 60% harder above 50%
            if (company.MarketShare >= 55) competitivePressure = 0.2; // 80% harder above 55%
            
            double finalGain = baseGain * diminishingFactor * competitivePressure;
            
            // Ensure we don't exceed 60% cap
            double maxAllowedGain = Math.Max(0, 60.0 - company.MarketShare);
            return Math.Min(finalGain, maxAllowedGain);
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
            var bonusCosts = CalculateBonusCosts();
            double cost = bonusCosts.Small;
            
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
            UpdateDynamicPricing(); // Update pricing after action
        }

        private void BonusLarge_Click(object sender, RoutedEventArgs e)
        {
            var bonusCosts = CalculateBonusCosts();
            double cost = bonusCosts.Large;
            
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
            UpdateDynamicPricing(); // Update pricing after action
        }

        private void SmallLoan_Click(object sender, RoutedEventArgs e)
        {
            double loanAmount = 100000;
            company.Capital += loanAmount;
            company.Risk += 10;
            company.Reputation -= 5;
            
            DecisionMade?.Invoke($"🏦 Small business loan of ${loanAmount:N0} secured! Risk increased by 10, reputation decreased by 5.");
            UpdateCompanyStatus();
        }

        private void MediumLoan_Click(object sender, RoutedEventArgs e)
        {
            double loanAmount = 500000;
            company.Capital += loanAmount;
            company.Risk += 20;
            company.Reputation -= 10;
            
            DecisionMade?.Invoke($"🏦 Medium business loan of ${loanAmount:N0} secured! Risk increased by 20, reputation decreased by 10.");
            UpdateCompanyStatus();
        }

        private void LargeLoan_Click(object sender, RoutedEventArgs e)
        {
            double loanAmount = 1000000;
            company.Capital += loanAmount;
            company.Risk += 35;
            company.Reputation -= 20;
            
            DecisionMade?.Invoke($"🏦 Large business loan of ${loanAmount:N0} secured! Risk increased by 35, reputation decreased by 20.");
            UpdateCompanyStatus();
        }

        // Strategic Decisions
        private void MarketingLocal_Click(object sender, RoutedEventArgs e)
        {
            var marketingCosts = CalculateMarketingCosts();
            double cost = marketingCosts.Local;
            
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
            UpdateDynamicPricing(); // Update pricing after action
        }

        private void MarketingNational_Click(object sender, RoutedEventArgs e)
        {
            var marketingCosts = CalculateMarketingCosts();
            double cost = marketingCosts.National;
            
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
            UpdateDynamicPricing(); // Update pricing after action
        }

        private void RetreatWeekend_Click(object sender, RoutedEventArgs e)
        {
            // Scale cost with employee count: base cost + per-employee cost
            int totalEmployees = departments.Values.Sum(d => d.GetEmployeeCount());
            double baseCost = 15000; // Reduced base cost
            double perEmployeeCost = 800; // Cost per employee
            double cost = baseCost + (totalEmployees * perEmployeeCost);
            
            if (company.Capital < cost)
            {
                MessageBox.Show($"Not enough capital for company retreat! Cost: ${cost:N0} (${baseCost:N0} base + ${perEmployeeCost:N0} × {totalEmployees} employees)", 
                    "Insufficient Funds", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            
            DecisionMade?.Invoke($"🏖️ Weekend company retreat organized! Cost ${cost:N0} ({totalEmployees} employees), morale boosted, risk reduced, team productivity improved!");
            UpdateCompanyStatus();
        }

        private void RetreatWeek_Click(object sender, RoutedEventArgs e)
        {
            // Scale cost with employee count: base cost + per-employee cost
            int totalEmployees = departments.Values.Sum(d => d.GetEmployeeCount());
            double baseCost = 35000; // Reduced base cost
            double perEmployeeCost = 1500; // Cost per employee for week-long retreat
            double cost = baseCost + (totalEmployees * perEmployeeCost);
            
            if (company.Capital < cost)
            {
                MessageBox.Show($"Not enough capital for week-long retreat! Cost: ${cost:N0} (${baseCost:N0} base + ${perEmployeeCost:N0} × {totalEmployees} employees)", 
                    "Insufficient Funds", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            
            DecisionMade?.Invoke($"🏖️ Week-long company retreat organized! Cost ${cost:N0} ({totalEmployees} employees), major morale boost, risk significantly reduced, excellent team building!");
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
            double cost = CalculateConsultantCost();
            
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
            UpdateDynamicPricing(); // Update pricing after action
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