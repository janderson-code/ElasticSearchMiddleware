#nullable enable

using br.com.sharklab.elasticsearch.Models.Generics;
using br.com.sharklab.elasticsearch.Models.Tasks;
using br.com.sharklab.elasticsearch.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace br.com.sharklab.elasticsearch
{
    internal sealed class ElasticSearchMiddleware : IMiddleware
    {
        private ConcurrentQueue<IndexingTask> _indexingTasks;

        public ElasticSearchMiddleware(ConcurrentQueue<IndexingTask> indexingTasks)
        {
            _indexingTasks = indexingTasks;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (ElasticUtils.ShouldExcludeController(context))
            {
                await next(context);
                return;
            }
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await GetRequestResponseBeforeIndex(context, stopwatch, next);
            }
            catch (Exception e)
            {
                await next(context);
            }
        }

        private async Task GetRequestResponseBeforeIndex(HttpContext context, Stopwatch stopwatch, RequestDelegate next)
        {
            string requestObj = await ReadRequestBody(context.Request);

            using (var buffer = new MemoryStream())
            {
                var originalBody = context.Response.Body;
                context.Response.Body = buffer;
                await next(context);
                buffer.Seek(0, SeekOrigin.Begin);
                string responseContent = await new StreamReader(buffer).ReadToEndAsync();
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(originalBody);

                context.Response.Body = originalBody;

                await CreateListIndexTasks(context, requestObj, responseContent, stopwatch);
            }
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            string requestObj;
            request.EnableBuffering();
            using (var memoryStream = new MemoryStream())
            {
                request.Body.Position = 0;
                await request.Body.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    requestObj = await reader.ReadToEndAsync();
                }
            }
            // Restauro o corpo do request original para que ele possa ser lido por outros middlewares
            request.Body.Position = 0;

            return requestObj;
        }

        private async Task CreateListIndexTasks(HttpContext context, string requestObj, string responseContent, Stopwatch stopwatch)
        {
            ControllerActionDescriptor? controllerActionDescriptor = context.Features?.Get<IEndpointFeature>()?
                .Endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor?>();

            if (controllerActionDescriptor == null) return;

            string nameOfIndex = ElasticUtils.CreateIndexNameApi(controllerActionDescriptor);

            var indexingTask = new IndexingTask
            {
                NameIndex = nameOfIndex,
                RequestObj = requestObj,
                ResponseContent = responseContent,
                StatusCode = context.Response.StatusCode,
                ElapsedTime = stopwatch.Elapsed.TotalMilliseconds,
                RequestHeader = JsonHelper.ConvertHeadersToDictionary(context.Request.Headers),
                ResponseHeader = JsonHelper.ConvertHeadersToDictionary(context.Response.Headers),
                RequestMethod = context.Request.Method,
                RequestQuery = context.Request.QueryString.Value,
                RequestPath = context.Request.Path,
            };

            _indexingTasks.Enqueue(indexingTask);
        }
    }
}