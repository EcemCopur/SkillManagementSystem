namespace SkillManagementSystem.Models
{
    public enum EmployeeStatus
    {
        Active = 1, 
        Terminated = 2, //İşten ayrılan çalışan için
        
    }


    public enum TrainingStatus
    {
        Suggested=1,
        Assigned=2,
        Planned=3,
        Ongoing=4,
        Completed=5,
        Cancelled=6,
    }
    public enum SkillDegree
    {
        Beginner=1,
        Developing=2,
        Competent=3,
        Advanced=4,
        Expert=5
    }
    public enum RecruitmentType
    {
        Internal=1,
        External=2
    }
    public enum TraininResult
    {
        Passed=1,
        Failed=2,
        Pending=3
    }

    public enum SkillType
    {
        Technical=1,
        Soft=2,
        NonClassified=3
    }
    public enum SkillSource
    {
        Training=1,
        Previous=2
    }

    public enum EmployeeTrainingStatus
    {
        Assigned,
        Planned,
        Ongoing,
        Completed,
        Cancelled
    }
    public enum ApplicationType
    {
        Group,
        Individual
    }
    public enum TrainingSource
    {
        Internal,
        External
    }

    public enum CostType
    {
        PerAttendee,
        Total
    }

    public enum ChangeType
    {
        PositionChange,
        SalaryChange,
        DepartmentTransfer,
        SkillDegreeChange
    }
}
