using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Forms
{
    // ==================== POSITION MANAGEMENT ====================
    public partial class PositionManagementForm : Form
    {
        private DataManager dataManager;
        private DataGridView positionGrid;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public PositionManagementForm(DataManager manager)
        {
            dataManager = manager;
            InitializeComponent();
            InitializeCustomComponents();
            LoadPositions();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Position Management";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnAdd = CreateButton("Add New", 10, 35, btnAdd_Click);
            btnEdit = CreateButton("Edit", 110, 35, btnEdit_Click);
            btnDelete = CreateButton("Delete", 210, 35, btnDelete_Click);
            btnRefresh = CreateButton("Refresh", 310, 35, (s, e) => LoadPositions());

            topPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(topPanel);

            positionGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            positionGrid.DoubleClick += btnEdit_Click;
            this.Controls.Add(positionGrid);
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var btn = new Button { Text = text, Location = new Point(x, y), Width = 90, Height = 30 };
            btn.Click += clickHandler;
            return btn;
        }

        private void LoadPositions()
        {
            var positions = dataManager.Positions.Select(p => new
            {
                p.Id,
                p.Name,
                Department = dataManager.Departments.FirstOrDefault(d => d.Id == p.DepartmentId)?.Name ?? "N/A",
                p.Capacity,
                p.PositionLevel,
                MinSalary = p.MinSalary.ToString("C"),
                MaxSalary = p.MaxSalary.ToString("C"),
                p.HiringCost,
                EmployeeCount = p.Employees?.Count ?? 0,
                OpenPositions = p.NumOpenPositions
            }).ToList();

            positionGrid.DataSource = positions;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new PositionEditForm(dataManager, null);
            if (form.ShowDialog() == DialogResult.OK) LoadPositions();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a position to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int posId = (int)positionGrid.SelectedRows[0].Cells["Id"].Value;
            var position = dataManager.Positions.FirstOrDefault(p => p.Id == posId);

            if (position != null)
            {
                var form = new PositionEditForm(dataManager, position);
                if (form.ShowDialog() == DialogResult.OK) LoadPositions();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (positionGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a position to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int posId = (int)positionGrid.SelectedRows[0].Cells["Id"].Value;

            if (dataManager.Employees.Any(e => e.PositionId == posId))
            {
                MessageBox.Show("Cannot delete position with active employees.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this position?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                dataManager.PositionRequiredSkills.RemoveAll(prs => prs.PositionId == posId);
                dataManager.PositionProcesses.RemoveAll(pp => pp.PositionId == posId);
                dataManager.Positions.RemoveAll(p => p.Id == posId);
                LoadPositions();
                MessageBox.Show("Position deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1100, 600);
            this.Name = "PositionManagementForm";
            this.ResumeLayout(false);
        }
    }

    // ==================== POSITION EDIT FORM ====================
    public partial class PositionEditForm : Form
    {
        private DataManager dataManager;
        private Position position;
        private bool isEditMode;
        private TextBox txtName, txtCapacity, txtLevel, txtMinSalary, txtMaxSalary, txtHiringCost;
        private ComboBox cmbDepartment, cmbReportsTo;
        private Button btnSave, btnCancel;

        public PositionEditForm(DataManager manager, Position pos)
        {
            dataManager = manager;
            position = pos;
            isEditMode = pos != null;
            InitializeComponent();
            InitializeCustomComponents();
            if (isEditMode) LoadPositionData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = isEditMode ? "Edit Position" : "Add New Position";
            this.Size = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int lw = 120, cw = 400, lm = 30, clm = lm + lw + 10, y = 20, vs = 45;

            var title = new Label { Text = this.Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(lm, y), AutoSize = true };
            this.Controls.Add(title);
            y += 50;

            AddLabel("Name:", lm, y);
            txtName = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtName);
            y += vs;

            AddLabel("Department:", lm, y);
            cmbDepartment = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            cmbDepartment.DataSource = dataManager.Departments.ToList();
            this.Controls.Add(cmbDepartment);
            y += vs;

            AddLabel("Capacity:", lm, y);
            txtCapacity = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtCapacity);
            y += vs;

            AddLabel("Position Level:", lm, y);
            txtLevel = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtLevel);
            y += vs;

            AddLabel("Min Salary:", lm, y);
            txtMinSalary = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtMinSalary);
            y += vs;

            AddLabel("Max Salary:", lm, y);
            txtMaxSalary = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtMaxSalary);
            y += vs;

            AddLabel("Hiring Cost:", lm, y);
            txtHiringCost = new TextBox { Location = new Point(clm, y), Width = cw };
            this.Controls.Add(txtHiringCost);
            y += vs;

            AddLabel("Reports To:", lm, y);
            cmbReportsTo = new ComboBox { Location = new Point(clm, y), Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportsTo.DisplayMember = "Name";
            cmbReportsTo.ValueMember = "Id";
            var positions = dataManager.Positions.ToList();
            positions.Insert(0, new Position { Id = 0, Name = "None" });
            cmbReportsTo.DataSource = positions;
            this.Controls.Add(cmbReportsTo);
            y += vs + 20;

            btnSave = new Button { Text = "Save", Location = new Point(clm, y), Width = 100, Height = 35, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
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

        private void LoadPositionData()
        {
            txtName.Text = position.Name;
            cmbDepartment.SelectedValue = position.DepartmentId;
            txtCapacity.Text = position.Capacity.ToString();
            txtLevel.Text = position.PositionLevel.ToString();
            txtMinSalary.Text = position.MinSalary.ToString();
            txtMaxSalary.Text = position.MaxSalary.ToString();
            txtHiringCost.Text = position.HiringCost.ToString();
            cmbReportsTo.SelectedValue = position.ReportsToPositionID ?? 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (!isEditMode)
                {
                    position = new Position { Id = dataManager.Positions.Any() ? dataManager.Positions.Max(p => p.Id) + 1 : 1 };
                    dataManager.Positions.Add(position);
                }

                position.Name = txtName.Text.Trim();
                position.DepartmentId = (int)cmbDepartment.SelectedValue;
                position.Capacity = int.Parse(txtCapacity.Text);
                position.PositionLevel = int.Parse(txtLevel.Text);
                position.MinSalary = decimal.Parse(txtMinSalary.Text);
                position.MaxSalary = decimal.Parse(txtMaxSalary.Text);
                position.HiringCost = decimal.Parse(txtHiringCost.Text);

                int reportsTo = (int)cmbReportsTo.SelectedValue;
                position.ReportsToPositionID = reportsTo == 0 ? (int?)null : reportsTo;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving position: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Position name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtCapacity.Text, out int cap) || cap < 0)
            {
                MessageBox.Show("Capacity must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtLevel.Text, out int level) || level < 1)
            {
                MessageBox.Show("Position level must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtMinSalary.Text, out decimal minSal) || minSal < 0)
            {
                MessageBox.Show("Min salary must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtMaxSalary.Text, out decimal maxSal) || maxSal < minSal)
            {
                MessageBox.Show("Max salary must be greater than or equal to min salary.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 650);
            this.Name = "PositionEditForm";
            this.ResumeLayout(false);
        }
    }

    // ==================== SKILL MANAGEMENT ====================
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

            cmbTypeFilter = new ComboBox { Location = new Point(100, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
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
            this.Controls.Add(skillGrid);
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

    // ==================== SKILL EDIT FORM ====================
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