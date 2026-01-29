using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CorporateChaos.Models;
using CorporateChaos.Systems;
using CorporateChaos.Views;

namespace CorporateChaos
{
    public partial class MainWindow : Window
    {
        private Company company = null!;
        private ChaosEngine chaos = null!;
        private DecisionSystem decisions = null!;
        private StringBuilder gameLog = null!;
        private int quarterNumber;
        private DataManager dataManager = null!;
        private SaveLoadManager saveLoadManager = null!;
        
        // New systems for employee management
        private Dictionary<Department, DepartmentStats> departments = null!;
        private List<Employee> hiredEmployees = null!; // Changed from availableEmployees
        private GameRunRecord? currentGameRun = null;
        
        // Quarterly summary tracking
        private List<string> currentQuarterEvents = null!;
        private bool hasNewEvents = false;
        
        // Peak performance tracking
        private GameScore currentGameScore = null!;

        public MainWindow()
        {
            InitializeComponent();
            dataManager = new DataManager();
            saveLoadManager = new SaveLoadManager();
            InitializeGame();
        }

        private void InitializeGame()
        {
            var config = dataManager.GetConfig();
            company = new Company();
            
            chaos = new ChaosEngine();
            decisions = new DecisionSystem();
            gameLog = new StringBuilder();
            quarterNumber = 1;
            
            // Initialize departments
            departments = new Dictionary<Department, DepartmentStats>();
            foreach (Department dept in Enum.GetValues<Department>())
            {
                departments[dept] = new DepartmentStats { Department = dept };
            }
            
            // Initialize hired employees list
            hiredEmployees = new List<Employee>();
            
            // Initialize quarterly events tracking
            currentQuarterEvents = new List<string>();
            
            // Initialize game run record
            currentGameRun = new GameRunRecord
            {
                StartDate = DateTime.Now,
                QuartersPlayed = 0
            };
            
            // Initialize peak performance tracking
            currentGameScore = new GameScore
            {
                QuartersPlayed = 0,
                EndReason = "In Progress"
            };
            
            // Set initial peak values
            UpdatePeakPerformance();
            
            // Update UI with initial values
            UpdateUI();
        }

        // Main Menu Navigation
        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            MainMenuGrid.Visibility = Visibility.Collapsed;
            SaveSlotsGrid.Visibility = Visibility.Visible;
        }

        private void LoadGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileManager = new SaveFileManager(saveLoadManager);
            saveFileManager.Owner = this;
            
            if (saveFileManager.ShowDialog() == true && saveFileManager.ShouldLoadGame && saveFileManager.SelectedSave != null)
            {
                LoadGameState(saveFileManager.SelectedSave);
                SaveSlotsGrid.Visibility = Visibility.Collapsed;
                CorporateGameGrid.Visibility = Visibility.Visible;
                StartCorporateGame();
            }
        }

        private void ScoreBoardBtn_Click(object sender, RoutedEventArgs e)
        {
            var highScoresWindow = new HighScoresWindow(dataManager);
            highScoresWindow.Owner = this;
            highScoresWindow.ShowDialog();
        }

        // Save Slots Navigation
        private void CorporateBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveSlotsGrid.Visibility = Visibility.Collapsed;
            CorporateGameGrid.Visibility = Visibility.Visible;
            StartCorporateGame();
        }

        private void BackToMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveSlotsGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
        }

        private void MainMenuBackBtn_Click(object sender, RoutedEventArgs e)
        {
            CorporateGameGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
            InitializeGame(); // Reset game state
        }

        // Corporate Game Logic
        private void StartCorporateGame()
        {
            UpdateUI();
            UpdateCurrentSettings();
            gameLog.Clear();
            gameLog.AppendLine("🏢 === CORPORATE EXECUTIVE DASHBOARD ACTIVATED ===");
            gameLog.AppendLine("Welcome to your new corporate adventure! Make strategic decisions each quarter.");
            gameLog.AppendLine("💡 Tip: Assign employees to departments for maximum efficiency!");
            gameLog.AppendLine();
        }

        private void UpdateUI()
        {
            // Update header with retirement progress
            int yearsCompleted = (quarterNumber - 1) / 4;
            int currentQuarterInYear = ((quarterNumber - 1) % 4) + 1;
            QuarterCounterText.Text = $"Y{yearsCompleted + 1} Q{currentQuarterInYear} ({quarterNumber}/120)";
            HeaderCapitalText.Text = $"${company.Capital:N0}";
            
            // Update company stats with new -100 to 100 ranges
            CapitalText.Text = $"${company.Capital:N0}";
            MarketShareText.Text = $"{company.MarketShare:F1}%";
            ReputationText.Text = $"{company.Reputation} ({GetReputationDescription(company.Reputation)})";
            MoraleText.Text = $"{company.Morale} ({GetMoraleDescription(company.Morale)})";
            RiskText.Text = $"{company.Risk} ({GetRiskDescription(company.Risk)})";
            
            // Update employee count
            int totalEmployees = departments.Values.Sum(d => d.GetEmployeeCount());
            company.EmployeeCount = totalEmployees;
            EmployeeCountText.Text = totalEmployees.ToString();
            
            // Update hired employees count
            HiredEmployeesCountText.Text = $"Total Hired: {hiredEmployees.Count} employees";
            
            // Update quarterly financials
            QuarterlyRevenueText.Text = $"${company.QuarterlyRevenue:N0}";
            QuarterlyExpensesText.Text = $"${company.QuarterlyExpenses:N0}";
            double netProfit = company.QuarterlyRevenue - company.QuarterlyExpenses;
            NetProfitText.Text = $"${netProfit:N0}";
            
            // Update crisis status
            CrisisStatusText.Text = chaos.GetCrisisStatusSummary();
            
            // Update department button tooltips with employee counts
            UpdateDepartmentButtonTooltips();
            
            // Ensure values stay within bounds
            company.ClampValues();
        }

        private void UpdateDepartmentButtonTooltips()
        {
            // Update tooltips
            MarketingDeptBtn.ToolTip = $"Marketing Department\nEmployees: {departments[Department.Marketing].GetEmployeeCount()}\nProductivity: {departments[Department.Marketing].GetTotalProductivity():F1}";
            OperationsDeptBtn.ToolTip = $"Operations Department\nEmployees: {departments[Department.Operations].GetEmployeeCount()}\nProductivity: {departments[Department.Operations].GetTotalProductivity():F1}";
            FinanceDeptBtn.ToolTip = $"Finance Department\nEmployees: {departments[Department.Finance].GetEmployeeCount()}\nProductivity: {departments[Department.Finance].GetTotalProductivity():F1}";
            HRDeptBtn.ToolTip = $"Human Resources Department\nEmployees: {departments[Department.HR].GetEmployeeCount()}\nProductivity: {departments[Department.HR].GetTotalProductivity():F1}";
            ITDeptBtn.ToolTip = $"IT Department\nEmployees: {departments[Department.IT].GetEmployeeCount()}\nProductivity: {departments[Department.IT].GetTotalProductivity():F1}";
            ResearchDeptBtn.ToolTip = $"Research Department\nEmployees: {departments[Department.Research].GetEmployeeCount()}\nProductivity: {departments[Department.Research].GetTotalProductivity():F1}";
            
            // Update employee count badges
            MarketingEmployeeCount.Text = departments[Department.Marketing].GetEmployeeCount().ToString();
            OperationsEmployeeCount.Text = departments[Department.Operations].GetEmployeeCount().ToString();
            FinanceEmployeeCount.Text = departments[Department.Finance].GetEmployeeCount().ToString();
            HREmployeeCount.Text = departments[Department.HR].GetEmployeeCount().ToString();
            ITEmployeeCount.Text = departments[Department.IT].GetEmployeeCount().ToString();
            ResearchEmployeeCount.Text = departments[Department.Research].GetEmployeeCount().ToString();
        }

        private void UpdateCurrentSettings()
        {
            // This method can be removed or simplified since we removed the settings display
        }

        // Control Knob Event Handlers
        private void RiskAppetiteCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (company != null && RiskAppetiteCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                company.RiskAppetite = Enum.Parse<RiskAppetite>(item.Tag.ToString()!);
            }
        }

        private void BudgetAllocationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (company != null && BudgetAllocationCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                company.BudgetAllocation = Enum.Parse<InvestmentLevel>(item.Tag.ToString()!);
            }
        }

        private void MarketStrategyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (company != null && MarketStrategyCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                company.MarketStrategy = Enum.Parse<MarketStrategy>(item.Tag.ToString()!);
            }
        }

        private void CrisisResponseCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (company != null && CrisisResponseCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                company.CrisisResponse = Enum.Parse<CrisisResponse>(item.Tag.ToString()!);
            }
        }

        // Employee Management - New Hiring System
        private void HireEmployeesBtn_Click(object sender, RoutedEventArgs e)
        {
            var hiringPanel = new HiringPanel(company, departments, quarterNumber);
            hiringPanel.Owner = this;
            
            // Subscribe to hiring events
            hiringPanel.EmployeeHired += OnEmployeeHired;
            hiringPanel.EmployeePassed += OnEmployeePassed;
            
            hiringPanel.ShowDialog();
        }

        private void OnEmployeeHired(Employee employee)
        {
            employee.QuarterHired = quarterNumber;
            employee.IsAssigned = false; // Will be assigned to department later
            hiredEmployees.Add(employee);
            
            UpdateUI();
            LogEvent($"✅ {employee.Name} hired! Assign them to a department for maximum efficiency.");
        }

        private void OnEmployeePassed(Employee employee)
        {
            LogEvent($"❌ Passed on hiring {employee.Name}");
        }

        // Executive Decisions Panel
        private void ExecutiveDecisionsBtn_Click(object sender, RoutedEventArgs e)
        {
            var executiveDecisions = new ExecutiveDecisions(company, departments);
            executiveDecisions.Owner = this;
            
            // Subscribe to decision events
            executiveDecisions.DecisionMade += OnExecutiveDecisionMade;
            
            executiveDecisions.ShowDialog();
        }

        private void OnExecutiveDecisionMade(string decisionDescription)
        {
            UpdateUI();
            LogEvent($"🎯 EXECUTIVE DECISION: {decisionDescription}");
        }

        // Current Events Button
        private void CurrentEventsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowQuarterlySummary();
            
            // Hide the new events indicator
            NewEventsIndicator.Visibility = Visibility.Collapsed;
            hasNewEvents = false;
        }

        private void ShowQuarterlySummary()
        {
            var quarterlySummary = new QuarterlySummary(quarterNumber - 1, company, departments, currentQuarterEvents);
            quarterlySummary.Owner = this;
            quarterlySummary.ShowDialog();
        }

        // Department Management - New Panel System
        private void DepartmentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                var deptName = button.Tag.ToString()!;
                if (Enum.TryParse<Department>(deptName, out Department department))
                {
                    ShowDepartmentPanel(department);
                }
            }
        }

        private void ShowDepartmentPanel(Department department)
        {
            var departmentPanel = new DepartmentPanel(department, departments, hiredEmployees);
            departmentPanel.Owner = this;
            
            // Subscribe to employee changes
            departmentPanel.EmployeesChanged += OnEmployeesChanged;
            
            departmentPanel.ShowDialog();
        }

        private void OnEmployeesChanged()
        {
            UpdateUI();
            LogEvent("👥 Employee assignments updated!");
        }

        // Save/Load System
        private void SaveGameBtn_Click(object sender, RoutedEventArgs e)
        {
            string saveName = $"Corporate_Q{quarterNumber}_{DateTime.Now:MMdd_HHmm}";
            
            var gameSave = new GameSave
            {
                SaveName = saveName,
                PlayerNickname = "Player", // Could be enhanced with input dialog
                CurrentQuarter = quarterNumber,
                Company = company,
                Departments = departments,
                AvailableEmployees = hiredEmployees, // Changed to hiredEmployees
                GameEvents = gameLog.ToString().Split('\n').ToList()
            };
            
            if (saveLoadManager.SaveGame(gameSave))
            {
                MessageBox.Show($"Game saved as: {gameSave.GetFileName()}", "Save Successful", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to save game!", "Save Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGameState(GameSave gameSave)
        {
            quarterNumber = gameSave.CurrentQuarter;
            company = gameSave.Company;
            departments = gameSave.Departments;
            hiredEmployees = gameSave.AvailableEmployees; // Load as hired employees
            
            gameLog = new StringBuilder();
            foreach (var eventText in gameSave.GameEvents)
            {
                gameLog.AppendLine(eventText);
            }
            
            // Update UI after loading
            UpdateUI();
        }

        // Quarter End Processing
        private void EndQuarterBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessQuarterEnd();
        }

        private void ProcessQuarterEnd()
        {
            // Process quarterly financials
            company.ProcessQuarterlyFinancials(departments);
            
            // Apply the new chaotic events system
            var chaosEvents = chaos.ApplyQuarterlyChaos(company, departments);
            
            // Add chaos events to current quarter tracking
            currentQuarterEvents.AddRange(chaosEvents);
            
            // Log all chaos events
            foreach (var eventText in chaosEvents)
            {
                LogEvent($"🎲 {eventText}");
            }
            
            // Update crisis status display
            UpdateCrisisStatus();
            
            // Update peak performance tracking
            UpdatePeakPerformance();
            
            // Update employee morale based on company performance
            UpdateEmployeeMorale();
            
            quarterNumber++;
            
            // Reset hiring refreshes for the new quarter
            if (company.LastRefreshQuarter != quarterNumber)
            {
                company.CurrentQuarterRefreshes = 0;
                company.LastRefreshQuarter = quarterNumber;
            }
            
            UpdateUI();
            
            // Show quarterly summary
            ShowQuarterlySummary();
            
            // Reset events for next quarter and show indicator
            currentQuarterEvents.Clear();
            hasNewEvents = true;
            NewEventsIndicator.Visibility = Visibility.Visible;
            
            // Check for game over conditions
            if (company.Capital <= 0)
            {
                HandleGameOver("Bankruptcy - Ran out of capital");
            }
            else if (company.MarketShare >= 70)
            {
                HandleGameOver("Victory - Market Dominance Achieved (70% Market Share)");
            }
            else if (company.EmployeeCount <= 0)
            {
                HandleGameOver("Business Failure - No employees left");
            }
            else if (quarterNumber > 120)
            {
                HandleGameOver("Retirement - You've reached the end of your 30-year career!");
            }
            
            LogEvent($"📅 Quarter {quarterNumber} begins! Use the hiring panel to recruit new talent.");
        }

        private void UpdateCrisisStatus()
        {
            CrisisStatusText.Text = chaos.GetCrisisStatusSummary();
        }

        private void UpdatePeakPerformance()
        {
            // Update peak performance metrics
            currentGameScore.UpdatePeakMetrics(company, quarterNumber);
            currentGameScore.QuartersPlayed = quarterNumber - 1;
        }

        private void UpdateEmployeeMorale()
        {
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    // Adjust morale based on company performance
                    if (company.QuarterlyRevenue > company.QuarterlyExpenses)
                    {
                        employee.Morale = Math.Min(100, employee.Morale + 5);
                    }
                    else
                    {
                        employee.Morale = Math.Max(0, employee.Morale - 10);
                    }
                }
            }
        }

        private void HandleGameOver(string endReason)
        {
            // Finalize the peak performance score
            currentGameScore.EndReason = endReason;
            currentGameScore.QuartersPlayed = quarterNumber - 1;
            
            // Set final performance for reference
            currentGameScore.FinalCapital = Math.Max(0, company.Capital);
            currentGameScore.FinalMarketShare = company.MarketShare;
            currentGameScore.FinalEmployees = company.EmployeeCount;

            int finalScore = currentGameScore.CalculateScore();
            var config = dataManager.GetConfig();

            // Save game run record
            if (currentGameRun != null)
            {
                currentGameRun.EndDate = DateTime.Now;
                currentGameRun.FinalScore = finalScore;
                currentGameRun.QuartersPlayed = quarterNumber - 1;
                currentGameRun.EndReason = endReason;
                currentGameRun.FinalStats = company;
                currentGameRun.PeakMarketShare = currentGameScore.PeakMarketShare;
                currentGameRun.MaxEmployees = currentGameScore.PeakEmployees;
                currentGameRun.TotalRevenue = currentGameScore.PeakRevenue;
                saveLoadManager.SaveGameRun(currentGameRun);
            }

            // Show nickname dialog
            var nicknameDialog = new NicknameDialog(finalScore, config);
            nicknameDialog.Owner = this;
            
            if (nicknameDialog.ShowDialog() == true && nicknameDialog.SaveScore)
            {
                currentGameScore.Nickname = nicknameDialog.PlayerNickname;
                dataManager.AddScore(currentGameScore);
                
                int rank = dataManager.GetPlayerRank(currentGameScore.Nickname);
                string rankText = rank > 0 ? $"You ranked #{rank} on the leaderboard!" : "Great job!";
                
                MessageBox.Show($"🎯 Final Score: {finalScore:N0} (Based on Peak Performance)\n{rankText}\n\n" +
                              $"📊 Peak Performance Summary:\n" +
                              $"• Peak Capital: ${currentGameScore.PeakCapital:N0} (Q{currentGameScore.PeakQuarter})\n" +
                              $"• Peak Revenue: ${currentGameScore.PeakRevenue:N0}\n" +
                              $"• Peak Profit: ${currentGameScore.PeakProfit:N0}\n" +
                              $"• Peak Market Share: {currentGameScore.PeakMarketShare:F1}%\n" +
                              $"• Peak Employees: {currentGameScore.PeakEmployees}\n" +
                              $"• Quarters Survived: {currentGameScore.QuartersPlayed}\n" +
                              $"• End Reason: {endReason}",
                              "Game Complete!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"🎯 Final Score: {finalScore:N0} (Based on Peak Performance)\n\n" +
                              $"📊 Peak Performance Summary:\n" +
                              $"• Peak Capital: ${currentGameScore.PeakCapital:N0} (Q{currentGameScore.PeakQuarter})\n" +
                              $"• Peak Revenue: ${currentGameScore.PeakRevenue:N0}\n" +
                              $"• Peak Profit: ${currentGameScore.PeakProfit:N0}\n" +
                              $"• Peak Market Share: {currentGameScore.PeakMarketShare:F1}%\n" +
                              $"• Peak Employees: {currentGameScore.PeakEmployees}\n" +
                              $"• Quarters Survived: {currentGameScore.QuartersPlayed}\n" +
                              $"• End Reason: {endReason}",
                              "Game Complete!", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Return to main menu
            MainMenuBackBtn_Click(this, new RoutedEventArgs());
        }

        private void LogEvent(string eventText)
        {
            gameLog.AppendLine(eventText);
            gameLog.AppendLine();
            
            // Add to current quarter events if it's a significant event
            if (eventText.Contains("🎲") || eventText.Contains("🎯") || eventText.Contains("✅") || eventText.Contains("❌"))
            {
                currentQuarterEvents.Add(eventText);
                
                // Show new events indicator
                if (!hasNewEvents)
                {
                    hasNewEvents = true;
                    NewEventsIndicator.Visibility = Visibility.Visible;
                }
            }
        }

        private string GetReputationDescription(int reputation)
        {
            return reputation switch
            {
                >= 80 => "Excellent",
                >= 60 => "Very Good",
                >= 40 => "Good",
                >= 20 => "Fair",
                >= 0 => "Neutral",
                >= -20 => "Poor",
                >= -40 => "Bad",
                >= -60 => "Very Bad",
                >= -80 => "Terrible",
                _ => "Disastrous"
            };
        }

        private string GetMoraleDescription(int morale)
        {
            return morale switch
            {
                >= 80 => "Excellent",
                >= 60 => "High",
                >= 40 => "Good",
                >= 20 => "Fair",
                >= 0 => "Neutral",
                >= -20 => "Low",
                >= -40 => "Poor",
                >= -60 => "Very Low",
                >= -80 => "Critical",
                _ => "Catastrophic"
            };
        }

        private string GetRiskDescription(int risk)
        {
            return risk switch
            {
                >= 80 => "Extreme",
                >= 60 => "Very High",
                >= 40 => "High",
                >= 20 => "Elevated",
                >= 0 => "Moderate",
                >= -20 => "Low",
                >= -40 => "Very Low",
                >= -60 => "Minimal",
                >= -80 => "Negligible",
                _ => "Ultra Safe"
            };
        }
    }
}