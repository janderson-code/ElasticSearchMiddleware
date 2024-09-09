using br.com.sharklab.elasticsearch.Models.Tasks;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace br.com.sharklab.elasticsearch.Jobs.Monitors
{
    public class QueueMonitor
    {
        private readonly IScheduler _scheduler;
        private readonly ConcurrentQueue<IndexingTask> _indexingTasks;
        private readonly CancellationToken _cancellationToken;

        public QueueMonitor(IScheduler scheduler, ConcurrentQueue<IndexingTask> indexingTasks, CancellationToken cancellationToken)
        {
            _scheduler = scheduler;
            _indexingTasks = indexingTasks;
            _cancellationToken = cancellationToken;
        }

        public async Task StartMonitoringAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_indexingTasks.Count > 0)
                {
                    // Disparar o job quando a fila estiver preenchida
                    await _scheduler.TriggerJob(JobKey.Create(nameof(ElasticIndexBackgroundService)));
                }

                await Task.Delay(TimeSpan.FromSeconds(2), _cancellationToken); // Verificar a cada 2 segundos ou quando o token de cancelamento for acionado
            }
        }
    }
}