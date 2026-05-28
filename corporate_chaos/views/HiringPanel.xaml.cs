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
        private const int MAX_REFRESHES_PER_QUARTER = 3;
        
        // Static set to track used names across all game sessions
        private static HashSet<string> usedNames = new HashSet<string>();
        
        // Static dictionary to persist candidates per quarter to prevent refresh exploit
        private static Dictionary<int, List<Employee>> quarterCandidates = new Dictionary<int, List<Employee>>();
        
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
            
            // Load existing candidates for this quarter or generate new ones if first time
            LoadOrGenerateCandidates();
            RefreshCandidatesList();
            UpdateHiringInfo();
            UpdateRefreshButton();
        }

        private void LoadOrGenerateCandidates()
        {
            // Check if we already have candidates for this quarter
            if (quarterCandidates.ContainsKey(currentQuarter) && quarterCandidates[currentQuarter].Count > 0)
            {
                // Load existing candidates for this quarter
                availableCandidates = new List<Employee>(quarterCandidates[currentQuarter]);
            }
            else
            {
                // Generate new candidates for this quarter (first time opening)
                GenerateCandidates();
                // Store them for this quarter
                quarterCandidates[currentQuarter] = new List<Employee>(availableCandidates);
            }
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
            // Use the improved Employee generation with unique names
            var employee = Employee.GenerateRandomEmployee(currentQuarter, usedNames);

            // Adjust employee quality based on hiring quality
            AdjustEmployeeQuality(employee, hiringQuality);

            return employee;
        }

        private void AdjustEmployeeQuality(Employee employee, double hiringQuality)
        {
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

            // Adjust productivity based on hiring quality
            if (hiringQuality >= 0.8)
            {
                employee.Productivity = Math.Min(100, employee.Productivity + random.Next(5, 15));
            }
            else if (hiringQuality <= 0.3)
            {
                employee.Productivity = Math.Max(30, employee.Productivity - random.Next(5, 15));
            }

            // Adjust morale based on hiring quality
            if (hiringQuality >= 0.7)
            {
                employee.Morale = Math.Min(100, employee.Morale + random.Next(5, 10));
            }
            else if (hiringQuality <= 0.4)
            {
                employee.Morale = Math.Max(40, employee.Morale - random.Next(5, 10));
            }
        }

        private int GetCurrentQuarter()
        {
            return currentQuarter;
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
                <= 5 => "Early Career (Q1-5): Mostly entry-level candidates available",
                <= 20 => "Mid Career (Q6-20): Balanced experience levels available", 
                _ => "Late Career (Q21+): Access to all experience levels including experts"
            };
            
            HiringInfoText.Text = $"Hiring Quality: {quality:P0} ({qualityText}) | Refreshes: {remainingRefreshes}/{MAX_REFRESHES_PER_QUARTER}";
            
            // Update the tip text with strategic hiring information
            var tipTextBlock = (TextBlock)this.FindName("TipTextBlock");
            if (tipTextBlock != null)
            {
                tipTextBlock.Text = $"💡 {quarterInfo} | Use skill keywords to identify department fit!";
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
                // Update persistent storage
                quarterCandidates[currentQuarter] = new List<Employee>(availableCandidates);
                
                RefreshCandidatesList();
                
                // Notify parent window
                EmployeeHired?.Invoke(employee);
            }
        }

        private void PassBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Remove from available candidates
                availableCandidates.Remove(employee);
                // Update persistent storage
                quarterCandidates[currentQuarter] = new List<Employee>(availableCandidates);
                
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
            // Update the persistent storage with new candidates
            quarterCandidates[currentQuarter] = new List<Employee>(availableCandidates);
            
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

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}