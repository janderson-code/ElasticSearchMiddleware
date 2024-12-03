#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using elasticsearch.Interfaces;
using elasticsearch.Models.Generics;
using elasticsearch.Models.Tasks;
using Quartz;

namespace elasticsearch.Jobs;

internal class ElasticIndexBackgroundService : IJob
{
    private ConcurrentQueue<IndexingTask> _indexingTasks;
    private readonly IElasticsearchService _elasticsearchService;

    public ElasticIndexBackgroundService(ConcurrentQueue<IndexingTask> indexingTasks,
        IElasticsearchService elasticsearchService)
    {
        _indexingTasks = indexingTasks;
        _elasticsearchService = elasticsearchService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        while (_indexingTasks.Count > 0)
        {
            if (!_indexingTasks.TryDequeue(out var task)) continue;

            try
            {
                await IndexDataToElastic(task, _elasticsearchService);
            }
            catch (Exception e)
            {
                _indexingTasks.Enqueue(task);
            }
        }
    }

    public async Task IndexDataToElastic(IndexingTask task, IElasticsearchService elasticSearch)
    {
        var dataRequestResponse = task;

        string indexName = task.NameIndex;

        await elasticSearch.IndexRequestResponse(dataRequestResponse, indexName);
    }
}