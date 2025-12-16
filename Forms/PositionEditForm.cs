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
    public partial class PositionEditForm : Form
    {
        private DataManager dataManager;
        private Position position;
        private bool isEditMode;
        private TextBox txtName, txtCapacity, txtLevel, txtMinSalary, txtMaxSalary, txtHiringCost;
        private ComboBox cmbDepartment, cmbReportsTo;
        private Button btnSave, btnCancel;

        public PositionEditForm(DataManager manager, Position pos)
        {
            dataManager = manager;
            position = pos;
            isEditMode = pos != null;
            InitializeComponent();
            InitializeCustomComponents();
            if (isEditMode) LoadPositionData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Position" : "Add New Position";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lw = 120, cw = 400, lm = 30, clm = lm + lw + 10, y = 20, vs = 45;

            var title = new Label { Text = this.Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 50;

            AddLabel("Name:", lm, y);
            txtName = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtName);
            y += vs;

            AddLabel("Department:", lm, y);
            cmbDepartment = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            cmbDepartment.DataSource = dataManager.Departments.ToList();
            this.Controls.Add(cmbDepartment);
            y += vs;

            AddLabel("Capacity:", lm, y);
            txtCapacity = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtCapacity);
            y += vs;

            AddLabel("Position Level:", lm, y);
            txtLevel = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtLevel);
            y += vs;

            AddLabel("Min Salary:", lm, y);
            txtMinSalary = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtMinSalary);
            y += vs;

            AddLabel("Max Salary:", lm, y);
            txtMaxSalary = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtMaxSalary);
            y += vs;

            AddLabel("Hiring Cost:", lm, y);
            txtHiringCost = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtHiringCost);
            y += vs;

            AddLabel("Reports To:", lm, y);
            cmbReportsTo = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportsTo.DisplayMember = "Name";
            cmbReportsTo.ValueMember = "Id";
            var positions = dataManager.Positions.ToList();
            positions.Insert(0, new Position { Id = 0, Name = "None" });
            cmbReportsTo.DataSource = positions;
            this.Controls.Add(cmbReportsTo);
            y += vs + 20;

            btnSave = new Button { Text = "Save", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(clm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label { Text = text, Location = new Point(x, y + 3), Width = 120, TextAlign = ContentAlignment.MiddleRight };
            this.Controls.Add(label);
        }

        private void LoadPositionData()
        {
            txtName.Text = position.Name;
            cmbDepartment.SelectedValue = position.DepartmentId;
            txtCapacity.Text = position.Capacity.ToString();
            txtLevel.Text = position.PositionLevel.ToString();
            txtMinSalary.Text = position.MinSalary.ToString();
            txtMaxSalary.Text = position.MaxSalary.ToString();
            txtHiringCost.Text = position.HiringCost.ToString();
            cmbReportsTo.SelectedValue = position.ReportsToPositionId ?? 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (!isEditMode)
                {
                    position = new Position { Id = dataManager.Positions.Any() ? dataManager.Positions.Max(p => p.Id) + 1 : 1 };
                    dataManager.Positions.Add(position);
                }

                position.Name = txtName.Text.Trim();
                position.DepartmentId = (int)cmbDepartment.SelectedValue;
                position.Capacity = int.Parse(txtCapacity.Text);
                position.PositionLevel = int.Parse(txtLevel.Text);
                position.MinSalary = decimal.Parse(txtMinSalary.Text);
                position.MaxSalary = decimal.Parse(txtMaxSalary.Text);
                position.HiringCost = decimal.Parse(txtHiringCost.Text);

                int reportsTo = (int)cmbReportsTo.SelectedValue;
                position.ReportsToPositionId = reportsTo == 0 ? (int?)null : reportsTo;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving position: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Position name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtCapacity.Text, out int cap) || cap < 0)
            {
                MessageBox.Show("Capacity must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtLevel.Text, out int level) || level < 1)
            {
                MessageBox.Show("Position level must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtMinSalary.Text, out decimal minSal) || minSal < 0)
            {
                MessageBox.Show("Min salary must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtMaxSalary.Text, out decimal maxSal) || maxSal < minSal)
            {
                MessageBox.Show("Max salary must be greater than or equal to min salary.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 650);
            this.Name = "PositionEditForm";
            this.ResumeLayout(false);
        }
    }
}
