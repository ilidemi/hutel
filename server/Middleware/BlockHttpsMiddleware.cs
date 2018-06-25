using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace hutel.Middleware
{
    public class RedirectToHttpsMiddleware
    {
        private readonly RequestDelegate _next;
    
        public RedirectToHttpsMiddleware(RequestDelegate next)
        {
            _next = next;
        }
    
        public async Task Invoke(HttpContext httpContext)
        {
            var protoHeader = httpContext.Request.Headers["X-Forwarded-Proto"].ToString().ToLower(CultureInfo.InvariantCulture);
            httpContext.Items["protocol"] = httpContext.Request.IsHttps
                ? "https"
                : protoHeader == "https"
                    ? "https"
                    : "http";
            if (protoHeader == "http")
            {
                var request = httpContext.Request;
                httpContext.Response.Redirect($"https://{request.Host}{request.Path}{request.QueryStri‌​ng}");
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
    
    public static class RedirectToHttpsMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedirectToHttpsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RedirectToHttpsMiddleware>();
        }
    }
}