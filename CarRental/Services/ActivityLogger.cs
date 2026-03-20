using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace CarRental.Services
{
    public static class ActivityLogger
    {
        public static async Task LogAsync(
            ApplicationDbContext db,
            HttpContext ctx,
            string user,
            string action,
            string description)
        {
            try
            {
                if (db == null) return;

                var log = new ActivityLog
                {
                    UserName = user ?? ctx?.User?.Identity?.Name ?? "anonymous",
                    Action = action,
                    Description = description,
                    IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString() ?? "",
                    UserAgent = ctx?.Request?.Headers["User-Agent"].ToString() ?? "",
                    Timestamp = DateTime.UtcNow
                };

                db.ActivityLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch
            {
                //logi nie moga blokowac apk
            }
        }
    }
}