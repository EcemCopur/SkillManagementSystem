using System;
using System.Collections.Generic;
using System.Linq;
using SkillManagementSystem.Models;
using SkillManagementSystem.Services.DTOs;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Services
{
    public class WorkerRelianceService
    {
        private readonly DataManager _dataManager;

        public WorkerRelianceService(DataManager dataManager)
        {
            _dataManager = dataManager;
        }

        /// <summary>
        /// Analyzes worker reliance issues across all processes
        /// </summary>
        public WorkerRelianceResult AnalyzeWorkerReliance(decimal trainingCostPerLevel = 5000m)
        {
            var result = new WorkerRelianceResult
            {
                TrainingCostPerLevel = trainingCostPerLevel
            };

            var issues = new List<WorkerRelianceIssue>();

            foreach (var process in _dataManager.Processes)
            {
                var issue = AnalyzeProcessReliance(process, trainingCostPerLevel);

                // Only include processes with reliance issues (priority 1, 2, or 3)
                if (issue.Priority <= 3)
                {
                    issues.Add(issue);
                }
            }

            // Sort by priority
            result.RelianceIssues = issues
                .OrderBy(i => i.Priority)
                .ThenByDescending(i => i.GapPercentage)
                .ToList();

            // Count by priority
            result.CriticalProcessesCount = issues.Count(i => i.Priority == 1);
            result.HighRiskProcessesCount = issues.Count(i => i.Priority == 2);
            result.BelowTargetProcessesCount = issues.Count(i => i.Priority == 3);

            return result;
        }

        private WorkerRelianceIssue AnalyzeProcessReliance(Process process, decimal trainingCostPerLevel)
        {
            var issue = new WorkerRelianceIssue
            {
                Process = process,
                AimedNumberOfWorkers = process.AimedNumberOfWorkers
            };

            // Find all capable workers
            var capableEmployees = FindCapableEmployees(process);
            issue.CurrentCapableEmployees = capableEmployees;
            issue.CurrentCapableWorkers = capableEmployees.Count;
            issue.WorkerGap = issue.AimedNumberOfWorkers - issue.CurrentCapableWorkers;

            // Calculate gap percentage
            if (issue.AimedNumberOfWorkers > 0)
            {
                issue.GapPercentage = (double)issue.WorkerGap / issue.AimedNumberOfWorkers * 100;
            }

            // Get assigned positions
            var assignedPositions = _dataManager.PositionProcesses
                .Where(pp => pp.ProcessId == process.Id)
                .Select(pp => _dataManager.Positions.FirstOrDefault(p => p.Id == pp.PositionId)?.Name)
                .Where(name => name != null)
                .ToList();
            issue.AssignedPositionNames = assignedPositions;

            // Determine priority
            if (issue.CurrentCapableWorkers == 0)
            {
                issue.Priority = 1;
                issue.PriorityReason = "CRITICAL: No capable workers";
            }
            else if (issue.CurrentCapableWorkers == 1)
            {
                issue.Priority = 2;
                issue.PriorityReason = "HIGH RISK: Only 1 capable worker (single point of failure)";
            }
            else if (issue.WorkerGap > 0)
            {
                issue.Priority = 3;
                issue.PriorityReason = $"BELOW TARGET: Gap of {issue.WorkerGap} workers ({issue.GapPercentage:F1}%)";
            }
            else
            {
                issue.Priority = 4; // No issues
                issue.PriorityReason = "OK: Meets or exceeds target";
            }

            // Suggest employees if there's a gap
            if (issue.WorkerGap > 0)
            {
                SuggestEmployeesForProcess(process, issue, trainingCostPerLevel);
            }

            return issue;
        }

        private List<Employee> FindCapableEmployees(Process process)
        {
            if (process.RequiredSkills == null || process.RequiredSkills.Count == 0)
                return new List<Employee>();

            return _dataManager.Employees
                .Where(emp =>
                    emp.Status == EmployeeStatus.Active &&
                    process.RequiredSkills.All(reqSkill =>
                        emp.Skills.Any(empSkill =>
                            empSkill.SkillId == reqSkill.SkillId &&
                            (int)empSkill.CurrentLevel >= reqSkill.RequiredLevel)))
                .ToList();
        }

        private void SuggestEmployeesForProcess(Process process, WorkerRelianceIssue issue, decimal trainingCostPerLevel)
        {
            // Get departments where this process is assigned
            var targetDepartmentIds = _dataManager.PositionProcesses
                .Where(pp => pp.ProcessId == process.Id)
                .Select(pp => _dataManager.Positions.FirstOrDefault(p => p.Id == pp.PositionId)?.DepartmentId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // Get all active employees who are NOT already capable
            var capableEmployeeIds = issue.CurrentCapableEmployees.Select(e => e.Id).ToList();
            var allEmployees = _dataManager.Employees
                .Where(e => e.Status == EmployeeStatus.Active && !capableEmployeeIds.Contains(e.Id))
                .ToList();

            var sameDeptSuggestions = new List<EmployeeSuggestion>();
            var crossDeptSuggestions = new List<EmployeeSuggestion>();

            foreach (var employee in allEmployees)
            {
                var suggestion = EvaluateEmployeeForProcess(employee, process, targetDepartmentIds, trainingCostPerLevel);

                // Only include employees who are close to qualifying (need 1-2 skills)
                if (suggestion.MissingSkillsCount <= 2 && suggestion.MissingSkillsCount > 0)
                {
                    if (suggestion.IsSameDepartment)
                    {
                        sameDeptSuggestions.Add(suggestion);
                    }
                    else
                    {
                        crossDeptSuggestions.Add(suggestion);
                    }
                }
            }

            // Sort suggestions by closest to qualifying (highest match %, fewest missing skills, lowest cost)
            issue.SameDepartmentSuggestions = sameDeptSuggestions
                .OrderByDescending(s => s.MatchPercentage)
                .ThenBy(s => s.MissingSkillsCount)
                .ThenBy(s => s.EstimatedTrainingCost)
                .Take(5)
                .ToList();

            issue.CrossDepartmentSuggestions = crossDeptSuggestions
                .OrderByDescending(s => s.MatchPercentage)
                .ThenBy(s => s.MissingSkillsCount)
                .ThenBy(s => s.EstimatedTrainingCost)
                .Take(5)
                .ToList();
        }

        private EmployeeSuggestion EvaluateEmployeeForProcess(
            Employee employee,
            Process process,
            List<int> targetDepartmentIds,
            decimal trainingCostPerLevel)
        {
            var suggestion = new EmployeeSuggestion
            {
                Employee = employee,
                DepartmentName = _dataManager.Departments.FirstOrDefault(d => d.Id == employee.DepartmentId)?.Name ?? "Unknown",
                IsSameDepartment = targetDepartmentIds.Contains(employee.DepartmentId)
            };

            var currentPosition = _dataManager.Positions.FirstOrDefault(p => p.Id == employee.PositionId);
            suggestion.CurrentPositionName = currentPosition?.Name ?? "Unknown";

            int matchingSkills = 0;
            decimal totalCost = 0;
            int totalHours = 0;

            foreach (var reqSkill in process.RequiredSkills)
            {
                var empSkill = employee.Skills.FirstOrDefault(es => es.SkillId == reqSkill.SkillId);
                var skill = _dataManager.Skills.FirstOrDefault(s => s.Id == reqSkill.SkillId);

                if (empSkill != null && (int)empSkill.CurrentLevel >= reqSkill.RequiredLevel)
                {
                    matchingSkills++;
                }
                else
                {
                    // Missing or insufficient skill
                    int gap = empSkill == null ? reqSkill.RequiredLevel :
                              reqSkill.RequiredLevel - (int)empSkill.CurrentLevel;

                    suggestion.SkillGaps.Add(new SkillGapDetail
                    {
                        SkillId = reqSkill.SkillId,
                        SkillName = skill?.Name ?? "Unknown",
                        RequiredLevel = reqSkill.RequiredLevel,
                        CurrentLevel = empSkill != null ? (int?)empSkill.CurrentLevel : null,
                        LevelGap = gap,
                        IsMissing = empSkill == null
                    });

                    totalCost += gap * trainingCostPerLevel;

                    // Find trainings that teach this skill
                    var trainingsForSkill = FindTrainingsForSkill(reqSkill.SkillId, reqSkill.RequiredLevel, employee);
                    foreach (var training in trainingsForSkill)
                    {
                        if (!suggestion.TrainingPath.Any(tp => tp.Training.Id == training.Id))
                        {
                            suggestion.TrainingPath.Add(CreateTrainingPathStep(training, employee, suggestion.TrainingPath.Count + 1));
                            totalHours += training.DurationHours;
                        }
                    }
                }
            }

            suggestion.MatchingSkillsCount = matchingSkills;
            suggestion.MissingSkillsCount = process.RequiredSkills.Count - matchingSkills;
            suggestion.MatchPercentage = (double)matchingSkills / process.RequiredSkills.Count * 100;
            suggestion.EstimatedTrainingCost = totalCost;
            suggestion.EstimatedTrainingDuration = totalHours;

            return suggestion;
        }

        private List<Training> FindTrainingsForSkill(int skillId, int targetLevel, Employee employee)
        {
            // First try to find trainings in employee's department
            var trainingsInDept = _dataManager.Trainings
                .Where(t => t.Status != TrainingStatus.Cancelled &&
                           t.TargetDepartmentId == employee.DepartmentId &&
                           t.TrainingSkills.Any(ts => ts.SkillId == skillId && ts.TargetLevel >= targetLevel))
                .OrderBy(t => t.Cost)
                .ToList();

            if (trainingsInDept.Any())
            {
                return trainingsInDept.Take(2).ToList();
            }

            // If no trainings in department, look company-wide
            return _dataManager.Trainings
                .Where(t => t.Status != TrainingStatus.Cancelled &&
                           t.TrainingSkills.Any(ts => ts.SkillId == skillId && ts.TargetLevel >= targetLevel))
                .OrderBy(t => t.Cost)
                .Take(2)
                .ToList();
        }

        private TrainingPathStep CreateTrainingPathStep(Training training, Employee employee, int stepNumber)
        {
            var step = new TrainingPathStep
            {
                StepNumber = stepNumber,
                Training = training,
                TrainingName = training.Name,
                MeetsPrerequisites = MeetsTrainingPrerequisites(employee, training)
            };

            step.SkillsItTeaches = training.TrainingSkills
                .Select(ts => _dataManager.Skills.FirstOrDefault(s => s.Id == ts.SkillId)?.Name ?? "Unknown")
                .ToList();

            if (!step.MeetsPrerequisites)
            {
                // List missing prerequisites
                foreach (var prereq in training.PrerequisiteSkills)
                {
                    var empSkill = employee.Skills.FirstOrDefault(es => es.SkillId == prereq.SkillId);
                    if (empSkill == null || (int)empSkill.CurrentLevel < prereq.MinimumLevel)
                    {
                        var skill = _dataManager.Skills.FirstOrDefault(s => s.Id == prereq.SkillId);
                        step.MissingPrerequisites.Add($"{skill?.Name ?? "Unknown"} (Level {prereq.MinimumLevel})");
                    }
                }

                foreach (var prereq in training.PrerequisiteTrainings)
                {
                    var completed = employee.Trainings.Any(et =>
                        et.TrainingId == prereq.PrerequisiteTrainingId &&
                        et.Status == EmployeeTrainingStatus.Completed &&
                        et.Result == TraininResult.Passed);

                    if (!completed)
                    {
                        var prereqTraining = _dataManager.Trainings.FirstOrDefault(t => t.Id == prereq.PrerequisiteTrainingId);
                        step.MissingPrerequisites.Add($"Training: {prereqTraining?.Name ?? "Unknown"}");
                    }
                }
            }

            return step;
        }

        private bool MeetsTrainingPrerequisites(Employee employee, Training training)
        {
            // Check prerequisite skills
            foreach (var prereqSkill in training.PrerequisiteSkills)
            {
                var empSkill = employee.Skills.FirstOrDefault(es => es.SkillId == prereqSkill.SkillId);
                if (empSkill == null || (int)empSkill.CurrentLevel < prereqSkill.MinimumLevel)
                {
                    return false;
                }
            }

            // Check prerequisite trainings
            foreach (var prereqTraining in training.PrerequisiteTrainings)
            {
                var completed = employee.Trainings.Any(et =>
                    et.TrainingId == prereqTraining.PrerequisiteTrainingId &&
                    et.Status == EmployeeTrainingStatus.Completed &&
                    et.Result == TraininResult.Passed);

                if (!completed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}