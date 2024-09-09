using System;
using elasticsearch.Utils;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using ILogger = Serilog.ILogger;

namespace elasticsearch.Models.LogTemplates
{
    public class CustomLogger
    {
        private static ILogger _logger;

        public static void ConfigureLogger(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Async(writeTo => writeTo.Elasticsearch(ConfigureElasticSink(configuration)))
                .CreateLogger();
        }

        public static void LogInformation(string message, Guid? guid)
        {
            _logger.Information($"{guid} - {message}");
        }
        public static void LogInformation(string message)
        {
            _logger.Information($"{message}");
        }

        public static void LogError(string message, Exception ex, Guid? guid)
        {
            _logger.Error(ex, $"{guid} - {message}");
        }
        public static void LogError(string message, Exception ex)
        {
            _logger.Error(ex, $"{message}");
        }
        public static void LogWarning(string message, Guid? guid)
        {
            _logger.Warning($"{guid} - {message}");
        }

        public static void LogWarning(string message)
        {
            _logger.Warning($"{message}");
        }

        public static void LogCritical(string message, object property, Guid guid)
        {
            _logger.Fatal<object>($"{guid} - {message}", property);
        }
        public static void LogCritical(string message, object property)
        {
            _logger.Fatal<object>($"{message}", property);
        }
        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration)
        {
            var jsonFormatter = new CompactJsonFormatter();
            var elastic = new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                ModifyConnectionSettings = x => x.BasicAuthentication(configuration["ElasticConfiguration:Username"], configuration["ElasticConfiguration:Password"]),
                AutoRegisterTemplate = true,
                TypeName = "_doc",
                IndexFormat = $"{ElasticUtils.GenerateIndexNameGeneralApplications()}-customlogger".ToLower(),
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                BatchAction = ElasticOpType.Create,
                CustomFormatter = jsonFormatter,
            };

            return elastic;
        }
    }
}