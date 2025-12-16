using System;
using System.Collections.Generic;
using SkillManagementSystem.Models;

namespace SkillManagementSystem.Services.DTOs
{
    // ==================== EMPLOYMENT ANALYSIS DTOs ====================

    public class InternalCandidateResult
    {
        public Employee Employee { get; set; }
        public string CurrentPositionName { get; set; }
        public decimal CurrentSalary { get; set; }
        public double MatchPercentage { get; set; }
        public double TotalScore { get; set; }
        public int MatchingSkillsCount { get; set; }
        public int RequiredSkillsCount { get; set; }
        public double AverageSkillGap { get; set; }
        public decimal EstimatedTrainingCost { get; set; }
        public List<SkillGapDetail> SkillGaps { get; set; }
        public List<SkillMatchDetail> MatchingSkills { get; set; }

        public InternalCandidateResult()
        {
            SkillGaps = new List<SkillGapDetail>();
            MatchingSkills = new List<SkillMatchDetail>();
        }
    }

    public class SkillGapDetail
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public int RequiredLevel { get; set; }
        public int? CurrentLevel { get; set; }  // null if skill not present
        public int LevelGap { get; set; }
        public bool IsMissing { get; set; }  // True if employee doesn't have the skill at all
    }

    public class SkillMatchDetail
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public int RequiredLevel { get; set; }
        public int CurrentLevel { get; set; }
        public bool MeetsRequirement { get; set; }
    }

    public class ExternalHiringOption
    {
        public string PositionName { get; set; }
        public decimal HiringCost { get; set; }
        public decimal StartingSalary { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class EmploymentAnalysisResult
    {
        public Position Position { get; set; }
        public List<InternalCandidateResult> InternalCandidates { get; set; }
        public ExternalHiringOption ExternalOption { get; set; }
        public decimal TrainingCostPerLevel { get; set; }

        public EmploymentAnalysisResult()
        {
            InternalCandidates = new List<InternalCandidateResult>();
        }
    }

    // ==================== CAPABILITY ENHANCEMENT DTOs ====================

    public class ProcessCapabilityGap
    {
        public Process Process { get; set; }
        public int Priority { get; set; }  // 1 = Highest (no capable workers), 2 = High (assigned to positions), 3 = Medium (worker gap)
        public string PriorityReason { get; set; }
        public int CapableWorkersCount { get; set; }
        public int AimedWorkersCount { get; set; }
        public int WorkerGap { get; set; }
        public List<string> AssignedPositionNames { get; set; }
        public List<MissingSkillSummary> MissingSkills { get; set; }
        public List<TrainingSuggestion> SuggestedTrainings { get; set; }
        public List<EmployeeSuggestion> QuickestFixEmployees { get; set; }  // Employees closest to qualifying
        public List<EmployeeSuggestion> CheapestFixEmployees { get; set; }  // Employees with lowest training cost

        public ProcessCapabilityGap()
        {
            AssignedPositionNames = new List<string>();
            MissingSkills = new List<MissingSkillSummary>();
            SuggestedTrainings = new List<TrainingSuggestion>();
            QuickestFixEmployees = new List<EmployeeSuggestion>();
            CheapestFixEmployees = new List<EmployeeSuggestion>();
        }
    }

    public class MissingSkillSummary
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public int RequiredLevel { get; set; }
        public int EmployeesWithSkill { get; set; }  // How many employees have this skill (at any level)
        public int EmployeesAtRequiredLevel { get; set; }  // How many meet the required level
    }

    public class TrainingSuggestion
    {
        public Training Training { get; set; }
        public List<int> SkillsItAddresses { get; set; }  // Which missing skills this training helps with
        public int EligibleEmployeesCount { get; set; }  // Employees who meet prerequisites
        public string TargetDepartmentName { get; set; }

        public TrainingSuggestion()
        {
            SkillsItAddresses = new List<int>();
        }
    }

    public class EmployeeSuggestion
    {
        public Employee Employee { get; set; }
        public string CurrentPositionName { get; set; }
        public string DepartmentName { get; set; }
        public int MatchingSkillsCount { get; set; }
        public int MissingSkillsCount { get; set; }
        public double MatchPercentage { get; set; }
        public List<SkillGapDetail> SkillGaps { get; set; }
        public List<TrainingPathStep> TrainingPath { get; set; }
        public decimal EstimatedTrainingCost { get; set; }
        public int EstimatedTrainingDuration { get; set; }  // Total hours
        public bool IsSameDepartment { get; set; }

        public EmployeeSuggestion()
        {
            SkillGaps = new List<SkillGapDetail>();
            TrainingPath = new List<TrainingPathStep>();
        }
    }

    public class TrainingPathStep
    {
        public int StepNumber { get; set; }
        public Training Training { get; set; }
        public string TrainingName { get; set; }
        public List<string> SkillsItTeaches { get; set; }
        public bool MeetsPrerequisites { get; set; }
        public List<string> MissingPrerequisites { get; set; }

        public TrainingPathStep()
        {
            SkillsItTeaches = new List<string>();
            MissingPrerequisites = new List<string>();
        }
    }

    public class CapabilityEnhancementResult
    {
        public List<ProcessCapabilityGap> ProcessGaps { get; set; }
        public int CriticalProcessesCount { get; set; }  // 0 capable workers
        public int HighRiskProcessesCount { get; set; }  // Assigned to positions
        public int BelowTargetProcessesCount { get; set; }  // Below aimed worker count
        public decimal TrainingCostPerLevel { get; set; }

        public CapabilityEnhancementResult()
        {
            ProcessGaps = new List<ProcessCapabilityGap>();
        }
    }

    // ==================== WORKER RELIANCE DTOs ====================

    public class WorkerRelianceIssue
    {
        public Process Process { get; set; }
        public int Priority { get; set; }  // 1 = Critical (0 workers), 2 = High Risk (1 worker), 3 = Below Target
        public string PriorityReason { get; set; }
        public int CurrentCapableWorkers { get; set; }
        public int AimedNumberOfWorkers { get; set; }
        public int WorkerGap { get; set; }
        public double GapPercentage { get; set; }  // (Gap / Aimed) * 100
        public List<Employee> CurrentCapableEmployees { get; set; }
        public List<EmployeeSuggestion> SameDepartmentSuggestions { get; set; }
        public List<EmployeeSuggestion> CrossDepartmentSuggestions { get; set; }
        public List<string> AssignedPositionNames { get; set; }

        public WorkerRelianceIssue()
        {
            CurrentCapableEmployees = new List<Employee>();
            SameDepartmentSuggestions = new List<EmployeeSuggestion>();
            CrossDepartmentSuggestions = new List<EmployeeSuggestion>();
            AssignedPositionNames = new List<string>();
        }
    }

    public class WorkerRelianceResult
    {
        public List<WorkerRelianceIssue> RelianceIssues { get; set; }
        public int CriticalProcessesCount { get; set; }  // 0 capable workers
        public int HighRiskProcessesCount { get; set; }  // 1 capable worker
        public int BelowTargetProcessesCount { get; set; }  // 2+ but below target
        public decimal TrainingCostPerLevel { get; set; }

        public WorkerRelianceResult()
        {
            RelianceIssues = new List<WorkerRelianceIssue>();
        }
    }
}