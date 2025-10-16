using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Налаштування бази даних ---
        var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
        string connectionString;

        switch (dbProvider)
        {
            case "SqlServer":
                connectionString = builder.Configuration.GetConnectionString("SqlServerConnection") ?? throw new InvalidOperationException("Connection string 'SqlServerConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
                break;
            case "Postgres":
                connectionString = builder.Configuration.GetConnectionString("PostgresConnection") ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));
                break;
            case "Sqlite":
                connectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? throw new InvalidOperationException("Connection string 'SqliteConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connectionString));
                break;
            default:
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Default connection string not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
                break;
        }

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequiredLength = 8;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllers();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "ClientApp", "build"))
        });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "ClientApp", "build"))
        });

        // Виклик сіду даних
        using (var scope = app.Services.CreateScope())
        {
            await IdentitySeed.SeedAsync(scope.ServiceProvider);
        }

        app.Run();
    }
}