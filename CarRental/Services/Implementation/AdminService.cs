using CarRental.Data;
using CarRental.Models;
using CarRental.Services.Interface;
using CarRental.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityLog>> GetLogsAsync()
        {
            return await _context.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(200)
                .ToListAsync();
        }

        public async Task<SecurityDashboardViewModel> GetSecurityDashboardAsync()
        {
            var failedLogins = await _context.ActivityLogs
                .Where(x => x.Action == "LOGIN_FAILED")
                .ToListAsync();

            var alerts = await _context.ActivityLogs
                .Where(x =>
                    x.Action.Contains("BRUTE_FORCE") ||
                    x.Action.Contains("LOCK") ||
                    x.Action.Contains("ALERT"))
                .ToListAsync();

            var recent = await _context.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(20)
                .ToListAsync();

            return new SecurityDashboardViewModel
            {
                FailedLogins = failedLogins.Count,
                Alerts = alerts.Count,
                Users = await _context.Users.CountAsync(),
                Reservations = await _context.Reservations.CountAsync(),
                FailedLoginEvents = failedLogins.Take(20).ToList(),
                AlertEvents = alerts.Take(20).ToList(),
                RecentEvents = recent
            };
        }

        public async Task<List<ActivityLog>> GetAlertsAsync()
        {
            return await _context.ActivityLogs
                .Where(x =>
                    x.Action.Contains("BRUTE_FORCE") ||
                    x.Action.Contains("LOCK") ||
                    x.Action.Contains("ALERT"))
                .OrderByDescending(x => x.Timestamp)
                .Take(200)
                .ToListAsync();
        }

        public async Task<SecuritySettingsViewModel> GetSettingsAsync()
        {
            var settings = await _context.SecuritySettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SecuritySettings();
                _context.SecuritySettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return new SecuritySettingsViewModel
            {
                EnableBruteForceProtection = settings.EnableBruteForceProtection,
                MaxLoginAttempts = settings.MaxLoginAttempts,
                SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
                LockoutMinutes = settings.LockoutMinutes,
                IpWindowMinutes = settings.IpWindowMinutes
            };
        }

        public async Task UpdateSettingsAsync(SecuritySettingsViewModel model)
        {
            var settings = await _context.SecuritySettings.FirstAsync();

            settings.EnableBruteForceProtection = model.EnableBruteForceProtection;
            settings.MaxLoginAttempts = model.MaxLoginAttempts;
            settings.IpWindowMinutes = model.IpWindowMinutes;
            settings.SessionTimeoutMinutes = model.SessionTimeoutMinutes;
            settings.LockoutMinutes = model.LockoutMinutes;

            await _context.SaveChangesAsync();
        }
    }
}
