using System;
using System.Threading;
using System.Threading.Tasks;
using AIQXCoreService.Implementation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIQXCoreService.Implementation.Jobs
{
    public class KpiJob : BaseCronJob
    {
        private readonly IServiceProvider _serviceProvider;

        public KpiJob(IScheduleConfig<KpiJob> config, IServiceProvider serviceProvider)
            : base(config.CronExpression, config.TimeZoneInfo, config.RunOnStartup)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<CronJobService>();
            await svc.DoWork(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
