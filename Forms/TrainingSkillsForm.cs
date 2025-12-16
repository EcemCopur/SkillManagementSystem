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
    public partial class TrainingSkillsForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private DataGridView skillsGrid;
        private Button btnAdd, btnRemove, btnClose;

        public TrainingSkillsForm(DataManager manager, Training train)
        {
            dataManager = manager;
            training = train;
            InitializeComponent();
            InitializeCustomComponents();
            LoadSkills();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"Skills Taught in: {training.Name}";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            var lblTitle = new Label { Text = "Skills taught in this training", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 15), AutoSize = true };
            topPanel.Controls.Add(lblTitle);
            this.Controls.Add(topPanel);

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
            var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 60, 10, 70) };
            gridPanel.Controls.Add(skillsGrid);
            this.Controls.Add(gridPanel);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };
            btnAdd = new Button { Text = "Add Skill", Location = new Point(10, 15), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;
            btnRemove = new Button { Text = "Remove", Location = new Point(120, 15), Width = 100, Height = 35 };
            btnRemove.Click += BtnRemove_Click;
            btnClose = new Button { Text = "Close", Location = new Point(570, 15), Width = 100, Height = 35 };
            btnClose.Click += (s, e) => this.Close();
            bottomPanel.Controls.AddRange(new Control[] { btnAdd, btnRemove, btnClose });
            this.Controls.Add(bottomPanel);
        }

        private void LoadSkills()
        {
            var skills = dataManager.TrainingSkills.Where(ts => ts.TrainingId == training.Id).Select(ts => new
            {
                SkillId = ts.SkillId,
                SkillName = dataManager.Skills.FirstOrDefault(s => s.Id == ts.SkillId)?.Name ?? "Unknown",
                TargetLevel = ts.TargetLevel,
                TargetLevelName = ((SkillDegree)ts.TargetLevel).ToString()
            }).ToList();

            skillsGrid.DataSource = skills;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddTrainingSkillForm(dataManager, training);
            if (form.ShowDialog() == DialogResult.OK) LoadSkills();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (skillsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a skill to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int skillId = (int)skillsGrid.SelectedRows[0].Cells["SkillId"].Value;
            dataManager.TrainingSkills.RemoveAll(ts => ts.TrainingId == training.Id && ts.SkillId == skillId);
            LoadSkills();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(700, 500);
            this.Name = "TrainingSkillsForm";
            this.ResumeLayout(false);
        }
    }
}
