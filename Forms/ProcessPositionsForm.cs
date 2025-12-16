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
    public partial class ProcessPositionsForm : Form
    {
        private DataManager dataManager;
        private Process process;
        private CheckedListBox clbPositions;
        private Button btnSave, btnCancel;

        public ProcessPositionsForm(DataManager manager, Process proc)
        {
            dataManager = manager;
            process = proc;
            InitializeComponent();
            InitializeCustomComponents();
            LoadPositions();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"Manage Positions for: {process.Name}";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lm = 30, y = 20;

            var title = new Label { Text = "Assign to Positions", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 40;

            clbPositions = new CheckedListBox { Location = new Point(lm, y), Width = 420, Height = 320 };
            clbPositions.DisplayMember = "Name";
            foreach (var pos in dataManager.Positions)
            {
                clbPositions.Items.Add(pos, false);
            }
            this.Controls.Add(clbPositions);
            y += 330;

            btnSave = new Button { Text = "Save", Location = new Point(lm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(lm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void LoadPositions()
        {
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
            dataManager.PositionProcesses.RemoveAll(pp => pp.ProcessId == process.Id);

            foreach (Position pos in clbPositions.CheckedItems)
            {
                dataManager.PositionProcesses.Add(new PositionProcess
                {
                    PositionId = pos.Id,
                    ProcessId = process.Id
                });
            }

            MessageBox.Show("Positions updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(500, 500);
            this.Name = "ProcessPositionsForm";
            this.ResumeLayout(false);
        }
    }
}
