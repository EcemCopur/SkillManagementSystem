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
    public class EmployeeTraining
    {
        
        public int EmployeeId { get; set; }
        public int TrainingId { get; set; }
        public int? TrainingSessionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EmployeeTrainingStatus Status { get; set; }
        public TraininResult? Result { get; set; }

        [JsonIgnore]
        public Employee Worker { get; set; }

        [JsonIgnore]
        public Training Training { get; set; }

        [JsonIgnore]
        public TrainingSession TrainingSession { get; set; }

        //Constructor
        public EmployeeTraining() 
        {
            Status = EmployeeTrainingStatus.Assigned;
        }

    }
}
