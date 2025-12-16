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
    public partial class EmploymentAnalysisForm : Form
    {
        private DataManager dataManager;
        private EmploymentAnalysisService analysisService;

        private ComboBox cmbDepartmentFilter, cmbLevelFilter, cmbPosition;
        private NumericUpDown numTrainingCost;
        private Button btnAnalyze, btnRefresh;
        private DataGridView gridCandidates;
        private Panel resultsPanel;
        private Label lblExternalCost, lblPositionInfo;

        public EmploymentAnalysisForm(DataManager manager)
        {
            dataManager = manager;
            analysisService = new EmploymentAnalysisService(dataManager);

            InitializeComponent();
            InitializeCustomComponents();
            LoadOpenPositions();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Employment Analysis";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top Panel - Filters and Controls
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var titleLabel = new Label
            {
                Text = "Employment Analysis - Open Positions",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            topPanel.Controls.Add(titleLabel);

            int y = 50;

            // Department Filter
            AddLabel(topPanel, "Filter by Department:", 10, y);
            cmbDepartmentFilter = new ComboBox { Location = new Point(150, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartmentFilter.Items.Add("All Departments");
            foreach (var dept in dataManager.Departments)
            {
                cmbDepartmentFilter.Items.Add(dept.Name);
            }
            cmbDepartmentFilter.SelectedIndex = 0;
            cmbDepartmentFilter.SelectedIndexChanged += (s, e) => LoadOpenPositions();
            topPanel.Controls.Add(cmbDepartmentFilter);

            // Level Filter
            AddLabel(topPanel, "Position Level:", 370, y);
            cmbLevelFilter = new ComboBox { Location = new Point(480, y), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLevelFilter.Items.AddRange(new object[] { "All Levels", "1-2", "3-4", "5+" });
            cmbLevelFilter.SelectedIndex = 0;
            cmbLevelFilter.SelectedIndexChanged += (s, e) => LoadOpenPositions();
            topPanel.Controls.Add(cmbLevelFilter);

            y += 40;

            // Position Selection
            AddLabel(topPanel, "Select Position:", 10, y);
            cmbPosition = new ComboBox { Location = new Point(150, y), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPosition.DisplayMember = "DisplayText";
            cmbPosition.ValueMember = "Position";
            topPanel.Controls.Add(cmbPosition);

            // Training Cost Parameter
            AddLabel(topPanel, "Training Cost/Level:", 570, y);
            numTrainingCost = new NumericUpDown
            {
                Location = new Point(720, y),
                Width = 100,
                Minimum = 0,
                Maximum = 100000,
                Value = 5000,
                Increment = 500,
                DecimalPlaces = 0
            };
            topPanel.Controls.Add(numTrainingCost);

            // Analyze Button
            btnAnalyze = new Button
            {
                Text = "Analyze Position",
                Location = new Point(840, y - 5),
                Width = 150,
                Height = 35,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAnalyze.Click += BtnAnalyze_Click;
            topPanel.Controls.Add(btnAnalyze);

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(1000, y - 5),
                Width = 100,
                Height = 35
            };
            btnRefresh.Click += (s, e) => LoadOpenPositions();
            topPanel.Controls.Add(btnRefresh);

            this.Controls.Add(topPanel);

            // Results Panel
            resultsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 160, 10, 10),
                BackColor = Color.WhiteSmoke,
                AutoScroll = true
            };
            this.Controls.Add(resultsPanel);

            ShowWelcomeMessage();
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            parent.Controls.Add(label);
        }

        private void LoadOpenPositions()
        {
            cmbPosition.Items.Clear();

            var openPositions = analysisService.GetOpenPositions();

            // Apply filters
            if (cmbDepartmentFilter.SelectedIndex > 0)
            {
                var deptName = cmbDepartmentFilter.SelectedItem.ToString();
                var dept = dataManager.Departments.FirstOrDefault(d => d.Name == deptName);
                if (dept != null)
                {
                    openPositions = analysisService.GetOpenPositionsByDepartment(dept.Id);
                }
            }

            if (cmbLevelFilter.SelectedIndex > 0)
            {
                switch (cmbLevelFilter.SelectedIndex)
                {
                    case 1: openPositions = openPositions.Where(p => p.PositionLevel >= 1 && p.PositionLevel <= 2).ToList(); break;
                    case 2: openPositions = openPositions.Where(p => p.PositionLevel >= 3 && p.PositionLevel <= 4).ToList(); break;
                    case 3: openPositions = openPositions.Where(p => p.PositionLevel >= 5).ToList(); break;
                }
            }

            foreach (var position in openPositions)
            {
                var dept = dataManager.Departments.FirstOrDefault(d => d.Id == position.DepartmentId);
                var displayText = $"{position.Name} - {dept?.Name ?? "N/A"} (Level {position.PositionLevel}) - {position.NumOpenPositions} openings";
                cmbPosition.Items.Add(new { DisplayText = displayText, Position = position });
            }

            if (cmbPosition.Items.Count > 0)
            {
                cmbPosition.SelectedIndex = 0;
            }
        }

        private void ShowWelcomeMessage()
        {
            resultsPanel.Controls.Clear();
            var welcomeLabel = new Label
            {
                Text = "Select a position and click 'Analyze Position' to see internal candidates and external hiring comparison.",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = Color.Gray
            };
            resultsPanel.Controls.Add(welcomeLabel);
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            if (cmbPosition.SelectedItem == null)
            {
                MessageBox.Show("Please select a position to analyze.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = (dynamic)cmbPosition.SelectedItem;
            Position position = selectedItem.Position;

            // Check if position has required skills
            var requiredSkills = dataManager.PositionRequiredSkills.Where(prs => prs.PositionId == position.Id).ToList();
            if (requiredSkills.Count == 0)
            {
                MessageBox.Show("This position has no required skills defined. Please define required skills first.",
                    "No Required Skills", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal trainingCost = numTrainingCost.Value;
                var result = analysisService.AnalyzePositionCandidates(position.Id, trainingCost);
                DisplayResults(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResults(EmploymentAnalysisResult result)
        {
            resultsPanel.Controls.Clear();
            int yPos = 10;

            // Position Info Section
            var posInfoPanel = CreateSectionPanel("Position Information", yPos, 1340);
            posInfoPanel.Height = 160;
            yPos += 30;

            var dept = dataManager.Departments.FirstOrDefault(d => d.Id == result.Position.DepartmentId);
            var reqSkillsCount = dataManager.PositionRequiredSkills.Count(prs => prs.PositionId == result.Position.Id);

            lblPositionInfo = new Label
            {
                Text = $"Position: {result.Position.Name}\n" +
                       $"Department: {dept?.Name ?? "N/A"}\n" +
                       $"Level: {result.Position.PositionLevel}\n" +
                       $"Salary Range: {result.Position.MinSalary:C} - {result.Position.MaxSalary:C}\n" +
                       $"Required Skills: {reqSkillsCount}\n" +
                       $"Open Positions: {result.Position.NumOpenPositions}",
                Location = new Point(15, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            posInfoPanel.Controls.Add(lblPositionInfo);
            resultsPanel.Controls.Add(posInfoPanel);
            yPos = posInfoPanel.Bottom + 10;

            // External Hiring Option
            var externalPanel = CreateSectionPanel("External Hiring Option", yPos, 1340);
            externalPanel.Height = 110;
            int panelY = 30;

            lblExternalCost = new Label
            {
                Text = $"Hiring Cost: {result.ExternalOption.HiringCost:C}\n" +
                       $"Starting Salary: {result.ExternalOption.StartingSalary:C}\n" +
                       $"Total First Year Cost: {result.ExternalOption.TotalCost:C}",
                Location = new Point(15, panelY),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            externalPanel.Controls.Add(lblExternalCost);
            resultsPanel.Controls.Add(externalPanel);
            yPos = externalPanel.Bottom + 10;

            // Internal Candidates
            var candidatesPanel = CreateSectionPanel($"Top Internal Candidates ({result.InternalCandidates.Count} found)", yPos, 1340);
            candidatesPanel.Height = 350;

            if (result.InternalCandidates.Count == 0)
            {
                var noResults = new Label
                {
                    Text = "No internal candidates found meeting the minimum 70% skill match requirement.",
                    Location = new Point(15, 40),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.OrangeRed
                };
                candidatesPanel.Controls.Add(noResults);
                resultsPanel.Controls.Add(candidatesPanel);
            }
            else
            {
                gridCandidates = new DataGridView
                {
                    Location = new Point(10, 35),
                    Width = 1320,
                    Height = 300,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    BackgroundColor = Color.White
                };

                var candidateData = result.InternalCandidates.Select(c => new
                {
                    Name = c.Employee.FullName,
                    CurrentPosition = c.CurrentPositionName,
                    CurrentSalary = c.CurrentSalary.ToString("C"),
                    Score = c.TotalScore.ToString("F2"),
                    MatchPercent = c.MatchPercentage.ToString("F1") + "%",
                    MatchingSkills = $"{c.MatchingSkillsCount}/{c.RequiredSkillsCount}",
                    AvgGap = c.AverageSkillGap.ToString("F2"),
                    TrainingCost = c.EstimatedTrainingCost.ToString("C"),
                    TotalCost = (c.EstimatedTrainingCost + c.CurrentSalary).ToString("C")
                }).ToList();

                gridCandidates.DataSource = candidateData;
                gridCandidates.SelectionChanged += (s, e) => ShowCandidateDetails(result);

                candidatesPanel.Controls.Add(gridCandidates);
                resultsPanel.Controls.Add(candidatesPanel);
                yPos = candidatesPanel.Bottom + 10;

                // Candidate Details Panel
                var detailsPanel = CreateSectionPanel("Candidate Details", yPos, 1340);
                detailsPanel.Name = "detailsPanel";
                detailsPanel.Height = 300;
                resultsPanel.Controls.Add(detailsPanel);

                // Force selection of first row and display details
                if (gridCandidates.Rows.Count > 0)
                {
                    gridCandidates.Rows[0].Selected = true;
                    ShowCandidateDetails(result);
                }
            }
        }

        private void ShowCandidateDetails(EmploymentAnalysisResult result)
        {
            if (gridCandidates.SelectedRows.Count == 0) return;

            var selectedRow = gridCandidates.SelectedRows[0];
            var candidateName = selectedRow.Cells["Name"].Value.ToString();
            var candidate = result.InternalCandidates.FirstOrDefault(c => c.Employee.FullName == candidateName);

            if (candidate == null) return;

            var detailsPanel = resultsPanel.Controls["detailsPanel"] as Panel;
            if (detailsPanel == null) return;

            detailsPanel.Controls.Clear();
            int yPos = 30;

            // Matching Skills
            if (candidate.MatchingSkills.Any())
            {
                var lblMatching = new Label
                {
                    Text = "✓ Matching Skills:",
                    Location = new Point(20, yPos),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.Green
                };
                detailsPanel.Controls.Add(lblMatching);
                yPos += 25;

                foreach (var skill in candidate.MatchingSkills)
                {
                    var status = skill.MeetsRequirement ? "✓" : "⚠";
                    var color = skill.MeetsRequirement ? Color.Green : Color.Orange;
                    var lblSkill = new Label
                    {
                        Text = $"  {status} {skill.SkillName}: Level {skill.CurrentLevel}/{skill.RequiredLevel}",
                        Location = new Point(40, yPos),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = color
                    };
                    detailsPanel.Controls.Add(lblSkill);
                    yPos += 20;
                }
            }

            yPos += 10;

            // Skill Gaps
            if (candidate.SkillGaps.Any())
            {
                var lblGaps = new Label
                {
                    Text = "✗ Skill Gaps (Training Needed):",
                    Location = new Point(20, yPos),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.Red
                };
                detailsPanel.Controls.Add(lblGaps);
                yPos += 25;

                foreach (var gap in candidate.SkillGaps)
                {
                    var gapText = gap.IsMissing ?
                        $"  ✗ {gap.SkillName}: MISSING (need Level {gap.RequiredLevel}) - Est. Cost: {gap.LevelGap * result.TrainingCostPerLevel:C}" :
                        $"  ⚠ {gap.SkillName}: Level {gap.CurrentLevel}/{gap.RequiredLevel} (gap: {gap.LevelGap}) - Est. Cost: {gap.LevelGap * result.TrainingCostPerLevel:C}";

                    var lblGap = new Label
                    {
                        Text = gapText,
                        Location = new Point(40, yPos),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = gap.IsMissing ? Color.DarkRed : Color.OrangeRed
                    };
                    detailsPanel.Controls.Add(lblGap);
                    yPos += 20;
                }
            }
        }

        private Panel CreateSectionPanel(string title, int yPos, int width)
        {
            var panel = new Panel
            {
                Location = new Point(10, yPos),
                Width = width,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Add title label
            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(10, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1400, 800);
            this.Name = "EmploymentAnalysisForm";
            this.ResumeLayout(false);
        }
    }
}