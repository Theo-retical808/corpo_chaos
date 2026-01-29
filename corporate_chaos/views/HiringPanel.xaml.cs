using System.Windows;
using System.Windows.Controls;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class HiringPanel : Window
    {
        private Company company;
        private Dictionary<Department, DepartmentStats> departments;
        private List<Employee> availableCandidates;
        private Random random = new Random();
        private int currentQuarter;
        private int refreshesUsed = 0;
        private const int MAX_REFRESHES_PER_QUARTER = 5;
        
        public event Action<Employee>? EmployeeHired;
        public event Action<Employee>? EmployeePassed;

        public HiringPanel(Company company, Dictionary<Department, DepartmentStats> departments, int currentQuarter)
        {
            InitializeComponent();
            this.company = company;
            this.departments = departments;
            this.currentQuarter = currentQuarter;
            this.availableCandidates = new List<Employee>();
            
            // Load refresh count for this quarter from company data
            LoadRefreshCount();
            
            GenerateCandidates();
            RefreshCandidatesList();
            UpdateHiringInfo();
            UpdateRefreshButton();
        }

        private void GenerateCandidates()
        {
            availableCandidates.Clear();
            
            // Calculate hiring quality based on HR performance, reputation, and morale
            double hiringQuality = CalculateHiringQuality();
            
            // Generate 3-8 candidates based on hiring quality
            int candidateCount = Math.Max(3, Math.Min(8, (int)(3 + hiringQuality * 5)));
            
            for (int i = 0; i < candidateCount; i++)
            {
                var candidate = GenerateQualityCandidate(hiringQuality);
                availableCandidates.Add(candidate);
            }
        }

        private double CalculateHiringQuality()
        {
            // Base quality starts at 0.3 (30%)
            double baseQuality = 0.3;
            
            // HR department performance (40% weight)
            double hrQuality = 0.0;
            if (departments.ContainsKey(Department.HR) && departments[Department.HR].Employees.Count > 0)
            {
                var hrDept = departments[Department.HR];
                double hrProductivity = hrDept.GetTotalProductivity();
                double hrEmployeeCount = hrDept.GetEmployeeCount();
                
                // Average productivity per HR employee (normalized to 0-1)
                hrQuality = Math.Min(1.0, (hrProductivity / hrEmployeeCount) / 100.0);
            }
            
            // Company reputation (30% weight) - normalized from -100/100 to 0-1
            double reputationQuality = Math.Max(0, (company.Reputation + 100) / 200.0);
            
            // Company morale (30% weight) - normalized from -100/100 to 0-1  
            double moraleQuality = Math.Max(0, (company.Morale + 100) / 200.0);
            
            // Combine factors
            double totalQuality = baseQuality + (hrQuality * 0.4) + (reputationQuality * 0.3) + (moraleQuality * 0.3);
            
            return Math.Min(1.0, totalQuality); // Cap at 100%
        }

        private Employee GenerateQualityCandidate(double hiringQuality)
        {
            var names = new[]
            {
                "Alex Johnson", "Sarah Chen", "Michael Brown", "Emma Davis", "James Wilson",
                "Lisa Garcia", "David Miller", "Anna Rodriguez", "Chris Taylor", "Maria Lopez",
                "Robert Anderson", "Jennifer White", "Kevin Lee", "Amanda Clark", "Daniel Hall",
                "Jessica Martinez", "Ryan Thompson", "Ashley Lewis", "Brandon Walker", "Nicole Young",
                "Thomas Moore", "Rachel Kim", "Steven Wright", "Michelle Turner", "Jason Scott",
                "Laura Adams", "Mark Phillips", "Stephanie Hill", "Andrew Green", "Samantha Baker"
            };

            var employee = new Employee
            {
                Name = names[random.Next(names.Length)],
                Specialization = (Department)random.Next(0, 6),
                QuarterHired = 0 // Will be set when hired
            };

            // Get current quarter from company (assuming we can access it)
            int currentQuarter = GetCurrentQuarter();

            // Apply quarter-based skill restrictions
            ApplyQuarterBasedSkillRestrictions(employee, currentQuarter, hiringQuality);

            // Generate position description and skills
            GeneratePositionDetails(employee, random);

            // Set risk level (lower quality hiring = higher risk candidates)
            if (hiringQuality >= 0.7)
            {
                employee.RiskLevel = (RiskLevel)random.Next(1, 3); // VeryLow to Low
            }
            else if (hiringQuality >= 0.5)
            {
                employee.RiskLevel = (RiskLevel)random.Next(1, 4); // VeryLow to Medium
            }
            else
            {
                employee.RiskLevel = (RiskLevel)random.Next(2, 6); // Low to VeryHigh
            }

            // Calculate salary based on skill, experience, and productivity
            double baseSalary = 3000; // Monthly base
            double skillMultiplier = (int)employee.OverallSkill * 0.3;
            double experienceMultiplier = employee.Experience * 0.1;
            double productivityMultiplier = employee.Productivity / 100.0;
            
            employee.Salary = baseSalary * (1 + skillMultiplier + experienceMultiplier) * productivityMultiplier;
            employee.Salary = Math.Round(employee.Salary, 0);

            return employee;
        }

        private int GetCurrentQuarter()
        {
            return currentQuarter;
        }

        private void ApplyQuarterBasedSkillRestrictions(Employee employee, int currentQuarter, double hiringQuality)
        {
            // Base skill distribution based on quarter
            SkillLevel baseSkill;
            int baseExperience;
            int baseProductivity;
            int baseMorale;

            if (currentQuarter <= 5) // Early game (Q1-5): Mostly entry-level
            {
                // 70% Trainee/Junior, 25% Mid, 5% Senior, 0% Expert
                double roll = random.NextDouble();
                if (roll < 0.70)
                {
                    baseSkill = random.NextDouble() < 0.6 ? SkillLevel.Trainee : SkillLevel.Junior;
                    baseExperience = random.Next(0, 3);
                    baseProductivity = random.Next(30, 70);
                }
                else if (roll < 0.95)
                {
                    baseSkill = SkillLevel.Mid;
                    baseExperience = random.Next(2, 6);
                    baseProductivity = random.Next(50, 80);
                }
                else
                {
                    baseSkill = SkillLevel.Senior;
                    baseExperience = random.Next(5, 10);
                    baseProductivity = random.Next(70, 90);
                }
            }
            else if (currentQuarter <= 20) // Mid game (Q6-20): More balanced
            {
                // 40% Trainee/Junior, 35% Mid, 20% Senior, 5% Expert
                double roll = random.NextDouble();
                if (roll < 0.40)
                {
                    baseSkill = random.NextDouble() < 0.5 ? SkillLevel.Trainee : SkillLevel.Junior;
                    baseExperience = random.Next(0, 4);
                    baseProductivity = random.Next(35, 75);
                }
                else if (roll < 0.75)
                {
                    baseSkill = SkillLevel.Mid;
                    baseExperience = random.Next(2, 8);
                    baseProductivity = random.Next(55, 85);
                }
                else if (roll < 0.95)
                {
                    baseSkill = SkillLevel.Senior;
                    baseExperience = random.Next(5, 12);
                    baseProductivity = random.Next(70, 95);
                }
                else
                {
                    baseSkill = SkillLevel.Expert;
                    baseExperience = random.Next(8, 15);
                    baseProductivity = random.Next(80, 98);
                }
            }
            else // Late game (Q21+): Access to all levels
            {
                // 20% Trainee/Junior, 30% Mid, 35% Senior, 15% Expert
                double roll = random.NextDouble();
                if (roll < 0.20)
                {
                    baseSkill = random.NextDouble() < 0.4 ? SkillLevel.Trainee : SkillLevel.Junior;
                    baseExperience = random.Next(0, 5);
                    baseProductivity = random.Next(40, 80);
                }
                else if (roll < 0.50)
                {
                    baseSkill = SkillLevel.Mid;
                    baseExperience = random.Next(3, 10);
                    baseProductivity = random.Next(60, 90);
                }
                else if (roll < 0.85)
                {
                    baseSkill = SkillLevel.Senior;
                    baseExperience = random.Next(6, 15);
                    baseProductivity = random.Next(75, 98);
                }
                else
                {
                    baseSkill = SkillLevel.Expert;
                    baseExperience = random.Next(10, 20);
                    baseProductivity = random.Next(85, 100);
                }
            }

            // Apply hiring quality modifiers
            if (hiringQuality >= 0.8) // Excellent hiring
            {
                baseProductivity = Math.Min(100, baseProductivity + random.Next(5, 15));
                baseMorale = random.Next(75, 95);
                // Chance to upgrade skill level
                if (random.NextDouble() < 0.3 && baseSkill < SkillLevel.Expert)
                {
                    baseSkill = (SkillLevel)((int)baseSkill + 1);
                    baseExperience += random.Next(1, 3);
                }
            }
            else if (hiringQuality >= 0.6) // Good hiring
            {
                baseProductivity = Math.Min(100, baseProductivity + random.Next(0, 10));
                baseMorale = random.Next(65, 85);
            }
            else if (hiringQuality >= 0.4) // Average hiring
            {
                baseMorale = random.Next(55, 80);
            }
            else // Poor hiring
            {
                baseProductivity = Math.Max(20, baseProductivity - random.Next(5, 15));
                baseMorale = random.Next(40, 70);
                // Chance to downgrade skill level
                if (random.NextDouble() < 0.2 && baseSkill > SkillLevel.Trainee)
                {
                    baseSkill = (SkillLevel)((int)baseSkill - 1);
                    baseExperience = Math.Max(0, baseExperience - random.Next(1, 3));
                }
            }

            employee.OverallSkill = baseSkill;
            employee.Experience = baseExperience;
            employee.Productivity = baseProductivity;
            employee.Morale = baseMorale;
        }

        private void GeneratePositionDetails(Employee employee, Random random)
        {
            var positionTemplates = new Dictionary<Department, (string[] descriptions, string[] keywords)>
            {
                [Department.Marketing] = (
                    new[] {
                        "Brand strategist with creative campaign experience",
                        "Digital marketing specialist focused on social media growth",
                        "Market research analyst with consumer behavior expertise",
                        "Content creator with strong storytelling abilities",
                        "SEO/SEM specialist with data-driven approach",
                        "Public relations coordinator with media connections"
                    },
                    new[] { "campaigns", "branding", "social media", "analytics", "content", "SEO", "PR", "creative" }
                ),
                [Department.Operations] = (
                    new[] {
                        "Process optimization expert with lean methodology background",
                        "Supply chain coordinator with vendor management skills",
                        "Quality assurance specialist focused on continuous improvement",
                        "Project manager with cross-functional team experience",
                        "Operations analyst with efficiency optimization focus",
                        "Logistics coordinator with distribution expertise"
                    },
                    new[] { "processes", "supply chain", "quality", "logistics", "efficiency", "lean", "coordination" }
                ),
                [Department.Finance] = (
                    new[] {
                        "Financial analyst with budgeting and forecasting expertise",
                        "Accounting specialist with regulatory compliance knowledge",
                        "Investment advisor with portfolio management experience",
                        "Cost analyst focused on expense optimization",
                        "Tax specialist with corporate finance background",
                        "Risk management analyst with audit experience"
                    },
                    new[] { "budgeting", "forecasting", "compliance", "investments", "analysis", "auditing", "taxation" }
                ),
                [Department.HR] = (
                    new[] {
                        "Talent acquisition specialist with recruitment expertise",
                        "Employee relations coordinator focused on workplace culture",
                        "Training and development specialist with learning programs",
                        "Compensation analyst with benefits administration skills",
                        "HR generalist with policy development experience",
                        "Organizational development consultant with change management"
                    },
                    new[] { "recruitment", "culture", "training", "benefits", "policies", "development", "relations" }
                ),
                [Department.IT] = (
                    new[] {
                        "Software developer with full-stack development skills",
                        "Systems administrator with network infrastructure expertise",
                        "Cybersecurity specialist focused on threat prevention",
                        "Database administrator with data management experience",
                        "IT support technician with troubleshooting abilities",
                        "DevOps engineer with automation and deployment skills"
                    },
                    new[] { "programming", "systems", "security", "databases", "support", "automation", "networks" }
                ),
                [Department.Research] = (
                    new[] {
                        "Research scientist with experimental design expertise",
                        "Data scientist with machine learning and analytics skills",
                        "Product development specialist with innovation focus",
                        "Market research analyst with statistical analysis background",
                        "R&D engineer with prototype development experience",
                        "Innovation consultant with emerging technology knowledge"
                    },
                    new[] { "research", "data science", "innovation", "analysis", "development", "experimentation", "technology" }
                )
            };

            var (descriptions, keywords) = positionTemplates[employee.Specialization];
            employee.PositionDescription = descriptions[random.Next(descriptions.Length)];
            
            // Add 2-4 relevant keywords
            var shuffledKeywords = keywords.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();
            employee.SkillKeywords = shuffledKeywords;
        }

        private void UpdateHiringInfo()
        {
            double quality = CalculateHiringQuality();
            int remainingRefreshes = MAX_REFRESHES_PER_QUARTER - refreshesUsed;
            
            string qualityText = quality switch
            {
                >= 0.8 => "Excellent - Attracting top talent!",
                >= 0.6 => "Good - Quality candidates available",
                >= 0.4 => "Average - Mixed candidate pool",
                _ => "Poor - Limited candidate quality"
            };

            string quarterInfo = currentQuarter switch
            {
                <= 5 => "Early Career (Q1-5): Mostly entry-level candidates (70% Trainee/Junior)",
                <= 20 => "Mid Career (Q6-20): Balanced experience levels available",
                _ => "Late Career (Q21+): Access to all experience levels including experts"
            };
            
            HiringInfoText.Text = $"Hiring Quality: {quality:P0} ({qualityText}) | Refreshes: {remainingRefreshes}/{MAX_REFRESHES_PER_QUARTER}";
            
            // Update the tip text with quarter-specific information
            var tipTextBlock = (TextBlock)this.FindName("TipTextBlock");
            if (tipTextBlock != null)
            {
                tipTextBlock.Text = $"💡 {quarterInfo}";
            }
        }

        private void RefreshCandidatesList()
        {
            CandidatesItemsControl.ItemsSource = null;
            CandidatesItemsControl.ItemsSource = availableCandidates;
        }

        private void HireBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Remove from available candidates
                availableCandidates.Remove(employee);
                RefreshCandidatesList();
                
                // Notify parent window
                EmployeeHired?.Invoke(employee);
                
                MessageBox.Show($"✅ {employee.Name} has been hired!\nThey will be available for department assignment.", 
                    "Employee Hired", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PassBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Remove from available candidates
                availableCandidates.Remove(employee);
                RefreshCandidatesList();
                
                // Notify parent window
                EmployeePassed?.Invoke(employee);
            }
        }

        private void RefreshCandidatesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (refreshesUsed >= MAX_REFRESHES_PER_QUARTER)
            {
                MessageBox.Show($"You have used all {MAX_REFRESHES_PER_QUARTER} candidate refreshes for this quarter.\nTry again next quarter!", 
                    "Refresh Limit Reached", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            refreshesUsed++;
            SaveRefreshCount();
            
            GenerateCandidates();
            RefreshCandidatesList();
            UpdateHiringInfo();
            UpdateRefreshButton();
        }

        private void LoadRefreshCount()
        {
            // Reset refresh count if it's a new quarter
            if (company.LastRefreshQuarter != currentQuarter)
            {
                company.CurrentQuarterRefreshes = 0;
                company.LastRefreshQuarter = currentQuarter;
            }
            
            refreshesUsed = company.CurrentQuarterRefreshes;
        }

        private void SaveRefreshCount()
        {
            company.CurrentQuarterRefreshes = refreshesUsed;
            company.LastRefreshQuarter = currentQuarter;
        }

        private void UpdateRefreshButton()
        {
            int remainingRefreshes = MAX_REFRESHES_PER_QUARTER - refreshesUsed;
            
            if (remainingRefreshes > 0)
            {
                RefreshCandidatesBtn.Content = $"🔄 Refresh Candidates ({remainingRefreshes} left)";
                RefreshCandidatesBtn.IsEnabled = true;
                RefreshLimitText.Text = $"⚠️ You can refresh the candidate list {remainingRefreshes} more times this quarter";
                RefreshLimitText.Foreground = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                RefreshCandidatesBtn.Content = "🔄 No Refreshes Left";
                RefreshCandidatesBtn.IsEnabled = false;
                RefreshLimitText.Text = "❌ No more refreshes available this quarter - try again next quarter!";
                RefreshLimitText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}