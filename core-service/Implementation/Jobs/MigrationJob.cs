using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIQXCoreService.Domain.Models;
using AIQXCoreService.Implementation.Persistence;
using AIQXCoreService.Implementation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MigrationJob : IHostedService
{
    private readonly IServiceScopeFactory scopeFactory;

    public MigrationJob(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var indexingService = scope.ServiceProvider.GetRequiredService<UseCaseIndexingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationJob>>();

            var start = DateTime.Now;

            logger.LogInformation("Running database schema migration");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation($"Completed database schema migration. Took {(DateTime.Now - start).TotalMilliseconds}ms");

            start = DateTime.Now;
            logger.LogInformation("Running migration job to set current step for old use cases");

            IList<UseCaseEntity> useCases = await dbContext.UseCases
                .Include(c => c.Steps)
                .Where(c => c.CurrentStep == UseCaseStep.InitialRequest)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            foreach (var useCase in useCases)
            {
                var completedSteps = useCase.Steps
                    .Where(s => s.CompletedAt != null)
                    .ToList();

                if (completedSteps.Count() > 0 && completedSteps.Count() < UseCaseStepEntity.StepsOrder.Count())
                {
                    var entry = dbContext.Entry(useCase);
                    entry.Entity.CurrentStep = UseCaseStepEntity.StepsOrder[completedSteps.Count()];
                }
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation($"Completed step migration job. Took {(DateTime.Now - start).TotalMilliseconds}ms");

            var contentIndexed = dbContext.UseCaseStepContent.Count() > 0;
            if (!contentIndexed)
            {
                start = DateTime.Now;
                logger.LogInformation("Running job to index content of old use cases");

                IList<UseCaseEntity> allUseCases = await dbContext.UseCases
                    .Include(c => c.Steps).ToListAsync();

                foreach (var useCase in allUseCases)
                {
                    await indexingService.IndexAsync(useCase);
                    foreach (var step in useCase.Steps)
                    {
                        await indexingService.IndexAsync(step);
                    }
                }

                logger.LogInformation($"Completed indexing job. Took {(DateTime.Now - start).TotalMilliseconds}ms");
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}