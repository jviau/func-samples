using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Samples.Startup;

namespace Startup
{
    public class Hello(ILogger<Hello> logger, StartupEager eager, StartupLazy lazy)
    {
        private readonly ILogger<Hello> _logger = logger;

        [Function("Hello")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            object something = eager.Value; // this will always be ready
            object something2 = await lazy.GetAsync(); // this may or may not be ready yet
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
