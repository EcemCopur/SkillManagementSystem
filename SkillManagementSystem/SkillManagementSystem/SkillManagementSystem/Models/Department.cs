using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Newtonsoft.Json;


namespace SkillManagementSystem.Models
{
    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public decimal Budget { get; set; }

        //İlişkisel veriler
        [JsonIgnore]
        public List<Position> Positions { get; set; }

        [JsonIgnore]
        public List<Employee> Employees { get; set; }

        public Department() 
        {
            Positions = new List<Position>();
            Employees = new List<Employee>();
            Budget= 0;
        }
    }
}
