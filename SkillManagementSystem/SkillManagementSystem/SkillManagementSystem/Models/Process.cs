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
    public class Process
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProcessDescription { get; set; }
        public int AimedNumberOfWorkers { get; set; }

        [JsonIgnore]
        public List<ProcessRequiredSkill> RequiredSkills { get; set; } = new List<ProcessRequiredSkill>();
    }


}
