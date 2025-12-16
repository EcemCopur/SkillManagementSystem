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
    public partial class TrainingPrereqTrainingsForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private ListBox lstPrereqTrainings;
        private Button btnAdd, btnRemove, btnClose;

        public TrainingPrereqTrainingsForm(DataManager manager, Training train)
        {
            dataManager = manager;
            training = train;
            InitializeComponent();
            InitializeCustomComponents();
            LoadPrereqTrainings();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"Prerequisite Trainings for: {training.Name}";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            var lblTitle = new Label { Text = "Required trainings before this one", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 15), AutoSize = true };
            topPanel.Controls.Add(lblTitle);
            this.Controls.Add(topPanel);

            lstPrereqTrainings = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 60, 10, 70) };
            gridPanel.Controls.Add(lstPrereqTrainings);
            this.Controls.Add(gridPanel);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };
            btnAdd = new Button { Text = "Add Training", Location = new Point(10, 15), Width = 120, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;
            btnRemove = new Button { Text = "Remove", Location = new Point(140, 15), Width = 100, Height = 35 };
            btnRemove.Click += BtnRemove_Click;
            btnClose = new Button { Text = "Close", Location = new Point(470, 15), Width = 100, Height = 35 };
            btnClose.Click += (s, e) => this.Close();
            bottomPanel.Controls.AddRange(new Control[] { btnAdd, btnRemove, btnClose });
            this.Controls.Add(bottomPanel);
        }

        private void LoadPrereqTrainings()
        {
            lstPrereqTrainings.Items.Clear();
            var prereqs = dataManager.TrainingPrerequisiteTrainings.Where(tpt => tpt.TrainingId == training.Id).ToList();
            foreach (var prereq in prereqs)
            {
                var prereqTraining = dataManager.Trainings.FirstOrDefault(t => t.Id == prereq.PrerequisiteTrainingId);
                if (prereqTraining != null)
                {
                    lstPrereqTrainings.Items.Add($"{prereqTraining.Name} (ID: {prereqTraining.Id})");
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddTrainingPrereqTrainingForm(dataManager, training);
            if (form.ShowDialog() == DialogResult.OK) LoadPrereqTrainings();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstPrereqTrainings.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a training to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selected = lstPrereqTrainings.SelectedItem.ToString();
            var idStr = selected.Substring(selected.IndexOf("ID: ") + 4).TrimEnd(')');
            int prereqId = int.Parse(idStr);
            dataManager.TrainingPrerequisiteTrainings.RemoveAll(tpt => tpt.TrainingId == training.Id && tpt.PrerequisiteTrainingId == prereqId);
            LoadPrereqTrainings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 450);
            this.Name = "TrainingPrereqTrainingsForm";
            this.ResumeLayout(false);
        }
    }
}
