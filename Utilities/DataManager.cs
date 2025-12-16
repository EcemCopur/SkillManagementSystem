using Newtonsoft.Json;
using SkillManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SkillManagementSystem.Utilities
{
    /// <summary>
    /// Manages all data persistence using JSON files
    /// </summary>
    public class DataManager
    {
        private readonly string _dataDirectory;

        // In-memory data stores
        public List<Employee> Employees { get; set; }
        public List<Department> Departments { get; set; }
        public List<Position> Positions { get; set; }
        public List<Skill> Skills { get; set; }
        public List<Training> Trainings { get; set; }
        public List<Process> Processes { get; set; }
        public List<TrainingSession> TrainingSessions { get; set; }
        public List<ChangeHistory> ChangeHistories { get; set; }

        // Junction tables
        public List<EmployeeSkill> EmployeeSkills { get; set; }
        public List<EmployeeTraining> EmployeeTrainings { get; set; }
        public List<PositionRequiredSkill> PositionRequiredSkills { get; set; }
        public List<PositionProcess> PositionProcesses { get; set; }
        public List<ProcessRequiredSkill> ProcessRequiredSkills { get; set; }
        public List<TrainingSkill> TrainingSkills { get; set; }
        public List<TrainingPrerequisiteSkill> TrainingPrerequisiteSkills { get; set; }
        public List<TrainingPrerequisiteTraining> TrainingPrerequisiteTrainings { get; set; }

        public DataManager(string dataDirectory = "Data")
        {
            _dataDirectory = dataDirectory;
            InitializeData();
        }

        public void InitializeData()
        {
            Employees = new List<Employee>();
            Departments = new List<Department>();
            Positions = new List<Position>();
            Skills = new List<Skill>();
            Trainings = new List<Training>();
            Processes = new List<Process>();
            TrainingSessions = new List<TrainingSession>();
            ChangeHistories = new List<ChangeHistory>();

            EmployeeSkills = new List<EmployeeSkill>();
            EmployeeTrainings = new List<EmployeeTraining>();
            PositionRequiredSkills = new List<PositionRequiredSkill>();
            PositionProcesses = new List<PositionProcess>();
            ProcessRequiredSkills = new List<ProcessRequiredSkill>();
            TrainingSkills = new List<TrainingSkill>();
            TrainingPrerequisiteSkills = new List<TrainingPrerequisiteSkill>();
            TrainingPrerequisiteTrainings = new List<TrainingPrerequisiteTraining>();
        }

        public void LoadAllData()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
                return;
            }

            Employees = LoadData<Employee>("employees.json");
            Departments = LoadData<Department>("departments.json");
            Positions = LoadData<Position>("positions.json");
            Skills = LoadData<Skill>("skills.json");
            Trainings = LoadData<Training>("trainings.json");
            Processes = LoadData<Process>("processes.json");
            TrainingSessions = LoadData<TrainingSession>("training_sessions.json");
            ChangeHistories = LoadData<ChangeHistory>("change_histories.json");

            EmployeeSkills = LoadData<EmployeeSkill>("employee_skills.json");
            EmployeeTrainings = LoadData<EmployeeTraining>("employee_trainings.json");
            PositionRequiredSkills = LoadData<PositionRequiredSkill>("position_required_skills.json");
            PositionProcesses = LoadData<PositionProcess>("position_processes.json");
            ProcessRequiredSkills = LoadData<ProcessRequiredSkill>("process_required_skills.json");
            TrainingSkills = LoadData<TrainingSkill>("training_skills.json");
            TrainingPrerequisiteSkills = LoadData<TrainingPrerequisiteSkill>("training_prerequisite_skills.json");
            TrainingPrerequisiteTrainings = LoadData<TrainingPrerequisiteTraining>("training_prerequisite_trainings.json");

            BuildNavigationProperties();
        }

        public void SaveAllData()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            SaveData(Employees, "employees.json");
            SaveData(Departments, "departments.json");
            SaveData(Positions, "positions.json");
            SaveData(Skills, "skills.json");
            SaveData(Trainings, "trainings.json");
            SaveData(Processes, "processes.json");
            SaveData(TrainingSessions, "training_sessions.json");
            SaveData(ChangeHistories, "change_histories.json");

            SaveData(EmployeeSkills, "employee_skills.json");
            SaveData(EmployeeTrainings, "employee_trainings.json");
            SaveData(PositionRequiredSkills, "position_required_skills.json");
            SaveData(PositionProcesses, "position_processes.json");
            SaveData(ProcessRequiredSkills, "process_required_skills.json");
            SaveData(TrainingSkills, "training_skills.json");
            SaveData(TrainingPrerequisiteSkills, "training_prerequisite_skills.json");
            SaveData(TrainingPrerequisiteTrainings, "training_prerequisite_trainings.json");
        }

        private List<T> LoadData<T>(string fileName)
        {
            string filePath = Path.Combine(_dataDirectory, fileName);
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        private void SaveData<T>(List<T> data, string fileName)
        {
            string filePath = Path.Combine(_dataDirectory, fileName);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Rebuilds all navigation properties after loading data
        /// </summary>
        public void BuildNavigationProperties()
        {
            // Build Employee relationships
            foreach (var emp in Employees)
            {
                emp.Skills = EmployeeSkills.FindAll(es => es.EmployeeId == emp.Id);
                emp.Trainings = EmployeeTrainings.FindAll(et => et.EmployeeId == emp.Id);
            }

            // Build Department relationships
            foreach (var dept in Departments)
            {
                dept.Positions = Positions.FindAll(p => p.DepartmentId == dept.Id);
                dept.Employees = Employees.FindAll(e => e.DepartmentId == dept.Id);
            }

            // Build Position relationships
            foreach (var pos in Positions)
            {
                pos.RequiredSkills = PositionRequiredSkills.FindAll(prs => prs.PositionId == pos.Id);
                pos.Processes = PositionProcesses.FindAll(pp => pp.PositionId == pos.Id);
                pos.Employees = Employees.FindAll(e => e.PositionId == pos.Id);
                pos.Department = Departments.Find(d => d.Id == pos.DepartmentId);
            }

            // Build Process relationships
            foreach (var proc in Processes)
            {
                proc.RequiredSkills = ProcessRequiredSkills.FindAll(prs => prs.ProcessId == proc.Id);
            }

            // Build Training relationships
            foreach (var training in Trainings)
            {
                training.TrainingSkills = TrainingSkills.FindAll(ts => ts.TrainingId == training.Id);
                training.PrerequisiteSkills = TrainingPrerequisiteSkills.FindAll(tps => tps.TrainingId == training.Id);
                training.PrerequisiteTrainings = TrainingPrerequisiteTrainings.FindAll(tpt => tpt.TrainingId == training.Id);
            }

            // Build junction table navigation properties
            foreach (var es in EmployeeSkills)
            {
                es.Employee = Employees.Find(e => e.Id == es.EmployeeId);
                es.Skill = Skills.Find(s => s.Id == es.SkillId);
            }

            foreach (var et in EmployeeTrainings)
            {
                et.Worker = Employees.Find(e => e.Id == et.EmployeeId);
                et.Training = Trainings.Find(t => t.Id == et.TrainingId);
                if (et.TrainingSessionId.HasValue)
                {
                    et.TrainingSession = TrainingSessions.Find(ts => ts.Id == et.TrainingSessionId.Value);
                }
            }

            foreach (var prs in PositionRequiredSkills)
            {
                prs.Position = Positions.Find(p => p.Id == prs.PositionId);
                prs.Skill = Skills.Find(s => s.Id == prs.SkillId);
            }

            foreach (var pp in PositionProcesses)
            {
                pp.Position = Positions.Find(p => p.Id == pp.PositionId);
                pp.Process = Processes.Find(proc => proc.Id == pp.ProcessId);
            }

            foreach (var prs in ProcessRequiredSkills)
            {
                prs.Process = Processes.Find(p => p.Id == prs.ProcessId);
                prs.Skill = Skills.Find(s => s.Id == prs.SkillId);
            }
        }

        /// <summary>
        /// Generates demo data for testing
        /// </summary>
        public void GenerateDemoData()
        {
            // Clear existing data
            InitializeData();

            // Create Skills
            Skills.Add(new Skill { Id = 1, Name = "C# Programming", Description = "Object-oriented programming in C#", Category = SkillType.Technical });
            Skills.Add(new Skill { Id = 2, Name = "SQL", Description = "Database management", Category = SkillType.Technical });
            Skills.Add(new Skill { Id = 3, Name = "Leadership", Description = "Team leadership abilities", Category = SkillType.Soft });
            Skills.Add(new Skill { Id = 4, Name = "Communication", Description = "Effective communication", Category = SkillType.Soft });

            // Create Departments
            Departments.Add(new Department { Id = 1, Name = "IT", Budget = 500000 });
            Departments.Add(new Department { Id = 2, Name = "HR", Budget = 200000 });

            // Create Positions
            Positions.Add(new Position
            {
                Id = 1,
                Name = "Senior Developer",
                DepartmentId = 1,
                Capacity = 5,
                PositionLevel = 3,
                MinSalary = 80000,
                MaxSalary = 120000,
                HiringCost = 5000
            });

            Positions.Add(new Position
            {
                Id = 2,
                Name = "Junior Developer",
                DepartmentId = 1,
                Capacity = 10,
                PositionLevel = 1,
                MinSalary = 40000,
                MaxSalary = 60000,
                HiringCost = 3000
            });

            Positions.Add(new Position
            {
                Id = 3,
                Name = "HR Manager",
                DepartmentId = 2,
                Capacity = 2,
                PositionLevel = 3,
                MinSalary = 70000,
                MaxSalary = 100000,
                HiringCost = 4000
            });

            // Create Employees with string IdentityNumber (11 digits)
            Employees.Add(new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IdentityNumber = "12345678901",  // Changed to string format
                DateOfBirth = new DateTime(1990, 1, 1),
                DepartmentId = 1,
                PositionId = 1,
                CurrentSalary = 90000,
                HireDate = DateTime.Now.AddYears(-2)
            });

            Employees.Add(new Employee
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                IdentityNumber = "98765432109",  // Changed to string format
                DateOfBirth = new DateTime(1992, 5, 15),
                DepartmentId = 1,
                PositionId = 2,
                CurrentSalary = 50000,
                HireDate = DateTime.Now.AddYears(-1)
            });

            Employees.Add(new Employee
            {
                Id = 3,
                FirstName = "Alice",
                LastName = "Johnson",
                IdentityNumber = "11122233344",  // Changed to string format
                DateOfBirth = new DateTime(1988, 8, 20),
                DepartmentId = 2,
                PositionId = 3,
                CurrentSalary = 85000,
                HireDate = DateTime.Now.AddYears(-3)
            });

            // Create Employee Skills
            EmployeeSkills.Add(new EmployeeSkill
            {
                EmployeeId = 1,
                SkillId = 1,
                CurrentLevel = SkillDegree.Advanced,
                SkillSource = SkillSource.Training
            });

            EmployeeSkills.Add(new EmployeeSkill
            {
                EmployeeId = 1,
                SkillId = 2,
                CurrentLevel = SkillDegree.Expert,
                SkillSource = SkillSource.Previous
            });

            EmployeeSkills.Add(new EmployeeSkill
            {
                EmployeeId = 2,
                SkillId = 1,
                CurrentLevel = SkillDegree.Developing,
                SkillSource = SkillSource.Training
            });

            EmployeeSkills.Add(new EmployeeSkill
            {
                EmployeeId = 3,
                SkillId = 3,
                CurrentLevel = SkillDegree.Advanced,
                SkillSource = SkillSource.Previous
            });

            EmployeeSkills.Add(new EmployeeSkill
            {
                EmployeeId = 3,
                SkillId = 4,
                CurrentLevel = SkillDegree.Expert,
                SkillSource = SkillSource.Previous
            });

            // Create Position Required Skills
            PositionRequiredSkills.Add(new PositionRequiredSkill
            {
                PositionId = 1,
                SkillId = 1,
                RequiredLevel = 4,  // Advanced
                IsMandatory = true
            });

            PositionRequiredSkills.Add(new PositionRequiredSkill
            {
                PositionId = 1,
                SkillId = 2,
                RequiredLevel = 3,  // Competent
                IsMandatory = true
            });

            PositionRequiredSkills.Add(new PositionRequiredSkill
            {
                PositionId = 2,
                SkillId = 1,
                RequiredLevel = 2,  // Developing
                IsMandatory = true
            });

            BuildNavigationProperties();
            SaveAllData();
        }
    }
}