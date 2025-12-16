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
    public class PositionProcess
    {
        public int PositionId { get; set; }
        public int ProcessId { get; set; }

        [JsonIgnore]
        public Position Position { get; set; }

        [JsonIgnore]
        public Process Process { get; set; }
    }
}
