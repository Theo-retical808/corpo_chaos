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

        public List<string> ApplyQuarterlyChaos(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();
            _currentEvents.Clear();
            _quartersSinceLastMajorEvent++;

            // Process existing crises
            ProcessActiveCrises(company, departments, events);

            // Generate new chaos events
            events.AddRange(GenerateEmployeeEvents(company, departments));
            events.AddRange(GenerateMarketEvents(company));
            events.AddRange(GenerateCrisisEvents(company));
            events.AddRange(GenerateReputationEvents(company, departments)); // New reputation-based events
            events.AddRange(GenerateMoraleEvents(company, departments)); // New morale-based events
            events.AddRange(GenerateRiskEvents(company, departments)); // New risk-based events
            events.AddRange(GenerateRandomChaos(company, departments));

            // Check for catastrophic events based on risk level
            if (_random.NextDouble() < company.GetCatastrophicEventChance())
            {
                events.AddRange(GenerateCatastrophicEvent(company, departments));
            }

            // Check for employee turnover based on morale
            events.AddRange(ProcessMoraleBasedTurnover(company, departments));

            _currentEvents.AddRange(events);
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

        private List<string> GenerateEmployeeEvents(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();
            
            foreach (var dept in departments.Values)
            {
                if (dept.Employees.Count == 0) continue;

                // Employee scouting events
                if (_random.NextDouble() < 0.15) // 15% chance per department
                {
                    var targetEmployee = dept.Employees[_random.Next(dept.Employees.Count)];
                    if (targetEmployee.OverallSkill >= SkillLevel.Senior)
                    {
                        events.Add(HandleEmployeeScouting(targetEmployee, dept, company));
                    }
                }

                // Employee fumble/mistake events
                if (_random.NextDouble() < 0.12) // 12% chance per department
                {
                    var fumbleEmployee = dept.Employees[_random.Next(dept.Employees.Count)];
                    events.Add(HandleEmployeeFumble(fumbleEmployee, dept, company));
                }

                // Employee retirement events
                if (_random.NextDouble() < 0.08) // 8% chance per department
                {
                    var retiringEmployee = dept.Employees.Where(e => e.Experience > 10).FirstOrDefault();
                    if (retiringEmployee != null)
                    {
                        events.Add(HandleEmployeeRetirement(retiringEmployee, dept, company));
                    }
                }

                // Employee breakthrough/innovation events
                if (_random.NextDouble() < 0.10) // 10% chance per department
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
                company.Capital -= raise * 4; // Quarterly impact
                return $"💰 RETENTION BONUS! {employee.Name} received a ${raise:N0}/month raise to stay. Quarterly cost: ${raise * 3:N0}";
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

        private List<string> GenerateMarketEvents(Company company)
        {
            var events = new List<string>();
            
            // Aggressive market events based on risk appetite
            double chaosMultiplier = company.RiskAppetite switch
            {
                RiskAppetite.Conservative => 0.7,
                RiskAppetite.Balanced => 1.0,
                RiskAppetite.Aggressive => 1.8,
                _ => 1.0
            };

            if (_random.NextDouble() < 0.25 * chaosMultiplier)
            {
                events.Add(GenerateMarketDisruption(company));
            }

            if (_random.NextDouble() < 0.20 * chaosMultiplier)
            {
                events.Add(GenerateCompetitorAction(company));
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
                "🔬 Scientific breakthrough makes products obsolete"
            };

            string disruption = disruptions[_random.Next(disruptions.Length)];
            
            if (_random.NextDouble() < 0.5) // 50% negative, 50% positive
            {
                double loss = company.Capital * (_random.NextDouble() * 0.10 + 0.05); // 0.05 to 0.15 multiplier
                int marketShareLoss = _random.Next(2, 6); // Increased from 1-4 to 2-6
                company.Capital -= loss;
                company.MarketShare -= marketShareLoss;
                return $"📉 MARKET DISRUPTION: {disruption} Capital -${loss:N0}, Market Share -{marketShareLoss}%";
            }
            else
            {
                double gain = _random.Next(20000, 60000);
                double marketShareGain = _random.NextDouble() * 1.0 + 0.5; // Reduced from 1-3 to 0.5-1.5
                company.Capital += gain;
                company.MarketShare += marketShareGain;
                return $"📈 MARKET OPPORTUNITY: {disruption} Capital +${gain:N0}, Market Share +{marketShareGain:F1}%";
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
            
            // Competitors target market leaders more aggressively
            double marketShareLoss = 0.5 + (_random.NextDouble() * 1.0); // Base 0.5-1.5%
            if (company.MarketShare >= 30) marketShareLoss *= 1.5; // 50% more aggressive
            if (company.MarketShare >= 50) marketShareLoss *= 2.0; // 100% more aggressive
            if (company.MarketShare >= 60) marketShareLoss *= 2.5; // 150% more aggressive
            
            company.Capital -= impact;
            company.Reputation -= reputationImpact;
            company.MarketShare -= marketShareLoss;
            company.Risk += 5;

            return $"🏢 COMPETITOR ATTACK: Major competitor {action}! Capital -${impact:N0}, Reputation -{reputationImpact}, Market Share -{marketShareLoss:F1}%, Risk +5";
        }

        private List<string> GenerateCrisisEvents(Company company)
        {
            var events = new List<string>();
            
            // Generate new crisis warnings
            if (_random.NextDouble() < 0.15 && _activeCrises.Count < 2) // Max 2 active crises
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
                CrisisLevel.Warning => 0.05,
                CrisisLevel.Critical => 0.15,
                CrisisLevel.Catastrophic => 0.30,
                _ => 0.05
            };

            // Crisis response affects outcome
            double responseMultiplier = company.CrisisResponse switch
            {
                CrisisResponse.Immediate => 0.6, // Best response
                CrisisResponse.Control => 0.8,   // Moderate response
                CrisisResponse.Absorb => 1.2,    // Worst response
                _ => 1.0
            };

            double capitalLoss = company.Capital * impact * responseMultiplier;
            int moraleLoss = (int)(20 * impact * responseMultiplier);
            int reputationLoss = (int)(15 * impact * responseMultiplier);

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

        private List<string> GenerateRandomChaos(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();
            
            // Pure chaos - completely unpredictable events
            if (_random.NextDouble() < 0.20) // 20% chance of pure chaos
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
        private List<string> GenerateReputationEvents(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();

            // Scandal events (more likely with low reputation)
            if (_random.NextDouble() < 0.15)
            {
                events.Add(GenerateScandal(company));
            }

            // Mismanagement events
            if (_random.NextDouble() < 0.12)
            {
                events.Add(GenerateMismanagement(company, departments));
            }

            // Positive PR events (more likely with high reputation)
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

            company.Reputation -= reputationLoss;
            company.Morale -= moraleLoss;
            company.Capital -= financialImpact;
            company.Risk += 10;

            return $"📰 SCANDAL! Company {scandal}! Reputation -{reputationLoss}, Morale -{moraleLoss}, Cost: ${financialImpact:N0}, Risk +10";
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

            company.Morale -= moraleLoss;
            company.Reputation -= reputationLoss;
            company.Capital -= financialImpact;
            company.Risk += 5;

            return $"🤦 MISMANAGEMENT! {mismanagement}! Morale -{moraleLoss}, Reputation -{reputationLoss}, Cost: ${financialImpact:N0}, Risk +5";
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
        private List<string> GenerateMoraleEvents(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();

            // Miscommunication events (more likely with low morale)
            if (company.Morale < -20 && _random.NextDouble() < 0.18)
            {
                events.Add(GenerateMiscommunication(company, departments));
            }

            // Team building success (more likely with decent morale)
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
        private List<string> GenerateRiskEvents(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();

            // Product defect events (more likely with high risk)
            if (_random.NextDouble() < 0.12)
            {
                events.Add(GenerateProductDefect(company));
            }

            // Quality control success (more likely with low risk)
            if (company.Risk < -10 && _random.NextDouble() < 0.08)
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
        private List<string> ProcessMoraleBasedTurnover(Company company, Dictionary<Department, DepartmentStats> departments)
        {
            var events = new List<string>();
            double turnoverChance = company.GetEmployeeTurnoverChance();

            foreach (var dept in departments.Values)
            {
                var employeesToRemove = new List<Employee>();
                
                foreach (var employee in dept.Employees)
                {
                    // Individual employee morale also affects turnover
                    double individualTurnoverChance = turnoverChance;
                    if (employee.Morale < 30) individualTurnoverChance *= 1.5;
                    if (employee.Morale < 10) individualTurnoverChance *= 2.0;

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
