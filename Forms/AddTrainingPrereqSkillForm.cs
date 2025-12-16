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
    public partial class AddTrainingPrereqSkillForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private ComboBox cmbSkill, cmbLevel;
        private Button btnSave, btnCancel;

        public AddTrainingPrereqSkillForm(DataManager manager, Training train)
        {
            dataManager = manager;
            training = train;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Add Prerequisite Skill";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lm = 30, clm = 150, y = 30, vs = 50, cw = 250;

            AddLabel("Skill:", lm, y);
            cmbSkill = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSkill.DisplayMember = "Name";
            cmbSkill.ValueMember = "Id";
            var existing = dataManager.TrainingPrerequisiteSkills.Where(tps => tps.TrainingId == training.Id).Select(tps => tps.SkillId).ToList();
            cmbSkill.DataSource = dataManager.Skills.Where(s => !existing.Contains(s.Id)).ToList();
            this.Controls.Add(cmbSkill);
            y += vs;

            AddLabel("Minimum Level:", lm, y);
            cmbLevel = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLevel.Items.AddRange(new object[] { SkillDegree.Beginner, SkillDegree.Developing, SkillDegree.Competent, SkillDegree.Advanced, SkillDegree.Expert });
            cmbLevel.SelectedIndex = 2;
            this.Controls.Add(cmbLevel);
            y += vs + 10;

            btnSave = new Button { Text = "Add", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) =>
            {
                if (cmbSkill.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a skill.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                dataManager.TrainingPrerequisiteSkills.Add(new TrainingPrerequisiteSkill { TrainingId = training.Id, SkillId = (int)cmbSkill.SelectedValue, MinimumLevel = (int)cmbLevel.SelectedItem });
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(clm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), Width = 120, TextAlign = ContentAlignment.MiddleRight });
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(450, 250);
            this.Name = "AddTrainingPrereqSkillForm";
            this.ResumeLayout(false);
        }
    }
}
