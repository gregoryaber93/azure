using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using MyApi.DTOs;
using MyApi.Entities;
using MyApi.Services;
using System.Diagnostics;
using System.Text.Json;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly QueueClient _queueClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrdersController> _logger;
        private readonly CosmosClient _cosmosClient;
        public OrdersController(IOrderService orderService, IConfiguration configuration, ILogger<OrdersController> logger, CosmosClient cosmosClient)
        {
            _orderService = orderService;
            _configuration = configuration;
            _logger = logger;
            _cosmosClient = cosmosClient;
            var connectionString = _configuration["Storage:ConnectionString"];
            _logger.LogError("LOG STREAM TEST 🔥🔥🔥");

            _queueClient = new QueueClient(
                    connectionString,
                    "orders",
                    new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? productName)
        {
            _logger.LogInformation("CI/CD TEST API");

            _logger.LogError("LOG STREAM TEST 🔥🔥🔥ADSASASDASDASDASDASDASDS");

            var orders = await _orderService.GetAll();

            if (!string.IsNullOrEmpty(productName))
            {
                orders = orders.Where(o => o.ProductName.Contains(productName));
            }

            orders.ToList()[0].ProductName = "CHANGED NAME";

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderService.GetByIdAsync(id);

            if (order == null)
                return NotFound();

            var dto = new OrderDto
            {
                Id = order.Id,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                Status = order.Status
            };

            return Ok(dto);
        }

        [HttpGet("events/{orderId}")]
        public async Task<IActionResult> Get(string orderId)
        {
            var container = _cosmosClient
                .GetContainer("orders-db", "orders-events");

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.orderId = @orderId")
                .WithParameter("@orderId", orderId);

            var iterator = container.GetItemQueryIterator<OrderEvent>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(orderId)
                });

            var results = new List<OrderEvent>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var id = await _orderService.CreateOrderAsync(dto.ProductName, dto.Quantity);

            var operationId = Activity.Current?.Id;

            var message = new
            {
                OrderId = id,
                OperationId = operationId
            };

            var json = JsonSerializer.Serialize(message);

            var response = await _queueClient.SendMessageAsync(json);

            _logger.LogInformation(response.ToString());

            return Ok(id);
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test()
        {
            var operationId = Activity.Current?.Id;

            _logger.LogInformation("STEP 1 - start");

            _logger.LogInformation("STEP 2 - doing work");

            var response = await _queueClient.SendMessageAsync("Test Test");

            //throw new Exception("FLOW TEST EXCEPTION");
            return Ok(operationId);
        }

        [HttpPost("bus")]
        public async Task<IActionResult> TestBus()
        {
            var connectionString = _configuration["ServiceBus:ConnectionString"];
            var topicName = _configuration["ServiceBus:TopicName"] ?? "orders-topic";
            var namespaceName = _configuration["ServiceBus:Namespace"] ?? "sb-learning-standard.servicebus.windows.net";

            var operationId = Activity.Current?.Id;

            var firstOrder = (await _orderService.GetAll()).FirstOrDefault();

            if (firstOrder == null)
            {
                return NotFound("No orders found to publish.");
            }

            var message = new
            {
                OrderId = firstOrder.Id,
                OperationId = operationId
            };

            var json = JsonSerializer.Serialize(message);

            try
            {
                await using var client = !string.IsNullOrWhiteSpace(connectionString)
                    ? new ServiceBusClient(connectionString)
                    : new ServiceBusClient(namespaceName, new DefaultAzureCredential());

                var sender = client.CreateSender(topicName);
                var messageUpgrated = new ServiceBusMessage(json);

                messageUpgrated.ApplicationProperties["type"] = "order-created";

                await sender.SendMessageAsync(messageUpgrated);
                _logger.LogInformation("Message sent to Service Bus topic {TopicName}", topicName);
                return Ok(operationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to Service Bus topic {TopicName}", topicName);
                return StatusCode(500, "Failed to publish message to Service Bus. Check ServiceBus configuration and identity permissions.");
            }
        }
    }
}