using System.Windows;
using System.Windows.Controls;
using CorporateChaos.Models;

namespace CorporateChaos.Views
{
    public partial class DepartmentPanel : Window
    {
        private Department currentDepartment;
        private Dictionary<Department, DepartmentStats> departments;
        private List<Employee> allHiredEmployees;
        
        public event Action? EmployeesChanged;

        public DepartmentPanel(Department department, Dictionary<Department, DepartmentStats> departments, List<Employee> allHiredEmployees)
        {
            InitializeComponent();
            this.currentDepartment = department;
            this.departments = departments;
            this.allHiredEmployees = allHiredEmployees;
            
            InitializeDepartmentView();
            RefreshEmployeeLists();
        }

        private void InitializeDepartmentView()
        {
            // Set department title and icon
            string departmentIcon = currentDepartment switch
            {
                Department.Marketing => "📢",
                Department.Operations => "⚙️",
                Department.Finance => "💰",
                Department.HR => "👥",
                Department.IT => "💻",
                Department.Research => "🔬",
                _ => "🏢"
            };
            
            string departmentImagePath = currentDepartment switch
            {
                Department.Marketing => "images/marketing.png",
                Department.Operations => "images/operations.png",
                Department.Finance => "images/finance.png",
                Department.HR => "images/human_resources.png",
                Department.IT => "images/it.png",
                Department.Research => "images/research.png",
                _ => "images/logo.png"
            };
            
            DepartmentTitleText.Text = $"{departmentIcon} {currentDepartment.ToString().ToUpper()} DEPARTMENT";
            
            // Update department statistics
            var deptStats = departments[currentDepartment];
            double totalProductivity = deptStats.GetTotalProductivity();
            double quarterlyCost = deptStats.GetQuarterlyCost();
            int employeeCount = deptStats.GetEmployeeCount();
            double efficiency = deptStats.Efficiency;
            
            DepartmentStatsText.Text = $"Employees: {employeeCount} | Productivity: {totalProductivity:F1} | Efficiency: {efficiency:F1}% | Quarterly Cost: ${quarterlyCost:N0}";
        }

        private void RefreshEmployeeLists()
        {
            // Current department employees
            var departmentEmployees = departments[currentDepartment].Employees.ToList();
            DepartmentEmployeesItemsControl.ItemsSource = null;
            DepartmentEmployeesItemsControl.ItemsSource = departmentEmployees;
            
            // Available hired employees (not assigned to any department)
            var availableEmployees = allHiredEmployees.Where(e => !e.IsAssigned).ToList();
            AvailableEmployeesItemsControl.ItemsSource = null;
            AvailableEmployeesItemsControl.ItemsSource = availableEmployees;
            
            // Update department stats
            InitializeDepartmentView();
        }

        private void AssignBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Assign employee to current department
                employee.AssignedDepartment = currentDepartment;
                employee.IsAssigned = true;
                departments[currentDepartment].Employees.Add(employee);
                
                RefreshEmployeeLists();
                EmployeesChanged?.Invoke();
                
                // Show assignment confirmation without revealing specialization
                if (employee.Specialization == currentDepartment)
                {
                    MessageBox.Show($"✅ {employee.Name} assigned to {currentDepartment}!\n🎯 Great fit: This employee seems well-suited for this role!", 
                        "Employee Assigned", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"✅ {employee.Name} assigned to {currentDepartment}!\n💡 Monitor their performance to see how well they adapt to this role.", 
                        "Employee Assigned", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void TransferBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Show transfer options
                var transferDialog = new TransferDialog(employee, departments.Keys.Where(d => d != currentDepartment).ToList());
                transferDialog.Owner = this;
                
                if (transferDialog.ShowDialog() == true && transferDialog.SelectedDepartment.HasValue)
                {
                    var newDepartment = transferDialog.SelectedDepartment.Value;
                    
                    // Remove from current department
                    departments[currentDepartment].Employees.Remove(employee);
                    
                    // Add to new department
                    employee.AssignedDepartment = newDepartment;
                    departments[newDepartment].Employees.Add(employee);
                    
                    RefreshEmployeeLists();
                    EmployeesChanged?.Invoke();
                    
                    MessageBox.Show($"🔄 {employee.Name} transferred to {newDepartment}!", 
                        "Employee Transferred", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void FireBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Show confirmation dialog
                var result = MessageBox.Show(
                    $"Are you sure you want to fire {employee.Name}?\n\n" +
                    $"This action cannot be undone and will:\n" +
                    $"• Remove them from the company permanently\n" +
                    $"• Potentially affect team morale\n" +
                    $"• Save ${employee.Salary:N0}/month in salary costs\n\n" +
                    $"⚠️ WARNING: If this leaves you with zero employees, your company will fail immediately!",
                    "Confirm Employee Termination",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Check if this would leave the company with zero employees
                    int totalEmployees = allHiredEmployees.Count;
                    if (totalEmployees <= 1)
                    {
                        MessageBox.Show(
                            "❌ Cannot fire this employee!\n\n" +
                            "This would leave your company with zero employees, " +
                            "which would result in immediate business failure.\n\n" +
                            "You must maintain at least one employee to keep the company operational.",
                            "Cannot Fire Last Employee",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    // Remove from department if assigned
                    if (employee.AssignedDepartment.HasValue)
                    {
                        departments[employee.AssignedDepartment.Value].Employees.Remove(employee);
                    }

                    // Remove from hired employees list
                    allHiredEmployees.Remove(employee);

                    // Apply morale impact to remaining employees
                    ApplyFiringMoraleImpact(employee);

                    RefreshEmployeeLists();
                    EmployeesChanged?.Invoke();

                    MessageBox.Show(
                        $"🔥 {employee.Name} has been terminated.\n\n" +
                        $"Salary savings: ${employee.Salary:N0}/month\n" +
                        $"Remaining employees: {allHiredEmployees.Count}\n\n" +
                        $"Team morale may be affected by this decision.",
                        "Employee Terminated",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        private void ApplyFiringMoraleImpact(Employee firedEmployee)
        {
            // Calculate morale impact based on fired employee's characteristics
            int moraleImpact = -5; // Base impact

            // Higher impact for senior employees or those with high morale
            if (firedEmployee.OverallSkill >= SkillLevel.Senior)
                moraleImpact -= 3;
            if (firedEmployee.Morale >= 80)
                moraleImpact -= 2;

            // Apply to all remaining employees
            foreach (var dept in departments.Values)
            {
                foreach (var employee in dept.Employees)
                {
                    employee.Morale = Math.Max(10, employee.Morale + moraleImpact);
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Simple transfer dialog
    public class TransferDialog : Window
    {
        public Department? SelectedDepartment { get; private set; }
        
        public TransferDialog(Employee employee, List<Department> availableDepartments)
        {
            this.Title = $"Transfer {employee.Name}";
            this.Width = 300;
            this.Height = 400;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 46));
            
            // Create UI programmatically for simplicity
            var grid = new Grid();
            grid.Margin = new Thickness(20);
            
            var stackPanel = new StackPanel();
            
            var titleText = new TextBlock
            {
                Text = $"Select new department for {employee.Name}:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = System.Windows.Media.Brushes.White
            };
            stackPanel.Children.Add(titleText);
            
            foreach (var dept in availableDepartments)
            {
                var button = new Button
                {
                    Content = dept.ToString(),
                    Height = 40,
                    Margin = new Thickness(0, 0, 0, 10),
                    Tag = dept
                };
                button.Click += (s, e) =>
                {
                    SelectedDepartment = (Department)((Button)s).Tag;
                    DialogResult = true;
                    Close();
                };
                stackPanel.Children.Add(button);
            }
            
            var cancelButton = new Button
            {
                Content = "Cancel",
                Height = 40,
                Margin = new Thickness(0, 20, 0, 0)
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            stackPanel.Children.Add(cancelButton);
            
            grid.Children.Add(stackPanel);
            this.Content = grid;
        }
    }
}