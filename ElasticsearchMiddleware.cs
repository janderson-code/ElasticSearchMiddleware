#nullable enable

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using elasticsearch.Extensions;
using elasticsearch.Extensions.Json;
using elasticsearch.Models.Tasks;
using elasticsearch.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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

            var originalBodyStream = context.Response.Body;
            
            try
            {
                await ReadResponseBody(context, next, requestObj, stopwatch, originalBodyStream);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex, requestObj, stopwatch);
                throw;
            }
            finally
            {
                // Restauro o corpo do response original para que ele possa ser lido por outros middlewares
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task ReadResponseBody(HttpContext context, RequestDelegate next, string requestObj, Stopwatch stopwatch,
            Stream originalBodyStream)
        {
            using (var buffer = new MemoryStream())
            {
                // Substitui o corpo da resposta pelo buffer temporário
                context.Response.Body = buffer;

                // Processa a próxima etapa no pipeline
                await next(context);

                // Obtém o conteúdo da resposta
                string responseContent = await StreamExtensions.ReadResponseContent(buffer);

                // Registra as informações no índice
                await CreateListIndexTasks(context, requestObj, responseContent, stopwatch);

                // Copia o conteúdo processado de volta para o fluxo original
                await StreamExtensions.CopyBufferToOriginalStream(buffer, originalBodyStream);
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

        private Task CreateListIndexTasks(HttpContext context, string requestObj, string responseContent,
            Stopwatch stopwatch)
        {
            ControllerActionDescriptor? controllerActionDescriptor = context.Features?.Get<IEndpointFeature>()?
                .Endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor?>();

            if (controllerActionDescriptor is null) return Task.CompletedTask;

            var requestType = controllerActionDescriptor?.Parameters?.FirstOrDefault()?.ParameterType;

            if (requestType is not null)
                requestObj = SensitiveDataExtensions.SanitizeSensitiveData(requestObj, requestType);

            var responseType = controllerActionDescriptor!.EndpointMetadata
                .OfType<ProducesResponseTypeAttribute>()
                .FirstOrDefault(c => c.StatusCode.Equals(200))!.Type;

            if (responseType is not null)
                responseContent = SensitiveDataExtensions.SanitizeSensitiveData(responseContent, responseType);

            string nameOfIndex = ElasticUtils.CreateIndexNameApi(controllerActionDescriptor!);

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

            return Task.CompletedTask;
        }

        private async Task HandleException(HttpContext context, Exception ex, string requestObj, Stopwatch stopwatch)
        {
            var errorResponse = JsonConvert.SerializeObject(new
            {
                ex.Message,
                ex.StackTrace,
                StatusCode = StatusCodes.Status500InternalServerError
            });

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await CreateListIndexTasks(context, requestObj, errorResponse, stopwatch);
        }
    }
}