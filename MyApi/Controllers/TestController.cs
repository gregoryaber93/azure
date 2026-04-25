using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly MyApi.Config.AzureFunctionsOptions _options;


        public TestController(IHttpClientFactory factory, IOptions<MyApi.Config.AzureFunctionsOptions> options)
        {
            _httpClient = factory.CreateClient();
            _options = options.Value;
        }

        [HttpGet("get")]
        public async Task<IActionResult> CallFunction()
        {
            var url = $"{_options.BaseUrl}/api/samples/get?id=1&name=api&verbose=true&code={_options.FunctionKey}";

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            return Content(content, "application/json");
        }

        [HttpPost("post")]
        public async Task<IActionResult> CallFunctionPost()
        {
            var url = $"{_options.BaseUrl}/api/samples/post?source=api&code={_options.FunctionKey}";
            
            var data = new
            {
                Name = "Grzegorz",
                Age = 30
            };

            var jsonData = JsonSerializer.Serialize(data);

            var jsonBody = new
            {
                data = jsonData,
                timestamp = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync(url, jsonBody);
            var content = await response.Content.ReadAsStringAsync();

            return Content(content, "application/json");
        }
    }
}