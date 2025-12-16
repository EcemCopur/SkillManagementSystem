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
    public partial class AddTrainingPrereqTrainingForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private ComboBox cmbTraining;
        private Button btnSave, btnCancel;

        public AddTrainingPrereqTrainingForm(DataManager manager, Training train)
        {
            dataManager = manager;
            training = train;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Add Prerequisite Training";
            this.Size = new Size(450, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lm = 30, clm = 150, y = 30, vs = 50, cw = 250;

            AddLabel("Training:", lm, y);
            cmbTraining = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTraining.DisplayMember = "Name";
            cmbTraining.ValueMember = "Id";
            var existing = dataManager.TrainingPrerequisiteTrainings.Where(tpt => tpt.TrainingId == training.Id).Select(tpt => tpt.PrerequisiteTrainingId).ToList();
            existing.Add(training.Id); // Can't be prerequisite to itself
            cmbTraining.DataSource = dataManager.Trainings.Where(t => !existing.Contains(t.Id)).ToList();
            this.Controls.Add(cmbTraining);
            y += vs + 10;

            btnSave = new Button { Text = "Add", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) =>
            {
                if (cmbTraining.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a training.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                dataManager.TrainingPrerequisiteTrainings.Add(new TrainingPrerequisiteTraining { TrainingId = training.Id, PrerequisiteTrainingId = (int)cmbTraining.SelectedValue });
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
            this.ClientSize = new Size(450, 200);
            this.Name = "AddTrainingPrereqTrainingForm";
            this.ResumeLayout(false);
        }
    }

}
