using Medical_center.Data;
using Medical_center.Models; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medical_center
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Зчитуємо з конфігурації, яку саме БД використовувати
            var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider");

            switch (dbProvider)
            {
                case "SqlServer":
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
                    break;

                case "Postgres":
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
                    break;

                case "Sqlite":
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
                    break;

                case "InMemory":
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("InMemoryDb"));
                    break;

                default:
                    throw new Exception("DatabaseProvider is not configured correctly in appsettings.json");
            }

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Використовуємо ApplicationUser, а не стандартний IdentityUser
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Конфігурація пайплайну запитів
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Додаємо перед Authorization
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
