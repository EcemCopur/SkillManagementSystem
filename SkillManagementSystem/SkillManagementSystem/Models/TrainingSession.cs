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
    public class TrainingSession
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public TrainingStatus Status { get; set; }
        public int CurrentEnrollmentCount { get; set; }
        public string TrainerName { get; set; }

        [JsonIgnore]
        public Training Training { get; set; }
    }
}
