using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    public class DecisionSystem
    {
        private Random _random = new Random();

        // These methods are now simplified since we moved to quarterly system
        // and employee management is handled through departments
        
        public string ProcessQuarterlyDecision(Company company, string decisionType, Dictionary<Department, DepartmentStats> departments)
        {
            double riskMultiplier = company.GetRiskMultiplier();
            double investmentMultiplier = company.GetInvestmentMultiplier();
            
            switch (decisionType.ToLower())
            {
                case "marketing":
                    return ProcessMarketingDecision(company, departments, riskMultiplier, investmentMultiplier);
                case "operations":
                    return ProcessOperationsDecision(company, departments, riskMultiplier, investmentMultiplier);
                case "finance":
                    return ProcessFinanceDecision(company, departments, riskMultiplier, investmentMultiplier);
                default:
                    return "Invalid decision type.";
            }
        }

        private string ProcessMarketingDecision(Company company, Dictionary<Department, DepartmentStats> departments, double riskMultiplier, double investmentMultiplier)
        {
            double baseCost = 50000; // Quarterly cost
            double baseMarketGain = 2;
            int baseRiskIncrease = 3;

            double actualCost = baseCost * investmentMultiplier;
            double marketGain = baseMarketGain * investmentMultiplier * riskMultiplier;
            int riskIncrease = (int)(baseRiskIncrease * riskMultiplier);

            // Marketing department effectiveness
            if (departments.ContainsKey(Department.Marketing))
            {
                double marketingEffectiveness = departments[Department.Marketing].GetTotalProductivity() / 100.0;
                marketGain *= (1 + marketingEffectiveness);
                actualCost *= (0.8 + marketingEffectiveness * 0.2); // Better efficiency reduces costs
            }

            // Market strategy affects outcome
            if (company.MarketStrategy == MarketStrategy.Innovation)
            {
                marketGain *= 1.3;
                riskIncrease += 2;
            }
            else if (company.MarketStrategy == MarketStrategy.Cost)
            {
                marketGain *= 0.8;
                actualCost *= 0.7;
            }

            company.Capital -= actualCost;
            company.MarketShare += marketGain;
            company.Risk += riskIncrease;

            if (_random.NextDouble() < 0.3)
            {
                company.Reputation += _random.Next(1, 4);
            }

            return $"📢 Quarterly marketing campaign executed! Cost: ${actualCost:N0}, Market Share +{marketGain:F1}%, Risk +{riskIncrease}";
        }

        private string ProcessOperationsDecision(Company company, Dictionary<Department, DepartmentStats> departments, double riskMultiplier, double investmentMultiplier)
        {
            double baseCost = 30000;
            int baseMoraleGain = 10;
            int baseRiskReduction = 5;

            double actualCost = baseCost * investmentMultiplier;
            int moraleGain = (int)(baseMoraleGain * investmentMultiplier);
            int riskReduction = (int)(baseRiskReduction * riskMultiplier);

            // Operations department effectiveness
            if (departments.ContainsKey(Department.Operations))
            {
                double operationsEffectiveness = departments[Department.Operations].GetTotalProductivity() / 100.0;
                moraleGain = (int)(moraleGain * (1 + operationsEffectiveness));
                riskReduction = (int)(riskReduction * (1 + operationsEffectiveness));
            }

            company.Capital -= actualCost;
            company.Morale += moraleGain;
            company.Risk -= riskReduction;

            if (company.Risk < 0) company.Risk = 0;

            return $"⚙️ Operations optimized for the quarter! Cost: ${actualCost:N0}, Morale +{moraleGain}, Risk -{riskReduction}";
        }

        private string ProcessFinanceDecision(Company company, Dictionary<Department, DepartmentStats> departments, double riskMultiplier, double investmentMultiplier)
        {
            double baseGain = 25000;
            int baseMoraleLoss = 8;
            int baseReputationLoss = 5;

            double actualGain = baseGain * investmentMultiplier;
            int moraleLoss = (int)(baseMoraleLoss * riskMultiplier);
            int reputationLoss = (int)(baseReputationLoss * riskMultiplier);

            // Finance department effectiveness
            if (departments.ContainsKey(Department.Finance))
            {
                double financeEffectiveness = departments[Department.Finance].GetTotalProductivity() / 100.0;
                actualGain *= (1 + financeEffectiveness);
                moraleLoss = (int)(moraleLoss * (1 - financeEffectiveness * 0.3)); // Better finance reduces negative impact
            }

            company.Capital += actualGain;
            company.Morale -= moraleLoss;
            company.Reputation -= reputationLoss;

            return $"💰 Financial optimization completed! Gained: ${actualGain:N0}, Morale -{moraleLoss}, Reputation -{reputationLoss}";
        }
    }
}
