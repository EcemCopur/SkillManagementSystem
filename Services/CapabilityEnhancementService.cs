using System;
using System.Collections.Generic;
using System.Linq;
using SkillManagementSystem.Models;
using SkillManagementSystem.Services.DTOs;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Services
{
    public class CapabilityEnhancementService
    {
        private readonly DataManager _dataManager;
        private const double MINIMUM_SKILL_MATCH_PERCENTAGE = 0.75; // 75%

        public CapabilityEnhancementService(DataManager dataManager)
        {
            _dataManager = dataManager;
        }

        /// <summary>
        /// Analyzes all processes and identifies capability gaps
        /// </summary>
        public CapabilityEnhancementResult AnalyzeCapabilityGaps(decimal trainingCostPerLevel = 5000m)
        {
            var result = new CapabilityEnhancementResult
            {
                TrainingCostPerLevel = trainingCostPerLevel
            };

            var processGaps = new List<ProcessCapabilityGap>();

            foreach (var process in _dataManager.Processes)
            {
                var gap = AnalyzeProcessCapability(process, trainingCostPerLevel);

                // Only include processes with capability issues
                if (gap.Priority <= 3)
                {
                    processGaps.Add(gap);
                }
            }

            // Sort by priority (1 = highest priority)
            result.ProcessGaps = processGaps
                .OrderBy(g => g.Priority)
                .ThenByDescending(g => g.WorkerGap)
                .ToList();

            // Count by priority
            result.CriticalProcessesCount = processGaps.Count(g => g.Priority == 1);
            result.HighRiskProcessesCount = processGaps.Count(g => g.Priority == 2);
            result.BelowTargetProcessesCount = processGaps.Count(g => g.Priority == 3);

            return result;
        }

        private ProcessCapabilityGap AnalyzeProcessCapability(Process process, decimal trainingCostPerLevel)
        {
            var gap = new ProcessCapabilityGap
            {
                Process = process,
                AimedWorkersCount = process.AimedNumberOfWorkers
            };

            // Count capable workers
            gap.CapableWorkersCount = CountCapableWorkers(process);
            gap.WorkerGap = gap.AimedWorkersCount - gap.CapableWorkersCount;

            // Get assigned positions
            var assignedPositions = _dataManager.PositionProcesses
                .Where(pp => pp.ProcessId == process.Id)
                .Select(pp => _dataManager.Positions.FirstOrDefault(p => p.Id == pp.PositionId)?.Name)
                .Where(name => name != null)
                .ToList();
            gap.AssignedPositionNames = assignedPositions;

            // Determine priority
            if (gap.CapableWorkersCount == 0)
            {
                gap.Priority = 1;
                gap.PriorityReason = "CRITICAL: No capable workers - process cannot be performed";
            }
            else if (assignedPositions.Count > 0 && gap.WorkerGap > 0)
            {
                gap.Priority = 2;
                gap.PriorityReason = "HIGH: Process assigned to positions but has worker gap";
            }
            else if (gap.WorkerGap > 0)
            {
                gap.Priority = 3;
                gap.PriorityReason = $"MEDIUM: Below target by {gap.WorkerGap} workers";
            }
            else
            {
                gap.Priority = 4; // No issues
                gap.PriorityReason = "OK: Meets or exceeds target";
            }

            // Analyze missing skills
            gap.MissingSkills = AnalyzeMissingSkills(process);

            // Suggest trainings
            gap.SuggestedTrainings = SuggestTrainingsForProcess(process);

            // Suggest employees (quickest and cheapest)
            SuggestEmployeesForProcess(process, gap, trainingCostPerLevel);

            return gap;
        }

        private int CountCapableWorkers(Process process)
        {
            if (process.RequiredSkills == null || process.RequiredSkills.Count == 0)
                return 0;

            return _dataManager.Employees.Count(emp =>
                emp.Status == EmployeeStatus.Active &&
                process.RequiredSkills.All(reqSkill =>
                    emp.Skills.Any(empSkill =>
                        empSkill.SkillId == reqSkill.SkillId &&
                        (int)empSkill.CurrentLevel >= reqSkill.RequiredLevel)));
        }

        private List<MissingSkillSummary> AnalyzeMissingSkills(Process process)
        {
            var summary = new List<MissingSkillSummary>();

            foreach (var reqSkill in process.RequiredSkills)
            {
                var skill = _dataManager.Skills.FirstOrDefault(s => s.Id == reqSkill.SkillId);

                var employeesWithSkill = _dataManager.Employees.Count(emp =>
                    emp.Status == EmployeeStatus.Active &&
                    emp.Skills.Any(es => es.SkillId == reqSkill.SkillId));

                var employeesAtRequiredLevel = _dataManager.Employees.Count(emp =>
                    emp.Status == EmployeeStatus.Active &&
                    emp.Skills.Any(es => es.SkillId == reqSkill.SkillId &&
                                        (int)es.CurrentLevel >= reqSkill.RequiredLevel));

                summary.Add(new MissingSkillSummary
                {
                    SkillId = reqSkill.SkillId,
                    SkillName = skill?.Name ?? "Unknown",
                    RequiredLevel = reqSkill.RequiredLevel,
                    EmployeesWithSkill = employeesWithSkill,
                    EmployeesAtRequiredLevel = employeesAtRequiredLevel
                });
            }

            return summary;
        }

        private List<TrainingSuggestion> SuggestTrainingsForProcess(Process process)
        {
            var suggestions = new List<TrainingSuggestion>();

            // Get all trainings that teach skills required by this process
            var relevantTrainings = _dataManager.Trainings
                .Where(t => t.Status != TrainingStatus.Cancelled &&
                           t.TrainingSkills.Any(ts =>
                               process.RequiredSkills.Any(prs => prs.SkillId == ts.SkillId)))
                .ToList();

            foreach (var training in relevantTrainings)
            {
                var targetDept = _dataManager.Departments.FirstOrDefault(d => d.Id == training.TargetDepartmentId);

                var suggestion = new TrainingSuggestion
                {
                    Training = training,
                    TargetDepartmentName = targetDept?.Name ?? "Unknown",
                    SkillsItAddresses = training.TrainingSkills
                        .Where(ts => process.RequiredSkills.Any(prs => prs.SkillId == ts.SkillId))
                        .Select(ts => ts.SkillId)
                        .ToList()
                };

                // Count eligible employees (meet prerequisites and from target department)
                suggestion.EligibleEmployeesCount = CountEligibleEmployees(training);

                suggestions.Add(suggestion);
            }

            return suggestions.OrderByDescending(s => s.SkillsItAddresses.Count).ToList();
        }

        private int CountEligibleEmployees(Training training)
        {
            var targetDeptEmployees = _dataManager.Employees
                .Where(e => e.Status == EmployeeStatus.Active &&
                           e.DepartmentId == training.TargetDepartmentId)
                .ToList();

            int eligibleCount = 0;

            foreach (var employee in targetDeptEmployees)
            {
                if (MeetsTrainingPrerequisites(employee, training))
                {
                    eligibleCount++;
                }
            }

            return eligibleCount;
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

        private void SuggestEmployeesForProcess(Process process, ProcessCapabilityGap gap, decimal trainingCostPerLevel)
        {
            var allSuggestions = new List<EmployeeSuggestion>();

            // Get employees from target departments (positions assigned to this process)
            var targetDepartmentIds = _dataManager.PositionProcesses
                .Where(pp => pp.ProcessId == process.Id)
                .Select(pp => _dataManager.Positions.FirstOrDefault(p => p.Id == pp.PositionId)?.DepartmentId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var targetDeptEmployees = _dataManager.Employees
                .Where(e => e.Status == EmployeeStatus.Active &&
                           targetDepartmentIds.Contains(e.DepartmentId))
                .ToList();

            // Evaluate each employee
            foreach (var employee in targetDeptEmployees)
            {
                var suggestion = EvaluateEmployeeForProcess(employee, process, targetDepartmentIds, trainingCostPerLevel);

                // Only include employees with at least 75% skill match
                if (suggestion.MatchPercentage >= MINIMUM_SKILL_MATCH_PERCENTAGE * 100)
                {
                    allSuggestions.Add(suggestion);
                }
            }

            // Sort for quickest fix (highest match percentage, fewest missing skills)
            gap.QuickestFixEmployees = allSuggestions
                .OrderByDescending(s => s.MatchPercentage)
                .ThenBy(s => s.MissingSkillsCount)
                .Take(5)
                .ToList();

            // Sort for cheapest fix (lowest training cost)
            gap.CheapestFixEmployees = allSuggestions
                .OrderBy(s => s.EstimatedTrainingCost)
                .ThenByDescending(s => s.MatchPercentage)
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
            return _dataManager.Trainings
                .Where(t => t.Status != TrainingStatus.Cancelled &&
                           t.TargetDepartmentId == employee.DepartmentId &&
                           t.TrainingSkills.Any(ts => ts.SkillId == skillId && ts.TargetLevel >= targetLevel))
                .OrderBy(t => t.Cost)
                .Take(2) // Top 2 cheapest trainings
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
    }
}