using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MealPlannerApp
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var port = context.Connection.LocalPort;
            var method = context.Request.Method;
            var path = context.Request.Path;

            await _next(context);

            sw.Stop();
            var statusCode = context.Response.StatusCode;
            if (statusCode >= 400 || method == "POST")
            {
                _logger.LogInformation("HTTP {method} {path} responded {statusCode} on port {port} in {elapsed}ms",
                    method, path, statusCode, port, sw.ElapsedMilliseconds);
            }
        }
    }
}