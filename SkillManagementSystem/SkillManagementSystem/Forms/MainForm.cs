using System;
using System.Drawing;
using System.Windows.Forms;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem
{
    public partial class MainForm : Form
    {
        private DataManager dataManager;
        private Panel mainPanel;
        private MenuStrip menuStrip;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Skill Management System";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create Menu Strip
            menuStrip = new MenuStrip();

            // Data Management Menu
            var dataMenu = new ToolStripMenuItem("Data Management");
            dataMenu.DropDownItems.Add(CreateMenuItem("Employees", OpenEmployeeManagement));
            dataMenu.DropDownItems.Add(CreateMenuItem("Departments", OpenDepartmentManagement));
            dataMenu.DropDownItems.Add(CreateMenuItem("Positions", OpenPositionManagement));
            dataMenu.DropDownItems.Add(CreateMenuItem("Skills", OpenSkillManagement));
            dataMenu.DropDownItems.Add(CreateMenuItem("Trainings", OpenTrainingManagement));
            dataMenu.DropDownItems.Add(CreateMenuItem("Processes", OpenProcessManagement));
            menuStrip.Items.Add(dataMenu);

            // Analysis Menu
            var analysisMenu = new ToolStripMenuItem("Analysis");
            analysisMenu.DropDownItems.Add(CreateMenuItem("Employment Analysis", OpenEmploymentAnalysis));
            analysisMenu.DropDownItems.Add(CreateMenuItem("Capability Enhancement", OpenCapabilityEnhancement));
            analysisMenu.DropDownItems.Add(CreateMenuItem("Worker Reliance", OpenWorkerReliance));
            menuStrip.Items.Add(analysisMenu);

            // Reports Menu
            var reportsMenu = new ToolStripMenuItem("Reports");
            reportsMenu.DropDownItems.Add(CreateMenuItem("Dashboard", OpenDashboard));
            reportsMenu.DropDownItems.Add(CreateMenuItem("Department Reports", OpenDepartmentReports));
            reportsMenu.DropDownItems.Add(CreateMenuItem("Training Reports", OpenTrainingReports));
            menuStrip.Items.Add(reportsMenu);

            // File Menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add(CreateMenuItem("Generate Demo Data", GenerateDemoData));
            fileMenu.DropDownItems.Add(CreateMenuItem("Save All", SaveAllData));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(CreateMenuItem("Exit", (s, e) => Application.Exit()));
            menuStrip.Items.Add(fileMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Create Main Panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            // Show welcome screen
            ShowWelcomeScreen();
        }

        private ToolStripMenuItem CreateMenuItem(string text, EventHandler clickHandler)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += clickHandler;
            return item;
        }

        private void ShowWelcomeScreen()
        {
            mainPanel.Controls.Clear();

            var welcomeLabel = new Label
            {
                Text = "Welcome to Skill Management System",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 50)
            };

            var instructionLabel = new Label
            {
                Text = "Select an option from the menu above to get started.\n\n" +
                       "Start by generating demo data from File > Generate Demo Data",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(50, 120)
            };

            mainPanel.Controls.Add(welcomeLabel);
            mainPanel.Controls.Add(instructionLabel);
        }

        private void LoadData()
        {
            dataManager = new DataManager();
            dataManager.LoadAllData();
        }

        private void SaveAllData(object sender, EventArgs e)
        {
            try
            {
                dataManager.SaveAllData();
                MessageBox.Show("All data saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateDemoData(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will clear all existing data and generate demo data. Continue?",
                "Generate Demo Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dataManager.GenerateDemoData();
                MessageBox.Show("Demo data generated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Event Handlers for Menu Items
        private void OpenEmployeeManagement(object sender, EventArgs e)
        {
            var form = new Forms.EmployeeManagementForm(dataManager);
            form.ShowDialog();
        }

        private void OpenDepartmentManagement(object sender, EventArgs e)
        {
            var form = new Forms.DepartmentManagementForm(dataManager);
            form.ShowDialog();
        }

        private void OpenPositionManagement(object sender, EventArgs e)
        {
            var form = new Forms.PositionManagementForm(dataManager);
            form.ShowDialog();
        }

        private void OpenSkillManagement(object sender, EventArgs e)
        {
            var form = new Forms.SkillManagementForm(dataManager);
            form.ShowDialog();
        }

        private void OpenTrainingManagement(object sender, EventArgs e)
        {
            MessageBox.Show("Training Management - Coming Soon!");
        }

        private void OpenProcessManagement(object sender, EventArgs e)
        {
            MessageBox.Show("Process Management - Coming Soon!");
        }

        private void OpenEmploymentAnalysis(object sender, EventArgs e)
        {
            MessageBox.Show("Employment Analysis - Coming Soon!");
        }

        private void OpenCapabilityEnhancement(object sender, EventArgs e)
        {
            MessageBox.Show("Capability Enhancement - Coming Soon!");
        }

        private void OpenWorkerReliance(object sender, EventArgs e)
        {
            MessageBox.Show("Worker Reliance - Coming Soon!");
        }

        private void OpenDashboard(object sender, EventArgs e)
        {
            MessageBox.Show("Dashboard - Coming Soon!");
        }

        private void OpenDepartmentReports(object sender, EventArgs e)
        {
            MessageBox.Show("Department Reports - Coming Soon!");
        }

        private void OpenTrainingReports(object sender, EventArgs e)
        {
            MessageBox.Show("Training Reports - Coming Soon!");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 800);
            this.Name = "MainForm";
            this.Text = "Skill Management System";
            this.ResumeLayout(false);
        }
    }
}