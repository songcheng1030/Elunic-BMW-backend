using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Services
{
    public class KpiService
    {
        private readonly ILogger<KpiService> _logger;
        private readonly AppDbContext _dbContext;

        public KpiService(ILogger<KpiService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, int>> CalculateForUseCaseAsync(Guid id)
        {
            var useCase = await _dbContext.UseCases
                .Where(u => u.Id == id)
                .Include(u => u.Steps)
                .FirstOrDefaultAsync();

            if (useCase == null)
            {
                throw new NotFoundException("UseCase not found");
            }

            if (useCase.Status == UseCaseStatus.Declined)
            {
                return CreateKpiDictionary(useCase.Steps);
            }

            var kpis = CreateKpiDictionary(useCase.Steps);
            //skip latest step if useCase is declined
            if (useCase.Status == UseCaseStatus.Declined)
            {
                return kpis;
            }

            var currentStep = useCase.CurrentStep;

            //Check if latest is already calculated
            var latestStep = useCase.Steps.FirstOrDefault(s => s.Type == currentStep);
            if (latestStep != null && latestStep.CompletedAt != null)
            {
                return kpis;
            }

            DateTime? dateToCalculate;
            var steps = useCase.getCompletedSteps();
            if (steps.Count == 0)
            {
                dateToCalculate = useCase.CreatedAt;
            }
            else
            {
                dateToCalculate = steps[steps.Count - 1].CompletedAt;
            }

            var dictKey = UseCaseStepEntity.StepToString(currentStep);
            kpis[dictKey] = GetDurationFromDates(DateTime.Now, dateToCalculate);

            return kpis;
        }

        private Dictionary<string, int> CreateKpiDictionary(ICollection<UseCaseStepEntity> steps)
        {
            var kpis = new Dictionary<string, int>();

            foreach (UseCaseStep enumStep in Enum.GetValues(typeof(UseCaseStep)))
            {
                // No calculations for the last step
                if (enumStep == UseCaseStep.Live)
                {
                    continue;
                }

                var key = UseCaseStepEntity.StepToString(enumStep);
                var step = steps.FirstOrDefault(item => item.Type == enumStep);
                var value = step != null ? step.Duration : 0;
                kpis.Add(key, value);
            }

            return kpis;
        }

        private int GetDurationFromDates(DateTime? current, DateTime? past)
        {
            TimeSpan ts = (TimeSpan)((DateTime)current - (DateTime)past);
            return (int)ts.TotalMinutes;
        }
    }
}
