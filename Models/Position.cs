using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkillManagementSystem.Models
{
    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public int Capacity { get; set; }
        public int PositionLevel { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public int? ReportsToPositionId { get; set; }
        public decimal HiringCost { get; set; }

        [JsonIgnore]
        public List<PositionRequiredSkill> RequiredSkills { get; set; } = new List<PositionRequiredSkill>();

        [JsonIgnore]
        public List<PositionProcess> Processes { get; set; } = new List<PositionProcess>();

        [JsonIgnore]
        public List<Employee> Employees { get; set; } = new List<Employee>();

        // Computed properties
        [JsonIgnore]
        public int NumOpenPositions => Capacity - Employees.Count;

        [JsonIgnore]
        public Department Department { get; set; }

        [JsonIgnore]
        public Position ReportsToPosition { get; set; }

        [JsonIgnore]
        public List<PositionProcess> UnmetProcesses
        {
            get
            {
                var unmetProcesses = new List<PositionProcess>();
                foreach (var positionProcess in Processes)
                {
                    // Check if any employee can perform this process
                    bool canPerform = Employees.Any(employee =>
                        positionProcess.Process.RequiredSkills.All(reqSkill =>
                            employee.Skills.Any(empSkill =>
                                empSkill.SkillId == reqSkill.SkillId &&
                                (int)empSkill.CurrentLevel >= reqSkill.RequiredLevel)));

                    if (!canPerform)
                    {
                        unmetProcesses.Add(positionProcess);
                    }
                }
                return unmetProcesses;
            }
        }
        
        //Constructor
        public Position()
        {
            RequiredSkills = new List<PositionRequiredSkill>();
            Processes = new List<PositionProcess>();
            Employees = new List<Employee>();
            MinSalary = 0;
            MaxSalary = 0;
        }
    }
}
