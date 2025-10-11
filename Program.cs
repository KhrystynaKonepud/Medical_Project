using Medical_center.Data;
using Medical_center.Models;
using Medical_center.Validators;              // ⬅ додаємо простір імен валідатора
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medical_center
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // БД (беремо з appsettings.json -> "DatabaseProvider")
            var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider");
            switch (dbProvider)
            {
                case "SqlServer":
                    builder.Services.AddDbContext<ApplicationDbContext>(o =>
                        o.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
                    break;
                case "Postgres":
                    builder.Services.AddDbContext<ApplicationDbContext>(o =>
                        o.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
                    break;
                case "Sqlite":
                    builder.Services.AddDbContext<ApplicationDbContext>(o =>
                        o.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
                    break;
                case "InMemory":
                    builder.Services.AddDbContext<ApplicationDbContext>(o =>
                        o.UseInMemoryDatabase("InMemoryDb"));
                    break;
                default:
                    throw new Exception("DatabaseProvider is not configured correctly in appsettings.json");
            }

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ЄДИНА реєстрація Identity — тільки з ApplicationUser
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(opt =>
                {
                    opt.SignIn.RequireConfirmedAccount = true;

                    // Вимоги до пароля: мінімум 8, складність
                    opt.Password.RequiredLength = 8;
                    opt.Password.RequireDigit = true;
                    opt.Password.RequireUppercase = true;
                    opt.Password.RequireNonAlphanumeric = true;
                    // Максимум 16 символів обмежуємо власним валідатором нижче
                })
                .AddPasswordValidator<MaxLengthPasswordValidator>()     // ⬅ максимум 16 символів
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            // Зовнішні логіни (Google)
            builder.Services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                    // options.CallbackPath = "/signin-google"; // за потреби можна зафіксувати явно
                });

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            // Сід ролей/адміна
            using (var scope = app.Services.CreateScope())
            {
                await IdentitySeed.SeedAsync(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}
