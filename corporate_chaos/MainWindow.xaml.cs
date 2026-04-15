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
using System.ComponentModel;
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
        private double previousQuarterStartCapital;
        private DataManager dataManager = null!;
        private SaveLoadManager saveLoadManager = null!;
        private StoryModeManager storyModeManager = null!;
        private BackgroundMusicManager backgroundMusicManager = null!;
        private bool isEndlessMode = false; // Track if we're in endless mode
        
        // New systems for employee management
        private Dictionary<Department, DepartmentStats> departments = null!;
        private List<Employee> hiredEmployees = null!; // Changed from availableEmployees
        private GameRunRecord? currentGameRun = null;
        
        // Quarterly summary tracking
        private List<string> currentQuarterEvents = null!;
        private List<string> previousQuarterEvents = null!; // Events from the quarter that just ended
        private bool hasNewEvents = false;
        
        // Peak performance tracking
        private GameScore currentGameScore = null!;

        public MainWindow()
        {
            InitializeComponent();
            dataManager = new DataManager();
            saveLoadManager = new SaveLoadManager();
            
            // Initialize and start background music
            System.Diagnostics.Debug.WriteLine("Initializing background music manager...");
            backgroundMusicManager = new BackgroundMusicManager();
            
            // Load and apply saved settings
            LoadAndApplySettings();
            
            // Add window closing event handler for cleanup
            this.Closing += MainWindow_Closing;
            
            InitializeGame();
            
            // Update music toggle button and start music after UI is loaded
            this.Loaded += (s, e) => 
            {
                UpdateMusicToggleButton();
                // Start music with a small delay to ensure UI is fully loaded
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    backgroundMusicManager.StartBackgroundMusic();
                }), System.Windows.Threading.DispatcherPriority.Background);
            };
        }

        private void LoadAndApplySettings()
        {
            var settings = SettingsManager.LoadSettings();
            
            // Apply audio settings
            backgroundMusicManager.SetVolume(settings.IsMuted ? 0 : settings.Volume);
            
            // Apply display settings
            if (settings.IsFullscreen)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                this.Width = settings.WindowWidth;
                this.Height = settings.WindowHeight;
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Clean up background music
            backgroundMusicManager?.StopBackgroundMusic();
            backgroundMusicManager?.Dispose();
        }

        private void StartStoryMode()
        {
            try
            {
                InitializeGame(true);
                MainMenuGrid.Visibility = Visibility.Collapsed;
                CorporateGameGrid.Visibility = Visibility.Visible;
                StartCorporateGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting story mode: {ex}");
                MessageBox.Show($"Error starting Story Mode:\n\n{ex.Message}\n\n{ex.StackTrace}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeGame(bool isStoryMode = false)
        {
            var config = dataManager.GetConfig();
            company = new Company();
            
            chaos = new ChaosEngine();
            decisions = new DecisionSystem();
            gameLog = new StringBuilder();
            quarterNumber = 1;
            
            // Initialize Story Mode Manager
            storyModeManager = new StoryModeManager(company);
            
            if (isStoryMode)
            {
                storyModeManager.StartNewStoryMode();
            }
            else
            {
                // Ensure story mode is explicitly disabled for sandbox
                storyModeManager.ResetStoryMode();
            }
            
            // Initialize departments
            departments = new Dictionary<Department, DepartmentStats>();
            foreach (Department dept in Enum.GetValues<Department>())
            {
                departments[dept] = new DepartmentStats { Department = dept };
            }
            
            // Initialize hired employees list
            hiredEmployees = new List<Employee>();
            
            // Setup starting employees for story mode
            if (isStoryMode)
            {
                storyModeManager.SetupStartingEmployees(departments, hiredEmployees);
            }
            
            // Initialize quarterly events tracking
            currentQuarterEvents = new List<string>();
            previousQuarterEvents = new List<string>();
            
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
            
            // Load Joan's avatar
            LoadJoanAvatar();
            
            // Update UI with initial values
            UpdateUI();
        }

        private void LoadJoanAvatar()
        {
            try
            {
                // Try to load assistant.png first
                var assistantUri = new Uri("pack://application:,,,/images/assistant.png");
                var assistantImage = new BitmapImage();
                assistantImage.BeginInit();
                assistantImage.UriSource = assistantUri;
                assistantImage.CacheOption = BitmapCacheOption.OnLoad;
                assistantImage.EndInit();
                
                JoanMainAvatar.Source = assistantImage;
            }
            catch
            {
                try
                {
                    // Fallback to human_resources.png
                    var fallbackUri = new Uri("pack://application:,,,/images/human_resources.png");
                    var fallbackImage = new BitmapImage();
                    fallbackImage.BeginInit();
                    fallbackImage.UriSource = fallbackUri;
                    fallbackImage.CacheOption = BitmapCacheOption.OnLoad;
                    fallbackImage.EndInit();
                    
                    JoanMainAvatar.Source = fallbackImage;
                }
                catch
                {
                    // If all else fails, leave it empty
                }
            }
        }

        // Main Menu Navigation
        private void StoryModeBtn_Click(object sender, RoutedEventArgs e)
        {
            // Show development warning first
            var warningResult = Views.ModernMessageBox.Show(
                "Story Mode is currently under active development and expansion.\n\n" +
                "You may experience:\n" +
                "• Bugs and unexpected behavior\n" +
                "• Incomplete features or storylines\n" +
                "• Inconsistent gameplay mechanics\n" +
                "• Save file compatibility issues\n" +
                "• Missing dialogue or narrative elements\n\n" +
                "We recommend using Sandbox Mode for the most stable experience.\n\n" +
                "Do you still want to proceed with Story Mode?",
                "Story Mode - Development Warning",
                Views.ModernMessageBox.MessageBoxType.Warning,
                Views.ModernMessageBox.MessageBoxButtons.YesNo,
                this);

            if (warningResult != MessageBoxResult.Yes)
            {
                return; // User chose not to proceed
            }

            // Show the original Story Mode welcome message
            var result = Views.ModernMessageBox.Show(
                "In Story Mode, you'll learn corporate management through guided tutorials with Secretary Joan.\n\n" +
                "Features:\n" +
                "• Step-by-step tutorials over 8 quarters\n" +
                "• Gradual unlock of game mechanics\n" +
                "• Narrative-driven scenarios\n" +
                "• Personal guidance from Secretary Joan\n\n" +
                "Would you like to start Story Mode?",
                "📖 Welcome to Story Mode!",
                Views.ModernMessageBox.MessageBoxType.Question,
                Views.ModernMessageBox.MessageBoxButtons.YesNo,
                this);

            if (result == MessageBoxResult.Yes)
            {
                StartStoryMode();
            }
        }

        private void SandboxModeBtn_Click(object sender, RoutedEventArgs e)
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

        private void OptionsBtn_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow(backgroundMusicManager, this);
            optionsWindow.ShowDialog();
        }

        private void QuitGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = Views.ModernMessageBox.ShowQuestion(
                "Are you sure you want to quit Corporate Chaos?\n\nAny unsaved progress will be lost.",
                "Quit Game",
                this);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // Save Slots Navigation
        private void CorporateBtn_Click(object sender, RoutedEventArgs e)
        {
            isEndlessMode = false;
            SaveSlotsGrid.Visibility = Visibility.Collapsed;
            CorporateGameGrid.Visibility = Visibility.Visible;
            StartCorporateGame();
        }

        private void EndlessModeBtn_Click(object sender, RoutedEventArgs e)
        {
            isEndlessMode = true;
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
            InitializeControlKnobs(); // Initialize the new dynamic control knobs
            UpdateUI();
            UpdateCurrentSettings();
            gameLog.Clear();
            
            // Clear previous quarter events when starting a new game
            currentQuarterEvents.Clear();
            previousQuarterEvents.Clear();
            hasNewEvents = false;
            NewEventsIndicator.Visibility = Visibility.Collapsed;
            
            // Debug: Log the current mode status
            System.Diagnostics.Debug.WriteLine($"StartCorporateGame - IsStoryMode: {storyModeManager.IsStoryMode}, IsEndlessMode: {isEndlessMode}");
            
            if (storyModeManager.IsStoryMode)
            {
                gameLog.AppendLine("📖 === STORY MODE ACTIVATED ===");
                gameLog.AppendLine("Welcome to your corporate journey with Secretary Joan as your guide!");
                
                // Show initial story event
                if (storyModeManager.ShouldShowStoryEvent(quarterNumber))
                {
                    storyModeManager.ShowStoryGuide(quarterNumber, this);
                }
            }
            else
            {
                string modeText = isEndlessMode ? "ENDLESS MODE" : "CORPORATE CHALLENGE";
                gameLog.AppendLine($"🏢 === {modeText} ACTIVATED ===");
                gameLog.AppendLine("Welcome to your new corporate adventure! Make strategic decisions each quarter.");
                gameLog.AppendLine("💡 All features are unlocked - hire employees, make executive decisions, and build your empire!");
            }
            
            gameLog.AppendLine("💡 Tip: Assign employees to departments for maximum efficiency!");
            gameLog.AppendLine();
        }

        public void UpdateUI()
        {
            // Update header with retirement progress (unless endless mode)
            int yearsCompleted = (quarterNumber - 1) / 4;
            int currentQuarterInYear = ((quarterNumber - 1) % 4) + 1;
            
            if (isEndlessMode)
            {
                QuarterCounterText.Text = $"Y{yearsCompleted + 1} Q{currentQuarterInYear} (Endless)";
            }
            else
            {
                QuarterCounterText.Text = $"Y{yearsCompleted + 1} Q{currentQuarterInYear} ({quarterNumber}/120)";
            }
            
            HeaderCapitalText.Text = $"${company.Capital:N0}";
            
            // Update company stats with new -100 to 100 ranges
            CapitalText.Text = $"${company.Capital:N0}";
            MarketShareText.Text = $"{company.MarketShare:F1}%";
            ReputationText.Text = $"{company.Reputation} ({GetReputationDescription(company.Reputation)})";
            MoraleText.Text = $"{company.Morale} ({GetMoraleDescription(company.Morale)})";
            RiskText.Text = $"{company.Risk} ({GetRiskDescription(company.Risk)})";
            
            // Update stat progress bars
            UpdateStatBars();
            
            // Update employee count
            int totalEmployees = departments.Values.Sum(d => d.GetEmployeeCount());
            company.EmployeeCount = totalEmployees;
            EmployeeCountText.Text = totalEmployees.ToString();
            
            // Update hired employees count
            HiredEmployeesCountText.Text = $"Total Hired: {hiredEmployees.Count} employees";
            
            // Update quarterly financials - expenses include operational + crisis + decisions
            double totalExpenses = company.QuarterlyExpenses + company.NetLoss + company.DecisionExpenses;
            QuarterlyRevenueText.Text = $"${company.QuarterlyRevenue:N0}";
            QuarterlyExpensesText.Text = $"${totalExpenses:N0}";
            double netResult = company.QuarterlyRevenue - totalExpenses;
            if (netResult >= 0)
            {
                NetResultLabel.Text = "Net Profit";
                NetResultText.Text = $"${netResult:N0}";
                NetResultText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                NetResultLabel.Text = "Net Loss";
                NetResultText.Text = $"-${Math.Abs(netResult):N0}";
                NetResultText.Foreground = System.Windows.Media.Brushes.LightCoral;
            }
            
            // Update crisis status
            CrisisStatusText.Text = chaos.GetCrisisStatusSummary();
            
            // Update department button tooltips with employee counts
            UpdateDepartmentButtonTooltips();
            
            // Update progressive unlocking for story mode
            UpdateProgressiveUnlocking();
            
            // Update story progress UI (story mode only)
            UpdateStoryProgressUI();
            
            // Ensure values stay within bounds
            company.ClampValues();
        }

        // Public method for external classes to refresh UI state
        public void RefreshUI()
        {
            UpdateUI();
        }

        private void UpdateStoryProgressUI()
        {
            // Only show story progress panel in story mode
            if (storyModeManager == null || !storyModeManager.IsStoryMode)
            {
                StoryProgressPanel.Visibility = Visibility.Collapsed;
                return;
            }
            
            StoryProgressPanel.Visibility = Visibility.Visible;
            
            // Update current act
            var currentAct = storyModeManager.StoryData.CurrentAct;
            CurrentActText.Text = GetActDisplayName(currentAct);
            
            // Calculate act progress
            var (actStart, actEnd) = GetActQuarterRange(currentAct);
            int actQuarter = quarterNumber - actStart + 1;
            int actTotalQuarters = actEnd - actStart + 1;
            double progressPercentage = (double)actQuarter / actTotalQuarters;
            
            // Update progress bar
            ActProgressBar.Width = progressPercentage * 250; // 250 is approximate panel width minus padding
            ActProgressText.Text = $"Q{actQuarter} of {actTotalQuarters}";
            
            // Update character relationships summary
            UpdateCharacterRelationshipsSummary();
        }
        
        private string GetActDisplayName(NarrativeAct act)
        {
            return act switch
            {
                NarrativeAct.Tutorial => "I: Tutorial",
                NarrativeAct.RisingAction => "II: Rising Action",
                NarrativeAct.Climax => "III: Climax",
                NarrativeAct.Resolution => "IV: Resolution",
                _ => "Unknown"
            };
        }
        
        private (int start, int end) GetActQuarterRange(NarrativeAct act)
        {
            return act switch
            {
                NarrativeAct.Tutorial => (1, 10),
                NarrativeAct.RisingAction => (11, 60),
                NarrativeAct.Climax => (61, 100),
                NarrativeAct.Resolution => (101, 120),
                _ => (1, 10)
            };
        }
        
        private void UpdateCharacterRelationshipsSummary()
        {
            var relationships = storyModeManager.StoryData.CharacterRelationships;
            var summaryLines = new List<string>();
            
            // Always show Joan first
            if (relationships.ContainsKey("joan"))
            {
                var joanRel = relationships["joan"];
                var joanPhase = GetRelationshipPhaseDisplay(joanRel.CurrentPhase);
                summaryLines.Add($"Joan: {joanPhase}");
            }
            else
            {
                summaryLines.Add("Joan: Professional");
            }
            
            // Show up to 2 other introduced characters with highest relationship scores
            var otherCharacters = relationships
                .Where(kvp => kvp.Key != "joan" && storyModeManager.IsCharacterIntroduced(kvp.Key))
                .OrderByDescending(kvp => (kvp.Value.TrustLevel + kvp.Value.ProfessionalRespect + kvp.Value.PersonalConnection) / 3)
                .Take(2)
                .ToList();
            
            foreach (var character in otherCharacters)
            {
                var characterName = GetCharacterDisplayName(character.Key);
                var phase = GetRelationshipPhaseDisplay(character.Value.CurrentPhase);
                summaryLines.Add($"{characterName}: {phase}");
            }
            
            // Show count of other characters if there are more
            int totalIntroduced = StoryScript.Characters.Values
                .Count(c => storyModeManager.IsCharacterIntroduced(c.CharacterId));
            
            if (totalIntroduced > 3)
            {
                summaryLines.Add($"+{totalIntroduced - 3} more");
            }
            
            // Relationship summary removed - now accessible via Relationships button
        }
        
        private string GetRelationshipPhaseDisplay(RelationshipPhase phase)
        {
            return phase switch
            {
                RelationshipPhase.FirstMeeting => "New",
                RelationshipPhase.ProfessionalAcquaintance => "Professional",
                RelationshipPhase.TrustedColleague => "Trusted",
                RelationshipPhase.PersonalFriend => "Friend",
                RelationshipPhase.LifelongBond => "Close Friend",
                RelationshipPhase.Strained => "Strained",
                RelationshipPhase.Hostile => "Hostile",
                _ => "Unknown"
            };
        }
        
        private string GetCharacterDisplayName(string characterId)
        {
            if (StoryScript.Characters.ContainsKey(characterId))
            {
                return StoryScript.Characters[characterId].Name.Split(' ')[0]; // First name only for compact display
            }
            return characterId;
        }

        private void UpdateStatBars()
        {
            // Market share bar (0-100%)
            if (MarketShareBar.Parent is System.Windows.Controls.Grid msGrid && msGrid.ActualWidth > 0)
                MarketShareBar.Width = Math.Max(0, (company.MarketShare / 100.0) * msGrid.ActualWidth);
            
            // Reputation bar (-100 to 100, normalize to 0-1)
            if (ReputationBar.Parent is System.Windows.Controls.Grid repGrid && repGrid.ActualWidth > 0)
                ReputationBar.Width = Math.Max(0, ((company.Reputation + 100) / 200.0) * repGrid.ActualWidth);
            
            // Morale bar (-100 to 100, normalize to 0-1)
            if (MoraleBar.Parent is System.Windows.Controls.Grid morGrid && morGrid.ActualWidth > 0)
                MoraleBar.Width = Math.Max(0, ((company.Morale + 100) / 200.0) * morGrid.ActualWidth);
            
            // Risk bar (0 to 100)
            if (RiskBar.Parent is System.Windows.Controls.Grid riskGrid && riskGrid.ActualWidth > 0)
                RiskBar.Width = Math.Max(0, (company.Risk / 100.0) * riskGrid.ActualWidth);
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

        private void UpdateProgressiveUnlocking()
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"UpdateProgressiveUnlocking - IsStoryMode: {storyModeManager?.IsStoryMode ?? false}");
            
            // In sandbox/endless mode, all buttons are always enabled
            if (storyModeManager == null || !storyModeManager.IsStoryMode)
            {
                HireEmployeesBtn.IsEnabled = true;
                ExecutiveDecisionsBtn.IsEnabled = true;
                HireEmployeesBtn.Opacity = 1.0;
                ExecutiveDecisionsBtn.Opacity = 1.0;
                HireEmployeesBtn.Content = "🎯 Hire New Employees";
                ExecutiveDecisionsBtn.Content = "📈 Executive Decisions";
                HireEmployeesBtn.ToolTip = "Hire new employees for your departments";
                ExecutiveDecisionsBtn.ToolTip = "Make strategic executive decisions";
                
                // Hide Characters button in sandbox mode
                if (CharactersBtn != null)
                {
                    CharactersBtn.Visibility = Visibility.Collapsed;
                }
                
                // Hide Relationships button in sandbox mode
                if (RelationshipsBtn != null)
                {
                    RelationshipsBtn.Visibility = Visibility.Collapsed;
                }
                
                System.Diagnostics.Debug.WriteLine("Sandbox mode - all buttons enabled");
                return;
            }

            // Progressive unlocking for story mode only
            bool hiringUnlocked = storyModeManager.IsMechanicUnlocked(MechanicType.EmployeeHiring);
            bool executiveUnlocked = storyModeManager.IsMechanicUnlocked(MechanicType.ExecutiveDecisions);

            System.Diagnostics.Debug.WriteLine($"Story mode - Quarter {quarterNumber} - Hiring unlocked: {hiringUnlocked}, Executive unlocked: {executiveUnlocked}");
            System.Diagnostics.Debug.WriteLine($"Unlocked mechanics: {string.Join(", ", storyModeManager.StoryData.UnlockedMechanics)}");

            HireEmployeesBtn.IsEnabled = hiringUnlocked;
            ExecutiveDecisionsBtn.IsEnabled = executiveUnlocked;
            
            // Visual feedback for locked buttons
            HireEmployeesBtn.Opacity = hiringUnlocked ? 1.0 : 0.4;
            ExecutiveDecisionsBtn.Opacity = executiveUnlocked ? 1.0 : 0.4;

            // Update button content to show lock status
            if (!hiringUnlocked)
            {
                HireEmployeesBtn.Content = "🔒 Hire New Employees";
                HireEmployeesBtn.ToolTip = "🔒 Unlocks in Quarter 2 - Employee Hiring Tutorial";
            }
            else
            {
                HireEmployeesBtn.Content = "🎯 Hire New Employees";
                HireEmployeesBtn.ToolTip = "Hire new employees for your departments";
            }

            if (!executiveUnlocked)
            {
                ExecutiveDecisionsBtn.Content = "🔒 Executive Decisions";
                ExecutiveDecisionsBtn.ToolTip = "🔒 Unlocks in Quarter 4 - Executive Decisions Tutorial";
            }
            else
            {
                ExecutiveDecisionsBtn.Content = "📈 Executive Decisions";
                ExecutiveDecisionsBtn.ToolTip = "Make strategic executive decisions";
            }
            
            // Show Characters button if any character besides Joan has been introduced
            if (CharactersBtn != null)
            {
                bool hasIntroducedCharacters = StoryScript.Characters.Values
                    .Any(c => c.CharacterId != "joan" && storyModeManager.IsCharacterIntroduced(c.CharacterId));
                
                CharactersBtn.Visibility = hasIntroducedCharacters ? Visibility.Visible : Visibility.Collapsed;
                
                if (hasIntroducedCharacters)
                {
                    int characterCount = StoryScript.Characters.Values
                        .Count(c => c.CharacterId != "joan" && storyModeManager.IsCharacterIntroduced(c.CharacterId));
                    CharactersBtn.ToolTip = $"Talk to characters you've met ({characterCount} available)";
                }
            }
            
            // Show Relationships button in story mode
            if (RelationshipsBtn != null)
            {
                RelationshipsBtn.Visibility = Visibility.Visible;
            }
        }

        private void UpdateCurrentSettings()
        {
            // This method can be removed or simplified since we removed the settings display
        }

        // Control Knob Event Handlers - Dynamic Button System
        private void RiskAppetite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && company != null)
            {
                var riskType = button.Tag.ToString()!;
                company.RiskAppetite = Enum.Parse<RiskAppetite>(riskType);
                UpdateControlKnobVisuals("Risk", riskType);
            }
        }

        private void BudgetAllocation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && company != null)
            {
                var budgetLevel = button.Tag.ToString()!;
                company.BudgetAllocation = Enum.Parse<InvestmentLevel>(budgetLevel);
                UpdateControlKnobVisuals("Budget", budgetLevel);
            }
        }

        private void MarketStrategy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && company != null)
            {
                var strategy = button.Tag.ToString()!;
                company.MarketStrategy = Enum.Parse<MarketStrategy>(strategy);
                UpdateControlKnobVisuals("Market", strategy);
            }
        }

        private void CrisisResponse_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && company != null)
            {
                var response = button.Tag.ToString()!;
                company.CrisisResponse = Enum.Parse<CrisisResponse>(response);
                UpdateControlKnobVisuals("Crisis", response);
            }
        }

        private void UpdateControlKnobVisuals(string category, string selectedValue)
        {
            // Update button appearances to show selection
            var activeColor = System.Windows.Media.Brushes.Green;
            var activeBorderColor = System.Windows.Media.Brushes.LightGreen;
            var inactiveColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 74, 106));
            var inactiveBorderColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(106, 106, 138));

            switch (category)
            {
                case "Risk":
                    RiskConservativeBtn.Background = selectedValue == "Conservative" ? activeColor : inactiveColor;
                    RiskConservativeBtn.BorderBrush = selectedValue == "Conservative" ? activeBorderColor : inactiveBorderColor;
                    RiskBalancedBtn.Background = selectedValue == "Balanced" ? activeColor : inactiveColor;
                    RiskBalancedBtn.BorderBrush = selectedValue == "Balanced" ? activeBorderColor : inactiveBorderColor;
                    RiskAggressiveBtn.Background = selectedValue == "Aggressive" ? activeColor : inactiveColor;
                    RiskAggressiveBtn.BorderBrush = selectedValue == "Aggressive" ? activeBorderColor : inactiveBorderColor;
                    break;
                case "Budget":
                    BudgetLowBtn.Background = selectedValue == "Low" ? activeColor : inactiveColor;
                    BudgetLowBtn.BorderBrush = selectedValue == "Low" ? activeBorderColor : inactiveBorderColor;
                    BudgetMediumBtn.Background = selectedValue == "Medium" ? activeColor : inactiveColor;
                    BudgetMediumBtn.BorderBrush = selectedValue == "Medium" ? activeBorderColor : inactiveBorderColor;
                    BudgetHighBtn.Background = selectedValue == "High" ? activeColor : inactiveColor;
                    BudgetHighBtn.BorderBrush = selectedValue == "High" ? activeBorderColor : inactiveBorderColor;
                    break;
                case "Market":
                    MarketCostBtn.Background = selectedValue == "Cost" ? activeColor : inactiveColor;
                    MarketCostBtn.BorderBrush = selectedValue == "Cost" ? activeBorderColor : inactiveBorderColor;
                    MarketQualityBtn.Background = selectedValue == "Quality" ? activeColor : inactiveColor;
                    MarketQualityBtn.BorderBrush = selectedValue == "Quality" ? activeBorderColor : inactiveBorderColor;
                    MarketInnovationBtn.Background = selectedValue == "Innovation" ? activeColor : inactiveColor;
                    MarketInnovationBtn.BorderBrush = selectedValue == "Innovation" ? activeBorderColor : inactiveBorderColor;
                    break;
                case "Crisis":
                    CrisisImmediateBtn.Background = selectedValue == "Immediate" ? activeColor : inactiveColor;
                    CrisisImmediateBtn.BorderBrush = selectedValue == "Immediate" ? activeBorderColor : inactiveBorderColor;
                    CrisisControlBtn.Background = selectedValue == "Control" ? activeColor : inactiveColor;
                    CrisisControlBtn.BorderBrush = selectedValue == "Control" ? activeBorderColor : inactiveBorderColor;
                    CrisisAbsorbBtn.Background = selectedValue == "Absorb" ? activeColor : inactiveColor;
                    CrisisAbsorbBtn.BorderBrush = selectedValue == "Absorb" ? activeBorderColor : inactiveBorderColor;
                    break;
            }
        }

        private void InitializeControlKnobs()
        {
            // Set default selections
            UpdateControlKnobVisuals("Risk", "Balanced");
            UpdateControlKnobVisuals("Budget", "Medium");
            UpdateControlKnobVisuals("Market", "Quality");
            UpdateControlKnobVisuals("Crisis", "Control");
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
            var quarterlySummary = new QuarterlySummary(quarterNumber - 1, company, departments, previousQuarterEvents);
            quarterlySummary.Owner = this;
            quarterlySummary.ShowDialog();
        }

        private void FinancialReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (quarterNumber <= 1)
            {
                Views.ModernMessageBox.ShowInformation(
                    "No financial data available yet.\n\nComplete your first quarter to see the financial report.",
                    "No Data", this);
                return;
            }

            var report = new Views.FinancialReport(quarterNumber - 1, company, departments, previousQuarterStartCapital);
            report.Owner = this;
            report.ShowDialog();
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
                Views.ModernMessageBox.ShowSuccess(
                    $"Your game has been saved successfully!\n\nFile: {gameSave.GetFileName()}", 
                    "Save Successful",
                    this);
            }
            else
            {
                Views.ModernMessageBox.ShowError(
                    "Failed to save game! Please check disk space and permissions.", 
                    "Save Error",
                    this);
            }
        }

        private void LoadGameState(GameSave gameSave)
        {
            quarterNumber = gameSave.CurrentQuarter;
            company = gameSave.Company;
            departments = gameSave.Departments;
            hiredEmployees = gameSave.AvailableEmployees; // Load as hired employees
            
            // Fix legacy employees that don't have gender/profile images
            foreach (var employee in hiredEmployees)
            {
                employee.EnsureProfileData();
            }
            
            // Also fix employees in departments
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.EnsureProfileData();
                }
            }
            
            gameLog = new StringBuilder();
            foreach (var eventText in gameSave.GameEvents)
            {
                gameLog.AppendLine(eventText);
            }
            
            // Initialize event tracking for loaded games
            currentQuarterEvents.Clear();
            previousQuarterEvents.Clear();
            
            // Update UI after loading
            UpdateUI();
        }

        // Secretary Joan Dialogue
        private void JoanDialogueBtn_Click(object sender, RoutedEventArgs e)
        {
            // Show a choice between traditional and branching dialogue
            var result = MessageBox.Show("Which dialogue mode would you like to try?\n\n" +
                                       "Yes = Traditional Joan Dialogue\n" +
                                       "No = New Branching Conversation (Demo)\n" +
                                       "Cancel = Cancel",
                                       "Joan Dialogue Mode", 
                                       MessageBoxButton.YesNoCancel, 
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Use adaptive dialogue if available, otherwise traditional
                if (storyModeManager?.ShouldUseAdaptiveDialogue() == true)
                {
                    storyModeManager.ShowJoanAdaptiveDialogue(departments, this, "user_requested");
                }
                else
                {
                    // Traditional dialogue
                    var joanDialogue = new JoanDialogue(company, departments, storyModeManager?.IsStoryMode ?? false, quarterNumber, storyModeManager);
                    joanDialogue.Owner = this;
                    joanDialogue.ShowDialog();
                }
            }
            else if (result == MessageBoxResult.No)
            {
                // New branching dialogue demo
                JoanDialogue.ShowBranchingDialogueExample(company, departments, quarterNumber, this);
            }
        }

        // Character Interactions
        private void CharactersBtn_Click(object sender, RoutedEventArgs e)
        {
            if (storyModeManager == null || !storyModeManager.IsStoryMode)
            {
                MessageBox.Show("Character interactions are only available in Story Mode.", 
                              "Story Mode Only", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            var characterWindow = new Views.CharacterInteractionWindow(company, storyModeManager, quarterNumber);
            characterWindow.Owner = this;
            characterWindow.ShowDialog();
        }

        // Relationships Window
        private void RelationshipsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (storyModeManager == null || !storyModeManager.IsStoryMode)
            {
                MessageBox.Show("Relationships are only available in Story Mode.", 
                              "Story Mode Only", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
                return;
            }

            var relationshipsWindow = new Views.RelationshipsWindow(
                storyModeManager.StoryData.CharacterRelationships,
                StoryScript.Characters
            );
            relationshipsWindow.Owner = this;
            relationshipsWindow.ShowDialog();
        }

        // Music Toggle
        private void MusicToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            backgroundMusicManager?.ToggleMute();
            UpdateMusicToggleButton();
        }

        private void UpdateMusicToggleButton()
        {
            if (backgroundMusicManager != null && MusicToggleBtn != null)
            {
                if (backgroundMusicManager.IsMuted())
                {
                    MusicToggleBtn.Content = "🔇";
                    MusicToggleBtn.ToolTip = "Background music is muted - click to unmute";
                }
                else
                {
                    MusicToggleBtn.Content = "🔊";
                    MusicToggleBtn.ToolTip = "Background music is playing - click to mute";
                }
            }
        }

        // Quarter End Processing
        private void EndQuarterBtn_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous quarter events (from 2 quarters ago) when starting a new quarter
            previousQuarterEvents.Clear();
            
            // Show Joan's end-of-quarter dialogue first
            ShowJoanEndQuarterDialogue();
            ProcessQuarterEnd();
        }

        private void ShowJoanEndQuarterDialogue()
        {
            // Use adaptive dialogue if available, otherwise traditional
            if (storyModeManager?.ShouldUseAdaptiveDialogue() == true)
            {
                storyModeManager.ShowJoanAdaptiveDialogue(departments, this, "quarterly_review");
            }
            else
            {
                var joanDialogue = new JoanDialogue(company, departments, storyModeManager?.IsStoryMode ?? false, quarterNumber, storyModeManager);
                joanDialogue.Owner = this;
                joanDialogue.ShowDialog();
            }
        }

        private void ProcessQuarterEnd()
        {
            // Capture starting capital before processing
            previousQuarterStartCapital = company.Capital;
            
            // Process quarterly financials
            company.ProcessQuarterlyFinancials(departments);
            
            // Apply story mode specific events and get the event descriptions
            if (storyModeManager.IsStoryMode)
            {
                var storyEvents = storyModeManager.ProcessStoryModeEvents(quarterNumber, departments);
                currentQuarterEvents.AddRange(storyEvents);
                
                // Log story events
                foreach (var eventText in storyEvents)
                {
                    LogEvent($"📖 {eventText}");
                }
                
                // Trigger choice consequences for this quarter
                storyModeManager.TriggerChoiceConsequences(quarterNumber);
                
                // Generate and display narrative events (post-tutorial)
                if (!storyModeManager.IsInTutorial && storyModeManager.NarrativeEngine != null)
                {
                    ProcessNarrativeEvents(quarterNumber);
                }
            }
            
            // Apply the chaotic events system
            // In story mode: only apply chaos after tutorial (Q10+)
            // In sandbox mode: always apply chaos from the start
            bool isInStoryTutorial = storyModeManager.IsStoryMode && storyModeManager.IsInTutorial;
            var chaosEvents = chaos.ApplyQuarterlyChaos(company, departments, isInStoryTutorial, quarterNumber);
            
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
            
            // Move current quarter events to previous quarter events (for next quarter's summary)
            previousQuarterEvents.Clear();
            previousQuarterEvents.AddRange(currentQuarterEvents);
            currentQuarterEvents.Clear();
            
            // Show story guide for current quarter BEFORE incrementing quarter number
            if (storyModeManager.ShouldShowStoryEvent(quarterNumber))
            {
                storyModeManager.ShowStoryGuide(quarterNumber, this);
            }
            
            quarterNumber++;
            
            // Reset hiring refreshes for the new quarter
            if (company.LastRefreshQuarter != quarterNumber)
            {
                company.CurrentQuarterRefreshes = 0;
                company.LastRefreshQuarter = quarterNumber;
            }
            
            // Complete quarter in story mode
            if (storyModeManager.IsStoryMode)
            {
                storyModeManager.CompleteQuarter(quarterNumber);
                
                // Check for act transitions and show narrative events
                ProcessActTransitionEvents(quarterNumber);
            }
            
            UpdateUI();
            
            // Show quarterly summary with events from the quarter that just ended
            ShowQuarterlySummary();
            
            // Show new events indicator for the events that just happened
            hasNewEvents = true;
            NewEventsIndicator.Visibility = Visibility.Visible;
            
            // Check for game over conditions
            
            // Track bankruptcy quarters
            if (company.Capital < 0)
            {
                company.ConsecutiveNegativeQuarters++;
            }
            else
            {
                company.ConsecutiveNegativeQuarters = 0; // Reset if capital is positive
            }
            
            // Bankruptcy condition: 2 consecutive quarters of negative capital
            if (company.ConsecutiveNegativeQuarters >= 2)
            {
                HandleGameOver("Bankruptcy - Company declared bankruptcy after 2 consecutive quarters of negative capital");
            }
            // Win condition 1: 65% market share
            else if (company.MarketShare >= 65)
            {
                HandleGameOver("Victory - Market Dominance Achieved (65% Market Share)");
            }
            // Win condition 2: $1 billion capital with sell company option
            else if (company.Capital >= 1000000000) // $1 billion
            {
                HandleBillionaireWin();
            }
            // Lose condition: No employees left
            else if (company.EmployeeCount <= 0 && quarterNumber > 1) // Allow Q1 to have 0 employees
            {
                HandleGameOver("Business Failure - No employees left to run the company");
            }
            // Retirement condition (unchanged)
            else if (!isEndlessMode && quarterNumber > 120)
            {
                HandleGameOver("Retirement - You've reached the end of your 30-year career!");
            }
            
            LogEvent($"📅 Quarter {quarterNumber} begins! Use the hiring panel to recruit new talent.");
        }

        private void ProcessNarrativeEvents(int quarter)
        {
            // Get distributed narrative events for this quarter
            var narrativeEvents = storyModeManager.NarrativeEngine!.GenerateDistributedEventsForQuarter(quarter);
            
            // Display each narrative event to the player
            foreach (var narrativeEvent in narrativeEvents)
            {
                ShowNarrativeEvent(narrativeEvent);
            }
        }

        private void ShowNarrativeEvent(NarrativeEvent narrativeEvent)
        {
            // Skip if this event has already been completed
            if (storyModeManager.StoryData.CompletedStoryEvents.Contains(narrativeEvent.EventId))
                return;

            // Create a dialogue conversation for the narrative event
            var conversation = new DialogueConversation
            {
                ConversationId = narrativeEvent.EventId,
                Title = narrativeEvent.Title,
                Participants = new List<string> { "player" },
                StartNodeId = "event_intro",
                CurrentNodeId = "event_intro"
            };

            // Add all involved characters to participants
            foreach (var characterId in narrativeEvent.InvolvedCharacters)
            {
                if (!conversation.Participants.Contains(characterId))
                {
                    conversation.Participants.Add(characterId);
                }
            }

            // Create the initial dialogue node
            var initialNode = new DialogueNode
            {
                NodeId = "event_intro",
                CharacterId = narrativeEvent.InvolvedCharacters.FirstOrDefault() ?? "joan",
                DialogueText = string.Join("\n\n", narrativeEvent.Dialogue),
                EmotionalTone = EmotionalTone.Neutral, // Default tone for narrative events
                Choices = narrativeEvent.Choices
            };

            conversation.Nodes.Add("event_intro", initialNode);

            // Show the dialogue using JoanDialogue
            var dialogue = new JoanDialogue(
                company,
                departments,
                conversation,
                storyModeManager.StoryData.CharacterRelationships,
                storyModeManager.StoryData.StoryFlags,
                true,
                quarterNumber,
                storyModeManager
            );

            dialogue.Owner = this;
            dialogue.Title = narrativeEvent.Title;
            dialogue.ShowDialog();

            // Log the narrative event
            string eventTypeIcon = narrativeEvent.EventType switch
            {
                NarrativeEventType.CharacterIntroduction => "👋",
                NarrativeEventType.RelationshipMilestone => "💫",
                NarrativeEventType.PersonalChallenge => "💭",
                NarrativeEventType.BusinessConflict => "⚔️",
                NarrativeEventType.EmotionalBeat => "❤️",
                NarrativeEventType.ChoiceConsequence => "🔄",
                NarrativeEventType.ActTransition => "🎭",
                NarrativeEventType.EndingSetup => "🎬",
                _ => "📖"
            };

            LogEvent($"{eventTypeIcon} STORY EVENT: {narrativeEvent.Title}");
            if (!string.IsNullOrEmpty(narrativeEvent.Description))
            {
                LogEvent($"   {narrativeEvent.Description}");
            }

            // Mark the event as completed
            storyModeManager.StoryData.CompletedStoryEvents.Add(narrativeEvent.EventId);
        }

        private void ProcessActTransitionEvents(int quarter)
        {
            // Check if this is an act transition quarter (Q11, Q61, Q101)
            if (quarter != 11 && quarter != 61 && quarter != 101)
                return;

            // Get the narrative engine from story mode manager
            if (storyModeManager?.CharacterManager == null)
                return;

            var narrativeEngine = new NarrativeEngine(
                storyModeManager.StoryData,
                company,
                storyModeManager.CharacterManager
            );

            // Generate act transition events
            var actTransitionEvents = narrativeEngine.GenerateEventsForQuarter(quarter)
                .Where(e => e.EventType == NarrativeEventType.ActTransition)
                .ToList();

            // Display each act transition event to the player
            foreach (var transitionEvent in actTransitionEvents)
            {
                ShowActTransitionEvent(transitionEvent);
            }
        }

        private void ShowActTransitionEvent(NarrativeEvent transitionEvent)
        {
            // Create a dialogue conversation for the act transition
            var conversation = new DialogueConversation
            {
                ConversationId = transitionEvent.EventId,
                Title = transitionEvent.Title,
                Participants = new List<string> { "player", "joan" },
                StartNodeId = "transition_intro",
                CurrentNodeId = "transition_intro"
            };

            // Create the initial dialogue node
            var initialNode = new DialogueNode
            {
                NodeId = "transition_intro",
                CharacterId = "joan",
                DialogueText = string.Join("\n\n", transitionEvent.Dialogue),
                EmotionalTone = EmotionalTone.Serious,
                Choices = transitionEvent.Choices
            };

            conversation.Nodes.Add("transition_intro", initialNode);

            // Show the dialogue using JoanDialogue
            var dialogue = new JoanDialogue(
                company,
                departments,
                conversation,
                storyModeManager.StoryData.CharacterRelationships,
                storyModeManager.StoryData.StoryFlags,
                true,
                quarterNumber,
                storyModeManager
            );

            dialogue.Owner = this;
            dialogue.Title = transitionEvent.Title;
            dialogue.ShowDialog();

            // Log the act transition
            LogEvent($"🎭 ACT TRANSITION: {transitionEvent.Title}");
            LogEvent($"   {transitionEvent.Description}");

            // Mark the event as completed
            storyModeManager.StoryData.CompletedStoryEvents.Add(transitionEvent.EventId);
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
                    double totalExp = company.QuarterlyExpenses + company.NetLoss + company.DecisionExpenses;
                    if (company.QuarterlyRevenue > totalExp)
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

        private void HandleBillionaireWin()
        {
            var result = MessageBox.Show(
                "🎉 CONGRATULATIONS! 🎉\n\n" +
                $"Your company has reached ${company.Capital:N0} in capital!\n\n" +
                "A major conglomerate has approached you with an acquisition offer. " +
                "They're willing to buy your company for a premium price, making you incredibly wealthy.\n\n" +
                "Do you want to sell your company and retire as a billionaire, or continue building your empire?\n\n" +
                "💰 SELL: Retire with massive wealth (Victory)\n" +
                "🏢 CONTINUE: Keep building your business empire",
                "Billionaire Decision", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                HandleGameOver("Victory - Sold company to conglomerate and retired as a billionaire");
            }
            else
            {
                // Continue playing - just log the event
                LogEvent("🏢 MAJOR DECISION: Declined acquisition offer - continuing to build the empire!");
                MessageBox.Show(
                    "You've chosen to continue building your empire!\n\n" +
                    "The conglomerate respects your decision. Your company continues to grow, " +
                    "and you remain in control of your destiny.\n\n" +
                    "💡 You can still win by reaching 70% market share or retire at quarter 120.",
                    "Empire Builder", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
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
                >= 10 => "Moderate",
                >= 5 => "Low",
                >= 1 => "Very Low",
                _ => "Minimal"
            };
        }
    }
}