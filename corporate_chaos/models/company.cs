using System.Text.Json.Serialization;

namespace CorporateChaos.Models
{
    public enum RiskAppetite
    {
        Conservative,
        Balanced,
        Aggressive
    }

    public enum InvestmentLevel
    {
        Low,
        Medium,
        High
    }

    public enum WorkforceFocus
    {
        Wellbeing,
        Efficiency
    }

    public enum MarketStrategy
    {
        Cost,
        Quality,
        Innovation
    }

    public enum CrisisResponse
    {
        Immediate,
        Control,
        Absorb
    }

    public enum EmployeeManagement
    {
        Low,
        Standard,
        High
    }

    public class Company
    {
        [JsonPropertyName("capital")]
        public double Capital { get; set; }

        [JsonPropertyName("reputation")]
        public int Reputation { get; set; }

        [JsonPropertyName("morale")]
        public int Morale { get; set; }

        [JsonPropertyName("marketShare")]
        public double MarketShare { get; set; }

        [JsonPropertyName("risk")]
        public int Risk { get; set; }

        [JsonPropertyName("employeeCount")]
        public int EmployeeCount { get; set; }

        [JsonPropertyName("quarterlyRevenue")]
        public double QuarterlyRevenue { get; set; }

        [JsonPropertyName("quarterlyExpenses")]
        public double QuarterlyExpenses { get; set; }

        // Control Knobs
        [JsonPropertyName("riskAppetite")]
        public RiskAppetite RiskAppetite { get; set; }

        [JsonPropertyName("budgetAllocation")]
        public InvestmentLevel BudgetAllocation { get; set; }

        [JsonPropertyName("workforceFocus")]
        public WorkforceFocus WorkforceFocus { get; set; }

        [JsonPropertyName("marketStrategy")]
        public MarketStrategy MarketStrategy { get; set; }

        [JsonPropertyName("crisisResponse")]
        public CrisisResponse CrisisResponse { get; set; }

        [JsonPropertyName("employeeManagement")]
        public EmployeeManagement EmployeeManagement { get; set; }

        // Department Budget Allocations (percentages)
        [JsonPropertyName("marketingBudget")]
        public double MarketingBudget { get; set; } = 15.0;

        [JsonPropertyName("operationsBudget")]
        public double OperationsBudget { get; set; } = 20.0;

        [JsonPropertyName("financeBudget")]
        public double FinanceBudget { get; set; } = 15.0;

        [JsonPropertyName("hrBudget")]
        public double HRBudget { get; set; } = 10.0;

        [JsonPropertyName("itBudget")]
        public double ITBudget { get; set; } = 20.0;

        [JsonPropertyName("researchBudget")]
        public double ResearchBudget { get; set; } = 20.0;

        // Hiring system tracking
        [JsonPropertyName("currentQuarterRefreshes")]
        public int CurrentQuarterRefreshes { get; set; } = 0;

        [JsonPropertyName("lastRefreshQuarter")]
        public int LastRefreshQuarter { get; set; } = 0;

        public Company()
        {
            Capital = 500000; // Increased for quarterly system
            Reputation = 0; // Changed to start at 0 (-100 to 100 range)
            Morale = 0; // Changed to start at 0 (-100 to 100 range)
            MarketShare = 5;
            Risk = 0; // Changed to start at 0 (-100 to 100 range)
            EmployeeCount = 0; // Will be managed through departments
            QuarterlyRevenue = 0;
            QuarterlyExpenses = 0;

            // Default settings
            RiskAppetite = RiskAppetite.Balanced;
            BudgetAllocation = InvestmentLevel.Medium;
            WorkforceFocus = WorkforceFocus.Efficiency;
            MarketStrategy = MarketStrategy.Quality;
            CrisisResponse = CrisisResponse.Control;
            EmployeeManagement = EmployeeManagement.Standard;
        }

        public double GetRiskMultiplier()
        {
            return RiskAppetite switch
            {
                RiskAppetite.Conservative => 0.7,
                RiskAppetite.Balanced => 1.0,
                RiskAppetite.Aggressive => 1.5,
                _ => 1.0
            };
        }

        public double GetInvestmentMultiplier()
        {
            return BudgetAllocation switch
            {
                InvestmentLevel.Low => 0.5,
                InvestmentLevel.Medium => 1.0,
                InvestmentLevel.High => 2.0,
                _ => 1.0
            };
        }

        public double GetEmployeeMultiplier()
        {
            return EmployeeManagement switch
            {
                EmployeeManagement.Low => 0.8,
                EmployeeManagement.Standard => 1.0,
                EmployeeManagement.High => 1.3,
                _ => 1.0
            };
        }

        // New method: Calculate reputation-based revenue modifier
        public double GetReputationRevenueModifier()
        {
            // Reputation ranges from -100 to 100
            // At 0: no bonus/penalty (1.0 multiplier)
            // At 100: +50% revenue bonus (1.5 multiplier)
            // At -100: -50% revenue penalty (0.5 multiplier)
            return 1.0 + (Reputation / 200.0);
        }

        // New method: Calculate risk-based catastrophic event chance
        public double GetCatastrophicEventChance()
        {
            // Risk ranges from -100 to 100
            // At 0: 5% base chance
            // At 100: 25% chance
            // At -100: 0% chance (very safe)
            double baseChance = 0.05; // 5% base chance
            double riskModifier = Math.Max(0, Risk) / 100.0 * 0.20; // Up to 20% additional risk
            return Math.Min(0.25, baseChance + riskModifier); // Cap at 25%
        }

        // New method: Calculate morale-based employee turnover chance
        public double GetEmployeeTurnoverChance()
        {
            // Morale ranges from -100 to 100
            // At 0: 10% base turnover chance
            // At -100: 30% turnover chance
            // At 100: 2% turnover chance
            double baseTurnover = 0.10; // 10% base turnover
            double moraleModifier = (-Morale / 100.0) * 0.20; // -100 morale adds 20% turnover
            return Math.Max(0.02, Math.Min(0.30, baseTurnover + moraleModifier));
        }

        // Helper method to clamp values to -100 to 100 range
        public void ClampValues()
        {
            Reputation = Math.Max(-100, Math.Min(100, Reputation));
            Morale = Math.Max(-100, Math.Min(100, Morale));
            Risk = Math.Max(-100, Math.Min(100, Risk));
            
            // Clamp market share to realistic bounds
            MarketShare = Math.Max(0, Math.Min(100, MarketShare));
        }

        // Calculate market share gain with diminishing returns
        private double GetMarketShareGain(double baseGain)
        {
            // Diminishing returns formula: gain decreases as market share increases
            double diminishingFactor = 1.0 - (MarketShare / 100.0);
            
            // Additional competitive pressure at higher market shares
            double competitivePressure = 1.0;
            if (MarketShare >= 50) competitivePressure = 0.5; // 50% harder above 50%
            if (MarketShare >= 60) competitivePressure = 0.3; // 70% harder above 60%
            if (MarketShare >= 65) competitivePressure = 0.2; // 80% harder above 65%
            
            return baseGain * diminishingFactor * competitivePressure;
        }

        // Calculate market share loss (less severe than gains)
        private double GetMarketShareLoss(double baseLoss)
        {
            // Market share loss is less affected by current position
            double competitorGain = 1.0 + (MarketShare / 200.0); // Competitors work harder against leaders
            return baseLoss * competitorGain;
        }

        // Apply natural market dynamics each quarter
        private void ApplyMarketDynamics()
        {
            // Natural market share decay - market leaders face more pressure
            double naturalDecay = 0.05; // Base 0.05% decay per quarter
            
            // Increased decay for market leaders
            if (MarketShare >= 30) naturalDecay = 0.08;
            if (MarketShare >= 50) naturalDecay = 0.12;
            if (MarketShare >= 60) naturalDecay = 0.15;
            
            // Apply decay with some randomness
            Random random = new Random();
            double actualDecay = naturalDecay * (0.8 + random.NextDouble() * 0.4); // 80-120% of base decay
            
            MarketShare -= actualDecay;
            
            // Reputation affects market share retention
            if (Reputation < -20)
            {
                MarketShare -= 0.1; // Additional loss for poor reputation
            }
            else if (Reputation > 50)
            {
                MarketShare += 0.05; // Small bonus for excellent reputation
            }
            
            // Morale affects competitive performance
            if (Morale < -20)
            {
                MarketShare -= 0.08; // Poor morale hurts competitiveness
            }
            
            // Risk affects market stability
            if (Risk > 50)
            {
                MarketShare -= 0.06; // High risk leads to market share loss
            }
        }

        // Apply budget allocations to department performance
        public void ApplyBudgetAllocations(Dictionary<Department, DepartmentStats> departments)
        {
            // Marketing budget affects reputation and market growth (with diminishing returns)
            if (MarketingBudget >= 25)
            {
                Reputation += 2;
                // Diminishing returns for market share - harder as you get bigger
                double marketShareGain = GetMarketShareGain(0.15); // Reduced from 0.3
                MarketShare += marketShareGain;
            }
            else if (MarketingBudget <= 5)
            {
                Reputation -= 1;
                // Market share naturally declines without marketing investment
                double marketShareLoss = GetMarketShareLoss(0.1);
                MarketShare -= marketShareLoss;
            }

            // Operations budget affects efficiency and risk management
            if (OperationsBudget >= 25)
            {
                Risk -= 2;
                if (departments.ContainsKey(Department.Operations))
                {
                    departments[Department.Operations].Efficiency = Math.Min(100, departments[Department.Operations].Efficiency + 3);
                }
            }
            else if (OperationsBudget <= 10)
            {
                Risk += 3;
            }

            // Finance budget affects cost management
            if (FinanceBudget >= 20)
            {
                // Better financial management
                QuarterlyExpenses *= 0.98; // 2% reduction in expenses
            }

            // HR budget affects morale and hiring
            if (HRBudget >= 20)
            {
                Morale += 3;
                if (departments.ContainsKey(Department.HR))
                {
                    departments[Department.HR].Efficiency = Math.Min(100, departments[Department.HR].Efficiency + 5);
                }
            }
            else if (HRBudget <= 5)
            {
                Morale -= 2;
            }

            // IT budget affects productivity and risk
            if (ITBudget >= 25)
            {
                Risk -= 3;
                // Boost productivity across all departments
                foreach (var dept in departments.Values)
                {
                    dept.Efficiency = Math.Min(100, dept.Efficiency + 2);
                }
            }
            else if (ITBudget <= 10)
            {
                Risk += 5;
            }

            // Research budget affects innovation and market position (with diminishing returns)
            if (ResearchBudget >= 25)
            {
                double marketShareGain = GetMarketShareGain(0.25); // Reduced from 0.5
                MarketShare += marketShareGain;
                Reputation += 1;
                if (departments.ContainsKey(Department.Research))
                {
                    departments[Department.Research].Efficiency = Math.Min(100, departments[Department.Research].Efficiency + 4);
                }
            }
        }

        public void ProcessQuarterlyFinancials(Dictionary<Department, DepartmentStats> departments)
        {
            // Apply budget allocations first
            ApplyBudgetAllocations(departments);
            
            // Apply natural market dynamics
            ApplyMarketDynamics();
            
            // Calculate quarterly expenses (salaries + operations)
            QuarterlyExpenses = departments.Values.Sum(d => d.GetQuarterlyCost());
            QuarterlyExpenses += 50000; // Base operational costs

            // Calculate quarterly revenue based on market share and department performance
            double baseRevenue = MarketShare * 10000; // Base revenue per market share point
            double departmentBonus = departments.Values.Sum(d => d.GetTotalProductivity()) / 100.0;
            
            // Apply reputation modifier to revenue
            double reputationModifier = GetReputationRevenueModifier();
            QuarterlyRevenue = baseRevenue * (1 + departmentBonus) * reputationModifier;

            // Apply to capital
            Capital += QuarterlyRevenue - QuarterlyExpenses;

            // Ensure values stay within bounds
            ClampValues();
        }
    }
}
