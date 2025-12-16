using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class EmployeeManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView employeeGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnViewSkills;
        private TextBox txtSearch;
        private ComboBox cmbDepartmentFilter, cmbStatusFilter;

        public EmployeeManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadEmployees();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Employee Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top Panel for filters and actions
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Search TextBox
            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                AutoSize = true
            };
            topPanel.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(70, 12),
                Width = 200
            };
            txtSearch.TextChanged += (s, e) => FilterEmployees();
            topPanel.Controls.Add(txtSearch);

            // Department Filter
            var lblDepartment = new Label
            {
                Text = "Department:",
                Location = new Point(290, 15),
                AutoSize = true
            };
            topPanel.Controls.Add(lblDepartment);

            cmbDepartmentFilter = new ComboBox
            {
                Location = new Point(380, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDepartmentFilter.SelectedIndexChanged += (s, e) => FilterEmployees();
            topPanel.Controls.Add(cmbDepartmentFilter);

            // Status Filter
            var lblStatus = new Label
            {
                Text = "Status:",
                Location = new Point(550, 15),
                AutoSize = true
            };
            topPanel.Controls.Add(lblStatus);

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(610, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Active", "Terminated" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterEmployees();
            topPanel.Controls.Add(cmbStatusFilter);

            // Buttons
            btnAdd = CreateButton("Add New", 10, 55, btnAdd_Click);
            btnEdit = CreateButton("Edit", 110, 55, btnEdit_Click);
            btnDelete = CreateButton("Delete", 210, 55, btnDelete_Click);
            btnViewSkills = CreateButton("View Skills", 310, 55, btnViewSkills_Click);
            btnRefresh = CreateButton("Refresh", 430, 55, (s, e) => LoadEmployees());

            topPanel.Controls.Add(btnAdd);
            topPanel.Controls.Add(btnEdit);
            topPanel.Controls.Add(btnDelete);
            topPanel.Controls.Add(btnViewSkills);
            topPanel.Controls.Add(btnRefresh);

            this.Controls.Add(topPanel);

            var employeeGridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 150, 10, 10)
            };
            this.Controls.Add(employeeGridPanel);

            // DataGridView
            employeeGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            employeeGrid.DoubleClick += btnEdit_Click;
            employeeGridPanel.Controls.Add(employeeGrid);

            // Load filter data
            LoadFilterData();
            
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

        private void LoadFilterData()
        {
            cmbDepartmentFilter.Items.Clear();
            cmbDepartmentFilter.Items.Add("All Departments");
            foreach (var dept in dataManager.Departments)
            {
                cmbDepartmentFilter.Items.Add(dept.Name);
            }
            cmbDepartmentFilter.SelectedIndex = 0;
        }

        private void LoadEmployees()
        {
            var employees = dataManager.Employees.Select(e => new
            {
                e.Id,
                e.FullName,
                e.IdentityNumber,
                DateOfBirth = e.DateOfBirth.ToString("yyyy-MM-dd"),
                Department = dataManager.Departments.FirstOrDefault(d => d.Id == e.DepartmentId)?.Name ?? "N/A",
                Position = dataManager.Positions.FirstOrDefault(p => p.Id == e.PositionId)?.Name ?? "N/A",
                e.CurrentSalary,
                HireDate = e.HireDate.ToString("yyyy-MM-dd"),
                Tenure = $"{e.Tenure} months",
                Status = e.Status.ToString(),
                SkillCount = e.Skills?.Count ?? 0,
                TrainingCount = e.Trainings?.Count ?? 0
            }).ToList();

            employeeGrid.DataSource = employees;
        }

        private void FilterEmployees()
        {
            var query = dataManager.Employees.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    e.IdentityNumber.ToString().Contains(searchTerm));
            }

            // Department filter
            if (cmbDepartmentFilter.SelectedIndex > 0)
            {
                var selectedDept = dataManager.Departments[cmbDepartmentFilter.SelectedIndex - 1];
                query = query.Where(e => e.DepartmentId == selectedDept.Id);
            }

            // Status filter
            if (cmbStatusFilter.SelectedIndex > 0)
            {
                var status = cmbStatusFilter.SelectedIndex == 1 ? EmployeeStatus.Active : EmployeeStatus.Terminated;
                query = query.Where(e => e.Status == status);
            }

            var filteredEmployees = query.Select(e => new
            {
                e.Id,
                e.FullName,
                e.IdentityNumber,
                DateOfBirth = e.DateOfBirth.ToString("yyyy-MM-dd"),
                Department = dataManager.Departments.FirstOrDefault(d => d.Id == e.DepartmentId)?.Name ?? "N/A",
                Position = dataManager.Positions.FirstOrDefault(p => p.Id == e.PositionId)?.Name ?? "N/A",
                e.CurrentSalary,
                HireDate = e.HireDate.ToString("yyyy-MM-dd"),
                Tenure = $"{e.Tenure} months",
                Status = e.Status.ToString(),
                SkillCount = e.Skills?.Count ?? 0,
                TrainingCount = e.Trainings?.Count ?? 0
            }).ToList();

            employeeGrid.DataSource = filteredEmployees;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new EmployeeEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadEmployees();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (employeeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an employee to edit.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = employeeGrid.SelectedRows[0];
            int employeeId = (int)selectedRow.Cells["Id"].Value;
            var employee = dataManager.Employees.FirstOrDefault(e => e.Id == employeeId);

            if (employee != null)
            {
                var form = new EmployeeEditForm(dataManager, employee);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadEmployees();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (employeeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an employee to delete.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete this employee? This will also remove all related skills and training records.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                var selectedRow = employeeGrid.SelectedRows[0];
                int employeeId = (int)selectedRow.Cells["Id"].Value;

                // Remove related records
                dataManager.EmployeeSkills.RemoveAll(es => es.EmployeeId == employeeId);
                dataManager.EmployeeTrainings.RemoveAll(et => et.EmployeeId == employeeId);
                dataManager.ChangeHistories.RemoveAll(ch => ch.EmployeeId == employeeId);
                dataManager.Employees.RemoveAll(e => e.Id == employeeId);

                LoadEmployees();
                MessageBox.Show("Employee deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnViewSkills_Click(object sender, EventArgs e)
        {
            if (employeeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an employee to view skills.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = employeeGrid.SelectedRows[0];
            int employeeId = (int)selectedRow.Cells["Id"].Value;
            var employee = dataManager.Employees.FirstOrDefault(e => e.Id == employeeId);

            if (employee != null)
            {
                var form = new EmployeeSkillsForm(dataManager, employee);
                form.ShowDialog();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 700);
            this.Name = "EmployeeManagementForm";
            this.Text = "Employee Management";
            this.ResumeLayout(false);
        }
    }
}