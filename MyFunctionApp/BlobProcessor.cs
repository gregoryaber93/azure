using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MyFunctionApp
{
    public class BlobProcessor
    {
        private readonly ILogger<BlobProcessor> _logger;

        public BlobProcessor(ILogger<BlobProcessor> logger)
        {
            _logger = logger;
        }

        // [Function("BlobProcessor")]
        // public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name, ILogger log)
        // {
        //     log.LogInformation($"Processing blob: {name}");

        //     using var reader = new StreamReader(myBlob);
        //     var content = await reader.ReadToEndAsync();

        //     log.LogInformation($"Raw content: {content}");

        //     try
        //     {
        //         var data = JsonSerializer.Deserialize<MyData>(content);

        //         log.LogInformation($"Parsed name: {data?.Name}");
        //         log.LogInformation($"Parsed age: {data?.Age}");
        //     }
        //     catch
        //     {
        //         log.LogError("Invalid JSON!");
        //     }
        // }

    [Function("BlobProcessor")]
    [BlobOutput("samples-output/processedFile_{name}.json", Connection = "AzureWebJobsStorage")]
    public async Task<string> Run(
        [BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream inputBlob,
        string name)
        {
        _logger.LogInformation($"Processing blob: {name}");
            using var reader = new StreamReader(inputBlob);
            var content = await reader.ReadToEndAsync();

            var data = JsonSerializer.Deserialize<MyData>(content);

            var result = new
            {
                originalName = data?.Name,
                age = data?.Age,
                processedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(result);
            _logger.LogInformation($"Processed content: {json}");

            return json;
        }
    }
}
