using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Medical_center.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Створення ролей
            string[] roleNames = { "Admin", "Doctor", "Patient" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Створення користувача-адміністратора
            var adminEmail = "admin@med.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    PhoneNumber = "+380000000000",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin#2025!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. За потреби можна додати тестового лікаря
            var doctorEmail = "doctor@med.local11";
            if (await userManager.FindByEmailAsync(doctorEmail) == null)
            {
                var doctorUser = new ApplicationUser
                {
                    UserName = doctorEmail,
                    Email = doctorEmail,
                    FullName = "John Doctor",
                    PhoneNumber = "+380111111111",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(doctorUser, "Doctor#2025!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");
            }
        }
    }
}
