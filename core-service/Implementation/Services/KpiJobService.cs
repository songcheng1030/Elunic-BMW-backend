using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIQXCoreService.Implementation.Services
{
    public interface CronJobService
    {
        Task DoWork(CancellationToken cancellationToken);
    }

    public class KpiJobService : CronJobService
    {
        private readonly ILogger<KpiJobService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly KpiCache _kpiCache = KpiCache.Instance;

        public KpiJobService(ILogger<KpiJobService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Running Kpi Calculation");
                var plantIds = await GetPlantIdsAsync();

                foreach (var plantId in plantIds)
                {
                    var kpis = CreateKpiDictionary();
                    var counts = CreateKpiDictionary();

                    var baseQuery = _dbContext.UseCaseSteps
                        .Where(s => s.UseCase.Plant.Id == plantId)
                        .Where(s => s.Type != UseCaseStep.Live)
                        // Keep the previous completed steps in calculations even if use case was declined.
                        .Where(s => s.UseCase.Status != UseCaseStatus.Declined || s.CompletedAt != null);

                    var completedStepsKpis = await baseQuery
                        .Where(s => s.CompletedAt != null)
                        .Select(s => new
                        {
                            Type = s.Type,
                            Duration = s.Duration
                        })
                        .GroupBy(s => s.Type, s => s.Duration, (type, durations) => new
                        {
                            Type = type,
                            Average = durations.Average(),
                            Count = durations.Count(),
                        })
                        .ToListAsync();

                    foreach (var step in completedStepsKpis)
                    {
                        var index = UseCaseStepEntity.StepToString(step.Type);
                        kpis[index] = step.Average;
                        counts[index] = step.Count;
                    }

                    var incompleteSteps = await baseQuery
                        .Where(s => s.Type != UseCaseStep.InitialRequest)
                        .Where(s => s.UseCase.Steps.Where(s => s.CompletedAt != null).Count() > 0)
                        // There might be already started steps that are not the current step.
                        // This happens when someone prefills a step and saves it.
                        // We skip these for kpi calculations.
                        .Where(s => s.Type == s.UseCase.CurrentStep)
                        .Where(s => s.CompletedAt == null)
                        .Select(s => new
                        {
                            Type = s.Type,
                            Duration = (int)((TimeSpan)(DateTime.Now - (DateTime)s.UseCase.Steps
                                .Where(s => s.CompletedAt != null)
                                .OrderByDescending(s => s.CompletedAt)
                                .Select(s => s.CompletedAt).FirstOrDefault())
                            ).TotalMinutes
                        })
                        .ToListAsync();

                    foreach (var step in incompleteSteps)
                    {
                        var index = UseCaseStepEntity.StepToString(step.Type);
                        // Update average with additional value and calculate new average.
                        kpis[index] = (int)((kpis[index] * counts[index] + step.Duration) / (counts[index] + 1));
                        counts[index] += 1;
                    }

                    var incompleteInitialStepDurations = await baseQuery
                        .Where(s => s.Type == UseCaseStep.InitialRequest)
                        .Where(s => s.CompletedAt == null)
                        .Select(s => (int)((TimeSpan)(DateTime.Now - (DateTime)s.UseCase.CreatedAt)).TotalMinutes)
                        .ToListAsync();

                    if (incompleteInitialStepDurations.Count() > 0)
                    {
                        var index = UseCaseStepEntity.StepToString(UseCaseStep.InitialRequest);
                        var newCount = incompleteInitialStepDurations.Count();
                        // Update average with multiple additional values and calculate new average.
                        kpis[index] = (int)((kpis[index] * counts[index] + incompleteInitialStepDurations.Sum()) / (counts[index] + newCount));
                        counts[index] += newCount;
                    }

                    // Add duration for a not yet created steps that.
                    // In other words the use case is ongoing and the previous step was completed but no new steps was added yet.
                    // In this case we will not find it in the UseCaseSteps collection. Instead we get the current step and calculate
                    // its duration as a diff to the last completed step and now.
                    var notStartedSteps = await _dbContext.UseCases
                        .Where(u => u.Plant.Id == plantId)
                        .Where(u => u.CurrentStep != UseCaseStep.Live)
                        .Where(u => u.Steps.All(s => s.CompletedAt != null))
                        .Select(u => new
                        {
                            Type = u.CurrentStep,
                            Duration = (int)(((TimeSpan)(DateTime.Now - (u.Steps.OrderByDescending(s => s.CompletedAt).Select(s => s.CompletedAt).FirstOrDefault() ?? u.CreatedAt))).TotalMinutes)
                        })
                        .ToListAsync();

                    foreach (var step in notStartedSteps)
                    {
                        var index = UseCaseStepEntity.StepToString(step.Type);
                        // Update average with additional value and calculate new average.
                        kpis[index] = (int)((kpis[index] * counts[index] + step.Duration) / (counts[index] + 1));
                        counts[index] += 1;
                    }

                    // Cast dictionary to int values.
                    _kpiCache.Plants[plantId] = kpis.ToDictionary(item => item.Key, item => (int)item.Value);
                }

                _logger.LogInformation($"Completed Kpi Calculation. Took {(DateTime.Now - start).TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                // Prevent job from throwing to keep it running.
                _logger.LogError(ex, "Could not calculate Kpis");
            }
        }

        private async Task<IList<string>> GetPlantIdsAsync()
        {
            return await _dbContext.Plants.Select(p => p.Id).ToListAsync();
        }

        private Dictionary<string, double> CreateKpiDictionary()
        {
            var kpis = new Dictionary<string, double>();

            foreach (UseCaseStep step in Enum.GetValues(typeof(UseCaseStep)))
            {
                // No calculations for the last step
                if (step == UseCaseStep.Live)
                {
                    continue;
                }

                var value = UseCaseStepEntity.StepToString(step);
                kpis.Add(value, 0);
            }

            return kpis;
        }
    }
}
