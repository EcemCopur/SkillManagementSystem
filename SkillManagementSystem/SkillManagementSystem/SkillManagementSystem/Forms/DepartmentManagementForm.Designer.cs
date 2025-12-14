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
            this.Controls.Add(departmentGrid);
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
            if (dataManager.Employees.Any(e => e.DepatmentId == deptId))
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

    // Department Edit Form
    public partial class DepartmentEditForm : Form
    {
        private DataManager dataManager;
        private Department department;
        private bool isEditMode;
        private TextBox txtName, txtBudget;
        private Button btnSave, btnCancel;

        public DepartmentEditForm(DataManager manager, Department dept)
        {
            dataManager = manager;
            department = dept;
            isEditMode = dept != null;

            InitializeComponent();
            InitializeCustomComponents();

            if (isEditMode)
            {
                LoadDepartmentData();
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Department" : "Add New Department";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int labelWidth = 100;
            int controlWidth = 300;
            int leftMargin = 40;
            int controlLeftMargin = leftMargin + labelWidth + 10;
            int currentY = 40;
            int verticalSpacing = 50;

            // Title
            var titleLabel = new Label
            {
                Text = isEditMode ? "Edit Department" : "Add New Department",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(leftMargin, currentY),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);
            currentY += 50;

            // Name
            AddLabel("Name:", leftMargin, currentY);
            txtName = new TextBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth
            };
            this.Controls.Add(txtName);
            currentY += verticalSpacing;

            // Budget
            AddLabel("Budget:", leftMargin, currentY);
            txtBudget = new TextBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth
            };
            this.Controls.Add(txtBudget);
            currentY += verticalSpacing + 20;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(controlLeftMargin, currentY),
                Width = 100,
                Height = 35,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(controlLeftMargin + 110, currentY),
                Width = 100,
                Height = 35
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                Width = 100,
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(label);
        }

        private void LoadDepartmentData()
        {
            txtName.Text = department.Name;
            txtBudget.Text = department.Budget.ToString();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                if (!isEditMode)
                {
                    department = new Department
                    {
                        Id = dataManager.Departments.Any() ? dataManager.Departments.Max(d => d.Id) + 1 : 1
                    };
                    dataManager.Departments.Add(department);
                }

                department.Name = txtName.Text.Trim();
                department.Budget = decimal.Parse(txtBudget.Text);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving department: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Department name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            if (!decimal.TryParse(txtBudget.Text, out decimal budget) || budget < 0)
            {
                MessageBox.Show("Budget must be a valid positive number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBudget.Focus();
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 300);
            this.Name = "DepartmentEditForm";
            this.Text = "Department Edit";
            this.ResumeLayout(false);
        }
    }
}