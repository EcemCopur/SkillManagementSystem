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
    public class PositionRequiredSkill
    {
        public int PositionId { get; set; }
        public int SkillID { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsMandatory { get; set; }

        [JsonIgnore]
        public Position Position { get; set; }

        [JsonIgnore]
        public Skill Skill { get; set; }
    }
}
