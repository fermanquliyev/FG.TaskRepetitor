using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FG.TaskRepetitor
{
    public sealed class AsyncTaskRepetitorHostedService(IServiceProvider serviceProvider, ILogger<AsyncTaskRepetitorHostedService> logger) : BackgroundService
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly ILogger<AsyncTaskRepetitorHostedService> logger = logger;
        private static readonly ConcurrentDictionary<Type, DateTime> AsyncNextRunTimes = new();

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Initializing {HostedService} at {DateTime}", nameof(AsyncTaskRepetitorHostedService), DateTime.UtcNow);
            using (var scope = serviceProvider.CreateScope())
            {
                var repetitiveTasks = scope.ServiceProvider.GetServices<AsyncRepetitiveTask>();
                foreach (var task in repetitiveTasks)
                {
                    task.CalculateNextRun();
                    AsyncNextRunTimes.TryAdd(task.GetType(), task.NextRun);
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
                    var repetitiveTasks = scope.ServiceProvider.GetServices<AsyncRepetitiveTask>().Where(x => AsyncNextRunTimes[x.GetType()] <= DateTime.UtcNow);
                    var tasks = new List<Task>();
                    foreach (var task in repetitiveTasks)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                logger.LogInformation("Executing task {TaskType} at {DateTime}", task.GetType().Name, DateTime.UtcNow);
                                await task.ExecuteAsync(stoppingToken);
                                task.CalculateNextRun();
                                AsyncNextRunTimes[task.GetType()] = task.NextRun;
                            }
                            catch (Exception ex)
                            {
                                await task.OnErrorAsync(ex, stoppingToken);
                                logger.LogError(ex, "Error executing task {TaskType} at {DateTime}", task.GetType().Name, DateTime.Now);
                                task.CalculateNextRetry();
                                AsyncNextRunTimes[task.GetType()] = task.NextRun;
                            }
                        }, stoppingToken));
                    }
                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var innerException in ex.InnerExceptions)
                        {
                            logger.LogError(innerException, "Unhandled exception during task execution.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unhandled exception during task execution.");
                    }
                }
                await Task.Delay(999, stoppingToken);
            }
        }
    }
}
