using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FG.TaskRepetitor
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddTaskRepetitor(this IServiceCollection services)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            var repetitiveTaskTypes = callingAssembly.GetTypes()
                .Where(t => typeof(RepetitiveTask).IsAssignableFrom(t))
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in repetitiveTaskTypes)
            {
                services.AddScoped(typeof(RepetitiveTask), type);
            }

            var asyncRepetitiveTaskTypes = callingAssembly.GetTypes()
                .Where(t => typeof(AsyncRepetitiveTask).IsAssignableFrom(t))
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in asyncRepetitiveTaskTypes)
            {
                services.AddScoped(typeof(AsyncRepetitiveTask), type);
            }

            services.AddHostedService<TaskRepetitorHostedService>();
            services.AddHostedService<AsyncTaskRepetitorHostedService>();
            return services;
        }
    }
}
