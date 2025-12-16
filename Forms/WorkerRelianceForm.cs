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
    public partial class WorkerRelianceForm : Form
    {
        private DataManager dataManager;
        private WorkerRelianceService analysisService;
        private WorkerRelianceResult lastResult;

        private NumericUpDown numTrainingCost;
        private Button btnAnalyze;
        private DataGridView gridRelianceIssues;
        private Panel detailsPanel;
        private Label lblSummary;
        private TabControl tabDetails;

        public WorkerRelianceForm(DataManager manager)
        {
            dataManager = manager;
            analysisService = new WorkerRelianceService(dataManager);

            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Worker Reliance Analysis";
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
                Text = "Worker Reliance - Single Point of Failure Analysis",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            topPanel.Controls.Add(titleLabel);

            var lblInfo = new Label
            {
                Text = "Identifies processes with insufficient worker coverage (0 workers, 1 worker, or below target).",
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
                Text = "Analyze Worker Reliance",
                Location = new Point(300, y - 5),
                Width = 200,
                Height = 35,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAnalyze.Click += BtnAnalyze_Click;
            topPanel.Controls.Add(btnAnalyze);

            this.Controls.Add(topPanel);

            // Main Content Area - Fixed positioning
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
                Text = "Click 'Analyze Worker Reliance' to start analysis..."
            };
            contentPanel.Controls.Add(lblSummary);

            // Reliance Issues Grid
            gridRelianceIssues = new DataGridView
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
            gridRelianceIssues.SelectionChanged += GridRelianceIssues_SelectionChanged;
            contentPanel.Controls.Add(gridRelianceIssues);

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
                Text = "Click 'Analyze Worker Reliance' to identify single points of failure and worker coverage gaps.",
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
                lastResult = analysisService.AnalyzeWorkerReliance(trainingCost);
                DisplayResults(lastResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAnalyze.Enabled = true;
                btnAnalyze.Text = "Analyze Worker Reliance";
            }
        }

        private void DisplayResults(WorkerRelianceResult result)
        {
            // Update summary
            lblSummary.Text = $"Found {result.RelianceIssues.Count} processes with reliance issues: " +
                             $"{result.CriticalProcessesCount} Critical (0 workers) | " +
                             $"{result.HighRiskProcessesCount} High Risk (1 worker) | " +
                             $"{result.BelowTargetProcessesCount} Below Target";

            // Display reliance issues
            var gridData = result.RelianceIssues.Select(issue => new
            {
                Priority = issue.Priority,
                ProcessName = issue.Process.Name,
                Status = issue.PriorityReason,
                Current = issue.CurrentCapableWorkers,
                Target = issue.AimedNumberOfWorkers,
                Gap = issue.WorkerGap,
                GapPercent = issue.GapPercentage.ToString("F1") + "%",
                Positions = string.Join(", ", issue.AssignedPositionNames),
                SameDept = issue.SameDepartmentSuggestions.Count,
                CrossDept = issue.CrossDepartmentSuggestions.Count
            }).ToList();

            gridRelianceIssues.DataSource = gridData;

            // Color code by priority
            foreach (DataGridViewRow row in gridRelianceIssues.Rows)
            {
                int priority = (int)row.Cells["Priority"].Value;
                switch (priority)
                {
                    case 1: row.DefaultCellStyle.BackColor = Color.LightCoral; break;
                    case 2: row.DefaultCellStyle.BackColor = Color.LightSalmon; break;
                    case 3: row.DefaultCellStyle.BackColor = Color.LightYellow; break;
                }
            }

            if (result.RelianceIssues.Any())
            {
                gridRelianceIssues.Rows[0].Selected = true;
            }
        }

        private void GridRelianceIssues_SelectionChanged(object sender, EventArgs e)
        {
            if (gridRelianceIssues.SelectedRows.Count == 0) return;

            var selectedRow = gridRelianceIssues.SelectedRows[0];
            var processName = selectedRow.Cells["ProcessName"].Value.ToString();
            ShowProcessDetails(processName);
        }

        private void ShowProcessDetails(string processName)
        {
            if (lastResult == null) return;

            var issue = lastResult.RelianceIssues.FirstOrDefault(i => i.Process.Name == processName);
            if (issue == null) return;

            detailsPanel.Controls.Clear();

            tabDetails = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            detailsPanel.Controls.Add(tabDetails);

            // Tab 1: Current Capable Workers
            var tabCurrent = new TabPage($"Current Workers ({issue.CurrentCapableWorkers})");
            var txtCurrent = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9) };
            txtCurrent.AppendText($"Currently Capable Workers for: {issue.Process.Name}\n");
            txtCurrent.AppendText(new string('=', 60) + "\n\n");

            if (issue.CurrentCapableEmployees.Any())
            {
                foreach (var emp in issue.CurrentCapableEmployees)
                {
                    var pos = dataManager.Positions.FirstOrDefault(p => p.Id == emp.PositionId);
                    var dept = dataManager.Departments.FirstOrDefault(d => d.Id == emp.DepartmentId);
                    txtCurrent.AppendText($"• {emp.FullName}\n");
                    txtCurrent.AppendText($"  - Position: {pos?.Name ?? "Unknown"}\n");
                    txtCurrent.AppendText($"  - Department: {dept?.Name ?? "Unknown"}\n");
                    txtCurrent.AppendText($"  - Skills: {emp.Skills?.Count ?? 0}\n\n");
                }
            }
            else
            {
                txtCurrent.AppendText("⚠ NO CAPABLE WORKERS - Process cannot be performed!\n");
            }
            tabCurrent.Controls.Add(txtCurrent);
            tabDetails.TabPages.Add(tabCurrent);

            // Tab 2: Same Department Suggestions
            var tabSameDept = new TabPage($"Same Department ({issue.SameDepartmentSuggestions.Count})");
            if (issue.SameDepartmentSuggestions.Any())
            {
                var gridSame = CreateEmployeeSuggestionGrid(issue.SameDepartmentSuggestions);
                tabSameDept.Controls.Add(gridSame);
            }
            else
            {
                var lblNone = new Label
                {
                    Text = "No employees in the same department are close to qualifying (need 1-2 skills).",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Gray
                };
                tabSameDept.Controls.Add(lblNone);
            }
            tabDetails.TabPages.Add(tabSameDept);

            // Tab 3: Cross Department Suggestions
            var tabCrossDept = new TabPage($"Cross Department ({issue.CrossDepartmentSuggestions.Count})");
            if (issue.CrossDepartmentSuggestions.Any())
            {
                var gridCross = CreateEmployeeSuggestionGrid(issue.CrossDepartmentSuggestions);
                tabCrossDept.Controls.Add(gridCross);
            }
            else
            {
                var lblNone = new Label
                {
                    Text = "No employees from other departments are close to qualifying.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.Gray
                };
                tabCrossDept.Controls.Add(lblNone);
            }
            tabDetails.TabPages.Add(tabCrossDept);

            // Tab 4: Training Path Details
            if (issue.SameDepartmentSuggestions.Any() || issue.CrossDepartmentSuggestions.Any())
            {
                var tabTraining = new TabPage("Training Paths");
                var txtTraining = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9) };
                txtTraining.AppendText($"Detailed Training Paths for: {issue.Process.Name}\n");
                txtTraining.AppendText(new string('=', 80) + "\n\n");

                var allSuggestions = issue.SameDepartmentSuggestions.Concat(issue.CrossDepartmentSuggestions).Take(5);
                foreach (var suggestion in allSuggestions)
                {
                    txtTraining.AppendText($"Employee: {suggestion.Employee.FullName} ({suggestion.DepartmentName})\n");
                    txtTraining.AppendText($"Match: {suggestion.MatchPercentage:F1}% | Cost: {suggestion.EstimatedTrainingCost:C} | Duration: {suggestion.EstimatedTrainingDuration}h\n\n");

                    txtTraining.AppendText("  Skill Gaps:\n");
                    foreach (var gap in suggestion.SkillGaps)
                    {
                        var status = gap.IsMissing ? "MISSING" : $"Level {gap.CurrentLevel}→{gap.RequiredLevel}";
                        txtTraining.AppendText($"    • {gap.SkillName}: {status} (gap: {gap.LevelGap})\n");
                    }

                    if (suggestion.TrainingPath.Any())
                    {
                        txtTraining.AppendText("\n  Suggested Training Path:\n");
                        foreach (var step in suggestion.TrainingPath)
                        {
                            var prereqStatus = step.MeetsPrerequisites ? "✓" : "✗ (prerequisites missing)";
                            txtTraining.AppendText($"    {step.StepNumber}. {step.TrainingName} {prereqStatus}\n");
                            txtTraining.AppendText($"       Skills: {string.Join(", ", step.SkillsItTeaches)}\n");
                            if (!step.MeetsPrerequisites && step.MissingPrerequisites.Any())
                            {
                                txtTraining.AppendText($"       Missing: {string.Join(", ", step.MissingPrerequisites)}\n");
                            }
                        }
                    }
                    else
                    {
                        txtTraining.AppendText("\n  ⚠ No suitable trainings found for required skills.\n");
                    }

                    txtTraining.AppendText("\n" + new string('-', 80) + "\n\n");
                }
                tabTraining.Controls.Add(txtTraining);
                tabDetails.TabPages.Add(tabTraining);
            }
        }

        private DataGridView CreateEmployeeSuggestionGrid(System.Collections.Generic.List<EmployeeSuggestion> suggestions)
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
                HasSkills = s.MatchingSkillsCount,
                NeedsSkills = s.MissingSkillsCount,
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
            this.Name = "WorkerRelianceForm";
            this.ResumeLayout(false);
        }
    }
}