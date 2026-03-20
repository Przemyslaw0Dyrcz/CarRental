
using CarRental.Data;
using System.Linq;

namespace CarRental.Services
{
    public class SecurityStatsService
    {
        private readonly ApplicationDbContext _db;

        public SecurityStatsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public int TotalLogins()
        {
            return _db.ActivityLogs.Count(x => x.Action == "LOGIN_SUCCESS");
        }

        public int FailedLogins()
        {
            return _db.ActivityLogs.Count(x => x.Action == "LOGIN_FAILED");
        }

        public int LogoutCount()
        {
            return _db.ActivityLogs.Count(x => x.Action == "LOGOUT");
        }

        public int Reservations()
        {
            return _db.ActivityLogs.Count(x => x.Action == "CREATE_RESERVATION");
        }
    }
}
