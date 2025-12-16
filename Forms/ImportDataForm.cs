using System;
using System.Drawing;
using System.Windows.Forms;
using OfficeOpenXml;
using SkillManagementSystem.Services;
using SkillManagementSystem.Utilities;



namespace SkillManagementSystem.Forms
{
    public partial class ImportDataForm : Form
    {
        private DataManager dataManager;
        private ExcelImportService importService;

        private TextBox txtFilePath;
        private Button btnBrowse, btnImport, btnClose;
        private CheckBox chkClearExisting;
        private RichTextBox txtResults;
        private ProgressBar progressBar;
        private Label lblStatus;

        public ImportDataForm(DataManager manager)
        {
            // Set EPPlus license context first
            if (ExcelPackage.License.LicenseKey == null)
            {
                ExcelPackage.License.SetNonCommercialPersonal("Ecem");
            }

            dataManager = manager;
            importService = new ExcelImportService(dataManager);

            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Import Data from Excel";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            var titleLabel = new Label
            {
                Text = "Import Data from Excel File",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            // Instructions
            var instructionLabel = new Label
            {
                Text = "Select an Excel file (.xlsx) with properly formatted data sheets.\n" +
                       "The file should contain sheets: Departments, Skills, Positions, Processes, Employees, Trainings, etc.",
                Location = new Point(20, 60),
                Size = new Size(740, 40),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(instructionLabel);

            // File selection
            var lblFile = new Label
            {
                Text = "Excel File:",
                Location = new Point(20, 120),
                AutoSize = true
            };
            this.Controls.Add(lblFile);

            txtFilePath = new TextBox
            {
                Location = new Point(20, 145),
                Width = 600,
                ReadOnly = true
            };
            this.Controls.Add(txtFilePath);

            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new Point(630, 143),
                Width = 130,
                Height = 27
            };
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            // Options
            chkClearExisting = new CheckBox
            {
                Text = "Clear existing data before import (recommended)",
                Location = new Point(20, 185),
                Width = 400,
                Checked = true
            };
            this.Controls.Add(chkClearExisting);

            // Import button
            btnImport = new Button
            {
                Text = "Import Data",
                Location = new Point(20, 220),
                Width = 150,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Enabled = false
            };
            btnImport.Click += BtnImport_Click;
            this.Controls.Add(btnImport);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(180, 220),
                Width = 100,
                Height = 40
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            // Progress
            progressBar = new ProgressBar
            {
                Location = new Point(20, 275),
                Width = 740,
                Height = 25,
                Visible = false
            };
            this.Controls.Add(progressBar);

            lblStatus = new Label
            {
                Text = "",
                Location = new Point(20, 305),
                Width = 740,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblStatus);

            // Results
            var lblResults = new Label
            {
                Text = "Import Results:",
                Location = new Point(20, 335),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblResults);

            txtResults = new RichTextBox
            {
                Location = new Point(20, 360),
                Width = 740,
                Height = 170,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtResults);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                openFileDialog.Title = "Select Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    btnImport.Enabled = true;
                    txtResults.Clear();
                    lblStatus.Text = "File selected. Click 'Import Data' to begin.";
                }
            }
        }

        private async void BtnImport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("Please select an Excel file first.", "No File Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm clear existing data
            if (chkClearExisting.Checked)
            {
                var result = MessageBox.Show(
                    "This will CLEAR ALL EXISTING DATA and import from the Excel file.\n\n" +
                    "Are you sure you want to continue?",
                    "Confirm Data Import",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;

                // Clear existing data
                dataManager.InitializeData();
            }

            // Disable controls
            btnImport.Enabled = false;
            btnBrowse.Enabled = false;
            chkClearExisting.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            txtResults.Clear();
            lblStatus.Text = "Importing data...";

            try
            {
                // Perform import (run on background thread to keep UI responsive)
                var importResult = await System.Threading.Tasks.Task.Run(() =>
                    importService.ImportFromExcel(txtFilePath.Text));

                // Display results
                DisplayResults(importResult);

                if (importResult.Success)
                {
                    // Save the imported data
                    dataManager.SaveAllData();
                    lblStatus.Text = "Import completed successfully!";
                    lblStatus.ForeColor = Color.Green;

                    MessageBox.Show(
                        "Data imported successfully!\n\n" +
                        $"Departments: {importResult.DepartmentsImported}\n" +
                        $"Skills: {importResult.SkillsImported}\n" +
                        $"Positions: {importResult.PositionsImported}\n" +
                        $"Processes: {importResult.ProcessesImported}\n" +
                        $"Employees: {importResult.EmployeesImported}\n" +
                        $"Trainings: {importResult.TrainingsImported}\n" +
                        $"Relationships: {importResult.RelationshipsImported}",
                        "Import Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    lblStatus.Text = "Import completed with errors. See details below.";
                    lblStatus.ForeColor = Color.Red;

                    MessageBox.Show(
                        $"Import completed with {importResult.Errors.Count} error(s).\n\n" +
                        "Please review the errors below and fix the Excel file.",
                        "Import Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Import failed!";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Error during import: {ex.Message}", "Import Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable controls
                btnImport.Enabled = true;
                btnBrowse.Enabled = true;
                chkClearExisting.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void DisplayResults(ExcelImportService.ImportResult result)
        {
            txtResults.Clear();

            // Summary
            txtResults.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
            txtResults.SelectionColor = Color.DarkBlue;
            txtResults.AppendText("=== IMPORT SUMMARY ===\n");
            txtResults.SelectionFont = new Font("Consolas", 9);
            txtResults.SelectionColor = Color.Black;

            txtResults.AppendText($"\nDepartments:  {result.DepartmentsImported} imported\n");
            txtResults.AppendText($"Skills:       {result.SkillsImported} imported\n");
            txtResults.AppendText($"Positions:    {result.PositionsImported} imported\n");
            txtResults.AppendText($"Processes:    {result.ProcessesImported} imported\n");
            txtResults.AppendText($"Employees:    {result.EmployeesImported} imported\n");
            txtResults.AppendText($"Trainings:    {result.TrainingsImported} imported\n");
            txtResults.AppendText($"Relationships: {result.RelationshipsImported} imported\n");

            // Warnings
            if (result.Warnings.Count > 0)
            {
                txtResults.AppendText("\n");
                txtResults.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                txtResults.SelectionColor = Color.Orange;
                txtResults.AppendText($"=== WARNINGS ({result.Warnings.Count}) ===\n");
                txtResults.SelectionFont = new Font("Consolas", 9);
                txtResults.SelectionColor = Color.Orange;

                foreach (var warning in result.Warnings)
                {
                    txtResults.AppendText($"⚠ {warning}\n");
                }
            }

            // Errors
            if (result.Errors.Count > 0)
            {
                txtResults.AppendText("\n");
                txtResults.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                txtResults.SelectionColor = Color.Red;
                txtResults.AppendText($"=== ERRORS ({result.Errors.Count}) ===\n");
                txtResults.SelectionFont = new Font("Consolas", 9);
                txtResults.SelectionColor = Color.Red;

                foreach (var error in result.Errors)
                {
                    txtResults.AppendText($"✗ {error}\n");
                }
            }

            // Success message
            if (result.Success)
            {
                txtResults.AppendText("\n");
                txtResults.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                txtResults.SelectionColor = Color.Green;
                txtResults.AppendText("✓ Import completed successfully!\n");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(800, 600);
            this.Name = "ImportDataForm";
            this.ResumeLayout(false);
        }
    }
}