using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using CarRental.Data;
using CarRental.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            if (context.User?.Identity?.IsAuthenticated ?? false)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var settings = await db.UserSecuritySettings.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();

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
                                context.Response.Redirect("/Account/Login?expired=true");
                                await CarRental.Services.ActivityLogger.LogAsync(
                                    db,
                                    context,
                                    user.Id,
                                    "SESSION_TIMEOUT",
                                    "User session expired"
                                    );
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
    public static class SessionTimeoutMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionTimeout(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionTimeoutMiddleware>();
        }
    }
}
