using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            // 1. Ролі (Admin, Doctor, Patient)
            string[] roles = { "Admin", "Doctor", "Patient" };
            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
            }

            // 2. Адміністратор
            var adminEmail = "admin@med.local";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };
                await userMgr.CreateAsync(admin, "Admin#2025!");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            // 3. (Необов’язково, але зручно)
            // Автоматичне призначення ролей користувачам,
            // якщо вони вже пов’язані з Doctor/Patient у базі
            var doctorUserIds = await db.Doctors
                .Where(d => d.UserId != null)
                .Select(d => d.UserId!)
                .ToListAsync();

            foreach (var uid in doctorUserIds)
            {
                var user = await userMgr.FindByIdAsync(uid);
                if (user != null && !(await userMgr.IsInRoleAsync(user, "Doctor")))
                    await userMgr.AddToRoleAsync(user, "Doctor");
            }

            var patientUserIds = await db.Patients
                .Where(p => p.UserId != null)
                .Select(p => p.UserId!)
                .ToListAsync();

            foreach (var uid in patientUserIds)
            {
                var user = await userMgr.FindByIdAsync(uid);
                if (user != null && !(await userMgr.IsInRoleAsync(user, "Patient")))
                    await userMgr.AddToRoleAsync(user, "Patient");
            }
        }
    }
}
