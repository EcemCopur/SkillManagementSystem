using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SkillManagementSystem.Models
{
    public class Employee
    {
        //Primary Key
        public int Id { get; set; }

        //Temel Bilgiler
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdentityNumber { get; set; }  // Changed from int to string
        public DateTime DateOfBirth { get; set; }

        //Work İnfo
        public DateTime HireDate { get; set; }
        public decimal CurrentSalary { get; set; }
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public EmployeeStatus Status { get; set; }
        public DateTime? TerminationDate { get; set; }

        //Navigation Props
        [JsonIgnore]
        public List<EmployeeSkill> Skills { get; set; }

        [JsonIgnore]
        public List<EmployeeTraining> Trainings { get; set; }

        [JsonIgnore]
        public List<int> ExpectedProcesses { get; set; } = new List<int>();



        //Computed Props

        //Tam isim
        public string FullName => $"{FirstName} {LastName}";

        //Çalışma Süresi
        public int Tenure
        {
            get
            {
                DateTime endDate = TerminationDate ?? DateTime.Now;
                int days = (endDate - HireDate).Days;
                return days / 30;
            }
        }

        //Constructor
        public Employee()
        {
            Skills = new List<EmployeeSkill>();
            Trainings = new List<EmployeeTraining>();

            Status = EmployeeStatus.Active;
            HireDate = DateTime.Now;

        }
    }
}