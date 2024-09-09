#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using elasticsearch.Interfaces;
using elasticsearch.Models.Generics;
using elasticsearch.Models.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace elasticsearch.Jobs;

internal class ElasticIndexBackgroundService : IJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ConcurrentQueue<IndexingTask> _indexingTasks;

    public ElasticIndexBackgroundService(IServiceScopeFactory serviceScopeFactory, ConcurrentQueue<IndexingTask> indexingTasks)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _indexingTasks = indexingTasks;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var elasticSearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        while (_indexingTasks.Count > 0)
        {
            if (!_indexingTasks.TryDequeue(out var task)) continue;

            try
            {
                await IndexDataToElastic(task, elasticSearchService);
            }
            catch (Exception e)
            {
                _indexingTasks.Enqueue(task);
            }
        }
    }

    public async Task IndexDataToElastic(IndexingTask task, IElasticsearchService elasticSearch)
    {
        GenericRequestResponse dataRequestResponse = task;

        string indexName = task.NameIndex;

        await elasticSearch.IndexRequestResponse(dataRequestResponse, indexName);
    }
}