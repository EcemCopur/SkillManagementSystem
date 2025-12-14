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
    public class ProcessRequiredSkill
    {
        public int ProcessId { get; set; }
        public int SkillId { get; set; }
        public int RequiredLevel { get; set; }

        [JsonIgnore]
        public Process Process { get; set; }

        [JsonIgnore]
        public Skill Skill { get; set; }
    }
}
