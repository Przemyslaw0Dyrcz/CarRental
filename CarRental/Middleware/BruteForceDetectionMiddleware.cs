using CarRental.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Account/Login") &&
                context.Request.Method == "POST")
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var settings = await db.SecuritySettings.FirstOrDefaultAsync();

                if (settings == null || !settings.EnableBruteForceProtection)
                {
                    await _next(context);
                    return;
                }

                var entry = attempts.GetOrAdd(ip, (0, DateTime.UtcNow));

                if (DateTime.UtcNow - entry.Time > TimeSpan.FromMinutes(settings.IpWindowMinutes))
                {
                    entry = (0, DateTime.UtcNow);
                }

                attempts[ip] = entry;

                if (entry.Count >= settings.MaxLoginAttempts)
                {
                    try
                    {
                     await   CarRental.Services.ActivityLogger.LogAsync(
                            db,
                            context,
                            null,
                            "BRUTE_FORCE_DETECTED",
                            $"Too many failed login attempts from {ip}"
                        );
                    }
                    catch { }

                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too many login attempts. Possible brute force.");
                    return;
                }
            }

            await _next(context);
        }

        public static void RegisterFailedAttempt(string ip)
        {
            var entry = attempts.GetOrAdd(ip, (0, DateTime.UtcNow));

            entry.Count++;

            attempts[ip] = entry;
        }

        public static void ResetAttempts(string ip)
        {
            attempts.TryRemove(ip, out _);
        }
    }
}