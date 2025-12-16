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
    public partial class ProcessEditForm : Form
    {
        private DataManager dataManager;
        private Process process;
        private bool isEditMode;
        private TextBox txtName, txtDescription, txtAimedWorkers;
        private CheckedListBox clbPositions;
        private Button btnSave, btnCancel;

        public ProcessEditForm(DataManager manager, Process proc)
        {
            dataManager = manager;
            process = proc;
            isEditMode = proc != null;
            InitializeComponent();
            InitializeCustomComponents();
            if (isEditMode) LoadProcessData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Process" : "Add New Process";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lm = 30, clm = 160, y = 20, vs = 45, cw = 380;

            var title = new Label { Text = this.Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 50;

            AddLabel("Process Name:", lm, y);
            txtName = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtName);
            y += vs;

            AddLabel("Description:", lm, y);
            txtDescription = new TextBox { Location = new Point(clm, y), Width = cw, Height = 80, Multiline = true };
            this.Controls.Add(txtDescription);
            y += 90;

            AddLabel("Aimed # of Workers:", lm, y);
            txtAimedWorkers = new TextBox { Location = new Point(clm, y), Width = 100 };
            this.Controls.Add(txtAimedWorkers);
            y += vs;

            AddLabel("Assign to Positions:", lm, y);
            clbPositions = new CheckedListBox { Location = new Point(clm, y), Width = cw, Height = 180 };
            clbPositions.DisplayMember = "Name";
            foreach (var pos in dataManager.Positions)
            {
                clbPositions.Items.Add(pos, false);
            }
            this.Controls.Add(clbPositions);
            y += 190;

            btnSave = new Button { Text = "Save", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(clm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label { Text = text, Location = new Point(x, y + 3), Width = 130, TextAlign = ContentAlignment.MiddleRight };
            this.Controls.Add(label);
        }

        private void LoadProcessData()
        {
            txtName.Text = process.Name;
            txtDescription.Text = process.ProcessDescription;
            txtAimedWorkers.Text = process.AimedNumberOfWorkers.ToString();

            var assignedPositionIds = dataManager.PositionProcesses
                .Where(pp => pp.ProcessId == process.Id)
                .Select(pp => pp.PositionId)
                .ToList();

            for (int i = 0; i < clbPositions.Items.Count; i++)
            {
                var pos = (Position)clbPositions.Items[i];
                if (assignedPositionIds.Contains(pos.Id))
                {
                    clbPositions.SetItemChecked(i, true);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (!isEditMode)
                {
                    process = new Process { Id = dataManager.Processes.Any() ? dataManager.Processes.Max(p => p.Id) + 1 : 1 };
                    dataManager.Processes.Add(process);
                }
                else
                {
                    // Remove existing position assignments
                    dataManager.PositionProcesses.RemoveAll(pp => pp.ProcessId == process.Id);
                }

                process.Name = txtName.Text.Trim();
                process.ProcessDescription = txtDescription.Text.Trim();
                process.AimedNumberOfWorkers = int.Parse(txtAimedWorkers.Text);

                // Add position assignments
                foreach (Position pos in clbPositions.CheckedItems)
                {
                    dataManager.PositionProcesses.Add(new PositionProcess
                    {
                        PositionId = pos.Id,
                        ProcessId = process.Id
                    });
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Process name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtAimedWorkers.Text, out int aimed) || aimed < 1)
            {
                MessageBox.Show("Aimed number of workers must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 600);
            this.Name = "ProcessEditForm";
            this.ResumeLayout(false);
        }
    }
}
