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
    public partial class AddProcessSkillForm : Form
    {
        private DataManager dataManager;
        private Process process;
        private ComboBox cmbSkill, cmbLevel;
        private Button btnSave, btnCancel;

        public AddProcessSkillForm(DataManager manager, Process proc)
        {
            dataManager = manager;
            process = proc;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Add Required Skill";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lm = 30, clm = 150, y = 30, vs = 50, cw = 250;

            var title = new Label { Text = "Add Required Skill", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 40;

            AddLabel("Skill:", lm, y);
            cmbSkill = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSkill.DisplayMember = "Name";
            cmbSkill.ValueMember = "Id";
            var existingSkillIds = dataManager.ProcessRequiredSkills.Where(prs => prs.ProcessId == process.Id).Select(prs => prs.SkillId).ToList();
            cmbSkill.DataSource = dataManager.Skills.Where(s => !existingSkillIds.Contains(s.Id)).ToList();
            this.Controls.Add(cmbSkill);
            y += vs;

            AddLabel("Required Level:", lm, y);
            cmbLevel = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLevel.Items.AddRange(new object[] { SkillDegree.Beginner, SkillDegree.Developing, SkillDegree.Competent, SkillDegree.Advanced, SkillDegree.Expert });
            cmbLevel.SelectedIndex = 2;
            this.Controls.Add(cmbLevel);
            y += vs + 10;

            btnSave = new Button { Text = "Add", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(clm + 110, y), Width = 100, Height = 35 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label { Text = text, Location = new Point(x, y + 3), Width = 120, TextAlign = ContentAlignment.MiddleRight };
            this.Controls.Add(label);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbSkill.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a skill.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dataManager.ProcessRequiredSkills.Add(new ProcessRequiredSkill
            {
                ProcessId = process.Id,
                SkillId = (int)cmbSkill.SelectedValue,
                RequiredLevel = (int)cmbLevel.SelectedItem
            });

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(450, 250);
            this.Name = "AddProcessSkillForm";
            this.ResumeLayout(false);
        }
    }
}
