using CarRental.Models;
using CarRental.ViewModels;

namespace CarRental.Services.Interface
{
    public interface IAdminService
    {
        Task<List<ActivityLog>> GetLogsAsync();
        Task<SecurityDashboardViewModel> GetSecurityDashboardAsync();
        Task<List<ActivityLog>> GetAlertsAsync();
        Task<SecuritySettingsViewModel> GetSettingsAsync();
        Task UpdateSettingsAsync(SecuritySettingsViewModel model);
    }
}
