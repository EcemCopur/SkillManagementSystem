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
    public partial class SkillEditForm : Form
    {
        private DataManager dataManager;
        private Skill skill;
        private bool isEditMode;
        private TextBox txtName, txtDescription;
        private ComboBox cmbCategory;
        private Button btnSave, btnCancel;

        public SkillEditForm(DataManager manager, Skill sk)
        {
            dataManager = manager;
            skill = sk;
            isEditMode = sk != null;
            InitializeComponent();
            InitializeCustomComponents();
            if (isEditMode) LoadSkillData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Skill" : "Add New Skill";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lw = 100, cw = 320, lm = 30, clm = lm + lw + 10, y = 20, vs = 50;

            var title = new Label { Text = this.Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 50;

            AddLabel("Name:", lm, y);
            txtName = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtName);
            y += vs;

            AddLabel("Description:", lm, y);
            txtDescription = new TextBox { Location = new Point(clm, y), Width = cw, Height = 60, Multiline = true };
            this.Controls.Add(txtDescription);
            y += 70;

            AddLabel("Category:", lm, y);
            cmbCategory = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { SkillType.Technical, SkillType.Soft, SkillType.NonClassified });
            cmbCategory.SelectedIndex = 0;
            this.Controls.Add(cmbCategory);
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
            var label = new Label { Text = text, Location = new Point(x, y + 3), Width = 100, TextAlign = ContentAlignment.MiddleRight };
            this.Controls.Add(label);
        }

        private void LoadSkillData()
        {
            txtName.Text = skill.Name;
            txtDescription.Text = skill.Description;
            cmbCategory.SelectedItem = skill.Category;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Skill name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isEditMode)
            {
                skill = new Skill { Id = dataManager.Skills.Any() ? dataManager.Skills.Max(s => s.Id) + 1 : 1 };
                dataManager.Skills.Add(skill);
            }

            skill.Name = txtName.Text.Trim();
            skill.Description = txtDescription.Text.Trim();
            skill.Category = (SkillType)cmbCategory.SelectedItem;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(500, 350);
            this.Name = "SkillEditForm";
            this.ResumeLayout(false);
        }
    }
}
