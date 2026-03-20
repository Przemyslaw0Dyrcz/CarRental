using CarRental.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Data
{
    public static class DemoDataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (!db.ActivityLogs.Any())
            {
                db.ActivityLogs.AddRange(
                    new ActivityLog
                    {
                        Timestamp = DateTime.UtcNow.AddMinutes(-15),
                        UserName = "marcin",
                        Action = "AccessRequestCreated",
                        Description = "Requested role Admin"
                    },
                    new ActivityLog
                    {
                        Timestamp = DateTime.UtcNow.AddMinutes(-10),
                        UserName = "admin@local",
                        Action = "AccessRequestApproved",
                        Description = "Approved Admin for marcin"
                    },
                    new ActivityLog
                    {
                        Timestamp = DateTime.UtcNow.AddMinutes(-5),
                        UserName = "(unknown)",
                        Description = "HTTP honeytoken triggered from IP 185.22.11.4"
                    }
                );
            }

            if (!db.AccessRequests.Any())
            {
                db.AccessRequests.Add(
                    new AccessRequest
                    {
                        RequesterId = "demo-user-id",
                        RequestedRole = "Admin",
                        Reason = "Need elevated privileges for deployment",
                        RequestedAt = DateTime.UtcNow.AddMinutes(-20),
                        Status = "Pending"
                    }
                );
            }

            await db.SaveChangesAsync();
        }
    }
}