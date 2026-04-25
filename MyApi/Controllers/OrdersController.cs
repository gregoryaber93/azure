using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;
using MyApi.Entities;
using MyApi.Services;
using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

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
        public OrdersController(IOrderService orderService, IConfiguration configuration, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _configuration = configuration;
            _logger = logger;
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

            await using var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender("orders-sb");

            var operationId = Activity.Current?.Id;

            var firstOrder = (await _orderService.GetAll()).FirstOrDefault();

            var message = new
            {
                OrderId = firstOrder?.Id,
                OperationId = operationId
            };

            var json = JsonSerializer.Serialize(message);

            await sender.SendMessageAsync(new ServiceBusMessage(json));

            _logger.LogInformation("Message sent to Service Bus");
            return Ok(operationId);
        }
    }
}