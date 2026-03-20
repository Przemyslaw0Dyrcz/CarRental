using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;

namespace CarRental.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<AccessRequest> AccessRequests { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<UserSecuritySettings> UserSecuritySettings { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarImage> CarImages { get; set; }
        public DbSet<CarReview> CarReviews { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<SecuritySettings> SecuritySettings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Car>()
                .Property(c => c.PricePerDay)
                .HasPrecision(10, 2);

            builder.Entity<Reservation>()
                .Property(r => r.TotalPrice)
                .HasPrecision(10, 2);

            builder.Entity<Reservation>()
                .HasIndex(r => new { r.CarId, r.FromDate, r.ToDate });

            builder.Entity<ActivityLog>()
                .HasIndex(a => a.Timestamp);

            builder.Entity<ActivityLog>()
                .HasIndex(a => a.UserName);
        }
    }

}

