using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyFunctionApp.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace MyFunctionApp
{
    public class OrderQueueFunction
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        public OrderQueueFunction(ILoggerFactory loggerFactory, AppDbContext context)
        {
            _logger = loggerFactory.CreateLogger<OrderQueueFunction>();
            _context = context;
        }

        // Storage:ConnectionString

        //[Function("ProcessOrderQueue")]
        //public void Run(
        //    [QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message)
        //{
        //    var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        //    _logger.LogInformation($"CONN: {(string.IsNullOrEmpty(conn) ? "NULL ❌" : "OK ✅")}");

        //    _logger.LogInformation(message);
        //    var order = TryDeserializeOrder(message);

        //    if (order == null)
        //    {
        //        _logger.LogError("Queue message could not be deserialized to OrderMessage.");
        //        return;
        //    }

        //    _logger.LogInformation($"🔥 Processing order: {order.Id} - {order.ProductName}");

        //    _logger.LogInformation($"✅ Order processed: {order.Id}");

        //    var orderDb = _context.Orders.Find(order.Id);
        //    if (orderDb == null)
        //    {
        //        _logger.LogError($"Order with ID {order.Id} not found in database.");
        //        return;
        //    }

        //    _logger.LogWarning("⚠️ Found existing order in database:");
        //    _logger.LogInformation(JsonSerializer.Serialize(orderDb));
        //    _logger.LogError("asdasd");

        //    orderDb.Status = "Processed";
        //    _context.SaveChanges();

        //    _logger.LogWarning("!!!!!!!!!!!!Everything is processed.!!!!!!!!!!!!!!!");
        //}

        //[Function("Test")]
        //public void Run(
        //    [QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message,
        //    FunctionContext context)
        //{
        //    var bindingData = context.BindingContext.BindingData;
        //    bindingData.TryGetValue("Id", out var id);
        //    bindingData.TryGetValue("DequeueCount", out var dequeueCount);

        //    var operationId = TryExtractOperationId(message);
        //    using var _ = _logger.BeginScope(new Dictionary<string, object?>
        //    {
        //        ["QueueMessageId"] = id?.ToString(),
        //        ["DequeueCount"] = dequeueCount?.ToString(),
        //        ["OperationId"] = operationId
        //    });

        //    _logger.LogInformation("Queue trigger invoked.");
        //    _logger.LogInformation("Payload: {Payload}", message);
        //}


        [Function("ProcessOrderQueueV2")]
        public void Run(
            [QueueTrigger("orders", Connection = "AzureWebJobsStorage")] string message)
        {
            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            _logger.LogInformation($"CONN: {(string.IsNullOrEmpty(conn) ? "NULL ❌" : "OK ✅")}");
            _logger.LogInformation("Queue function invoked.");

            QueueMessage? messageObject;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                messageObject = JsonSerializer.Deserialize<QueueMessage>(message, options);

                //if (messageObject.OrderId == 26)
                //{
                //    throw new Exception("TEST ERROR");
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed");
                _logger.LogError(ex, "Deserialization failed");
                throw;
            }

            if (messageObject is null)
            {
                _logger.LogError("Queue payload is empty or invalid.");
                return;
            }

            if (messageObject.OrderId is null)
            {
                _logger.LogError("Queue payload does not contain a valid OrderId.");
                return;
            }

            using var activity = new Activity("ProcessOrder");

            if (!string.IsNullOrEmpty(messageObject.OperationId))
            {
                activity.SetParentId(messageObject.OperationId);
            }

            activity.Start();

            var orderDb = _context.Orders.Find(messageObject.OrderId.Value);

            if (orderDb == null)
            {
                _logger.LogError($"Order with ID {messageObject.OrderId} not found in database.");
                return;
            }

            _logger.LogInformation(JsonSerializer.Serialize(orderDb));
            orderDb.Status = "Processed";
            _context.SaveChanges();

            _logger.LogInformation("Order {OrderId} processed.", messageObject.OrderId.Value);
        }

        //[Function("ProcessOrderSB")]
        //public void RunSb(
        //    [ServiceBusTrigger("orders-sb", Connection = "ServiceBus:ConnectionString")] string message)
        //{
        //    _logger.LogWarning($"SB MESSAGE: {message}");
        //    //throw new Exception("BOOM 💣");
        //}

        [Function("ProcessOrderTopic")]
        public async Task RunProcessOrderTopic([ServiceBusTrigger(
        "orders-topic",
        "order-processing",
        Connection = "ServiceBus:ConnectionString")] string message)
        {
            var orderDb = _context.Orders.FirstOrDefault(x => x.Status == "Pending");
            if (orderDb == null)
            {
                _logger.LogError($"No pending orders found in database.");
                return;
            }

            orderDb.Status = "Processed1";
            _context.SaveChanges();
            _logger.LogInformation($"Processing order: {message}");
        }

        [Function("AnalyticsOrderTopic")]
        public async Task RunAnalyticsOrderTopic([ServiceBusTrigger(
        "orders-topic",
        "order-analytics",
        Connection = "ServiceBus:ConnectionString")] string message)
        {
            var orderDb = _context.Orders.LastOrDefault(x => x.Status == "Pending");
            if (orderDb == null)
            {
                _logger.LogError($"No pending orders found in database.");
                return;
            }

            orderDb.Status = "Processed2";
            _context.SaveChanges();
            _logger.LogInformation($"Analytics received: {message}");
        }

        private static OrderMessage? TryDeserializeOrder(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<OrderMessage>(message);
            }
            catch
            {
                // Some senders encode queue payload as Base64 instead of plain JSON.
                try
                {
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(message));
                    return JsonSerializer.Deserialize<OrderMessage>(decoded);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static string? TryExtractOperationId(string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(message);
                if (doc.RootElement.TryGetProperty("OperationId", out var operationIdElement) &&
                    operationIdElement.ValueKind == JsonValueKind.String)
                {
                    return operationIdElement.GetString();
                }
            }
            catch
            {
                // Ignore invalid JSON payloads in diagnostic path.
            }

            return null;
        }
    }

    public class OrderMessage
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class QueueMessage
    {
        public int? OrderId { get; set; }
        public string? OperationId { get; set; }
    }
}
