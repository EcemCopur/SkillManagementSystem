using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace SkillManagementSystem.Models
{
    public class ChangeHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public ChangeType ChangeType { get; set; }  // Already in your Enums
        public DateTime ChangeDate { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Notes { get; set; }

        [JsonIgnore]
        public Employee Employee { get; set; }

        public ChangeHistory()
        {
            ChangeDate = DateTime.Now;
        }
    }
}
