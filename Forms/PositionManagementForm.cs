using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class PositionManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView positionGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public PositionManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadPositions();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Position Management";
            this.Size = new Size(1100, 600);
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
            btnRefresh = CreateButton("Refresh", 310, 35, (s, e) => LoadPositions());

            topPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(topPanel);

            var positionGridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20,120,20,20),
                BackColor = Color.LightGray
            };

            this.Controls.Add(positionGridContainer);

            positionGrid = new DataGridView
            {
               
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                
            };
            positionGrid.DoubleClick += btnEdit_Click;
            positionGridContainer.Controls.Add(positionGrid);
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = 90, Height = 30 };
            btn.Click += clickHandler;
            return btn;
        }

        private void LoadPositions()
        {
            var positions = dataManager.Positions.Select(p => new
            {
                p.Id,
                p.Name,
                Department = dataManager.Departments.FirstOrDefault(d => d.Id == p.DepartmentId)?.Name ?? "N/A",
                p.Capacity,
                p.PositionLevel,
                MinSalary = p.MinSalary.ToString("C"),
                MaxSalary = p.MaxSalary.ToString("C"),
                p.HiringCost,
                EmployeeCount = p.Employees?.Count ?? 0,
                OpenPositions = p.NumOpenPositions
            }).ToList();

            positionGrid.DataSource = positions;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new PositionEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK) LoadPositions();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a position to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int posId = (int)positionGrid.SelectedRows[0].Cells["Id"].Value;
            var position = dataManager.Positions.FirstOrDefault(p => p.Id == posId);

            if (position != null)
            {
                var form = new PositionEditForm(dataManager, position);
                if (form.ShowDialog() == DialogResult.OK) LoadPositions();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a position to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int posId = (int)positionGrid.SelectedRows[0].Cells["Id"].Value;

            if (dataManager.Employees.Any(e => e.PositionId == posId))
            {
                MessageBox.Show("Cannot delete position with active employees.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this position?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                dataManager.PositionRequiredSkills.RemoveAll(prs => prs.PositionId == posId);
                dataManager.PositionProcesses.RemoveAll(pp => pp.PositionId == posId);
                dataManager.Positions.RemoveAll(p => p.Id == posId);
                LoadPositions();
                MessageBox.Show("Position deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1100, 600);
            this.Name = "PositionManagementForm";
            this.ResumeLayout(false);
        }
    }

}