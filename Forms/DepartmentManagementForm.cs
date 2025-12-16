using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class DepartmentManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView departmentGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public DepartmentManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadDepartments();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Department Management";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top Panel
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
            btnRefresh = CreateButton("Refresh", 310, 35, (s, e) => LoadDepartments());

            topPanel.Controls.Add(btnAdd);
            topPanel.Controls.Add(btnEdit);
            topPanel.Controls.Add(btnDelete);
            topPanel.Controls.Add(btnRefresh);

            this.Controls.Add(topPanel);

            var depGridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 120, 20, 20),
                BackColor = Color.LightGray
            };
            this.Controls.Add(depGridPanel);

            // DataGridView
            departmentGrid = new DataGridView
            {   
                
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            departmentGrid.DoubleClick += btnEdit_Click;
            depGridPanel.Controls.Add(departmentGrid);
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 90,
                Height = 30
            };
            btn.Click += clickHandler;
            return btn;
        }

        private void LoadDepartments()
        {
            var departments = dataManager.Departments.Select(d => new
            {
                d.Id,
                d.Name,
                d.Budget,
                PositionCount = d.Positions?.Count ?? 0,
                EmployeeCount = d.Employees?.Count ?? 0
            }).ToList();

            departmentGrid.DataSource = departments;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new DepartmentEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadDepartments();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (departmentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a department to edit.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = departmentGrid.SelectedRows[0];
            int deptId = (int)selectedRow.Cells["Id"].Value;
            var department = dataManager.Departments.FirstOrDefault(d => d.Id == deptId);

            if (department != null)
            {
                var form = new DepartmentEditForm(dataManager, department);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadDepartments();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (departmentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a department to delete.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = departmentGrid.SelectedRows[0];
            int deptId = (int)selectedRow.Cells["Id"].Value;

            // Check if department has employees or positions
            if (dataManager.Employees.Any(e => e.DepartmentId == deptId))
            {
                MessageBox.Show("Cannot delete department with active employees.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dataManager.Positions.Any(p => p.DepartmentId == deptId))
            {
                MessageBox.Show("Cannot delete department with positions. Delete positions first.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this department?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                dataManager.Departments.RemoveAll(d => d.Id == deptId);
                LoadDepartments();
                MessageBox.Show("Department deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 600);
            this.Name = "DepartmentManagementForm";
            this.Text = "Department Management";
            this.ResumeLayout(false);
        }
    }

}