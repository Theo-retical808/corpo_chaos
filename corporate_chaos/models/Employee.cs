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

    public enum Gender
    {
        Male,
        Female
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

        [JsonPropertyName("gender")]
        public Gender Gender { get; set; }

        [JsonPropertyName("profileImagePath")]
        public string ProfileImagePath { get; set; } = string.Empty;

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
            
            // Determine gender randomly
            var gender = random.Next(2) == 0 ? Gender.Male : Gender.Female;
            
            // Load names from JSON data
            var nameData = CorporateChaos.Systems.GameDataLoader.LoadNames();
            var firstNames = gender == Gender.Male ? nameData.MaleFirstNames : nameData.FemaleFirstNames;
            var lastNames = nameData.LastNames;
            
            // Generate unique name
            string fullName;
            int attempts = 0;
            do
            {
                string firstName = firstNames[random.Next(firstNames.Count)];
                string lastName = lastNames[random.Next(lastNames.Count)];
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
                Gender = gender,
                Productivity = random.Next(40, 96), // 40-95
                RiskLevel = (RiskLevel)random.Next(1, 6),
                Specialization = (Department)random.Next(0, 6),
                Experience = random.Next(0, 15),
                Morale = random.Next(60, 91), // Start with decent morale
                QuarterHired = currentQuarter
            };
            
            // Assign random profile image based on gender
            if (gender == Gender.Male)
            {
                int imageNumber = random.Next(1, 11); // 1-10
                employee.ProfileImagePath = $"images/emp_male/emp{imageNumber}.png";
            }
            else
            {
                int imageNumber = random.Next(1, 11); // 1-10
                employee.ProfileImagePath = $"images/emp_female/efp{imageNumber}.png";
            }

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
            // Load position data from JSON
            var positionData = CorporateChaos.Systems.GameDataLoader.LoadPositions();
            var deptName = employee.Specialization.ToString();
            
            if (positionData.Departments.ContainsKey(deptName))
            {
                var dept = positionData.Departments[deptName];
                employee.PositionDescription = dept.Descriptions[random.Next(dept.Descriptions.Count)];
                
                // Add 2-4 relevant keywords
                var shuffledKeywords = dept.Keywords.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();
                employee.SkillKeywords = shuffledKeywords;
            }
            else
            {
                // Fallback for unknown departments
                employee.PositionDescription = "General business professional";
                employee.SkillKeywords = new List<string> { "business", "management" };
            }
        }

        // Method to fix legacy employees loaded from old saves that don't have gender/profile image
        public void EnsureProfileData()
        {
            // If gender is not set or profile image is missing, assign them
            if (string.IsNullOrEmpty(ProfileImagePath))
            {
                var random = new Random(Name.GetHashCode()); // Use name as seed for consistency
                
                // Determine gender based on name if not set
                if (Gender == default(Gender))
                {
                    // Use JSON-loaded names to determine gender
                    var nameData = CorporateChaos.Systems.GameDataLoader.LoadNames();
                    var firstName = Name.Split(' ')[0];
                    Gender = nameData.MaleFirstNames.Contains(firstName) ? Gender.Male : Gender.Female;
                }
                
                // Assign profile image based on gender
                if (Gender == Gender.Male)
                {
                    int imageNumber = (random.Next(1, 11)); // 1-10
                    ProfileImagePath = $"images/emp_male/emp{imageNumber}.png";
                }
                else
                {
                    int imageNumber = (random.Next(1, 11)); // 1-10
                    ProfileImagePath = $"images/emp_female/efp{imageNumber}.png";
                }
            }
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