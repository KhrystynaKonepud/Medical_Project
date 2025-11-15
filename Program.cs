using Medical_center.Data;
using Medical_center.Models;
using Medical_center.Validators;                 // кастомний валідатор пароля
using Microsoft.AspNetCore.Authentication;       // Challenge/AuthenticationProperties
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;       // DataProtection
using Microsoft.AspNetCore.Http;                 // SameSiteMode, CookieSecurePolicy
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;           // Jwt validation
using System.Text;
using System.IO;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ---------------- DB provider switch ----------------
        var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
        string connectionString;

        switch (dbProvider)
        {
            case "SqlServer":
                connectionString = builder.Configuration.GetConnectionString("SqlServerConnection")
                    ?? throw new InvalidOperationException("Connection string 'SqlServerConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
                break;

            case "Postgres":
                connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
                    ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));
                break;

            case "Sqlite":
                connectionString = builder.Configuration.GetConnectionString("SqliteConnection")
                    ?? throw new InvalidOperationException("Connection string 'SqliteConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connectionString));
                break;

            default:
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Default connection string not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
                break;
        }

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // ---------------- Identity + password/username rules ----------------
        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Пароль: 8–16, мінімум 1 цифра, 1 спецсимвол, 1 велика літера
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = false;

                // Username = Email, має бути унікальним
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Кастомний валідатор: максимум 16 символів
        builder.Services.AddTransient<IPasswordValidator<ApplicationUser>, MaxLengthPasswordValidator<ApplicationUser>>();

        // ---------------- Cookies ----------------
        builder.Services.ConfigureApplicationCookie(opt =>
        {
            opt.Cookie.Name = ".Med.Auth";
            opt.Cookie.HttpOnly = true;
            opt.Cookie.SameSite = SameSiteMode.None;              // повернення з зовнішніх редіректів
            opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // тільки HTTPS
            opt.LoginPath = "/api/auth/unauthorized";
            opt.SlidingExpiration = true;
            opt.ExpireTimeSpan = TimeSpan.FromHours(2);
        });

        // External cookie (тимчасовий для зовнішніх логінів)
        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, opt =>
        {
            opt.Cookie.SameSite = SameSiteMode.None;
            opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Cookie policy (гарантуємо SameSite=None => Secure=true)
        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.OnAppendCookie = c =>
            {
                if (c.CookieOptions.SameSite == SameSiteMode.None)
                    c.CookieOptions.Secure = true;
            };
            options.OnDeleteCookie = c =>
            {
                if (c.CookieOptions.SameSite == SameSiteMode.None)
                    c.CookieOptions.Secure = true;
            };
        });

        // ---------------- CORS (для локальних клієнтів/мобільного) ----------------
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("AllowLocalAll", p =>
                p.WithOrigins("https://localhost:7263", "https://127.0.0.1:7263")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials());
        });

        // ---------------- Authentication: Google (умовно) + JWT Bearer ----------------
        var disableExternal = builder.Configuration.GetValue<bool>("Auth:DisableExternalIdP");

        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            // Для веб — кукі; для зовнішнього логіну — external cookie
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        });

        if (!disableExternal)
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                options.CallbackPath = "/signin-google";                           // стандартний callback
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SaveTokens = true;

                options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/login?error=google-remote-failure");
                        ctx.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });
        }

        // JWT (Bearer) для API
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        var jwtKey = builder.Configuration["Jwt:Key"];

        authBuilder.AddJwtBearer("Bearer", options =>
        {
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });

        // ---------------- Authorization (ролі/політики) ----------------
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("DoctorOnly", p => p.RequireRole("Doctor"));
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        });

        // ---------------- DataProtection: ключі на диску ----------------
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(
                Path.Combine(builder.Environment.ContentRootPath, "keys")))
            .SetApplicationName("Medical_center");

        builder.Services.AddControllers();

        var app = builder.Build();

        // Apply database migrations automatically
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Only redirect to HTTPS in production when configured
        if (!app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("UseHttpsRedirection", true))
        {
            app.UseHttpsRedirection();
        }

        // Видача зібраного React (ClientApp/build)
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "ClientApp", "build"))
        });

        app.UseRouting();

        // ВАЖЛИВО: до UseAuthentication
        app.UseCookiePolicy();

        app.UseCors("AllowLocalAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "ClientApp", "build"))
        });

        // Сід ролей/адміна
        using (var scope = app.Services.CreateScope())
        {
            await IdentitySeed.SeedAsync(scope.ServiceProvider);
        }

        app.Run();
    }
}
