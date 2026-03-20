
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CarRental.Middleware
{
    public class LoginRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private static ConcurrentDictionary<string, int> attempts = new();

        public LoginRateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Account/Login"))
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                attempts.AddOrUpdate(ip, 1, (k, v) => v + 1);

                if (attempts[ip] > 10)
                {
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too many login attempts");
                    return;
                }
            }

            await _next(context);
        }
    }
}
