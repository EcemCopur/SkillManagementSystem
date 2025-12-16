using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    public partial class EmployeeSkillsForm : Form
    {
        private DataManager dataManager;
        private Employee employee;
        private DataGridView skillsGrid;
        private Button btnAddSkill, btnRemoveSkill, btnClose;
        private Label lblEmployeeName;

        public EmployeeSkillsForm(DataManager manager, Employee emp)
        {
            dataManager = manager;
            employee = emp;

            InitializeComponent();
            InitializeCustomComponents();
            LoadEmployeeSkills();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Employee Skills";
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

            lblEmployeeName = new Label
            {
                Text = $"Skills for: {employee.FullName}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            topPanel.Controls.Add(lblEmployeeName);

            var lblInfo = new Label
            {
                Text = "Manage employee skills and their proficiency levels",
                Location = new Point(10, 40),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            topPanel.Controls.Add(lblInfo);

            this.Controls.Add(topPanel);

            var skillGridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 80, 10, 10)
            };

            this.Controls.Add(skillGridPanel);

            // DataGridView for skills
            skillsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            skillGridPanel.Controls.Add(skillsGrid);

            // Bottom Panel for buttons
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            btnAddSkill = new Button
            {
                Text = "Add Skill",
                Location = new Point(10, 15),
                Width = 100,
                Height = 35,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddSkill.Click += BtnAddSkill_Click;
            bottomPanel.Controls.Add(btnAddSkill);

            btnRemoveSkill = new Button
            {
                Text = "Remove Skill",
                Location = new Point(120, 15),
                Width = 110,
                Height = 35
            };
            btnRemoveSkill.Click += BtnRemoveSkill_Click;
            bottomPanel.Controls.Add(btnRemoveSkill);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(770, 15),
                Width = 100,
                Height = 35
            };
            btnClose.Click += (s, e) => this.Close();
            bottomPanel.Controls.Add(btnClose);

            this.Controls.Add(bottomPanel);
        }

        private void LoadEmployeeSkills()
        {
            var employeeSkills = dataManager.EmployeeSkills
                .Where(es => es.EmployeeId == employee.Id)
                .Select(es => new
                {
                    SkillId = es.SkillId,
                    SkillName = dataManager.Skills.FirstOrDefault(s => s.Id == es.SkillId)?.Name ?? "Unknown",
                    Category = dataManager.Skills.FirstOrDefault(s => s.Id == es.SkillId)?.Category.ToString() ?? "N/A",
                    CurrentLevel = es.CurrentLevel.ToString(),
                    Source = es.SkillSource.ToString(),
                    AcquisitionDate = es.AcquisitionDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            skillsGrid.DataSource = employeeSkills;
        }

        private void BtnAddSkill_Click(object sender, EventArgs e)
        {
            var form = new AddEmployeeSkillForm(dataManager, employee);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadEmployeeSkills();
            }
        }

        private void BtnRemoveSkill_Click(object sender, EventArgs e)
        {
            if (skillsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a skill to remove.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to remove this skill from the employee?",
                "Confirm Remove",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var selectedRow = skillsGrid.SelectedRows[0];
                int skillId = (int)selectedRow.Cells["SkillId"].Value;

                dataManager.EmployeeSkills.RemoveAll(es =>
                    es.EmployeeId == employee.Id && es.SkillId == skillId);

                LoadEmployeeSkills();
                MessageBox.Show("Skill removed successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 600);
            this.Name = "EmployeeSkillsForm";
            this.Text = "Employee Skills";
            this.ResumeLayout(false);
        }
    }

    
}