using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace CarRental.Services
{
    public class RoleExpiryService : BackgroundService
    {
        private readonly IServiceProvider _svc;
        public RoleExpiryService(IServiceProvider svc) { _svc = svc; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _svc.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var now = DateTime.UtcNow;
                    var expired = await db.AccessRequests
                                          .Where(r => r.Status == "Approved" && r.ExpiresAt <= now)
                                          .ToListAsync(stoppingToken);

                    foreach (var r in expired)
                    {
                        var user = await userManager.FindByIdAsync(r.RequesterId);
                        if (user != null)
                        {
                            await userManager.RemoveFromRoleAsync(user, r.RequestedRole);
                            r.Status = "Expired";
                            db.ActivityLogs.Add(new ActivityLog
                            {
                                Timestamp = now,
                                UserName = "system",
                                Action = "RoleExpired",
                                Description = $"Expired role {r.RequestedRole} for {user.UserName} (request {r.Id})"
                            });
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch { /* swallow - don't kill host */ }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
