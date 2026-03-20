using System.Collections.Generic;
using CarRental.Models;

namespace CarRental.ViewModels
{
    public class SecurityDashboardViewModel
    {
        public int FailedLogins { get; set; }

        public int Alerts { get; set; }

        public int Users { get; set; }

        public int Reservations { get; set; }

        public List<string> ChartLabels { get; set; } = new();

        public List<int> ChartData { get; set; } = new();

        public List<IpAttackViewModel> TopAttackers { get; set; } = new();

        public List<ActivityLog> FailedLoginEvents { get; set; } = new();

        public List<ActivityLog> AlertEvents { get; set; } = new();

        public List<ActivityLog> RecentEvents { get; set; } = new();
    }

    public class IpAttackViewModel
    {
        public string Ip { get; set; }

        public int Attempts { get; set; }
    }
}