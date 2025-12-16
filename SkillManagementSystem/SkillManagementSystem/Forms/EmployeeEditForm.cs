using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class EmployeeEditForm : Form
    {
        private DataManager dataManager;
        private Employee employee;
        private bool isEditMode;

        // Controls
        private TextBox txtFirstName, txtLastName, txtIdentityNumber, txtSalary;
        private DateTimePicker dtpDateOfBirth, dtpHireDate, dtpTerminationDate;
        private ComboBox cmbDepartment, cmbPosition, cmbStatus;
        private CheckBox chkIsTerminated;
        private Button btnSave, btnCancel;

        public EmployeeEditForm(DataManager manager, Employee emp)
        {
            dataManager = manager;
            employee = emp;
            isEditMode = emp != null;

            InitializeComponent();
            InitializeCustomComponents();

            if (isEditMode)
            {
                LoadEmployeeData();
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Employee" : "Add New Employee";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int labelWidth = 120;
            int controlWidth = 400;
            int leftMargin = 30;
            int controlLeftMargin = leftMargin + labelWidth + 10;
            int verticalSpacing = 45;
            int currentY = 20;

            // Title Label
            var titleLabel = new Label
            {
                Text = isEditMode ? "Edit Employee Information" : "Add New Employee",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(leftMargin, currentY),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);
            currentY += 50;

            // First Name
            AddLabel("First Name:", leftMargin, currentY);
            txtFirstName = new TextBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth
            };
            this.Controls.Add(txtFirstName);
            currentY += verticalSpacing;

            // Last Name
            AddLabel("Last Name:", leftMargin, currentY);
            txtLastName = new TextBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth
            };
            this.Controls.Add(txtLastName);
            currentY += verticalSpacing;

            // Identity Number
            AddLabel("Identity Number:", leftMargin, currentY);
            txtIdentityNumber = new TextBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth
            };
            this.Controls.Add(txtIdentityNumber);
            currentY += verticalSpacing;

            // Date of Birth
            AddLabel("Date of Birth:", leftMargin, currentY);
            dtpDateOfBirth = new DateTimePicker
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpDateOfBirth);
            currentY += verticalSpacing;

            // Department
            AddLabel("Department:", leftMargin, currentY);
            cmbDepartment = new ComboBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            cmbDepartment.DataSource = dataManager.Departments.ToList();
            cmbDepartment.SelectedIndexChanged += CmbDepartment_SelectedIndexChanged;
            this.Controls.Add(cmbDepartment);
            currentY += verticalSpacing;

            // Position
            AddLabel("Position:", leftMargin, currentY);
            cmbPosition = new ComboBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPosition.DisplayMember = "Name";
            cmbPosition.ValueMember = "Id";
            this.Controls.Add(cmbPosition);
            currentY += verticalSpacing;

            // Salary
            AddLabel("Current Salary:", leftMargin, currentY);
            txtSalary = new TextBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth
            };
            this.Controls.Add(txtSalary);
            currentY += verticalSpacing;

            // Hire Date
            AddLabel("Hire Date:", leftMargin, currentY);
            dtpHireDate = new DateTimePicker
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpHireDate);
            currentY += verticalSpacing;

            // Status
            AddLabel("Status:", leftMargin, currentY);
            cmbStatus = new ComboBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "Active", "Terminated" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            this.Controls.Add(cmbStatus);
            currentY += verticalSpacing;

            // Termination Date (initially hidden)
            AddLabel("Termination Date:", leftMargin, currentY);
            dtpTerminationDate = new DateTimePicker
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                Format = DateTimePickerFormat.Short,
                Visible = false
            };
            this.Controls.Add(dtpTerminationDate);
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
                Location = new Point(controlLeftMargin + 120, currentY),
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
                Width = 120,
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(label);
        }

        private void CmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDepartment.SelectedValue is int deptId)
            {
                var positions = dataManager.Positions.Where(p => p.DepartmentId == deptId).ToList();
                cmbPosition.DataSource = positions;

                if (positions.Count > 0 && !isEditMode)
                {
                    cmbPosition.SelectedIndex = 0;
                }
            }
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isTerminated = cmbStatus.SelectedIndex == 1;
            dtpTerminationDate.Visible = isTerminated;
        }

        private void LoadEmployeeData()
        {
            txtFirstName.Text = employee.FirstName;
            txtLastName.Text = employee.LastName;
            txtIdentityNumber.Text = employee.IdentityNumber.ToString();
            dtpDateOfBirth.Value = employee.DateOfBirth;
            txtSalary.Text = employee.CurrentSalary.ToString();
            dtpHireDate.Value = employee.HireDate;

            // Set department first
            cmbDepartment.SelectedValue = employee.DepartmentId;

            // Then set position
            cmbPosition.SelectedValue = employee.PositionId;

            // Set status
            cmbStatus.SelectedIndex = employee.Status == EmployeeStatus.Active ? 0 : 1;

            if (employee.TerminationDate.HasValue)
            {
                dtpTerminationDate.Value = employee.TerminationDate.Value;
                dtpTerminationDate.Visible = true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                if (!isEditMode)
                {
                    // Create new employee
                    employee = new Employee
                    {
                        Id = dataManager.Employees.Any() ? dataManager.Employees.Max(e => e.Id) + 1 : 1
                    };
                    dataManager.Employees.Add(employee);
                }
                else
                {
                    // Track changes for history
                    TrackChanges();
                }

                // Update employee properties
                employee.FirstName = txtFirstName.Text.Trim();
                employee.LastName = txtLastName.Text.Trim();
                employee.IdentityNumber = int.Parse(txtIdentityNumber.Text);
                employee.DateOfBirth = dtpDateOfBirth.Value;
                employee.DepartmentId = (int)cmbDepartment.SelectedValue;
                employee.PositionId = (int)cmbPosition.SelectedValue;
                employee.CurrentSalary = decimal.Parse(txtSalary.Text);
                employee.HireDate = dtpHireDate.Value;
                employee.Status = cmbStatus.SelectedIndex == 0 ? EmployeeStatus.Active : EmployeeStatus.Terminated;

                if (employee.Status == EmployeeStatus.Terminated)
                {
                    employee.TerminationDate = dtpTerminationDate.Value;
                }
                else
                {
                    employee.TerminationDate = null;
                }

                // Update expected processes based on position
                var position = dataManager.Positions.FirstOrDefault(p => p.Id == employee.PositionId);
                if (position != null)
                {
                    employee.ExpectedProcesses = position.Processes.Select(pp => pp.ProcessId).ToList();
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TrackChanges()
        {
            // Track position changes
            if (employee.PositionId != (int)cmbPosition.SelectedValue)
            {
                var history = new ChangeHistory
                {
                    Id = dataManager.ChangeHistories.Any() ? dataManager.ChangeHistories.Max(h => h.Id) + 1 : 1,
                    EmployeeId = employee.Id,
                    ChangeType = ChangeType.PositionChange,
                    OldValue = dataManager.Positions.FirstOrDefault(p => p.Id == employee.PositionId)?.Name ?? "N/A",
                    NewValue = dataManager.Positions.FirstOrDefault(p => p.Id == (int)cmbPosition.SelectedValue)?.Name ?? "N/A",
                    ChangeDate = DateTime.Now
                };
                dataManager.ChangeHistories.Add(history);
            }

            // Track salary changes
            decimal newSalary = decimal.Parse(txtSalary.Text);
            if (employee.CurrentSalary != newSalary)
            {
                var history = new ChangeHistory
                {
                    Id = dataManager.ChangeHistories.Any() ? dataManager.ChangeHistories.Max(h => h.Id) + 1 : 1,
                    EmployeeId = employee.Id,
                    ChangeType = ChangeType.SalaryChange,
                    OldValue = employee.CurrentSalary.ToString(),
                    NewValue = newSalary.ToString(),
                    ChangeDate = DateTime.Now
                };
                dataManager.ChangeHistories.Add(history);
            }

            // Track department changes
            if (employee.DepartmentId != (int)cmbDepartment.SelectedValue)
            {
                var history = new ChangeHistory
                {
                    Id = dataManager.ChangeHistories.Any() ? dataManager.ChangeHistories.Max(h => h.Id) + 1 : 1,
                    EmployeeId = employee.Id,
                    ChangeType = ChangeType.DepartmentTransfer,
                    OldValue = dataManager.Departments.FirstOrDefault(d => d.Id == employee.DepartmentId)?.Name ?? "N/A",
                    NewValue = dataManager.Departments.FirstOrDefault(d => d.Id == (int)cmbDepartment.SelectedValue)?.Name ?? "N/A",
                    ChangeDate = DateTime.Now
                };
                dataManager.ChangeHistories.Add(history);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("First name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }

            if (!int.TryParse(txtIdentityNumber.Text, out _))
            {
                MessageBox.Show("Identity number must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdentityNumber.Focus();
                return false;
            }

            if (!decimal.TryParse(txtSalary.Text, out decimal salary) || salary < 0)
            {
                MessageBox.Show("Salary must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSalary.Focus();
                return false;
            }

            if (cmbDepartment.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a department.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbPosition.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a position.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(600, 650);
            this.Name = "EmployeeEditForm";
            this.Text = "Employee Edit";
            this.ResumeLayout(false);
        }
    }
}