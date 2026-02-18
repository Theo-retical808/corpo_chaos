using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    public enum CrisisLevel
    {
        None,
        Warning,
        Critical,
        Catastrophic
    }

    public class CrisisEvent
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CrisisLevel Level { get; set; }
        public int QuartersRemaining { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChaosEngine
    {
        private Random _random = new Random();
        private int _quartersSinceLastMajorEvent = 0;
        private List<CrisisEvent> _activeCrises = new List<CrisisEvent>();
        private List<string> _currentEvents = new List<string>();

        public List<CrisisEvent> GetActiveCrises() => _activeCrises;
        public List<string> GetCurrentEvents() => _currentEvents;

        // Calculate progressive difficulty multiplier based on quarter number
        private double GetProgressiveDifficultyMultiplier(int quarterNumber)
        {
            // Progressive difficulty from Q1 to Q40
            // Q1-5: 0.2x multiplier (very easy)
            // Q6-10: 0.4x multiplier (easy)
            // Q11-20: 0.6x multiplier (moderate)
            // Q21-30: 0.8x multiplier (challenging)
            // Q31-40: 1.0x multiplier (full difficulty)
            // Q40+: 1.0x multiplier (maximum balanced difficulty)
            
            if (quarterNumber <= 5) return 0.2; // Very easy early game
            if (quarterNumber <= 10) return 0.4; // Easy
            if (quarterNumber <= 20) return 0.6; // Moderate
            if (quarterNumber <= 30) return 0.8; // Challenging
            return 1.0; // Full difficulty from Q31+
        }

        public List<string> ApplyQuarterlyChaos(Company company, Dictionary<Department, DepartmentStats> departments, bool isStoryModeTutorial = false, int quarterNumber = 1)
        {
            var events = new List<string>();
            _currentEvents.Clear();
            _quartersSinceLastMajorEvent++;

            // Skip chaos events during story mode tutorial (first 10 quarters)
            if (isStoryModeTutorial)
            {
                // Only apply basic employee productivity changes and minimal events
                events.AddRange(GenerateBasicStoryEvents(company, departments));
                _currentEvents.AddRange(events);
                return events;
            }

            // Get progressive difficulty multiplier based on quarter
            double difficultyMultiplier = GetProgressiveDifficultyMultiplier(quarterNumber);

            // Process existing crises
            ProcessActiveCrises(company, departments, events);

            // Generate new chaos events with progressive difficulty
            events.AddRange(GenerateEmployeeEvents(company, departments, difficultyMultiplier));
            events.AddRange(GenerateMarketEvents(company, difficultyMultiplier));
            events.AddRange(GenerateCrisisEvents(company, difficultyMultiplier));
            events.AddRange(GenerateReputationEvents(company, departments, difficultyMultiplier));
            events.AddRange(GenerateMoraleEvents(company, departments, difficultyMultiplier));
            events.AddRange(GenerateRiskEvents(company, departments, difficultyMultiplier));
            events.AddRange(GenerateRandomChaos(company, departments, difficultyMultiplier));

            // Check for catastrophic events based on risk level (with progressive difficulty)
            double catastrophicChance = company.GetCatastrophicEventChance() * difficultyMultiplier;
            if (_random.NextDouble() < catastrophicChance)
            {
                events.AddRange(GenerateCatastrophicEvent(company, departments));
            }

            // Check for employee turnover based on morale (with progressive difficulty)
            events.AddRange(ProcessMoraleBasedTurnover(company, departments, difficultyMultiplier));

            _currentEvents.AddRange(events);
            return events;
        }

        private List<string> GenerateBasicStoryEvents(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();

            // Only generate very mild, educational events during tutorial
            if (_random.NextDouble() < 0.3) // 30% chance of any event
            {
                var eventType = _random.Next(4);
                switch (eventType)
                {
                    case 0:
                        events.Add("📈 Market research shows positive trends for your industry sector.");
                        company.Reputation += _random.Next(1, 4);
                        break;
                    case 1:
                        events.Add("💼 Your team completed a successful project, boosting productivity.");
                        foreach (var dept in departments.Values)
                        {
                            foreach (var employee in dept.Employees)
                            {
                                employee.Productivity = Math.Min(100, employee.Productivity + _random.Next(1, 3));
                            }
                        }
                        break;
                    case 2:
                        events.Add("🤝 Positive client feedback improves company reputation.");
                        company.Reputation += _random.Next(2, 6);
                        break;
                    case 3:
                        events.Add("⚡ Minor operational improvements reduce risk levels.");
                        company.Risk = Math.Max(-100, company.Risk - _random.Next(1, 4));
                        break;
                }
            }

            return events;
        }

        private void ProcessActiveCrises(Company company, Dictionary<Department, DepartmentStats> departments, List<string> events)
        {
            for (int i = _activeCrises.Count - 1; i >= 0; i--)
            {
                var crisis = _activeCrises[i];
                crisis.QuartersRemaining--;

                if (crisis.QuartersRemaining <= 0)
                {
                    // Crisis reaches its climax
                    events.Add(ResolveCrisis(crisis, company, departments));
                    _activeCrises.RemoveAt(i);
                }
                else
                {
                    // Crisis continues to build
                    events.Add($"🚨 ONGOING CRISIS: {crisis.Title} - {crisis.QuartersRemaining} quarters remaining!");
                    ApplyCrisisEffects(crisis, company, departments);
                }
            }
        }

        private List<string> GenerateEmployeeEvents(Company company, Dictionary<Department, DepartmentStats> departments, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();
            
            foreach (var dept in departments.Values)
            {
                if (dept.Employees.Count == 0) continue;

                // Employee scouting events (reduced by difficulty multiplier)
                if (_random.NextDouble() < (0.15 * difficultyMultiplier)) // 15% chance per department
                {
                    var targetEmployee = dept.Employees[_random.Next(dept.Employees.Count)];
                    if (targetEmployee.OverallSkill >= SkillLevel.Senior)
                    {
                        events.Add(HandleEmployeeScouting(targetEmployee, dept, company));
                    }
                }

                // Employee fumble/mistake events (reduced by difficulty multiplier)
                if (_random.NextDouble() < (0.12 * difficultyMultiplier)) // 12% chance per department
                {
                    var fumbleEmployee = dept.Employees[_random.Next(dept.Employees.Count)];
                    events.Add(HandleEmployeeFumble(fumbleEmployee, dept, company));
                }

                // Employee retirement events (reduced by difficulty multiplier)
                if (_random.NextDouble() < (0.08 * difficultyMultiplier)) // 8% chance per department
                {
                    var retiringEmployee = dept.Employees.Where(e => e.Experience > 10).FirstOrDefault();
                    if (retiringEmployee != null)
                    {
                        events.Add(HandleEmployeeRetirement(retiringEmployee, dept, company));
                    }
                }

                // Employee breakthrough/innovation events (positive events not reduced)
                if (_random.NextDouble() < 0.10) // 10% chance per department (unchanged)
                {
                    var innovativeEmployee = dept.Employees.Where(e => e.OverallSkill >= SkillLevel.Mid).FirstOrDefault();
                    if (innovativeEmployee != null)
                    {
                        events.Add(HandleEmployeeBreakthrough(innovativeEmployee, dept, company));
                    }
                }
            }

            return events;
        }

        private string HandleEmployeeScouting(Employee employee, DepartmentStats dept, Company company)
        {
            double scoutingOffer = employee.Salary * (_random.NextDouble() * 1.0 + 1.5); // 1.5 to 2.5 multiplier
            
            if (_random.NextDouble() < 0.6) // 60% chance they leave
            {
                dept.Employees.Remove(employee);
                company.Morale -= 5;
                company.Reputation -= 2;
                return $"💼 TALENT POACHED! {employee.Name} from {dept.Department} was scouted by competitors for ${scoutingOffer:N0}! Morale -{5}, Reputation -{2}";
            }
            else
            {
                // Employee stays but demands raise
                double raise = employee.Salary * 0.3;
                employee.Salary += raise;
                double quarterlyCost = raise * 4; // Quarterly impact
                company.Capital -= quarterlyCost;
                company.NetLoss += quarterlyCost; // Track as net loss
                return $"💰 RETENTION BONUS! {employee.Name} received a ${raise:N0}/month raise to stay. Quarterly cost: ${quarterlyCost:N0}";
            }
        }

        private string HandleEmployeeFumble(Employee employee, DepartmentStats dept, Company company)
        {
            string[] fumbleTypes = {
                "accidentally deleted critical files",
                "sent confidential data to wrong client",
                "made a costly calculation error",
                "missed an important deadline",
                "caused a system outage",
                "leaked sensitive information",
                "made a public relations blunder"
            };

            string fumble = fumbleTypes[_random.Next(fumbleTypes.Length)];
            double financialImpact = _random.Next(5000, 25000);
            int reputationLoss = _random.Next(3, 8);
            int moraleLoss = _random.Next(2, 6);

            // Higher skill employees cause bigger problems when they fumble
            if (employee.OverallSkill >= SkillLevel.Senior)
            {
                financialImpact *= 2;
                reputationLoss += 3;
            }

            company.Capital -= financialImpact;
            company.NetLoss += financialImpact; // Track as net loss
            company.Reputation -= reputationLoss;
            company.Morale -= moraleLoss;
            employee.Morale -= 15; // Personal impact

            return $"🤦 EMPLOYEE FUMBLE! {employee.Name} ({dept.Department}) {fumble}! Cost: ${financialImpact:N0}, Reputation -{reputationLoss}, Morale -{moraleLoss}";
        }

        private string HandleEmployeeRetirement(Employee employee, DepartmentStats dept, Company company)
        {
            dept.Employees.Remove(employee);
            
            // Retirement impact depends on employee's contribution
            double knowledgeLoss = employee.GetEffectiveProductivity() * 0.5;
            company.Morale -= (int)(knowledgeLoss / 10);
            
            // Retirement party costs
            double partyCost = _random.Next(2000, 8000);
            company.Capital -= partyCost;
            company.NetLoss += partyCost; // Track as net loss
            company.Morale += 3; // Party boosts morale slightly

            return $"👴 RETIREMENT! {employee.Name} from {dept.Department} retired after {employee.Experience} years. Knowledge loss: {knowledgeLoss:F1}, Party cost: ${partyCost:N0}";
        }

        private string HandleEmployeeBreakthrough(Employee employee, DepartmentStats dept, Company company)
        {
            string[] breakthroughs = {
                "developed a cost-saving process",
                "discovered a new market opportunity",
                "created an innovative solution",
                "improved operational efficiency",
                "secured a major client",
                "solved a long-standing problem",
                "invented a game-changing feature"
            };

            string breakthrough = breakthroughs[_random.Next(breakthroughs.Length)];
            double benefit = _random.Next(15000, 50000);
            int reputationGain = _random.Next(2, 6);
            int moraleGain = _random.Next(3, 8);

            company.Capital += benefit;
            company.Reputation += reputationGain;
            company.Morale += moraleGain;
            employee.Morale += 20; // Personal boost
            employee.Productivity = Math.Min(100, employee.Productivity + 5);

            return $"💡 BREAKTHROUGH! {employee.Name} ({dept.Department}) {breakthrough}! Benefit: ${benefit:N0}, Reputation +{reputationGain}, Morale +{moraleGain}";
        }

        private List<string> GenerateMarketEvents(Company company, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();
            
            // Aggressive market events based on risk appetite (with progressive difficulty)
            double chaosMultiplier = company.RiskAppetite switch
            {
                RiskAppetite.Conservative => 0.7,
                RiskAppetite.Balanced => 1.0,
                RiskAppetite.Aggressive => 1.8,
                _ => 1.0
            };

            // Apply progressive difficulty to market events
            double finalMultiplier = chaosMultiplier * difficultyMultiplier;

            if (_random.NextDouble() < (0.25 * finalMultiplier))
            {
                events.Add(GenerateMarketDisruption(company));
            }

            if (_random.NextDouble() < (0.20 * finalMultiplier))
            {
                events.Add(GenerateCompetitorAction(company));
            }

            // Add new financial crisis events to threaten capital hoarding (with progressive difficulty)
            if (_random.NextDouble() < (0.15 * finalMultiplier))
            {
                events.Add(GenerateFinancialCrisis(company));
            }

            return events;
        }

        private string GenerateMarketDisruption(Company company)
        {
            string[] disruptions = {
                "🌪️ New technology disrupts your market segment",
                "📱 Consumer preferences shift dramatically",
                "🏛️ Government regulations change overnight",
                "💱 Currency fluctuations affect international business",
                "🛒 E-commerce platform changes algorithms",
                "📺 Viral social media trend impacts brand perception",
                "🔬 Scientific breakthrough makes products obsolete",
                "🌍 Global supply chain disruption",
                "⚡ Energy crisis affects operational costs",
                "🦠 Health crisis changes consumer behavior"
            };

            string disruption = disruptions[_random.Next(disruptions.Length)];
            
            if (_random.NextDouble() < 0.7) // 70% negative, 30% positive (increased negative chance)
            {
                double lossPercentage = _random.NextDouble() * 0.20 + 0.12; // 12-32% of capital (increased from 8-23%)
                double loss = company.Capital * lossPercentage;
                loss = Math.Max(loss, 50000); // Increased minimum impact
                
                int marketShareLoss = _random.Next(4, 12); // Increased from 3-8 to 4-12
                int reputationLoss = _random.Next(8, 18); // Increased from 5-12 to 8-18
                int riskIncrease = 15; // Increased from 10 to 15
                
                company.Capital -= loss;
                company.NetLoss += loss; // Track this as net loss
                company.MarketShare -= marketShareLoss;
                company.Reputation -= reputationLoss;
                company.Risk = Math.Min(100, company.Risk + riskIncrease); // Ensure risk doesn't exceed 100
                
                return $"📉 MARKET DISRUPTION: {disruption} Capital -${loss:N0}, Market Share -{marketShareLoss}%, Reputation -{reputationLoss}, Risk +{riskIncrease}";
            }
            else
            {
                double gain = _random.Next(25000, 80000); // Keep positive impact same
                double marketShareGain = _random.NextDouble() * 1.2 + 0.8; // 0.8-2.0%
                int reputationGain = _random.Next(8, 15);
                
                company.Capital += gain;
                company.MarketShare += marketShareGain;
                company.Reputation += reputationGain;
                
                return $"📈 MARKET OPPORTUNITY: {disruption} Capital +${gain:N0}, Market Share +{marketShareGain:F1}%, Reputation +{reputationGain}";
            }
        }

        private string GenerateCompetitorAction(Company company)
        {
            string[] actions = {
                "launches aggressive price war",
                "poaches your key clients",
                "copies your business model",
                "spreads negative publicity",
                "files patent lawsuit",
                "undercuts your pricing by 30%",
                "releases competing product early"
            };

            string action = actions[_random.Next(actions.Length)];
            double impact = _random.Next(10000, 40000);
            int reputationImpact = _random.Next(2, 7);
            int riskIncrease = 5;
            
            // Competitors target market leaders more aggressively
            double marketShareLoss = 0.5 + (_random.NextDouble() * 1.0); // Base 0.5-1.5%
            if (company.MarketShare >= 30) marketShareLoss *= 1.5; // 50% more aggressive
            if (company.MarketShare >= 50) marketShareLoss *= 2.0; // 100% more aggressive
            if (company.MarketShare >= 60) marketShareLoss *= 2.5; // 150% more aggressive
            
            company.Capital -= impact;
            company.NetLoss += impact; // Track as net loss
            company.Reputation -= reputationImpact;
            company.MarketShare -= marketShareLoss;
            company.Risk = Math.Min(100, company.Risk + riskIncrease); // Ensure risk doesn't exceed 100

            return $"🏢 COMPETITOR ATTACK: Major competitor {action}! Capital -${impact:N0}, Reputation -{reputationImpact}, Market Share -{marketShareLoss:F1}%, Risk +{riskIncrease}";
        }

        private string GenerateFinancialCrisis(Company company)
        {
            string[] crises = {
                "💸 Major client defaults on payment",
                "🏦 Bank increases interest rates on business loans",
                "📉 Stock market crash affects investment portfolio",
                "💰 Tax audit results in additional penalties",
                "🔧 Critical equipment failure requires emergency replacement",
                "⚖️ Lawsuit settlement requires immediate payment",
                "🌊 Insurance premium spike due to industry claims",
                "💳 Credit line reduced by financial institution",
                "🏭 Factory lease renewal at 40% higher rate",
                "📊 Accounting error reveals hidden liabilities",
                "🚛 Supply chain disruption requires expensive alternatives",
                "💻 IT system failure requires costly emergency repairs"
            };

            string crisis = crises[_random.Next(crises.Length)];
            
            // Scale financial impact based on company size (capital) - INCREASED IMPACT
            double impactPercentage = _random.NextDouble() * 0.18 + 0.12; // 12-30% of capital (increased from 8-20%)
            double financialImpact = company.Capital * impactPercentage;
            
            // Minimum impact to prevent trivial amounts - INCREASED
            financialImpact = Math.Max(financialImpact, 40000); // Increased from 25000
            
            // Additional effects - INCREASED
            int riskIncrease = _random.Next(12, 20); // Increased from 8-15
            int reputationImpact = _random.Next(5, 12); // Increased from 3-8
            
            company.Capital -= financialImpact;
            company.NetLoss += financialImpact; // Track as net loss
            company.Risk = Math.Min(100, company.Risk + riskIncrease); // Ensure risk doesn't exceed 100
            company.Reputation -= reputationImpact;
            
            return $"💥 FINANCIAL CRISIS: {crisis}! Cost: ${financialImpact:N0}, Risk +{riskIncrease}, Reputation -{reputationImpact}";
        }

        private List<string> GenerateCrisisEvents(Company company, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();
            
            // Generate new crisis warnings (with progressive difficulty)
            if (_random.NextDouble() < (0.15 * difficultyMultiplier) && _activeCrises.Count < 2) // Max 2 active crises
            {
                var newCrisis = GenerateNewCrisis(company);
                _activeCrises.Add(newCrisis);
                events.Add($"⚠️ CRISIS WARNING: {newCrisis.Title} - {newCrisis.Description} ({newCrisis.QuartersRemaining} quarters to prepare!)");
            }

            return events;
        }

        private CrisisEvent GenerateNewCrisis(Company company)
        {
            var crisisTypes = new[]
            {
                new { Title = "Economic Recession Looming", Desc = "Market indicators suggest major downturn approaching", Quarters = 3, Level = CrisisLevel.Critical },
                new { Title = "Industry Regulation Changes", Desc = "New compliance requirements will be mandatory", Quarters = 2, Level = CrisisLevel.Warning },
                new { Title = "Technology Obsolescence", Desc = "Your core technology may become outdated", Quarters = 4, Level = CrisisLevel.Critical },
                new { Title = "Major Competitor Merger", Desc = "Two rivals are planning to merge and dominate market", Quarters = 2, Level = CrisisLevel.Warning },
                new { Title = "Supply Chain Crisis", Desc = "Critical suppliers facing major disruptions", Quarters = 3, Level = CrisisLevel.Critical },
                new { Title = "Cybersecurity Threat", Desc = "Industry-wide security vulnerabilities discovered", Quarters = 1, Level = CrisisLevel.Catastrophic },
                new { Title = "Environmental Regulations", Desc = "New green policies will affect operations", Quarters = 4, Level = CrisisLevel.Warning }
            };

            var crisis = crisisTypes[_random.Next(crisisTypes.Length)];
            return new CrisisEvent
            {
                Title = crisis.Title,
                Description = crisis.Desc,
                Level = crisis.Level,
                QuartersRemaining = crisis.Quarters,
                IsActive = true
            };
        }

        private string ResolveCrisis(CrisisEvent crisis, Company company, Dictionary<Department, DepartmentStats> departments)
        {
            double impact = crisis.Level switch
            {
                CrisisLevel.Warning => 0.08,      // Increased from 0.05
                CrisisLevel.Critical => 0.22,     // Increased from 0.15
                CrisisLevel.Catastrophic => 0.40, // Increased from 0.30
                _ => 0.08
            };

            // Crisis response affects outcome
            double responseMultiplier = company.CrisisResponse switch
            {
                CrisisResponse.Immediate => 0.6, // Best response
                CrisisResponse.Control => 0.8,   // Moderate response
                CrisisResponse.Absorb => 1.4,    // Increased from 1.2 - worse response
                _ => 1.0
            };

            double capitalLoss = company.Capital * impact * responseMultiplier;
            int moraleLoss = (int)(25 * impact * responseMultiplier); // Increased from 20
            int reputationLoss = (int)(20 * impact * responseMultiplier); // Increased from 15

            company.Capital -= capitalLoss;
            company.Morale -= moraleLoss;
            company.Reputation -= reputationLoss;

            return $"💥 CRISIS RESOLVED: {crisis.Title}! Impact: Capital -${capitalLoss:N0}, Morale -{moraleLoss}, Reputation -{reputationLoss}";
        }

        private void ApplyCrisisEffects(CrisisEvent crisis, Company company, Dictionary<Department, DepartmentStats> departments)
        {
            // Ongoing crisis effects
            double stress = crisis.Level switch
            {
                CrisisLevel.Warning => 1,
                CrisisLevel.Critical => 3,
                CrisisLevel.Catastrophic => 5,
                _ => 1
            };

            company.Morale -= (int)stress;
            company.Risk += (int)stress;

            // Affect employee morale in all departments
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.Morale -= (int)stress;
                    if (employee.Morale < 0) employee.Morale = 0;
                }
            }
        }

        private List<string> GenerateRandomChaos(Company company, Dictionary<Department, DepartmentStats> departments, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();
            
            // Pure chaos - completely unpredictable events (with progressive difficulty)
            if (_random.NextDouble() < (0.20 * difficultyMultiplier)) // 20% chance of pure chaos
            {
                string[] chaosEvents = {
                    "🎲 A viral TikTok about your company goes viral (randomly positive or negative)",
                    "🎪 Your office building gets featured in a reality TV show",
                    "🦄 A unicorn startup tries to acquire you with cryptocurrency",
                    "🎮 Your employees start a company-wide gaming tournament during work hours",
                    "🍕 Free pizza delivery mix-up leads to unexpected client meeting",
                    "🚁 CEO gets stuck in elevator with major investor",
                    "📱 Company phone system gets hacked to only play elevator music",
                    "🎨 Intern accidentally redesigns company logo, everyone loves it",
                    "🐕 Office therapy dog becomes internet famous",
                    "☕ Coffee machine breaks, productivity drops 50%"
                };

                string chaosEvent = chaosEvents[_random.Next(chaosEvents.Length)];
                
                // Random positive or negative impact
                if (_random.NextDouble() < 0.5)
                {
                    double gain = _random.Next(5000, 25000);
                    int moraleGain = _random.Next(5, 15);
                    company.Capital += gain;
                    company.Morale += moraleGain;
                    events.Add($"🎉 RANDOM CHAOS: {chaosEvent} Unexpected benefit: +${gain:N0}, Morale +{moraleGain}");
                }
                else
                {
                    double loss = _random.Next(3000, 15000);
                    int moraleLoss = _random.Next(2, 8);
                    company.Capital -= loss;
                    company.NetLoss += loss;
                    company.Morale -= moraleLoss;
                    events.Add($"🤪 RANDOM CHAOS: {chaosEvent} Unexpected cost: -${loss:N0}, Morale -{moraleLoss}");
                }
            }

            return events;
        }

        public string GetCrisisStatusSummary()
        {
            if (_activeCrises.Count == 0)
                return "🟢 No active crises";

            var summary = new List<string>();
            foreach (var crisis in _activeCrises)
            {
                string icon = crisis.Level switch
                {
                    CrisisLevel.Warning => "🟡",
                    CrisisLevel.Critical => "🟠",
                    CrisisLevel.Catastrophic => "🔴",
                    _ => "⚪"
                };
                summary.Add($"{icon} {crisis.Title} ({crisis.QuartersRemaining}Q)");
            }

            return string.Join("\n", summary);
        }

        // New method: Generate reputation-based events (scandals, PR issues)
        private List<string> GenerateReputationEvents(Company company, Dictionary<Department, DepartmentStats> departments, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();

            // Scandal events (more likely with low reputation) - with progressive difficulty
            if (_random.NextDouble() < (0.15 * difficultyMultiplier))
            {
                events.Add(GenerateScandal(company));
            }

            // Mismanagement events - with progressive difficulty
            if (_random.NextDouble() < (0.12 * difficultyMultiplier))
            {
                events.Add(GenerateMismanagement(company, departments));
            }

            // Positive PR events (more likely with high reputation) - not reduced by difficulty
            if (company.Reputation > 20 && _random.NextDouble() < 0.08)
            {
                events.Add(GeneratePositivePR(company));
            }

            return events;
        }

        private string GenerateScandal(Company company)
        {
            string[] scandals = {
                "leaked internal emails reveal questionable practices",
                "former employee whistleblower goes public",
                "social media backlash over company policies",
                "executive caught in personal scandal",
                "data privacy violation discovered",
                "discriminatory hiring practices exposed",
                "environmental damage cover-up revealed",
                "tax avoidance scheme becomes public",
                "insider trading allegations surface"
            };

            string scandal = scandals[_random.Next(scandals.Length)];
            int reputationLoss = _random.Next(15, 35);
            int moraleLoss = _random.Next(10, 20);
            double financialImpact = _random.Next(50000, 150000);
            int riskIncrease = 10;

            company.Reputation -= reputationLoss;
            company.Morale -= moraleLoss;
            company.Capital -= financialImpact;
            company.NetLoss += financialImpact; // Track as net loss
            company.Risk = Math.Min(100, company.Risk + riskIncrease); // Ensure risk doesn't exceed 100

            return $"📰 SCANDAL! Company {scandal}! Reputation -{reputationLoss}, Morale -{moraleLoss}, Cost: ${financialImpact:N0}, Risk +{riskIncrease}";
        }

        private string GenerateMismanagement(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            string[] mismanagements = {
                "budget allocated to wrong department",
                "critical project deadline missed due to poor planning",
                "resources wasted on failed initiative",
                "communication breakdown between departments",
                "strategic decision backfires spectacularly",
                "vendor contract negotiated poorly",
                "talent acquisition strategy fails",
                "operational efficiency drops due to poor processes"
            };

            string mismanagement = mismanagements[_random.Next(mismanagements.Length)];
            int moraleLoss = _random.Next(8, 18);
            int reputationLoss = _random.Next(5, 12);
            double financialImpact = _random.Next(25000, 75000);
            int riskIncrease = 5;

            company.Morale -= moraleLoss;
            company.Reputation -= reputationLoss;
            company.Capital -= financialImpact;
            company.NetLoss += financialImpact; // Track as net loss
            company.Risk = Math.Min(100, company.Risk + riskIncrease); // Ensure risk doesn't exceed 100

            return $"🤦 MISMANAGEMENT! {mismanagement}! Morale -{moraleLoss}, Reputation -{reputationLoss}, Cost: ${financialImpact:N0}, Risk +{riskIncrease}";
        }

        private string GeneratePositivePR(Company company)
        {
            string[] positiveEvents = {
                "wins industry excellence award",
                "featured in major business magazine",
                "CEO gives inspiring keynote speech",
                "company's charity work gets recognition",
                "innovative product receives media praise",
                "workplace culture highlighted as exemplary",
                "sustainability efforts gain public attention",
                "employee volunteer program makes headlines"
            };

            string positiveEvent = positiveEvents[_random.Next(positiveEvents.Length)];
            int reputationGain = _random.Next(8, 20);
            int moraleGain = _random.Next(5, 15);
            double financialBenefit = _random.Next(15000, 45000);

            company.Reputation += reputationGain;
            company.Morale += moraleGain;
            company.Capital += financialBenefit;

            return $"🌟 POSITIVE PR! Company {positiveEvent}! Reputation +{reputationGain}, Morale +{moraleGain}, Benefit: ${financialBenefit:N0}";
        }

        // New method: Generate morale-based events
        private List<string> GenerateMoraleEvents(Company company, Dictionary<Department, DepartmentStats> departments, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();

            // Miscommunication events (more likely with low morale) - with progressive difficulty
            if (company.Morale < -20 && _random.NextDouble() < (0.18 * difficultyMultiplier))
            {
                events.Add(GenerateMiscommunication(company, departments));
            }

            // Team building success (more likely with decent morale) - not reduced by difficulty
            if (company.Morale > 10 && _random.NextDouble() < 0.10)
            {
                events.Add(GenerateTeamBuildingSuccess(company));
            }

            return events;
        }

        private string GenerateMiscommunication(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            string[] miscommunications = {
                "critical information not shared between teams",
                "project requirements misunderstood",
                "client expectations not properly communicated",
                "deadline changes not relayed to all stakeholders",
                "budget constraints not communicated clearly",
                "policy changes cause confusion across departments",
                "meeting outcomes not documented or shared",
                "technical specifications lost in translation"
            };

            string miscommunication = miscommunications[_random.Next(miscommunications.Length)];
            int moraleLoss = _random.Next(10, 20);
            double financialImpact = _random.Next(15000, 40000);
            int reputationLoss = _random.Next(3, 8);

            company.Morale -= moraleLoss;
            company.Capital -= financialImpact;
            company.Reputation -= reputationLoss;
            company.Risk += 3;

            return $"📞 MISCOMMUNICATION! {miscommunication}! Morale -{moraleLoss}, Cost: ${financialImpact:N0}, Reputation -{reputationLoss}, Risk +3";
        }

        private string GenerateTeamBuildingSuccess(Company company)
        {
            string[] teamEvents = {
                "successful company retreat boosts collaboration",
                "cross-department project exceeds expectations",
                "employee recognition program shows results",
                "mentorship program creates strong bonds",
                "innovation workshop generates breakthrough ideas",
                "company culture initiative improves satisfaction",
                "team lunch tradition strengthens relationships"
            };

            string teamEvent = teamEvents[_random.Next(teamEvents.Length)];
            int moraleGain = _random.Next(8, 18);
            int reputationGain = _random.Next(2, 6);

            company.Morale += moraleGain;
            company.Reputation += reputationGain;

            return $"🤝 TEAM SUCCESS! {teamEvent}! Morale +{moraleGain}, Reputation +{reputationGain}";
        }

        // New method: Generate risk-based events (product defects, quality issues)
        private List<string> GenerateRiskEvents(Company company, Dictionary<Department, DepartmentStats> departments, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();

            // Product defect events (more likely with high risk) - with progressive difficulty
            if (_random.NextDouble() < (0.12 * difficultyMultiplier))
            {
                events.Add(GenerateProductDefect(company));
            }

            // Quality control success (more likely with low risk) - not reduced by difficulty
            if (company.Risk < 10 && _random.NextDouble() < 0.08)
            {
                events.Add(GenerateQualitySuccess(company));
            }

            return events;
        }

        private string GenerateProductDefect(Company company)
        {
            string[] defects = {
                "manufacturing defect discovered in latest batch",
                "software bug causes customer data loss",
                "safety issue identified in product design",
                "quality control failure leads to recalls",
                "supplier provides substandard materials",
                "packaging defect damages product reputation",
                "performance issues reported by multiple customers",
                "compatibility problems with existing systems"
            };

            string defect = defects[_random.Next(defects.Length)];
            bool massRecall = _random.NextDouble() < 0.3; // 30% chance of mass recall

            if (massRecall)
            {
                int reputationLoss = _random.Next(25, 45);
                double recallCost = _random.Next(100000, 300000);
                int moraleLoss = _random.Next(15, 25);

                company.Reputation -= reputationLoss;
                company.Capital -= recallCost;
                company.Morale -= moraleLoss;
                company.Risk += 15;

                return $"🚨 MASS RECALL! {defect} triggers massive product recall! Reputation -{reputationLoss}, Cost: ${recallCost:N0}, Morale -{moraleLoss}, Risk +15";
            }
            else
            {
                int reputationLoss = _random.Next(8, 18);
                double fixCost = _random.Next(20000, 60000);
                int moraleLoss = _random.Next(5, 12);

                company.Reputation -= reputationLoss;
                company.Capital -= fixCost;
                company.Morale -= moraleLoss;
                company.Risk += 5;

                return $"🔧 PRODUCT DEFECT! {defect}! Reputation -{reputationLoss}, Fix cost: ${fixCost:N0}, Morale -{moraleLoss}, Risk +5";
            }
        }

        private string GenerateQualitySuccess(Company company)
        {
            string[] qualityWins = {
                "receives industry quality certification",
                "zero-defect milestone achieved",
                "customer satisfaction scores reach new high",
                "quality improvement process shows results",
                "supplier partnership enhances product quality",
                "rigorous testing prevents potential issues",
                "quality assurance team prevents major defect"
            };

            string qualityWin = qualityWins[_random.Next(qualityWins.Length)];
            int reputationGain = _random.Next(10, 20);
            int moraleGain = _random.Next(8, 15);
            double benefit = _random.Next(25000, 60000);

            company.Reputation += reputationGain;
            company.Morale += moraleGain;
            company.Capital += benefit;
            company.Risk -= 8;

            return $"🏆 QUALITY SUCCESS! Company {qualityWin}! Reputation +{reputationGain}, Morale +{moraleGain}, Benefit: ${benefit:N0}, Risk -8";
        }

        // New method: Generate catastrophic events based on risk level
        private List<string> GenerateCatastrophicEvent(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();

            string[] catastrophicEvents = {
                "major data breach exposes customer information",
                "factory fire destroys primary production facility",
                "class-action lawsuit filed against company",
                "regulatory investigation launched",
                "key patent invalidated by court ruling",
                "major client cancels all contracts",
                "cyber attack cripples company operations",
                "environmental disaster linked to company operations"
            };

            string catastrophe = catastrophicEvents[_random.Next(catastrophicEvents.Length)];
            int reputationLoss = _random.Next(30, 60);
            double financialImpact = company.Capital * (_random.NextDouble() * 0.15 + 0.10); // 10-25% of capital
            int moraleLoss = _random.Next(20, 40);

            company.Reputation -= reputationLoss;
            company.Capital -= financialImpact;
            company.Morale -= moraleLoss;
            company.Risk += 20;

            events.Add($"💥 CATASTROPHIC EVENT! {catastrophe}! Reputation -{reputationLoss}, Cost: ${financialImpact:N0}, Morale -{moraleLoss}, Risk +20");

            return events;
        }

        // New method: Process morale-based employee turnover
        private List<string> ProcessMoraleBasedTurnover(Company company, Dictionary<Department, DepartmentStats> departments, double difficultyMultiplier = 1.0)
        {
            var events = new List<string>();
            double turnoverChance = company.GetEmployeeTurnoverChance() * difficultyMultiplier;

            foreach (var dept in departments.Values)
            {
                var employeesToRemove = new List<Employee>();
                
                foreach (var employee in dept.Employees)
                {
                    // Individual employee morale also affects turnover
                    double individualTurnoverChance = turnoverChance;
                    
                    // High individual morale prevents quitting
                    if (employee.Morale > 80)
                    {
                        individualTurnoverChance = 0.0; // No quitting when individual morale is high
                    }
                    else
                    {
                        if (employee.Morale < 30) individualTurnoverChance *= 1.5;
                        if (employee.Morale < 10) individualTurnoverChance *= 2.0;
                    }

                    if (_random.NextDouble() < individualTurnoverChance)
                    {
                        employeesToRemove.Add(employee);
                    }
                }

                foreach (var employee in employeesToRemove)
                {
                    dept.Employees.Remove(employee);
                    company.Morale -= 2; // Each departure hurts overall morale
                    events.Add($"👋 EMPLOYEE QUIT! {employee.Name} from {dept.Department} left due to low morale. Company morale -2");
                }
            }

            return events;
        }
    }
}
