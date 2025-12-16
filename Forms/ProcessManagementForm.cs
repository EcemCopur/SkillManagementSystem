using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class ProcessManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView processGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnManageSkills, btnManagePositions;

        public ProcessManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadProcesses();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Process Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnAdd = CreateButton("Add New", 10, 35, btnAdd_Click);
            btnEdit = CreateButton("Edit", 110, 35, btnEdit_Click);
            btnDelete = CreateButton("Delete", 210, 35, btnDelete_Click);
            btnManageSkills = CreateButton("Manage Skills", 310, 35, btnManageSkills_Click);
            btnManagePositions = CreateButton("Manage Positions", 430, 35, btnManagePositions_Click);
            btnRefresh = CreateButton("Refresh", 570, 35, (s, e) => LoadProcesses());

            topPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnManageSkills, btnManagePositions, btnRefresh });
            this.Controls.Add(topPanel);

            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 90, 10, 10)
            };
            this.Controls.Add(gridPanel);

            processGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            processGrid.DoubleClick += btnEdit_Click;
            gridPanel.Controls.Add(processGrid);
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = 100, Height = 30 };
            btn.Click += clickHandler;
            return btn;
        }

        private void LoadProcesses()
        {
            var processes = dataManager.Processes.Select(p => new
            {
                p.Id,
                p.Name,
                Description = p.ProcessDescription?.Length > 50 ? p.ProcessDescription.Substring(0, 50) + "..." : p.ProcessDescription,
                p.AimedNumberOfWorkers,
                RequiredSkillsCount = p.RequiredSkills?.Count ?? 0,
                PositionsCount = dataManager.PositionProcesses.Count(pp => pp.ProcessId == p.Id),
                CapableWorkersCount = CountCapableWorkers(p.Id)
            }).ToList();

            processGrid.DataSource = processes;
        }

        private int CountCapableWorkers(int processId)
        {
            var process = dataManager.Processes.FirstOrDefault(p => p.Id == processId);
            if (process == null || process.RequiredSkills == null) return 0;

            return dataManager.Employees.Count(emp =>
                process.RequiredSkills.All(reqSkill =>
                    emp.Skills.Any(empSkill =>
                        empSkill.SkillId == reqSkill.SkillId &&
                        (int)empSkill.CurrentLevel >= reqSkill.RequiredLevel)));
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new ProcessEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK) LoadProcesses();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (processGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a process to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int processId = (int)processGrid.SelectedRows[0].Cells["Id"].Value;
            var process = dataManager.Processes.FirstOrDefault(p => p.Id == processId);

            if (process != null)
            {
                var form = new ProcessEditForm(dataManager, process);
                if (form.ShowDialog() == DialogResult.OK) LoadProcesses();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (processGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a process to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this process?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                int processId = (int)processGrid.SelectedRows[0].Cells["Id"].Value;
                dataManager.ProcessRequiredSkills.RemoveAll(prs => prs.ProcessId == processId);
                dataManager.PositionProcesses.RemoveAll(pp => pp.ProcessId == processId);
                dataManager.Processes.RemoveAll(p => p.Id == processId);
                LoadProcesses();
                MessageBox.Show("Process deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnManageSkills_Click(object sender, EventArgs e)
        {
            if (processGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a process to manage skills.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int processId = (int)processGrid.SelectedRows[0].Cells["Id"].Value;
            var process = dataManager.Processes.FirstOrDefault(p => p.Id == processId);

            if (process != null)
            {
                var form = new ProcessSkillsForm(dataManager, process);
                form.ShowDialog();
                LoadProcesses();
            }
        }

        private void btnManagePositions_Click(object sender, EventArgs e)
        {
            if (processGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a process to manage positions.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int processId = (int)processGrid.SelectedRows[0].Cells["Id"].Value;
            var process = dataManager.Processes.FirstOrDefault(p => p.Id == processId);

            if (process != null)
            {
                var form = new ProcessPositionsForm(dataManager, process);
                form.ShowDialog();
                LoadProcesses();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 700);
            this.Name = "ProcessManagementForm";
            this.ResumeLayout(false);
        }
    }


}