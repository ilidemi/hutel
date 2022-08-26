using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace hutel.Middleware
{
    public class SingleUserAuthMiddleware
    {
        private readonly string _secret;
        private readonly string _userId;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public SingleUserAuthMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _secret = Environment.GetEnvironmentVariable("HUTEL_SINGLE_USER_AUTH_SECRET");
            _userId = Environment.GetEnvironmentVariable("HUTEL_SINGLE_USER_ID");
            _next = next;
            _logger = loggerFactory.CreateLogger<SingleUserAuthMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var headerSecretValues = httpContext.Request.Headers["x-hutel-single-user-auth-secret"];
            if (headerSecretValues.Count == 0)
            {
                await _next.Invoke(httpContext);
                return;
            }

            if (headerSecretValues[0] != _secret)
            {
                _logger.LogInformation("Secret doesn't match: ${headerSecretValues[0]}");
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            httpContext.Items["UserId"] = _userId;
            await _next.Invoke(httpContext);
        }
    }

    public static class SingleUserAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseSingleUserAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SingleUserAuthMiddleware>();
        }
    }
}