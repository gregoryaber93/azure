namespace MyFunctionApp
{
    public class ProcessOrder
    {
        // [FunctionName("ProcessOrder")]
        // public void Run([QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        // {
        //     log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        // }

        // [FunctionName("ProcessOrder")]
        // public void Run([QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message,ILogger log)
        // {
        //     log.LogInformation($"Processing order: {message}");

        //     throw new Exception("Test error!");
        // }


        // [FunctionName("TestHttp")]
        // public IActionResult RunTest(
        //     [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        //     ILogger log)
        // {
        //     log.LogInformation("HTTP works");
        //     return new OkObjectResult("OK");
        // }

        // [FunctionName("ProcessOrderV2")]
        // public void Run(
        //             [QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message, ILogger log)
        // {
        //     log.LogInformation("🔥 FUNCTION STARTED 🔥");
        //     log.LogInformation($"Message: {message}");
        // }


        // [FunctionName("ProcessOrderV5")]
        // public async Task Run(
        //     [QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message,
        //     ILogger log)
        // {
        //     log.LogInformation("V5 START");

        //     var data = JsonSerializer.Deserialize<MyData>(message, new JsonSerializerOptions
        //     {
        //         PropertyNameCaseInsensitive = true
        //     });

        //     log.LogInformation($"Name: {data?.Name}");
        // }


        // [FunctionName("ProcessOrderV3")]
        // public async Task Run([QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message,
        //                     [Blob("samples-output/from-queue-{rand-guid}.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outputBlob, ILogger log)
        // {
        //     log.LogInformation("asdasdasdasdasdasdasdasd");

        //     log.LogInformation($"Processing queue message: {message}");

        //     var data = JsonSerializer.Deserialize<MyData>(message, new JsonSerializerOptions
        //     {
        //         PropertyNameCaseInsensitive = true
        //     });

        //     if (data == null)
        //     {
        //         log.LogError("Deserialization failed!");
        //         return;
        //     }

        //     var result = new
        //     {
        //         data?.Name,
        //         data?.Age,
        //         processedAt = DateTime.UtcNow
        //     };

        //     var json = JsonSerializer.Serialize(result);
        //     var bytes = Encoding.UTF8.GetBytes(json);

        //     await outputBlob.WriteAsync(bytes, 0, bytes.Length);
        // }
    }
}
