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
    
    public partial class SkillManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView skillGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private ComboBox cmbTypeFilter;

        public SkillManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadSkills();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Skill Management";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblFilter = new Label { Text = "Filter by Type:", Location = new Point(10, 15), AutoSize = true };
            topPanel.Controls.Add(lblFilter);

            cmbTypeFilter = new ComboBox { Location = new Point(120, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTypeFilter.Items.AddRange(new object[] { "All", "Technical", "Soft", "Non-Classified" });
            cmbTypeFilter.SelectedIndex = 0;
            cmbTypeFilter.SelectedIndexChanged += (s, e) => FilterSkills();
            topPanel.Controls.Add(cmbTypeFilter);

            btnAdd = CreateButton("Add New", 10, 45, btnAdd_Click);
            btnEdit = CreateButton("Edit", 110, 45, btnEdit_Click);
            btnDelete = CreateButton("Delete", 210, 45, btnDelete_Click);
            btnRefresh = CreateButton("Refresh", 310, 45, (s, e) => LoadSkills());

            topPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(topPanel);

            var skillGridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 80, 20, 20),
                BackColor = Color.LightGray
            };

            this.Controls.Add(skillGridContainer);

            skillGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            skillGrid.DoubleClick += btnEdit_Click;
            skillGridContainer.Controls.Add(skillGrid);
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = 90, Height = 30 };
            btn.Click += clickHandler;
            return btn;
        }

        private void LoadSkills()
        {
            var skills = dataManager.Skills.Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                Category = s.Category.ToString(),
                LevelRange = $"{s.MinLevel} - {s.MaxLevel}"
            }).ToList();

            skillGrid.DataSource = skills;
        }

        private void FilterSkills()
        {
            if (cmbTypeFilter.SelectedIndex == 0)
            {
                LoadSkills();
                return;
            }

            var selectedType = (SkillType)(cmbTypeFilter.SelectedIndex);
            var skills = dataManager.Skills.Where(s => s.Category == selectedType).Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                Category = s.Category.ToString(),
                LevelRange = $"{s.MinLevel} - {s.MaxLevel}"
            }).ToList();

            skillGrid.DataSource = skills;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new SkillEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK) LoadSkills();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (skillGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a skill to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int skillId = (int)skillGrid.SelectedRows[0].Cells["Id"].Value;
            var skill = dataManager.Skills.FirstOrDefault(s => s.Id == skillId);

            if (skill != null)
            {
                var form = new SkillEditForm(dataManager, skill);
                if (form.ShowDialog() == DialogResult.OK) LoadSkills();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (skillGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a skill to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int skillId = (int)skillGrid.SelectedRows[0].Cells["Id"].Value;

            if (dataManager.EmployeeSkills.Any(es => es.SkillId == skillId))
            {
                MessageBox.Show("Cannot delete skill that is assigned to employees.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this skill?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                dataManager.Skills.RemoveAll(s => s.Id == skillId);
                LoadSkills();
                MessageBox.Show("Skill deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1000, 600);
            this.Name = "SkillManagementForm";
            this.ResumeLayout(false);
        }
    }
}
