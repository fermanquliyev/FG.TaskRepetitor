using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FG.TaskRepetitor
{
    public sealed class TaskRepetitorHostedService(IServiceProvider serviceProvider, ILogger<TaskRepetitorHostedService> logger) : BackgroundService
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly ILogger<TaskRepetitorHostedService> logger = logger;
        private static readonly Dictionary<Type, DateTime> NextRunTimes = new();

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var repetitiveTasks = scope.ServiceProvider.GetServices<RepetitiveTask>();
                foreach (var task in repetitiveTasks)
                {
                    task.CalculateNextRun();
                    NextRunTimes.TryAdd(task.GetType(), task.NextRun);
                }
            }
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var repetitiveTasks = scope.ServiceProvider.GetServices<RepetitiveTask>().Where(x => NextRunTimes[x.GetType()] <= DateTime.Now);
                    foreach (var task in repetitiveTasks)
                    {
                        try
                        {
                            task.Execute();
                            task.CalculateNextRun();
                            NextRunTimes[task.GetType()] = task.NextRun;
                        }
                        catch (Exception ex)
                        {
                            task.OnError(ex);
                            logger.LogError(ex, "Error executing task {TaskType} at {DateTime}", task.GetType().Name, DateTime.Now);
                            task.CalculateNextRetry();
                            NextRunTimes[task.GetType()] = task.NextRun;
                        }
                    }
                }
                await Task.Delay(999, stoppingToken);
            }
        }
    }
}
