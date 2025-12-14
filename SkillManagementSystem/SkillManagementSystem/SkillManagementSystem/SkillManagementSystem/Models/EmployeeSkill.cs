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
    public class EmployeeSkill
    {
        public int EmployeeId { get; set; }
        public int SkillId { get; set; }
        public SkillDegree CurrentLevel { get; set; }
        public SkillSource SkillSource { get; set; }
        public DateTime AcquisitionDate { get; set; }

        [JsonIgnore]
        public Employee Employee { get; set; }

        [JsonIgnore]
        public Skill Skill { get; set; }

        //Constructor
        public EmployeeSkill()
        {
            CurrentLevel = SkillDegree.Beginner;
            AcquisitionDate = DateTime.Now;
        }
    }
}
