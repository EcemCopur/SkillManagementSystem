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
    public partial class TrainingEditForm : Form
    {
        private DataManager dataManager;
        private Training training;
        private bool isEditMode;
        private TextBox txtName, txtDescription, txtCapacity, txtCost, txtDuration;
        private ComboBox cmbDepartment, cmbApplicationType, cmbSource, cmbCostType, cmbStatus;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private Button btnSave, btnCancel, btnManageSkills, btnManagePrereqSkills, btnManagePrereqTrainings;

        public TrainingEditForm(DataManager manager, Training train)
        {
            dataManager = manager;
            training = train;
            isEditMode = train != null;
            InitializeComponent();
            InitializeCustomComponents();
            if (isEditMode) LoadTrainingData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Training" : "Add New Training";
            this.Size = new Size(700, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.AutoScroll = true;

            int lm = 30, clm = 180, y = 20, vs = 45, cw = 450;

            var title = new Label { Text = this.Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 50;

            // Basic Info
            var grpBasic = new GroupBox { Text = "Basic Information", Location = new Point(lm, y), Width = 630, Height = 250 };
            int gy = 25;

            AddLabelTo(grpBasic, "Training Name:", 10, gy);
            txtName = new TextBox { Location = new Point(150, gy), Width = 450 };
            grpBasic.Controls.Add(txtName);
            gy += vs;

            AddLabelTo(grpBasic, "Description:", 10, gy);
            txtDescription = new TextBox { Location = new Point(150, gy), Width = 450, Height = 60, Multiline = true };
            grpBasic.Controls.Add(txtDescription);
            gy += 70;

            AddLabelTo(grpBasic, "Target Department:", 10, gy);
            cmbDepartment = new ComboBox { Location = new Point(150, gy), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            cmbDepartment.DataSource = dataManager.Departments.ToList();
            grpBasic.Controls.Add(cmbDepartment);
            gy += vs;

            AddLabelTo(grpBasic, "Status:", 10, gy);
            cmbStatus = new ComboBox { Location = new Point(150, gy), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { TrainingStatus.Suggested, TrainingStatus.Assigned, TrainingStatus.Planned, TrainingStatus.Ongoing, TrainingStatus.Completed, TrainingStatus.Cancelled });
            cmbStatus.SelectedIndex = 0;
            grpBasic.Controls.Add(cmbStatus);

            this.Controls.Add(grpBasic);
            y += 260;

            // Training Details
            var grpDetails = new GroupBox { Text = "Training Details", Location = new Point(lm, y), Width = 630, Height = 200 };
            gy = 25;

            AddLabelTo(grpDetails, "Application Type:", 10, gy);
            cmbApplicationType = new ComboBox { Location = new Point(150, gy), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbApplicationType.Items.AddRange(new object[] { ApplicationType.Group, ApplicationType.Individual });
            cmbApplicationType.SelectedIndex = 0;
            grpDetails.Controls.Add(cmbApplicationType);
            gy += vs;

            AddLabelTo(grpDetails, "Capacity:", 10, gy);
            txtCapacity = new TextBox { Location = new Point(150, gy), Width = 100 };
            grpDetails.Controls.Add(txtCapacity);
            gy += vs;

            AddLabelTo(grpDetails, "Source:", 10, gy);
            cmbSource = new ComboBox { Location = new Point(150, gy), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSource.Items.AddRange(new object[] { TrainingSource.Internal, TrainingSource.External });
            cmbSource.SelectedIndex = 0;
            grpDetails.Controls.Add(cmbSource);
            gy += vs;

            AddLabelTo(grpDetails, "Duration (Hours):", 10, gy);
            txtDuration = new TextBox { Location = new Point(150, gy), Width = 100 };
            grpDetails.Controls.Add(txtDuration);

            this.Controls.Add(grpDetails);
            y += 210;

            // Cost Info
            var grpCost = new GroupBox { Text = "Cost Information", Location = new Point(lm, y), Width = 630, Height = 100 };
            gy = 25;

            AddLabelTo(grpCost, "Cost Type:", 10, gy);
            cmbCostType = new ComboBox { Location = new Point(150, gy), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCostType.Items.AddRange(new object[] { CostType.PerAttendee, CostType.Total });
            cmbCostType.SelectedIndex = 0;
            grpCost.Controls.Add(cmbCostType);

            AddLabelTo(grpCost, "Cost:", 320, gy);
            txtCost = new TextBox { Location = new Point(370, gy), Width = 150 };
            grpCost.Controls.Add(txtCost);

            this.Controls.Add(grpCost);
            y += 110;

            // Schedule
            var grpSchedule = new GroupBox { Text = "Schedule", Location = new Point(lm, y), Width = 630, Height = 100 };
            gy = 25;

            AddLabelTo(grpSchedule, "Start Date:", 10, gy);
            dtpStartDate = new DateTimePicker { Location = new Point(150, gy), Width = 200, Format = DateTimePickerFormat.Short };
            grpSchedule.Controls.Add(dtpStartDate);

            AddLabelTo(grpSchedule, "End Date:", 370, gy);
            dtpEndDate = new DateTimePicker { Location = new Point(450, gy), Width = 150, Format = DateTimePickerFormat.Short };
            grpSchedule.Controls.Add(dtpEndDate);

            this.Controls.Add(grpSchedule);
            y += 110;

            // Management Buttons
            var grpManage = new GroupBox { Text = "Manage Prerequisites & Skills", Location = new Point(lm, y), Width = 630, Height = 80 };
            gy = 25;

            btnManageSkills = new Button { Text = "Manage Skills", Location = new Point(10, gy), Width = 150, Height = 30 };
            btnManageSkills.Click += BtnManageSkills_Click;
            grpManage.Controls.Add(btnManageSkills);

            btnManagePrereqSkills = new Button { Text = "Prereq Skills", Location = new Point(170, gy), Width = 150, Height = 30 };
            btnManagePrereqSkills.Click += BtnManagePrereqSkills_Click;
            grpManage.Controls.Add(btnManagePrereqSkills);

            btnManagePrereqTrainings = new Button { Text = "Prereq Trainings", Location = new Point(330, gy), Width = 150, Height = 30 };
            btnManagePrereqTrainings.Click += BtnManagePrereqTrainings_Click;
            grpManage.Controls.Add(btnManagePrereqTrainings);

            this.Controls.Add(grpManage);
            y += 90;

            // Save/Cancel
            btnSave = new Button { Text = "Save", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(clm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabelTo(Control parent, string text, int x, int y)
        {
            var label = new Label { Text = text, Location = new Point(x, y + 3), Width = 130, TextAlign = ContentAlignment.MiddleRight };
            parent.Controls.Add(label);
        }

        private void LoadTrainingData()
        {
            txtName.Text = training.Name;
            txtDescription.Text = training.Description;
            cmbDepartment.SelectedValue = training.TargetDepartmentId;
            cmbStatus.SelectedItem = training.Status;
            cmbApplicationType.SelectedItem = training.ApplicationType;
            txtCapacity.Text = training.Capacity.ToString();
            cmbSource.SelectedItem = training.Source;
            txtDuration.Text = training.DurationHours.ToString();
            cmbCostType.SelectedItem = training.CostType;
            txtCost.Text = training.Cost.ToString();

            if (training.StartDate.HasValue)
                dtpStartDate.Value = training.StartDate.Value;
            if (training.EndDate.HasValue)
                dtpEndDate.Value = training.EndDate.Value;
        }

        private void BtnManageSkills_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
                MessageBox.Show("Please save the training first before managing skills.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var form = new TrainingSkillsForm(dataManager, training);
            form.ShowDialog();
        }

        private void BtnManagePrereqSkills_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
                MessageBox.Show("Please save the training first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var form = new TrainingPrereqSkillsForm(dataManager, training);
            form.ShowDialog();
        }

        private void BtnManagePrereqTrainings_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
                MessageBox.Show("Please save the training first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var form = new TrainingPrereqTrainingsForm(dataManager, training);
            form.ShowDialog();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (!isEditMode)
                {
                    training = new Training { Id = dataManager.Trainings.Any() ? dataManager.Trainings.Max(t => t.Id) + 1 : 1 };
                    dataManager.Trainings.Add(training);
                }

                training.Name = txtName.Text.Trim();
                training.Description = txtDescription.Text.Trim();
                training.TargetDepartmentId = (int)cmbDepartment.SelectedValue;
                training.Status = (TrainingStatus)cmbStatus.SelectedItem;
                training.ApplicationType = (ApplicationType)cmbApplicationType.SelectedItem;
                training.Capacity = int.Parse(txtCapacity.Text);
                training.Source = (TrainingSource)cmbSource.SelectedItem;
                training.DurationHours = int.Parse(txtDuration.Text);
                training.CostType = (CostType)cmbCostType.SelectedItem;
                training.Cost = decimal.Parse(txtCost.Text);
                training.StartDate = dtpStartDate.Value;
                training.EndDate = dtpEndDate.Value;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving training: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Training name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtCapacity.Text, out int cap) || cap < 1)
            {
                MessageBox.Show("Capacity must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtDuration.Text, out int dur) || dur < 1)
            {
                MessageBox.Show("Duration must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtCost.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Cost must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(700, 750);
            this.Name = "TrainingEditForm";
            this.ResumeLayout(false);
        }
    }
}
