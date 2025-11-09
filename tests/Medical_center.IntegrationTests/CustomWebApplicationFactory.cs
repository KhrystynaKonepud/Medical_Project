using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Medical_center.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        SeedTestData(db, roleManager, userManager).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the database with test data. Error: {Message}", ex.Message);
                    }
                }
            });
        }

        private static async System.Threading.Tasks.Task SeedTestData(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Doctor"))
                await roleManager.CreateAsync(new IdentityRole("Doctor"));

            if (!await roleManager.RoleExistsAsync("Patient"))
                await roleManager.CreateAsync(new IdentityRole("Patient"));

            if (await userManager.FindByEmailAsync("testpatient@test.com") == null)
            {
                var patient = new ApplicationUser
                {
                    UserName = "testpatient@test.com",
                    Email = "testpatient@test.com",
                    FullName = "Test Patient",
                    PhoneNumber = "+380111111111",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(patient, "Patient@123");
                await userManager.AddToRoleAsync(patient, "Patient");

                context.Patients.Add(new Patient
                {
                    UserId = patient.Id,
                    EmergencyContact = "+380999999999"
                });
            }

            if (await userManager.FindByEmailAsync("testdoctor@test.com") == null)
            {
                var doctor = new ApplicationUser
                {
                    UserName = "testdoctor@test.com",
                    Email = "testdoctor@test.com",
                    FullName = "Dr. Test Doctor",
                    PhoneNumber = "+380222222222",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(doctor, "Doctor@123");
                await userManager.AddToRoleAsync(doctor, "Doctor");

                context.Doctors.Add(new Doctor
                {
                    UserId = doctor.Id,
                    Specialization = "General Practice",
                    ExperienceYears = 5,
                    Bio = "Test doctor for integration tests",
                    Rating = 4.5m
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
