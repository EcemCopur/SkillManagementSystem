using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Services;
using SkillManagementSystem.Services.DTOs;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class CapabilityEnhancementForm : Form
    {
        private DataManager dataManager;
        private CapabilityEnhancementService analysisService;
        private CapabilityEnhancementResult lastResult;

        private NumericUpDown numTrainingCost;
        private Button btnAnalyze;
        private DataGridView gridProcessGaps;
        private Panel detailsPanel;
        private Label lblSummary;
        private TabControl tabDetails;

        public CapabilityEnhancementForm(DataManager manager)
        {
            dataManager = manager;
            analysisService = new CapabilityEnhancementService(dataManager);

            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Capability Enhancement Analysis";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top Panel
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var titleLabel = new Label
            {
                Text = "Capability Enhancement - Process Skill Gaps",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            topPanel.Controls.Add(titleLabel);

            var lblInfo = new Label
            {
                Text = "Identifies processes that cannot be performed due to lack of skilled workers.",
                Location = new Point(10, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };
            topPanel.Controls.Add(lblInfo);

            int y = 75;
            AddLabel(topPanel, "Training Cost per Level:", 10, y);
            numTrainingCost = new NumericUpDown
            {
                Location = new Point(180, y),
                Width = 100,
                Minimum = 0,
                Maximum = 100000,
                Value = 5000,
                Increment = 500
            };
            topPanel.Controls.Add(numTrainingCost);

            btnAnalyze = new Button
            {
                Text = "Analyze All Processes",
                Location = new Point(300, y - 5),
                Width = 180,
                Height = 35,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAnalyze.Click += BtnAnalyze_Click;
            topPanel.Controls.Add(btnAnalyze);

            this.Controls.Add(topPanel);

            // Main Content Area - Split Panel for better layout
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(contentPanel);

            // Summary Label
            lblSummary = new Label
            {
                Location = new Point(10, 130),
                Width = 1360,
                Height = 40,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.LightYellow,
                Padding = new Padding(10),
                Text = "Click 'Analyze All Processes' to start analysis..."
            };
            contentPanel.Controls.Add(lblSummary);

            // Process Gaps Grid
            gridProcessGaps = new DataGridView
            {
                Location = new Point(10, 180),
                Width = 1360,
                Height = 320,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            gridProcessGaps.SelectionChanged += GridProcessGaps_SelectionChanged;
            contentPanel.Controls.Add(gridProcessGaps);

            // Details Panel with Tabs
            detailsPanel = new Panel
            {
                Location = new Point(10, 510),
                Width = 1360,
                Height = 340,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            contentPanel.Controls.Add(detailsPanel);

            ShowWelcomeMessage();
        }

        private void GridProcessGaps_SelectionChanged(object sender, EventArgs e)
        {
            if (gridProcessGaps.SelectedRows.Count == 0) return;

            var selectedRow = gridProcessGaps.SelectedRows[0];
            var processName = selectedRow.Cells["ProcessName"].Value.ToString();
            ShowProcessDetails(processName);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                AutoSize = true
            };
            parent.Controls.Add(label);
        }

        private void ShowWelcomeMessage()
        {
            detailsPanel.Controls.Clear();
            var welcomeLabel = new Label
            {
                Text = "Click 'Analyze All Processes' to identify capability gaps and training opportunities.",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(20, 180),
                ForeColor = Color.Gray
            };
            detailsPanel.Controls.Add(welcomeLabel);
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                btnAnalyze.Enabled = false;
                btnAnalyze.Text = "Analyzing...";
                Application.DoEvents();

                decimal trainingCost = numTrainingCost.Value;
                lastResult = analysisService.AnalyzeCapabilityGaps(trainingCost);
                DisplayResults(lastResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAnalyze.Enabled = true;
                btnAnalyze.Text = "Analyze All Processes";
            }
        }

        private void DisplayResults(CapabilityEnhancementResult result)
        {
            // Update summary
            lblSummary.Text = $"Found {result.ProcessGaps.Count} processes with capability issues: " +
                             $"{result.CriticalProcessesCount} Critical | " +
                             $"{result.HighRiskProcessesCount} High Risk | " +
                             $"{result.BelowTargetProcessesCount} Below Target";

            // Display process gaps
            var gridData = result.ProcessGaps.Select(g => new
            {
                Priority = g.Priority,
                ProcessName = g.Process.Name,
                Status = g.PriorityReason,
                Capable = g.CapableWorkersCount,
                Target = g.AimedWorkersCount,
                Gap = g.WorkerGap,
                Positions = string.Join(", ", g.AssignedPositionNames),
                MissingSkills = g.MissingSkills.Count,
                Trainings = g.SuggestedTrainings.Count,
                QuickFix = g.QuickestFixEmployees.Count,
                CheapFix = g.CheapestFixEmployees.Count
            }).ToList();

            gridProcessGaps.DataSource = gridData;

            // Color code by priority
            foreach (DataGridViewRow row in gridProcessGaps.Rows)
            {
                int priority = (int)row.Cells["Priority"].Value;
                switch (priority)
                {
                    case 1: row.DefaultCellStyle.BackColor = Color.LightCoral; break;
                    case 2: row.DefaultCellStyle.BackColor = Color.LightSalmon; break;
                    case 3: row.DefaultCellStyle.BackColor = Color.LightYellow; break;
                }
            }

            if (result.ProcessGaps.Any())
            {
                gridProcessGaps.Rows[0].Selected = true;
            }
        }

        private void ShowProcessDetails(string processName)
        {
            if (lastResult == null) return;

            var gap = lastResult.ProcessGaps.FirstOrDefault(g => g.Process.Name == processName);
            if (gap == null) return;

            detailsPanel.Controls.Clear();

            tabDetails = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            detailsPanel.Controls.Add(tabDetails);

            // Tab 1: Missing Skills
            var tabMissingSkills = new TabPage("Missing Skills");
            var txtMissingSkills = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9) };
            txtMissingSkills.AppendText($"Missing Skills Analysis for: {gap.Process.Name}\n");
            txtMissingSkills.AppendText(new string('=', 60) + "\n\n");

            foreach (var skill in gap.MissingSkills)
            {
                txtMissingSkills.AppendText($"• {skill.SkillName} (Required Level: {skill.RequiredLevel})\n");
                txtMissingSkills.AppendText($"  - Employees with this skill: {skill.EmployeesWithSkill}\n");
                txtMissingSkills.AppendText($"  - At required level: {skill.EmployeesAtRequiredLevel}\n\n");
            }
            tabMissingSkills.Controls.Add(txtMissingSkills);
            tabDetails.TabPages.Add(tabMissingSkills);

            // Tab 2: Suggested Trainings
            var tabTrainings = new TabPage("Suggested Trainings");
            var txtTrainings = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9) };
            txtTrainings.AppendText($"Training Suggestions for: {gap.Process.Name}\n");
            txtTrainings.AppendText(new string('=', 60) + "\n\n");

            foreach (var training in gap.SuggestedTrainings)
            {
                txtTrainings.AppendText($"• {training.Training.Name}\n");
                txtTrainings.AppendText($"  - Target Department: {training.TargetDepartmentName}\n");
                txtTrainings.AppendText($"  - Eligible Employees: {training.EligibleEmployeesCount}\n");
                txtTrainings.AppendText($"  - Cost: {training.Training.Cost:C} ({training.Training.CostType})\n");
                txtTrainings.AppendText($"  - Duration: {training.Training.DurationHours} hours\n");
                txtTrainings.AppendText($"  - Addresses skills: {string.Join(", ", training.SkillsItAddresses.Select(id => dataManager.Skills.FirstOrDefault(s => s.Id == id)?.Name ?? "Unknown"))}\n\n");
            }
            tabTrainings.Controls.Add(txtTrainings);
            tabDetails.TabPages.Add(tabTrainings);

            // Tab 3: Quickest Fix Employees
            var tabQuick = new TabPage($"Quickest Fix ({gap.QuickestFixEmployees.Count})");
            var gridQuick = CreateEmployeeSuggestionGrid(gap.QuickestFixEmployees, lastResult.TrainingCostPerLevel);
            tabQuick.Controls.Add(gridQuick);
            tabDetails.TabPages.Add(tabQuick);

            // Tab 4: Cheapest Fix Employees
            var tabCheap = new TabPage($"Cheapest Fix ({gap.CheapestFixEmployees.Count})");
            var gridCheap = CreateEmployeeSuggestionGrid(gap.CheapestFixEmployees, lastResult.TrainingCostPerLevel);
            tabCheap.Controls.Add(gridCheap);
            tabDetails.TabPages.Add(tabCheap);
        }

        private DataGridView CreateEmployeeSuggestionGrid(System.Collections.Generic.List<EmployeeSuggestion> suggestions, decimal trainingCostPerLevel)
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            var gridData = suggestions.Select(s => new
            {
                Employee = s.Employee.FullName,
                Position = s.CurrentPositionName,
                Department = s.DepartmentName,
                Match = s.MatchPercentage.ToString("F1") + "%",
                Missing = s.MissingSkillsCount,
                TrainingCost = s.EstimatedTrainingCost.ToString("C"),
                Duration = s.EstimatedTrainingDuration + "h",
                Trainings = s.TrainingPath.Count
            }).ToList();

            grid.DataSource = gridData;
            return grid;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1400, 900);
            this.Name = "CapabilityEnhancementForm";
            this.ResumeLayout(false);
        }
    }
}