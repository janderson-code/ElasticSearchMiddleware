#nullable enable

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using elasticsearch.Extensions.Json;
using elasticsearch.Models.Tasks;
using elasticsearch.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;

namespace elasticsearch
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

            await GetRequestResponseBeforeIndex(context, next);
        }

        private async Task GetRequestResponseBeforeIndex(HttpContext context, RequestDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();

            string requestObj = await ReadRequestBody(context.Request);

            var controllerActionDescriptor = context.Features?.Get<IEndpointFeature>()?
                .Endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor?>();

            if (controllerActionDescriptor is not null)
            {
                var requestType = controllerActionDescriptor?.Parameters?.FirstOrDefault()?.ParameterType;

                if (requestType is not null)
                {
                    requestObj = SensitiveDataExtensions.SanitizeSensitiveData(requestObj, requestType);
                }
            }

            var originalBody = context.Response.Body;

            try
            {
                using (var buffer = new MemoryStream())
                {
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
            catch (Exception ex)
            {
                var responseContent = new
                {
                    ex.Message,
                    ex.StackTrace,
                    StatusCode = 500
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await CreateListIndexTasks(context, requestObj, JsonConvert.SerializeObject(responseContent),
                    stopwatch);

                throw;
            }
            finally
            {
                context.Response.Body = originalBody;
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

        private async Task CreateListIndexTasks(HttpContext context, string requestObj, string responseContent,
            Stopwatch stopwatch)
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