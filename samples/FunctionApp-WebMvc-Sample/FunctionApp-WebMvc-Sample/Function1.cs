using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RazorLight;

namespace FunctionApp_WebMvc_Sample
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, ExecutionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            
            var engine = new RazorLightEngineBuilder().SetOperatingAssembly(Assembly.GetExecutingAssembly())
                .UseEmbeddedResourcesProject(typeof(Function1)).UseMemoryCachingProvider().Build();

            var model = new { Name = "John Doe", Time = DateTime.UtcNow };
            var path = Path.Combine(Environment.GetEnvironmentVariable("HOME") ??"" , @"wwwroot" , "Index.cshtml");
            string template = await File.ReadAllTextAsync(path);

            var response = req.CreateResponse(HttpStatusCode.OK);
            string result = await engine.CompileRenderStringAsync("templateKey", template, model);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await response.WriteStringAsync(result);
            return response;
        }
    }
}
