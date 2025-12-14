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
    public class Training
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TargetDepartmentId {  get; set; }

        public ApplicationType ApplicationType { get; set; }
        public int Capacity { get; set; }
        public TrainingSource Source { get; set; }
        public CostType CostType { get; set; }
        public decimal Cost { get; set; }
        public int DurationHours { get; set; }
        public TrainingStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public List<TrainingSkill> TrainingSkills { get; set; } = new List<TrainingSkill>();

        [JsonIgnore]
        public List<TrainingPrerequisiteTraining> PrerequisiteTrainings { get; set; } = new List<TrainingPrerequisiteTraining>();

        [JsonIgnore]
        public List<TrainingPrerequisiteSkill> PrerequisiteSkills { get; set; } = new List<TrainingPrerequisiteSkill>();


    }
}
