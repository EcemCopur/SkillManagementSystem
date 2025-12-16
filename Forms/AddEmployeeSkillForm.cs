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
    public partial class AddEmployeeSkillForm : Form
    {
        private DataManager dataManager;
        private Employee employee;
        private ComboBox cmbSkill, cmbLevel, cmbSource;
        private DateTimePicker dtpAcquisitionDate;
        private Button btnSave, btnCancel;

        public AddEmployeeSkillForm(DataManager manager, Employee emp)
        {
            dataManager = manager;
            employee = emp;

            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Add Skill to Employee";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int labelWidth = 120;
            int controlWidth = 300;
            int leftMargin = 30;
            int controlLeftMargin = leftMargin + labelWidth + 10;
            int currentY = 30;
            int verticalSpacing = 50;

            // Title
            var titleLabel = new Label
            {
                Text = $"Add Skill for {employee.FullName}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(leftMargin, currentY),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);
            currentY += 40;

            // Skill
            AddLabel("Skill:", leftMargin, currentY);
            cmbSkill = new ComboBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSkill.DisplayMember = "Name";
            cmbSkill.ValueMember = "Id";

            // Only show skills not already added
            var existingSkillIds = dataManager.EmployeeSkills
                .Where(es => es.EmployeeId == employee.Id)
                .Select(es => es.SkillId)
                .ToList();

            var availableSkills = dataManager.Skills
                .Where(s => !existingSkillIds.Contains(s.Id))
                .ToList();

            cmbSkill.DataSource = availableSkills;
            this.Controls.Add(cmbSkill);
            currentY += verticalSpacing;

            // Level
            AddLabel("Skill Level:", leftMargin, currentY);
            cmbLevel = new ComboBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLevel.Items.AddRange(new object[]
            {
            SkillDegree.Beginner,
            SkillDegree.Developing,
            SkillDegree.Competent,
            SkillDegree.Advanced,
            SkillDegree.Expert
            });
            cmbLevel.SelectedIndex = 0;
            this.Controls.Add(cmbLevel);
            currentY += verticalSpacing;

            // Source
            AddLabel("Source:", leftMargin, currentY);
            cmbSource = new ComboBox
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSource.Items.AddRange(new object[] { SkillSource.Training, SkillSource.Previous });
            cmbSource.SelectedIndex = 0;
            this.Controls.Add(cmbSource);
            currentY += verticalSpacing;

            // Acquisition Date
            AddLabel("Acquisition Date:", leftMargin, currentY);
            dtpAcquisitionDate = new DateTimePicker
            {
                Location = new Point(controlLeftMargin, currentY),
                Width = controlWidth,
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpAcquisitionDate);
            currentY += verticalSpacing + 10;

            // Buttons
            btnSave = new Button
            {
                Text = "Add Skill",
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
                Width = 120,
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(label);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbSkill.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a skill.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newSkill = new EmployeeSkill
            {
                EmployeeId = employee.Id,
                SkillId = (int)cmbSkill.SelectedValue,
                CurrentLevel = (SkillDegree)cmbLevel.SelectedItem,
                SkillSource = (SkillSource)cmbSource.SelectedItem,
                AcquisitionDate = dtpAcquisitionDate.Value
            };

            dataManager.EmployeeSkills.Add(newSkill);

            // Track skill addition in change history
            var history = new ChangeHistory
            {
                Id = dataManager.ChangeHistories.Any() ? dataManager.ChangeHistories.Max(h => h.Id) + 1 : 1,
                EmployeeId = employee.Id,
                ChangeType = ChangeType.SkillDegreeChange,
                OldValue = "No Skill",
                NewValue = $"{cmbSkill.Text} - {cmbLevel.SelectedItem}",
                ChangeDate = DateTime.Now,
                Notes = "Skill added"
            };
            dataManager.ChangeHistories.Add(history);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 350);
            this.Name = "AddEmployeeSkillForm";
            this.Text = "Add Employee Skill";
            this.ResumeLayout(false);
        }
    }
}
