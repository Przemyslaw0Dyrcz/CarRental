using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CarRental.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = new[] { "Admin", "Dealer", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
            var adminEmail = "admin@local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = "admin", Email = adminEmail, FullName = "Administrator" };
                var r = await userManager.CreateAsync(admin, "Admin123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
