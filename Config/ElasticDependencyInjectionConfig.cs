using System.Collections.Concurrent;
using elasticsearch.Interfaces;
using elasticsearch.Jobs.Config;
using elasticsearch.Models.Services;
using elasticsearch.Models.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace elasticsearch.Config
{
    public static class ElasticDependencyInjectionConfig
    {
        public static IServiceCollection AddElasticServiceDI(this IServiceCollection services)
        {
            services.AddScoped<IElasticsearchService, ElasticsearchService>();

            services.AddSingleton<ConcurrentQueue<IndexingTask>>();

            services.AddScoped<ElasticSearchMiddleware>();

            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            services.ConfigureOptions<ElasticBackgroundServiceSetup>();

            return services;
        }

        public static IApplicationBuilder UseElasticMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ElasticSearchMiddleware>();

            return app;
        }
    }
}