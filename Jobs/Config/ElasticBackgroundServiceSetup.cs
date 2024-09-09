using br.com.sharklab.elasticsearch.Models.Tasks;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Concurrent;

namespace br.com.sharklab.elasticsearch.Jobs.Config
{
    internal class ElasticBackgroundServiceSetup : IConfigureOptions<QuartzOptions>
    {
        private readonly ConcurrentQueue<IndexingTask> _indexingTasks;
        private readonly IServiceProvider _serviceProvider;

        public ElasticBackgroundServiceSetup(IServiceProvider serviceProvider, ConcurrentQueue<IndexingTask> indexingTasks)
        {
            _serviceProvider = serviceProvider;
            _indexingTasks = indexingTasks;
        }

        public void Configure(QuartzOptions options)
        {
            var jobkey = JobKey.Create(nameof(ElasticIndexBackgroundService));

            options
                .AddJob<ElasticIndexBackgroundService>(jobBuilder => jobBuilder.WithIdentity(jobkey))
                .AddTrigger(trigger =>
                {
                    trigger.ForJob(jobkey)
                        .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(5).RepeatForever());
                });

            // TO DO CONFIGURAR CLASSSE QUE SERVIRA COMO MONITOR DA LISTA DE DADOS E SER O GATILHO PARA O JOB SER CHAMADO

            // Configurar o QueueMonitor para monitorar a fila
            //var schedulerFactory = new StdSchedulerFactory();
            //var scheduler = schedulerFactory.GetScheduler().Result;
            //var cancellationTokenSource = new CancellationTokenSource();
            //var queueMonitor = new QueueMonitor(scheduler, _indexingTasks, cancellationTokenSource.Token);

            //// Iniciar o monitoramento da fila em segundo plano a cada 2 segundos
            //Task.Run(async () =>
            //{
            //    while (!cancellationTokenSource.Token.IsCancellationRequested)
            //    {
            //        await queueMonitor.StartMonitoringAsync();
            //    }
            //});
        }
    }
}