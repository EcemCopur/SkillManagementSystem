using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkillManagementSystem.Forms
{
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
