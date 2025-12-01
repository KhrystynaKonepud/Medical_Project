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
using Microsoft.OpenApi.Models;
using Asp.Versioning;                            // API versioning
using Asp.Versioning.ApiExplorer;                // API Explorer для Swagger
using System.Linq;                               // LINQ для фільтрів Swagger

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

            case "MySql":
                connectionString = builder.Configuration.GetConnectionString("MySqlConnection")
                    ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
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
                p.SetIsOriginAllowed(origin =>
                    origin.StartsWith("https://localhost") ||
                    origin.StartsWith("https://127.0.0.1") ||
                    origin.StartsWith("http://localhost") ||
                    origin.StartsWith("http://127.0.0.1"))
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

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Ігнорування циклічних залежностей при серіалізації
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                // Опціонально: писати null значення
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

        // ---------------- API Versioning ----------------
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // ---------------- Swagger ----------------
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            // Створюємо документи для кожної версії API
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Medical Center API",
                Version = "v1",
                Description = "API версія 1.0 - базова функціональність"
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Medical Center API",
                Version = "v2",
                Description = "API версія 2.0 - розширена функціональність зі статистикою"
            });

            // Додаємо підтримку JWT Bearer токенів
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введіть JWT токен. Приклад: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        // ---------------- Database initialization with conflict prevention ----------------
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Check if database can connect
                var canConnect = await db.Database.CanConnectAsync();

                // Always try to migrate or create database without deleting existing data
                logger.LogInformation("Ensuring database exists and is up to date...");

                try
                {
                    // Try to apply migrations first
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception)
                {
                    // If migrations fail, ensure database is created
                    await db.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database created successfully");
                }

                // Seed roles and admin user (will only seed if they don't exist)
                await IdentitySeed.SeedAsync(scope.ServiceProvider);

                // Seed test data (doctors, patients, appointments) - only if tables are empty
                await DataSeed.SeedAsync(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
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

        // Swagger UI (тільки в Development)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"Medical Center API {description.GroupName}");
                }

                c.RoutePrefix = "swagger";
            });
        }

        app.MapControllers();

        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "ClientApp", "build"))
        });

        app.Run();
    }
}