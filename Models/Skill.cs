using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillManagementSystem.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SkillDegree> Levels { get; set; }

        public SkillType Category { get; set; }
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 5;


        public Skill()
        {
            Category = SkillType.NonClassified;
            Levels = new List<SkillDegree>();
        }
    }
}
