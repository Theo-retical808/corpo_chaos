using System.Text.Json.Serialization;

namespace CorporateChaos.Models
{
    public enum Department
    {
        Marketing,
        Operations,
        Finance,
        HR,
        IT,
        Research
    }

    public enum SkillLevel
    {
        Trainee = 1,
        Junior = 2,
        Mid = 3,
        Senior = 4,
        Expert = 5
    }

    public enum RiskLevel
    {
        VeryLow = 1,
        Low = 2,
        Medium = 3,
        High = 4,
        VeryHigh = 5
    }

    public class Employee
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public Department? AssignedDepartment { get; set; }

        [JsonPropertyName("productivity")]
        public int Productivity { get; set; } // 1-100

        [JsonPropertyName("salary")]
        public double Salary { get; set; }

        [JsonPropertyName("riskLevel")]
        public RiskLevel RiskLevel { get; set; }

        [JsonPropertyName("overallSkill")]
        public SkillLevel OverallSkill { get; set; }

        [JsonPropertyName("specialization")]
        public Department Specialization { get; set; }

        [JsonPropertyName("experience")]
        public int Experience { get; set; } // Years of experience

        [JsonPropertyName("morale")]
        public int Morale { get; set; } // 1-100

        [JsonPropertyName("isAssigned")]
        public bool IsAssigned { get; set; } = false;

        [JsonPropertyName("quarterHired")]
        public int QuarterHired { get; set; }

        [JsonPropertyName("positionDescription")]
        public string PositionDescription { get; set; } = string.Empty;

        [JsonPropertyName("skillKeywords")]
        public List<string> SkillKeywords { get; set; } = new List<string>();

        public double GetEffectiveProductivity()
        {
            double baseProductivity = Productivity;
            
            // Morale affects productivity
            double moraleMultiplier = Morale / 100.0;
            
            // Specialization bonus if assigned to matching department
            double specializationBonus = 1.0;
            if (AssignedDepartment.HasValue && AssignedDepartment.Value == Specialization)
            {
                specializationBonus = 1.2; // 20% bonus
            }
            
            return baseProductivity * moraleMultiplier * specializationBonus;
        }

        public double GetQuarterlyCost()
        {
            return Salary * 3; // Quarterly cost (3 months)
        }

        public static Employee GenerateRandomEmployee(int currentQuarter, HashSet<string>? usedNames = null)
        {
            var random = new Random();
            
            // Expanded name lists for unique generation
            var firstNames = new[]
            {
                // Male names
                "Alexander", "Andrew", "Anthony", "Benjamin", "Brandon", "Brian", "Carlos", "Charles", "Christopher", "Daniel",
                "David", "Edward", "Eric", "Frank", "George", "Henry", "James", "Jason", "Jeffrey", "John",
                "Jonathan", "Joseph", "Joshua", "Justin", "Kevin", "Mark", "Matthew", "Michael", "Nicholas", "Patrick",
                "Paul", "Peter", "Richard", "Robert", "Ryan", "Samuel", "Scott", "Stephen", "Steven", "Thomas",
                "Timothy", "William", "Aaron", "Adam", "Adrian", "Alan", "Albert", "Antonio", "Arthur", "Austin",
                "Blake", "Bruce", "Carl", "Christian", "Craig", "Derek", "Douglas", "Eugene", "Gabriel", "Gary",
                "Gregory", "Harold", "Ian", "Isaac", "Jacob", "Jeremy", "Jesse", "Joel", "Jordan", "Kenneth",
                "Kyle", "Lawrence", "Louis", "Lucas", "Marcus", "Martin", "Nathan", "Noah", "Oliver", "Oscar",
                "Philip", "Ralph", "Raymond", "Roger", "Sean", "Simon", "Victor", "Walter", "Wayne", "Zachary",
                
                // Female names
                "Amanda", "Amy", "Andrea", "Angela", "Anna", "Ashley", "Barbara", "Betty", "Brenda", "Carol",
                "Catherine", "Christine", "Deborah", "Diana", "Donna", "Dorothy", "Elizabeth", "Emily", "Emma", "Frances",
                "Helen", "Jennifer", "Jessica", "Karen", "Kimberly", "Laura", "Linda", "Lisa", "Margaret", "Maria",
                "Marie", "Mary", "Michelle", "Nancy", "Patricia", "Rebecca", "Ruth", "Sandra", "Sarah", "Sharon",
                "Stephanie", "Susan", "Teresa", "Virginia", "Abigail", "Alice", "Allison", "Amber", "Andrea", "Angela",
                "Ann", "Anne", "Annie", "April", "Beverly", "Bonnie", "Carolyn", "Cheryl", "Christina", "Cynthia",
                "Debra", "Denise", "Diane", "Doris", "Evelyn", "Gloria", "Grace", "Hannah", "Heather", "Irene",
                "Jacqueline", "Jane", "Janet", "Janice", "Jean", "Joan", "Joyce", "Judith", "Julie", "Kathleen",
                "Kathryn", "Kelly", "Lori", "Louise", "Megan", "Melissa", "Nicole", "Pamela", "Rachel", "Rose",
                "Samantha", "Sara", "Shannon", "Shirley", "Tiffany", "Victoria", "Wanda", "Wendy", "Yvonne"
            };

            var lastNames = new[]
            {
                "Adams", "Alexander", "Allen", "Anderson", "Baker", "Barnes", "Bell", "Bennett", "Brooks", "Brown",
                "Butler", "Campbell", "Carter", "Clark", "Collins", "Cook", "Cooper", "Davis", "Edwards", "Evans",
                "Fisher", "Foster", "Garcia", "Gonzalez", "Gray", "Green", "Hall", "Harris", "Henderson", "Hill",
                "Howard", "Hughes", "Jackson", "James", "Johnson", "Jones", "Kelly", "King", "Lee", "Lewis",
                "Lopez", "Martin", "Martinez", "Miller", "Mitchell", "Moore", "Morgan", "Murphy", "Nelson", "Parker",
                "Patterson", "Perez", "Peterson", "Phillips", "Powell", "Price", "Reed", "Richardson", "Rivera", "Roberts",
                "Robinson", "Rodriguez", "Rogers", "Ross", "Russell", "Sanchez", "Scott", "Smith", "Stewart", "Taylor",
                "Thomas", "Thompson", "Torres", "Turner", "Walker", "Ward", "Washington", "Watson", "White", "Williams",
                "Wilson", "Wood", "Wright", "Young", "Bailey", "Bryant", "Coleman", "Cox", "Diaz", "Ellis",
                "Flores", "Ford", "Freeman", "Gibson", "Graham", "Griffin", "Hayes", "Henry", "Hernandez", "Hunt",
                "Jenkins", "Jordan", "Knight", "Lawrence", "Long", "Mason", "McDonald", "Murray", "Owens", "Palmer",
                "Perry", "Porter", "Reynolds", "Rice", "Rose", "Sanders", "Simmons", "Stone", "Sullivan", "Webb",
                "Wells", "West", "Woods", "Adams", "Armstrong", "Austin", "Bishop", "Black", "Boyd", "Bradley",
                "Burns", "Burton", "Carpenter", "Chapman", "Cole", "Crawford", "Cross", "Curtis", "Duncan", "Elliott",
                "Ferguson", "Fields", "Fleming", "Fletcher", "Fox", "Francis", "Franklin", "Gardner", "George", "Gilbert",
                "Gordon", "Grant", "Greene", "Hamilton", "Hansen", "Harper", "Harrison", "Hart", "Harvey", "Hawkins"
            };

            // Generate unique name
            string fullName;
            int attempts = 0;
            do
            {
                string firstName = firstNames[random.Next(firstNames.Length)];
                string lastName = lastNames[random.Next(lastNames.Length)];
                fullName = $"{firstName} {lastName}";
                attempts++;
                
                // Fallback: add middle initial if we can't find unique name after many attempts
                if (attempts > 50)
                {
                    char middleInitial = (char)('A' + random.Next(26));
                    fullName = $"{firstName} {middleInitial}. {lastName}";
                }
                
                // Ultimate fallback: add number suffix
                if (attempts > 100)
                {
                    fullName = $"{firstName} {lastName} {random.Next(1, 100)}";
                    break;
                }
            }
            while (usedNames != null && usedNames.Contains(fullName));

            // Add name to used names set
            usedNames?.Add(fullName);

            var employee = new Employee
            {
                Name = fullName,
                Productivity = random.Next(40, 96), // 40-95
                RiskLevel = (RiskLevel)random.Next(1, 6),
                Specialization = (Department)random.Next(0, 6),
                Experience = random.Next(0, 15),
                Morale = random.Next(60, 91), // Start with decent morale
                QuarterHired = currentQuarter
            };

            // Apply quarter-based skill restrictions
            ApplyQuarterBasedSkillRestrictions(employee, currentQuarter, random);

            // Generate position description and skills based on specialization
            GeneratePositionDetails(employee, random);

            // Calculate salary based on skill, experience, and productivity
            double baseSalary = 3000; // Monthly base
            double skillMultiplier = (int)employee.OverallSkill * 0.3;
            double experienceMultiplier = employee.Experience * 0.1;
            double productivityMultiplier = employee.Productivity / 100.0;
            
            employee.Salary = baseSalary * (1 + skillMultiplier + experienceMultiplier) * productivityMultiplier;
            employee.Salary = Math.Round(employee.Salary, 0);

            return employee;
        }

        private static void ApplyQuarterBasedSkillRestrictions(Employee employee, int currentQuarter, Random random)
        {
            if (currentQuarter <= 5) // Early game (Q1-5): Mostly entry-level
            {
                // 70% Trainee/Junior, 25% Mid, 5% Senior, 0% Expert
                double roll = random.NextDouble();
                if (roll < 0.70)
                {
                    employee.OverallSkill = random.NextDouble() < 0.6 ? SkillLevel.Trainee : SkillLevel.Junior;
                    employee.Experience = random.Next(0, 3);
                    employee.Productivity = Math.Max(30, employee.Productivity - random.Next(0, 20));
                }
                else if (roll < 0.95)
                {
                    employee.OverallSkill = SkillLevel.Mid;
                    employee.Experience = random.Next(2, 6);
                }
                else
                {
                    employee.OverallSkill = SkillLevel.Senior;
                    employee.Experience = random.Next(5, 10);
                    employee.Productivity = Math.Min(95, employee.Productivity + random.Next(0, 15));
                }
            }
            else if (currentQuarter <= 20) // Mid game (Q6-20): More balanced
            {
                // 40% Trainee/Junior, 35% Mid, 20% Senior, 5% Expert
                double roll = random.NextDouble();
                if (roll < 0.40)
                {
                    employee.OverallSkill = random.NextDouble() < 0.5 ? SkillLevel.Trainee : SkillLevel.Junior;
                    employee.Experience = random.Next(0, 4);
                }
                else if (roll < 0.75)
                {
                    employee.OverallSkill = SkillLevel.Mid;
                    employee.Experience = random.Next(2, 8);
                }
                else if (roll < 0.95)
                {
                    employee.OverallSkill = SkillLevel.Senior;
                    employee.Experience = random.Next(5, 12);
                    employee.Productivity = Math.Min(95, employee.Productivity + random.Next(0, 10));
                }
                else
                {
                    employee.OverallSkill = SkillLevel.Expert;
                    employee.Experience = random.Next(8, 15);
                    employee.Productivity = Math.Min(98, employee.Productivity + random.Next(5, 20));
                }
            }
            else // Late game (Q21+): Access to all levels
            {
                // 20% Trainee/Junior, 30% Mid, 35% Senior, 15% Expert
                double roll = random.NextDouble();
                if (roll < 0.20)
                {
                    employee.OverallSkill = random.NextDouble() < 0.4 ? SkillLevel.Trainee : SkillLevel.Junior;
                    employee.Experience = random.Next(0, 5);
                }
                else if (roll < 0.50)
                {
                    employee.OverallSkill = SkillLevel.Mid;
                    employee.Experience = random.Next(3, 10);
                }
                else if (roll < 0.85)
                {
                    employee.OverallSkill = SkillLevel.Senior;
                    employee.Experience = random.Next(6, 15);
                    employee.Productivity = Math.Min(95, employee.Productivity + random.Next(0, 10));
                }
                else
                {
                    employee.OverallSkill = SkillLevel.Expert;
                    employee.Experience = random.Next(10, 20);
                    employee.Productivity = Math.Min(100, employee.Productivity + random.Next(10, 25));
                }
            }
        }

        private static void GeneratePositionDetails(Employee employee, Random random)
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
    }

    public class DepartmentStats
    {
        [JsonPropertyName("department")]
        public Department Department { get; set; }

        [JsonPropertyName("employees")]
        public List<Employee> Employees { get; set; } = new List<Employee>();

        [JsonPropertyName("efficiency")]
        public double Efficiency { get; set; } = 50.0; // Base efficiency

        public double GetTotalProductivity()
        {
            return Employees.Sum(e => e.GetEffectiveProductivity());
        }

        public double GetQuarterlyCost()
        {
            return Employees.Sum(e => e.GetQuarterlyCost());
        }

        public int GetEmployeeCount()
        {
            return Employees.Count;
        }
    }
}