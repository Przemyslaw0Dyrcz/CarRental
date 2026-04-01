using CarRental.Services.Interface;
using CarRental.Services.Implementation;
using System;
using System.Threading.Tasks;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services.Implementation
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task LogAsync(string user, string action, string description)
        {
            var ctx = _http.HttpContext;

            var log = new ActivityLog
            {
                UserName = user ?? "Anonymous",
                Action = action,
                Description = description,
                Timestamp = DateTime.UtcNow,
                IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString() ?? "",
                UserAgent = ctx?.Request?.Headers["User-Agent"].ToString() ?? ""
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
