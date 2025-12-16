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
    public partial class TrainingSessionEditForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private TrainingSession session;
        private bool isEditMode;
        private DateTimePicker dtpStart, dtpEnd;
        private ComboBox cmbStatus;
        private TextBox txtEnrollment, txtTrainer;
        private Button btnSave, btnCancel;

        public TrainingSessionEditForm(DataManager manager, Training train, TrainingSession sess)
        {
            dataManager = manager;
            training = train;
            session = sess;
            isEditMode = sess != null;
            InitializeComponent();
            InitializeCustomComponents();
            if (isEditMode) LoadSessionData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Session" : "Add Session";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lm = 30, clm = 160, y = 30, vs = 50, cw = 280;

            var title = new Label { Text = this.Text, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 40;

            AddLabel("Start Date:", lm, y);
            dtpStart = new DateTimePicker { Location = new Point(clm, y), Width = cw, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpStart);
            y += vs;

            AddLabel("End Date:", lm, y);
            dtpEnd = new DateTimePicker { Location = new Point(clm, y), Width = cw, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpEnd);
            y += vs;

            AddLabel("Status:", lm, y);
            cmbStatus = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { TrainingStatus.Planned, TrainingStatus.Ongoing, TrainingStatus.Completed, TrainingStatus.Cancelled });
            cmbStatus.SelectedIndex = 0;
            this.Controls.Add(cmbStatus);
            y += vs;

            AddLabel("Current Enrollment:", lm, y);
            txtEnrollment = new TextBox { Location = new Point(clm, y), Width = 100 };
            txtEnrollment.Text = "0";
            this.Controls.Add(txtEnrollment);
            y += vs;

            AddLabel("Trainer Name:", lm, y);
            txtTrainer = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtTrainer);
            y += vs + 10;

            btnSave = new Button { Text = "Save", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(clm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), Width = 130, TextAlign = ContentAlignment.MiddleRight });
        }

        private void LoadSessionData()
        {
            dtpStart.Value = session.SessionStartDate;
            dtpEnd.Value = session.SessionEndDate;
            cmbStatus.SelectedItem = session.Status;
            txtEnrollment.Text = session.CurrentEnrollmentCount.ToString();
            txtTrainer.Text = session.TrainerName;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtEnrollment.Text, out int enrollment) || enrollment < 0)
            {
                MessageBox.Show("Enrollment must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (enrollment > training.Capacity)
            {
                MessageBox.Show($"Enrollment cannot exceed training capacity ({training.Capacity}).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpEnd.Value < dtpStart.Value)
            {
                MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isEditMode)
            {
                session = new TrainingSession { Id = dataManager.TrainingSessions.Any() ? dataManager.TrainingSessions.Max(ts => ts.Id) + 1 : 1, TrainingId = training.Id };
                dataManager.TrainingSessions.Add(session);
            }

            session.SessionStartDate = dtpStart.Value;
            session.SessionEndDate = dtpEnd.Value;
            session.Status = (TrainingStatus)cmbStatus.SelectedItem;
            session.CurrentEnrollmentCount = enrollment;
            session.TrainerName = txtTrainer.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(500, 400);
            this.Name = "TrainingSessionEditForm";
            this.ResumeLayout(false);
        }
    }
}
