using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using SkillManagementSystem.Models;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Services
{
    public class ExcelImportService
    {
        private DataManager _dataManager;
        private List<string> _errors;
        private List<string> _warnings;

        public ExcelImportService(DataManager dataManager)
        {
            _dataManager = dataManager;
            _errors = new List<string>();
            _warnings = new List<string>();
        }

        public class ImportResult
        {
            public bool Success { get; set; }
            public List<string> Errors { get; set; }
            public List<string> Warnings { get; set; }
            public int DepartmentsImported { get; set; }
            public int PositionsImported { get; set; }
            public int SkillsImported { get; set; }
            public int ProcessesImported { get; set; }
            public int EmployeesImported { get; set; }
            public int TrainingsImported { get; set; }
            public int RelationshipsImported { get; set; }

            public ImportResult()
            {
                Errors = new List<string>();
                Warnings = new List<string>();
            }
        }

        public ImportResult ImportFromExcel(string filePath)
        {
            var result = new ImportResult();
            _errors.Clear();
            _warnings.Clear();

            // Set EPPlus license for version 8+
            if (ExcelPackage.License.LicenseKey == null)
            {
                ExcelPackage.License.SetNonCommercialPersonal("Ecem");
            }

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // Import in dependency order
                    result.DepartmentsImported = ImportDepartments(package);
                    result.SkillsImported = ImportSkills(package);
                    result.PositionsImported = ImportPositions(package);
                    result.ProcessesImported = ImportProcesses(package);
                    result.EmployeesImported = ImportEmployees(package);
                    result.TrainingsImported = ImportTrainings(package);

                    // Import relationships
                    int relationships = 0;
                    relationships += ImportEmployeeSkills(package);
                    relationships += ImportPositionRequiredSkills(package);
                    relationships += ImportPositionProcesses(package);
                    relationships += ImportProcessRequiredSkills(package);
                    relationships += ImportTrainingSkills(package);
                    relationships += ImportTrainingPrerequisiteSkills(package);
                    relationships += ImportTrainingPrerequisiteTrainings(package);
                    result.RelationshipsImported = relationships;

                    // Rebuild navigation properties
                    _dataManager.BuildNavigationProperties();
                }

                result.Errors = _errors;
                result.Warnings = _warnings;
                result.Success = _errors.Count == 0;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Fatal error: {ex.Message}");
                return result;
            }
        }

        private int ImportDepartments(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Departments"];
            if (sheet == null)
            {
                _warnings.Add("Departments sheet not found");
                return 0;
            }

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var dept = new Department
                    {
                        Id = GetInt(sheet, row, 1),
                        Name = GetString(sheet, row, 2),
                        Budget = GetDecimal(sheet, row, 3)
                    };

                    if (string.IsNullOrWhiteSpace(dept.Name))
                    {
                        _errors.Add($"Departments Row {row}: Name is required");
                        continue;
                    }

                    _dataManager.Departments.Add(dept);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"Departments Row {row}: {ex.Message}");
                }
            }

            return count;
        }

        private int ImportSkills(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Skills"];
            if (sheet == null)
            {
                _warnings.Add("Skills sheet not found");
                return 0;
            }

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var skill = new Skill
                    {
                        Id = GetInt(sheet, row, 1),
                        Name = GetString(sheet, row, 2),
                        Description = GetString(sheet, row, 3),
                        Category = (SkillType)GetInt(sheet, row, 4),
                        MinLevel = GetInt(sheet, row, 5, 1),
                        MaxLevel = GetInt(sheet, row, 6, 5)
                    };

                    if (string.IsNullOrWhiteSpace(skill.Name))
                    {
                        _errors.Add($"Skills Row {row}: Name is required");
                        continue;
                    }

                    _dataManager.Skills.Add(skill);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"Skills Row {row}: {ex.Message}");
                }
            }

            return count;
        }

        private int ImportPositions(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Positions"];
            if (sheet == null)
            {
                _warnings.Add("Positions sheet not found");
                return 0;
            }

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var position = new Position
                    {
                        Id = GetInt(sheet, row, 1),
                        Name = GetString(sheet, row, 2),
                        DepartmentId = GetInt(sheet, row, 3),
                        Capacity = GetInt(sheet, row, 4),
                        PositionLevel = GetInt(sheet, row, 5),
                        MinSalary = GetDecimal(sheet, row, 6),
                        MaxSalary = GetDecimal(sheet, row, 7),
                        ReportsToPositionId = GetNullableInt(sheet, row, 8),
                        HiringCost = GetDecimal(sheet, row, 9)
                    };

                    if (string.IsNullOrWhiteSpace(position.Name))
                    {
                        _errors.Add($"Positions Row {row}: Name is required");
                        continue;
                    }

                    if (!_dataManager.Departments.Any(d => d.Id == position.DepartmentId))
                    {
                        _errors.Add($"Positions Row {row}: Department {position.DepartmentId} not found");
                        continue;
                    }

                    _dataManager.Positions.Add(position);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"Positions Row {row}: {ex.Message}");
                }
            }

            return count;
        }

        private int ImportProcesses(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Processes"];
            if (sheet == null)
            {
                _warnings.Add("Processes sheet not found");
                return 0;
            }

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var process = new Process
                    {
                        Id = GetInt(sheet, row, 1),
                        Name = GetString(sheet, row, 2),
                        ProcessDescription = GetString(sheet, row, 3),
                        AimedNumberOfWorkers = GetInt(sheet, row, 4)
                    };

                    if (string.IsNullOrWhiteSpace(process.Name))
                    {
                        _errors.Add($"Processes Row {row}: Name is required");
                        continue;
                    }

                    _dataManager.Processes.Add(process);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"Processes Row {row}: {ex.Message}");
                }
            }

            return count;
        }

        private int ImportEmployees(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Employees"];
            if (sheet == null)
            {
                _warnings.Add("Employees sheet not found");
                return 0;
            }

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var employee = new Employee
                    {
                        Id = GetInt(sheet, row, 1),
                        FirstName = GetString(sheet, row, 2),
                        LastName = GetString(sheet, row, 3),
                        IdentityNumber = GetString(sheet, row, 4),
                        DateOfBirth = GetDateTime(sheet, row, 5),
                        DepartmentId = GetInt(sheet, row, 6),
                        PositionId = GetInt(sheet, row, 7),
                        CurrentSalary = GetDecimal(sheet, row, 8),
                        HireDate = GetDateTime(sheet, row, 9),
                        Status = (EmployeeStatus)GetInt(sheet, row, 10, 1),
                        TerminationDate = GetNullableDateTime(sheet, row, 11)
                    };

                    // Validate
                    if (string.IsNullOrWhiteSpace(employee.FirstName) || string.IsNullOrWhiteSpace(employee.LastName))
                    {
                        _errors.Add($"Employees Row {row}: First and Last name are required");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(employee.IdentityNumber) || employee.IdentityNumber.Length != 11)
                    {
                        _errors.Add($"Employees Row {row}: Identity number must be 11 digits");
                        continue;
                    }

                    if (!_dataManager.Departments.Any(d => d.Id == employee.DepartmentId))
                    {
                        _errors.Add($"Employees Row {row}: Department {employee.DepartmentId} not found");
                        continue;
                    }

                    if (!_dataManager.Positions.Any(p => p.Id == employee.PositionId))
                    {
                        _errors.Add($"Employees Row {row}: Position {employee.PositionId} not found");
                        continue;
                    }

                    _dataManager.Employees.Add(employee);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"Employees Row {row}: {ex.Message}");
                }
            }

            return count;
        }

        private int ImportTrainings(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Trainings"];
            if (sheet == null)
            {
                _warnings.Add("Trainings sheet not found");
                return 0;
            }

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var training = new Training
                    {
                        Id = GetInt(sheet, row, 1),
                        Name = GetString(sheet, row, 2),
                        Description = GetString(sheet, row, 3),
                        TargetDepartmentId = GetInt(sheet, row, 4),
                        ApplicationType = (ApplicationType)GetInt(sheet, row, 5),
                        Capacity = GetInt(sheet, row, 6),
                        Source = (TrainingSource)GetInt(sheet, row, 7),
                        CostType = (CostType)GetInt(sheet, row, 8),
                        Cost = GetDecimal(sheet, row, 9),
                        DurationHours = GetInt(sheet, row, 10),
                        Status = (TrainingStatus)GetInt(sheet, row, 11, 1),
                        StartDate = GetNullableDateTime(sheet, row, 12),
                        EndDate = GetNullableDateTime(sheet, row, 13)
                    };

                    if (string.IsNullOrWhiteSpace(training.Name))
                    {
                        _errors.Add($"Trainings Row {row}: Name is required");
                        continue;
                    }

                    if (!_dataManager.Departments.Any(d => d.Id == training.TargetDepartmentId))
                    {
                        _errors.Add($"Trainings Row {row}: Department {training.TargetDepartmentId} not found");
                        continue;
                    }

                    _dataManager.Trainings.Add(training);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"Trainings Row {row}: {ex.Message}");
                }
            }

            return count;
        }

        // Relationship imports
        private int ImportEmployeeSkills(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["EmployeeSkills"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var es = new EmployeeSkill
                    {
                        EmployeeId = GetInt(sheet, row, 1),
                        SkillId = GetInt(sheet, row, 2),
                        CurrentLevel = (SkillDegree)GetInt(sheet, row, 3),
                        SkillSource = (SkillSource)GetInt(sheet, row, 4),
                        AcquisitionDate = GetDateTime(sheet, row, 5, DateTime.Now)
                    };

                    _dataManager.EmployeeSkills.Add(es);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"EmployeeSkills Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        private int ImportPositionRequiredSkills(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["PositionRequiredSkills"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var prs = new PositionRequiredSkill
                    {
                        PositionId = GetInt(sheet, row, 1),
                        SkillId = GetInt(sheet, row, 2),
                        RequiredLevel = GetInt(sheet, row, 3),
                        IsMandatory = GetBool(sheet, row, 4, true)
                    };

                    _dataManager.PositionRequiredSkills.Add(prs);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"PositionRequiredSkills Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        private int ImportPositionProcesses(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["PositionProcesses"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var pp = new PositionProcess
                    {
                        PositionId = GetInt(sheet, row, 1),
                        ProcessId = GetInt(sheet, row, 2)
                    };

                    _dataManager.PositionProcesses.Add(pp);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"PositionProcesses Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        private int ImportProcessRequiredSkills(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["ProcessRequiredSkills"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var prs = new ProcessRequiredSkill
                    {
                        ProcessId = GetInt(sheet, row, 1),
                        SkillId = GetInt(sheet, row, 2),
                        RequiredLevel = GetInt(sheet, row, 3)
                    };

                    _dataManager.ProcessRequiredSkills.Add(prs);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"ProcessRequiredSkills Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        private int ImportTrainingSkills(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["TrainingSkills"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var ts = new TrainingSkill
                    {
                        TrainingId = GetInt(sheet, row, 1),
                        SkillId = GetInt(sheet, row, 2),
                        TargetLevel = GetInt(sheet, row, 3)
                    };

                    _dataManager.TrainingSkills.Add(ts);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"TrainingSkills Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        private int ImportTrainingPrerequisiteSkills(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["TrainingPrerequisiteSkills"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var tps = new TrainingPrerequisiteSkill
                    {
                        TrainingId = GetInt(sheet, row, 1),
                        SkillId = GetInt(sheet, row, 2),
                        MinimumLevel = GetInt(sheet, row, 3)
                    };

                    _dataManager.TrainingPrerequisiteSkills.Add(tps);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"TrainingPrerequisiteSkills Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        private int ImportTrainingPrerequisiteTrainings(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["TrainingPrerequisiteTrainings"];
            if (sheet == null) return 0;

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                try
                {
                    var tpt = new TrainingPrerequisiteTraining
                    {
                        TrainingId = GetInt(sheet, row, 1),
                        PrerequisiteTrainingId = GetInt(sheet, row, 2)
                    };

                    _dataManager.TrainingPrerequisiteTrainings.Add(tpt);
                    count++;
                }
                catch (Exception ex)
                {
                    _errors.Add($"TrainingPrerequisiteTrainings Row {row}: {ex.Message}");
                }
            }
            return count;
        }

        // Helper methods
        private string GetString(ExcelWorksheet sheet, int row, int col, string defaultValue = "")
        {
            var cell = sheet.Cells[row, col].Value;
            return cell?.ToString() ?? defaultValue;
        }

        private int GetInt(ExcelWorksheet sheet, int row, int col, int defaultValue = 0)
        {
            var cell = sheet.Cells[row, col].Value;
            if (cell == null) return defaultValue;
            if (int.TryParse(cell.ToString(), out int result))
                return result;
            return defaultValue;
        }

        private int? GetNullableInt(ExcelWorksheet sheet, int row, int col)
        {
            var cell = sheet.Cells[row, col].Value;
            if (cell == null) return null;
            if (int.TryParse(cell.ToString(), out int result))
                return result;
            return null;
        }

        private decimal GetDecimal(ExcelWorksheet sheet, int row, int col, decimal defaultValue = 0)
        {
            var cell = sheet.Cells[row, col].Value;
            if (cell == null) return defaultValue;
            if (decimal.TryParse(cell.ToString(), out decimal result))
                return result;
            return defaultValue;
        }

        private DateTime GetDateTime(ExcelWorksheet sheet, int row, int col, DateTime? defaultValue = null)
        {
            var cell = sheet.Cells[row, col].Value;
            if (cell == null) return defaultValue ?? DateTime.Now;
            if (cell is DateTime dt) return dt;
            if (DateTime.TryParse(cell.ToString(), out DateTime result))
                return result;
            return defaultValue ?? DateTime.Now;
        }

        private DateTime? GetNullableDateTime(ExcelWorksheet sheet, int row, int col)
        {
            var cell = sheet.Cells[row, col].Value;
            if (cell == null) return null;
            if (cell is DateTime dt) return dt;
            if (DateTime.TryParse(cell.ToString(), out DateTime result))
                return result;
            return null;
        }

        private bool GetBool(ExcelWorksheet sheet, int row, int col, bool defaultValue = false)
        {
            var cell = sheet.Cells[row, col].Value;
            if (cell == null) return defaultValue;
            var str = cell.ToString().ToLower();
            if (str == "true" || str == "1" || str == "yes") return true;
            if (str == "false" || str == "0" || str == "no") return false;
            return defaultValue;
        }
    }
}