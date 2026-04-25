using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/queue")]
    public class QueueController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly MyApi.Config.AzureFunctionsOptions _options;
        private readonly IConfiguration _configuration;

        public QueueController(IHttpClientFactory factory, IOptions<MyApi.Config.AzureFunctionsOptions> options, IConfiguration configuration)
        {
            _httpClient = factory.CreateClient();
            _options = options.Value;
            _configuration = configuration;
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendToQueue([FromQuery] string name)
        {
            var connectionString = _configuration["Storage:ConnectionString"];

            var queueClient = new QueueClient(
                    connectionString,
                    "orders",
                    new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

            await queueClient.CreateIfNotExistsAsync();

            var message = JsonSerializer.Serialize(new
            {
                Name = name,
                Age = 30
            });

            await queueClient.SendMessageAsync(message);

            return Ok("Message sent to queue");
        }

        [HttpPost("queueTest")]
        public async Task<IActionResult> SendToQueueTest([FromQuery] string name)
        {
            var connectionString = _configuration["Storage:ConnectionString"];

            var queueClient = new QueueClient(
                    connectionString,
                    "orders",
                    new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

            await queueClient.CreateIfNotExistsAsync();


            await queueClient.SendMessageAsync("Test message");

            return Ok("Message sent to queue");
        }
    }
}