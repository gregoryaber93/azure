using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MyFunctionApp
{
    public class HelloFunction
    {
        private readonly ILogger<HelloFunction> _logger;

        public HelloFunction(ILogger<HelloFunction> logger)
        {
            _logger = logger;
        }

        [Function("SampleGet")]
        public HttpResponseData SampleGet(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "samples/get")] HttpRequestData req)
        {
            _logger.LogInformation("Sample GET called.");

            var query = ParseQuery(req.Url.Query);
            query.TryGetValue("id", out string? id);
            query.TryGetValue("name", out string? name);
            query.TryGetValue("verbose", out string? verbose);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString(JsonSerializer.Serialize(new OperationResponse
            {
                method = "GET",
                routeId = id,
                queryName = name,
                queryVerbose = verbose,
                message = "Wywolano metode GET z przekazanymi parametrami.",
                utc = DateTime.UtcNow
            }));

            return response;
        }

        [Function("SamplePost")]
        public async Task<HttpResponseData> SamplePost(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "samples/post")] HttpRequestData req)
        {
            _logger.LogInformation("Sample POST called.");

            var body = await ReadBodyAsJson(req);
            var query = ParseQuery(req.Url.Query);
            query.TryGetValue("source", out string? source);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonSerializer.Serialize(new OperationResponse
            {
                method = "POST",
                querySource = source,
                body = body,
                message = "Wywolano metode POST.",
                utc = DateTime.UtcNow
            }));

            return response;
        }

        [Function("SamplePut")]
        public async Task<HttpResponseData> SamplePut(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "samples/put/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("Sample PUT called.");

            var body = await ReadBodyAsJson(req);
            var query = ParseQuery(req.Url.Query);
            query.TryGetValue("mode", out string? mode);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonSerializer.Serialize(new OperationResponse
            {
                method = "PUT",
                routeId = id,
                queryMode = mode,
                body = body,
                message = $"Wywolano metode PUT dla id={id}.",
                utc = DateTime.UtcNow
            }));

            return response;
        }

        [Function("SampleDelete")]
        public HttpResponseData SampleDelete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "samples/delete/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("Sample DELETE called.");

            var query = ParseQuery(req.Url.Query);
            query.TryGetValue("reason", out string? reason);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString(JsonSerializer.Serialize(new OperationResponse
            {
                method = "DELETE",
                routeId = id,
                queryReason = reason,
                message = $"Wywolano metode DELETE dla id={id}.",
                utc = DateTime.UtcNow
            }));

            return response;
        }

        [Function("SamplePatch")]
        public async Task<HttpResponseData> SamplePatch(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "samples/patch/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("Sample PATCH called.");

            var body = await ReadBodyAsJson(req);
            var query = ParseQuery(req.Url.Query);
            query.TryGetValue("field", out string? field);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonSerializer.Serialize(new OperationResponse
            {
                method = "PATCH",
                routeId = id,
                queryField = field,
                body = body,
                message = $"Wywolano metode PATCH dla id={id}.",
                utc = DateTime.UtcNow
            }));

            return response;
        }

        private static async Task<object> ReadBodyAsJson(HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new { };
            }

            try
            {
                return JsonSerializer.Deserialize<object>(requestBody) ?? new { };
            }
            catch
            {
                return new { raw = requestBody };
            }
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            string normalized = query.StartsWith("?") ? query.Substring(1) : query;
            string[] parts = normalized.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string[] kv = part.Split('=', 2);
                string key = Uri.UnescapeDataString(kv[0]);
                string value = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;
                result[key] = value;
            }

            return result;
        }
    }

    public class OperationResponse
    {
        public string? method { get; set; }
        public string? routeId { get; set; }
        public string? queryName { get; set; }
        public string? queryVerbose { get; set; }
        public string? querySource { get; set; }
        public string? queryMode { get; set; }
        public string? queryReason { get; set; }
        public string? queryField { get; set; }
        public object? body { get; set; }
        public string? message { get; set; }
        public DateTime utc { get; set; }
    }
}
