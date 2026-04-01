using CarRental.Data;
using CarRental.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace CarRental.Middleware
{
    public class BruteForceDetectionMiddleware
    {
        private readonly RequestDelegate _next;

        private static ConcurrentDictionary<string, (int Count, DateTime Time)> attempts
            = new();

        public BruteForceDetectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            ApplicationDbContext db,
            IMemoryCache cache,
            IActivityLogger logger)
        {
            if (context.Request.Path.StartsWithSegments("/Account/Login") &&
                context.Request.Method == "POST")
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var settings = await cache.GetOrCreateAsync("security_settings", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await db.SecuritySettings.FirstOrDefaultAsync();
                });

                if (settings == null || !settings.EnableBruteForceProtection)
                {
                    await _next(context);
                    return;
                }

                var entry = attempts.GetOrAdd(ip, (0, DateTime.UtcNow));

                if (DateTime.UtcNow - entry.Time > TimeSpan.FromMinutes(settings.IpWindowMinutes))
                {
                    entry = (0, DateTime.UtcNow);
                    attempts[ip] = entry;
                }

                if (entry.Count >= settings.MaxLoginAttempts)
                {
                    await logger.LogAsync(
                        null,
                        "BRUTE_FORCE_DETECTED",
                        $"Too many failed login attempts from {ip}"
                    );

                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too many login attempts.");
                    return;
                }

                await _next(context);

                if (context.Response.StatusCode == 302)
                {
                    attempts.TryRemove(ip, out _);
                }
                else
                {
                    entry.Count++;
                    attempts[ip] = (entry.Count, DateTime.UtcNow);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}