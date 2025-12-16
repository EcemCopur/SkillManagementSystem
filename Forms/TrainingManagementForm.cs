using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class TrainingManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView trainingGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnManageSessions;
        private ComboBox cmbStatusFilter;

        public TrainingManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadTrainings();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Training Management";
            this.Size = new Size(1300, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblFilter = new Label { Text = "Status:", Location = new Point(10, 15), AutoSize = true };
            topPanel.Controls.Add(lblFilter);

            cmbStatusFilter = new ComboBox { Location = new Point(65, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Suggested", "Assigned", "Planned", "Ongoing", "Completed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterTrainings();
            topPanel.Controls.Add(cmbStatusFilter);

            btnAdd = CreateButton("Add New", 10, 50, btnAdd_Click);
            btnEdit = CreateButton("Edit", 110, 50, btnEdit_Click);
            btnDelete = CreateButton("Delete", 210, 50, btnDelete_Click);
            btnManageSessions = CreateButton("Manage Sessions", 310, 50, btnManageSessions_Click);
            btnRefresh = CreateButton("Refresh", 440, 50, (s, e) => LoadTrainings());

            topPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnManageSessions, btnRefresh });
            this.Controls.Add(topPanel);

            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 100, 10, 10)
            };
            this.Controls.Add(gridPanel);

            trainingGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            trainingGrid.DoubleClick += btnEdit_Click;
            gridPanel.Controls.Add(trainingGrid);
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = 90, Height = 30 };
            btn.Click += clickHandler;
            return btn;
        }

        private void LoadTrainings()
        {
            var trainings = dataManager.Trainings.Select(t => new
            {
                t.Id,
                t.Name,
                Department = dataManager.Departments.FirstOrDefault(d => d.Id == t.TargetDepartmentId)?.Name ?? "N/A",
                t.ApplicationType,
                t.Capacity,
                t.Source,
                Cost = t.Cost.ToString("C"),
                t.DurationHours,
                t.Status,
                SkillsCount = t.TrainingSkills?.Count ?? 0,
                PrereqSkillsCount = t.PrerequisiteSkills?.Count ?? 0,
                PrereqTrainingsCount = t.PrerequisiteTrainings?.Count ?? 0
            }).ToList();

            trainingGrid.DataSource = trainings;
        }

        private void FilterTrainings()
        {
            if (cmbStatusFilter.SelectedIndex == 0)
            {
                LoadTrainings();
                return;
            }

            var status = (TrainingStatus)(cmbStatusFilter.SelectedIndex);
            var trainings = dataManager.Trainings.Where(t => t.Status == status).Select(t => new
            {
                t.Id,
                t.Name,
                Department = dataManager.Departments.FirstOrDefault(d => d.Id == t.TargetDepartmentId)?.Name ?? "N/A",
                t.ApplicationType,
                t.Capacity,
                t.Source,
                Cost = t.Cost.ToString("C"),
                t.DurationHours,
                t.Status,
                SkillsCount = t.TrainingSkills?.Count ?? 0,
                PrereqSkillsCount = t.PrerequisiteSkills?.Count ?? 0,
                PrereqTrainingsCount = t.PrerequisiteTrainings?.Count ?? 0
            }).ToList();

            trainingGrid.DataSource = trainings;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new TrainingEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK) LoadTrainings();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (trainingGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a training to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int trainingId = (int)trainingGrid.SelectedRows[0].Cells["Id"].Value;
            var training = dataManager.Trainings.FirstOrDefault(t => t.Id == trainingId);

            if (training != null)
            {
                var form = new TrainingEditForm(dataManager, training);
                if (form.ShowDialog() == DialogResult.OK) LoadTrainings();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (trainingGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a training to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this training?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                int trainingId = (int)trainingGrid.SelectedRows[0].Cells["Id"].Value;
                dataManager.TrainingSkills.RemoveAll(ts => ts.TrainingId == trainingId);
                dataManager.TrainingPrerequisiteSkills.RemoveAll(tps => tps.TrainingId == trainingId);
                dataManager.TrainingPrerequisiteTrainings.RemoveAll(tpt => tpt.TrainingId == trainingId || tpt.PrerequisiteTrainingId == trainingId);
                dataManager.TrainingSessions.RemoveAll(ts => ts.TrainingId == trainingId);
                dataManager.EmployeeTrainings.RemoveAll(et => et.TrainingId == trainingId);
                dataManager.Trainings.RemoveAll(t => t.Id == trainingId);
                LoadTrainings();
                MessageBox.Show("Training deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnManageSessions_Click(object sender, EventArgs e)
        {
            if (trainingGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a training to manage sessions.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int trainingId = (int)trainingGrid.SelectedRows[0].Cells["Id"].Value;
            var training = dataManager.Trainings.FirstOrDefault(t => t.Id == trainingId);

            if (training != null)
            {
                var form = new TrainingSessionsForm(dataManager, training);
                form.ShowDialog();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1300, 700);
            this.Name = "TrainingManagementForm";
            this.ResumeLayout(false);
        }
    }
}
