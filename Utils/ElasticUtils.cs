using System;
using System.Linq;
using elasticsearch.Attributes;
using elasticsearch.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;

namespace elasticsearch.Utils
{
    internal static class ElasticUtils
    {
        internal static bool ShouldExcludeController(HttpContext context)
        {
            try
            {
                if (context.Features.Get<IEndpointFeature>() == null) return true;

                var controllerActionDescriptor = context.Features?.Get<IEndpointFeature>().Endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor?>();

                if (controllerActionDescriptor is null) return true;

                var attributes = controllerActionDescriptor.ControllerTypeInfo.CustomAttributes;

                bool containsElastic = attributes.Any(attribute => attribute.AttributeType == typeof(ExcludeElasticsearchAttribute));

                return containsElastic;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        internal static string CreateIndexNameApi(ControllerActionDescriptor actionDescriptor)
        {
            var nameOfIndex = $"{GenerateIndexNameGeneralApplications()}-{actionDescriptor.ControllerName}";

            return nameOfIndex.ToLower();
        }

        internal static string AddEnvironmentToIndexNameApi(string indexName, IOptions<ElasticConfiguration> elasticOptions)
        {
            indexName = !string.IsNullOrEmpty(elasticOptions.Value.Environment)
                ? $"{indexName}-{elasticOptions.Value.Environment}"
                : indexName;

            return indexName.ToLower();
        }

        internal static string GenerateIndexNameGeneralApplications(IOptions<ElasticConfiguration> elasticOptions = null)
        {
            string indexName = !string.IsNullOrEmpty(elasticOptions?.Value.Environment)
                ? $"{AppDomain.CurrentDomain.FriendlyName}-{elasticOptions.Value.Environment}"
                : AppDomain.CurrentDomain.FriendlyName;

            return indexName.ToLower();
        }
    }
}