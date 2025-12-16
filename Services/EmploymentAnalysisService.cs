using System;
using System.Collections.Generic;
using System.Linq;
using SkillManagementSystem.Models;
using SkillManagementSystem.Services.DTOs;
using SkillManagementSystem.Utilities;

namespace SkillManagementSystem.Services
{
    public class EmploymentAnalysisService
    {
        private readonly DataManager _dataManager;

        // Scoring weights (must sum to 100%)
        private const double MATCHING_SKILLS_WEIGHT = 0.60;  // 60%
        private const double LEVEL_PROXIMITY_WEIGHT = 0.25;  // 25%
        private const double COST_WEIGHT = 0.15;             // 15%

        private const double MINIMUM_MATCH_PERCENTAGE = 0.70; // 70%

        public EmploymentAnalysisService(DataManager dataManager)
        {
            _dataManager = dataManager;
        }

        /// <summary>
        /// Analyzes internal candidates for a position and compares with external hiring
        /// </summary>
        public EmploymentAnalysisResult AnalyzePositionCandidates(int positionId, decimal trainingCostPerLevel = 5000m)
        {
            var position = _dataManager.Positions.FirstOrDefault(p => p.Id == positionId);
            if (position == null)
                throw new ArgumentException("Position not found");

            var result = new EmploymentAnalysisResult
            {
                Position = position,
                TrainingCostPerLevel = trainingCostPerLevel
            };

            // Get required skills for the position
            var requiredSkills = _dataManager.PositionRequiredSkills
                .Where(prs => prs.PositionId == positionId)
                .ToList();

            if (requiredSkills.Count == 0)
            {
                // No required skills defined
                return result;
            }

            // Analyze internal candidates
            var internalCandidates = AnalyzeInternalCandidates(position, requiredSkills, trainingCostPerLevel);
            result.InternalCandidates = internalCandidates.OrderByDescending(c => c.TotalScore).Take(5).ToList();

            // Calculate external hiring option
            result.ExternalOption = CalculateExternalHiringCost(position);

            return result;
        }

        private List<InternalCandidateResult> AnalyzeInternalCandidates(
            Position position,
            List<PositionRequiredSkill> requiredSkills,
            decimal trainingCostPerLevel)
        {
            var candidates = new List<InternalCandidateResult>();

            // Get all active employees
            var activeEmployees = _dataManager.Employees
                .Where(e => e.Status == EmployeeStatus.Active)
                .ToList();

            foreach (var employee in activeEmployees)
            {
                var candidate = EvaluateCandidate(employee, position, requiredSkills, trainingCostPerLevel);

                // Only include candidates who meet minimum match percentage
                if (candidate.MatchPercentage >= MINIMUM_MATCH_PERCENTAGE * 100)
                {
                    candidates.Add(candidate);
                }
            }

            return candidates;
        }

        private InternalCandidateResult EvaluateCandidate(
            Employee employee,
            Position position,
            List<PositionRequiredSkill> requiredSkills,
            decimal trainingCostPerLevel)
        {
            var result = new InternalCandidateResult
            {
                Employee = employee,
                CurrentSalary = employee.CurrentSalary,
                RequiredSkillsCount = requiredSkills.Count
            };

            // Get employee's current position
            var currentPosition = _dataManager.Positions.FirstOrDefault(p => p.Id == employee.PositionId);
            result.CurrentPositionName = currentPosition?.Name ?? "Unknown";

            // Analyze each required skill
            int matchingSkillsCount = 0;
            double totalGap = 0;
            int gapCount = 0;
            decimal totalTrainingCost = 0;

            foreach (var requiredSkill in requiredSkills)
            {
                var employeeSkill = employee.Skills?.FirstOrDefault(es => es.SkillId == requiredSkill.SkillId);
                var skill = _dataManager.Skills.FirstOrDefault(s => s.Id == requiredSkill.SkillId);

                if (employeeSkill != null)
                {
                    // Employee has this skill
                    int currentLevel = (int)employeeSkill.CurrentLevel;
                    int requiredLevel = requiredSkill.RequiredLevel;

                    var matchDetail = new SkillMatchDetail
                    {
                        SkillId = requiredSkill.SkillId,
                        SkillName = skill?.Name ?? "Unknown",
                        RequiredLevel = requiredLevel,
                        CurrentLevel = currentLevel,
                        MeetsRequirement = currentLevel >= requiredLevel
                    };
                    result.MatchingSkills.Add(matchDetail);

                    if (currentLevel >= requiredLevel)
                    {
                        matchingSkillsCount++;
                    }
                    else
                    {
                        // Has skill but below required level
                        int gap = requiredLevel - currentLevel;
                        totalGap += gap;
                        gapCount++;
                        totalTrainingCost += gap * trainingCostPerLevel;

                        result.SkillGaps.Add(new SkillGapDetail
                        {
                            SkillId = requiredSkill.SkillId,
                            SkillName = skill?.Name ?? "Unknown",
                            RequiredLevel = requiredLevel,
                            CurrentLevel = currentLevel,
                            LevelGap = gap,
                            IsMissing = false
                        });
                    }
                }
                else
                {
                    // Employee doesn't have this skill
                    int gap = requiredSkill.RequiredLevel;
                    totalGap += gap;
                    gapCount++;
                    totalTrainingCost += gap * trainingCostPerLevel;

                    result.SkillGaps.Add(new SkillGapDetail
                    {
                        SkillId = requiredSkill.SkillId,
                        SkillName = skill?.Name ?? "Unknown",
                        RequiredLevel = requiredSkill.RequiredLevel,
                        CurrentLevel = null,
                        LevelGap = gap,
                        IsMissing = true
                    });
                }
            }

            result.MatchingSkillsCount = matchingSkillsCount;
            result.MatchPercentage = (double)matchingSkillsCount / requiredSkills.Count * 100;
            result.AverageSkillGap = gapCount > 0 ? totalGap / gapCount : 0;
            result.EstimatedTrainingCost = totalTrainingCost;

            // Calculate total score using weighted system
            result.TotalScore = CalculateWeightedScore(result, requiredSkills.Count, trainingCostPerLevel);

            return result;
        }

        private double CalculateWeightedScore(
            InternalCandidateResult candidate,
            int totalRequiredSkills,
            decimal trainingCostPerLevel)
        {
            // 1. Matching Skills Score (60%) - normalized 0-100
            double matchingScore = (double)candidate.MatchingSkillsCount / totalRequiredSkills * 100;

            // 2. Level Proximity Score (25%) - inverse of average gap, normalized 0-100
            // Lower gap = higher score
            // Assume max gap per skill is 5 (Expert level)
            double maxPossibleGap = 5.0;
            double proximityScore = candidate.AverageSkillGap == 0 ? 100 :
                (1 - (candidate.AverageSkillGap / maxPossibleGap)) * 100;
            proximityScore = Math.Max(0, proximityScore); // Ensure non-negative

            // 3. Cost Score (15%) - inverse of cost, normalized 0-100
            // Lower cost = higher score
            // Assume max training cost for normalization (e.g., 5 skills * 5 levels * cost per level)
            decimal maxPossibleCost = totalRequiredSkills * 5 * trainingCostPerLevel;
            double costScore = maxPossibleCost == 0 ? 100 :
                (1 - ((double)candidate.EstimatedTrainingCost / (double)maxPossibleCost)) * 100;
            costScore = Math.Max(0, costScore); // Ensure non-negative

            // Calculate weighted total score
            double totalScore = (matchingScore * MATCHING_SKILLS_WEIGHT) +
                               (proximityScore * LEVEL_PROXIMITY_WEIGHT) +
                               (costScore * COST_WEIGHT);

            return Math.Round(totalScore, 2);
        }

        private ExternalHiringOption CalculateExternalHiringCost(Position position)
        {
            // Starting salary = midpoint of salary range
            decimal startingSalary = (position.MinSalary + position.MaxSalary) / 2;

            return new ExternalHiringOption
            {
                PositionName = position.Name,
                HiringCost = position.HiringCost,
                StartingSalary = startingSalary,
                TotalCost = position.HiringCost + startingSalary
            };
        }

        /// <summary>
        /// Get all open positions (capacity > current employees)
        /// </summary>
        public List<Position> GetOpenPositions()
        {
            return _dataManager.Positions
                .Where(p => p.NumOpenPositions > 0)
                .OrderByDescending(p => p.NumOpenPositions)
                .ToList();
        }

        /// <summary>
        /// Get open positions filtered by department
        /// </summary>
        public List<Position> GetOpenPositionsByDepartment(int departmentId)
        {
            return _dataManager.Positions
                .Where(p => p.DepartmentId == departmentId && p.NumOpenPositions > 0)
                .OrderByDescending(p => p.NumOpenPositions)
                .ToList();
        }

        /// <summary>
        /// Get open positions filtered by position level
        /// </summary>
        public List<Position> GetOpenPositionsByLevel(int minLevel, int maxLevel)
        {
            return _dataManager.Positions
                .Where(p => p.PositionLevel >= minLevel &&
                           p.PositionLevel <= maxLevel &&
                           p.NumOpenPositions > 0)
                .OrderByDescending(p => p.NumOpenPositions)
                .ToList();
        }
    }
}