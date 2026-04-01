using CarRental.Data;
using CarRental.Models;
using CarRental.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CarRental.Middleware
{
    public class SessionTimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IMemoryCache cache,
            IActivityLogger logger)
        {
            if (context.User?.Identity?.IsAuthenticated ?? false)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user != null)
                {
                    var settings = await db.UserSecuritySettings
                        .FirstOrDefaultAsync(s => s.UserId == user.Id);

                    var globalSettings = await cache.GetOrCreateAsync("security_settings", async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                        return await db.SecuritySettings.FirstOrDefaultAsync();
                    });

                    if (globalSettings == null)
                    {
                        await _next(context);
                        return;
                    }

                    int timeout = settings?.SessionTimeoutMinutes
                                  ?? globalSettings.SessionTimeoutMinutes;

                    var lastActivity = context.Session.GetString("LastActivityTime");

                    if (!string.IsNullOrEmpty(lastActivity))
                    {
                        if (DateTime.TryParse(lastActivity, out var lastTime))
                        {
                            if (DateTime.UtcNow - lastTime > TimeSpan.FromMinutes(timeout))
                            {
                                context.Session.Clear();
                                await userManager.UpdateSecurityStampAsync(user);

                                await logger.LogAsync(
                                    user.Id,
                                    "SESSION_TIMEOUT",
                                    "User session expired"
                                );

                                context.Response.Redirect("/Account/Login?expired=true");
                                return;
                            }
                        }
                    }

                    context.Session.SetString("LastActivityTime", DateTime.UtcNow.ToString("o"));
                }
            }

            await _next(context);
        }
    }
}