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
    public partial class TrainingSessionsForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private DataGridView sessionsGrid;
        private Button btnAdd, btnEdit, btnDelete, btnClose;

        public TrainingSessionsForm(DataManager manager, Training train)
        {
            dataManager = manager;
            training = train;
            InitializeComponent();
            InitializeCustomComponents();
            LoadSessions();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"Training Sessions for: {training.Name}";
            this.Size = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(10) };
            var lblTitle = new Label { Text = "Training Sessions", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 15), AutoSize = true };
            topPanel.Controls.Add(lblTitle);
            this.Controls.Add(topPanel);

            sessionsGrid = new DataGridView
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
            gridPanel.Controls.Add(sessionsGrid);
            this.Controls.Add(gridPanel);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };
            btnAdd = new Button { Text = "Add Session", Location = new Point(10, 15), Width = 110, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;
            btnEdit = new Button { Text = "Edit", Location = new Point(130, 15), Width = 100, Height = 35 };
            btnEdit.Click += BtnEdit_Click;
            btnDelete = new Button { Text = "Delete", Location = new Point(240, 15), Width = 100, Height = 35 };
            btnDelete.Click += BtnDelete_Click;
            btnClose = new Button { Text = "Close", Location = new Point(770, 15), Width = 100, Height = 35 };
            btnClose.Click += (s, e) => this.Close();
            bottomPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnClose });
            this.Controls.Add(bottomPanel);
        }

        private void LoadSessions()
        {
            var sessions = dataManager.TrainingSessions.Where(ts => ts.TrainingId == training.Id).Select(ts => new
            {
                ts.Id,
                StartDate = ts.SessionStartDate.ToString("yyyy-MM-dd"),
                EndDate = ts.SessionEndDate.ToString("yyyy-MM-dd"),
                ts.Status,
                ts.CurrentEnrollmentCount,
                Capacity = training.Capacity,
                ts.TrainerName
            }).ToList();

            sessionsGrid.DataSource = sessions;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new TrainingSessionEditForm(dataManager, training, null);
            if (form.ShowDialog() == DialogResult.OK) LoadSessions();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (sessionsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a session to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int sessionId = (int)sessionsGrid.SelectedRows[0].Cells["Id"].Value;
            var session = dataManager.TrainingSessions.FirstOrDefault(ts => ts.Id == sessionId);
            if (session != null)
            {
                var form = new TrainingSessionEditForm(dataManager, training, session);
                if (form.ShowDialog() == DialogResult.OK) LoadSessions();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (sessionsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a session to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("Delete this training session?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                int sessionId = (int)sessionsGrid.SelectedRows[0].Cells["Id"].Value;
                dataManager.TrainingSessions.RemoveAll(ts => ts.Id == sessionId);
                dataManager.EmployeeTrainings.Where(et => et.TrainingSessionId == sessionId).ToList().ForEach(et => et.TrainingSessionId = null);
                LoadSessions();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(900, 500);
            this.Name = "TrainingSessionsForm";
            this.ResumeLayout(false);
        }
    }
}
