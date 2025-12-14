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
        public int? ReportsToPositionID { get; set; }
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
        public List<PositionProcess> UnmetProcesses
        {
            get
            {
                var unmetProcesses = new List<PositionProcess>();
                foreach (var process in Processes)
                {
                    bool canPerform = Employees.Any(worker =>
                        process.RequiredSkills.All(reqSkill =>
                            worker.Skills.Any(ws =>
                                ws.SkillId == reqSkill.SkillID &&
                                ws.SkillDegree >= reqSkill.RequiredLevel)));

                    if (!canPerform)
                    {
                        unmetProcesses.Add(process);
                    }
                }
                return unmetProcesses;
            }
        }

        [JsonIgnore]
        public List<PositionSkill> RequiredSkills { get; set; }
        

        //Constructor
        public Position()
        {
            RequiredSkills = new List<PositionSkill>();
            Processes = new List<PositionProcess>();
            MinSalary = 0;
            MaxSalary = 0;
        }
    }
}
