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
    public class TrainingPrerequisiteSkill
    {
        public int TrainingId { get; set; }
        public int SkillId { get; set; }
        public int MinimumLevel { get; set; }

        [JsonIgnore]
        public Training Training { get; set; }

        [JsonIgnore]
        public Skill Skill { get; set; }
    }
}
