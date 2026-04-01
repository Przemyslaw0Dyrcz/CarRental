using CarRental.Data;
using CarRental.Models;
using CarRental.Services.Interface;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services.Implementation
{
    public class ActivityLoggerService : IActivityLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;

        public ActivityLoggerService(
            ApplicationDbContext context,
            IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task LogAsync(string? user, string action, string description)
        {
            try
            {
                var ctx = _http.HttpContext;

                var log = new ActivityLog
                {
                    UserName = user ?? ctx?.User?.Identity?.Name ?? "anonymous",
                    Action = action,
                    Description = description,
                    IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString() ?? "",
                    UserAgent = ctx?.Request?.Headers["User-Agent"].ToString() ?? "",
                    Timestamp = DateTime.UtcNow
                };

                await _context.ActivityLogs.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // NIE wywalamy aplikacji
            }
        }
    }
}