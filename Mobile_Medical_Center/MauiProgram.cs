using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Mobile_Medical_Center.Services;
using Mobile_Medical_Center.Views.Pages;

namespace Mobile_Medical_Center
{
    public static class MauiProgram
    {
        private static string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "medical_center_debug.log"
        );

        private static void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
            }
            catch { }
        }

        public static MauiApp CreateMauiApp()
        {
            LogToFile("========== MauiProgram.CreateMauiApp STARTED ==========");
            var builder = MauiApp.CreateBuilder();

            try
            {
                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                LogToFile("Services registration starting...");
                // Register Services (BEFORE database is initialized)
                builder.Services.AddSingleton<IAuthService, AuthService>();
                builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

                // Register Pages as Transient (lazy loading, not created at startup)
                builder.Services.AddTransient<LoginPage>();
                builder.Services.AddTransient<DashboardPage>();
                builder.Services.AddTransient<DoctorsPage>();
                builder.Services.AddTransient<PatientsPage>();
                builder.Services.AddTransient<AppointmentsPage>();
                builder.Services.AddTransient<AboutPage>();
                LogToFile("Services registered successfully");

#if DEBUG
                builder.Logging.AddDebug();
#endif

                LogToFile("Building MAUI app...");
                var mauiApp = builder.Build();
                LogToFile("MAUI app built successfully");

                // Store service provider for ServiceHelper
                ServiceHelper.Services = mauiApp.Services;

                // Initialize database synchronously before returning
                LogToFile("About to initialize database...");
                InitializeDatabaseSync(mauiApp);
                LogToFile("========== MauiProgram.CreateMauiApp COMPLETED ==========");

                return mauiApp;
            }
            catch (Exception ex)
            {
                LogToFile($"MAUI initialization error: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"MAUI initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private static void InitializeDatabaseSync(MauiApp app)
        {
            try
            {
                LogToFile("Starting database initialization...");

                var dbService = app.Services.GetService<IDatabaseService>();
                if (dbService == null)
                {
                    LogToFile("ERROR: DatabaseService not resolved from DI container");
                    return;
                }

                LogToFile("DatabaseService resolved, initializing database...");

                // Synchronously wait for database initialization to complete
                dbService.InitializeAsync().GetAwaiter().GetResult();

                LogToFile("✓ Database initialized successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"✗ Database initialization error: {ex.Message}");
                LogToFile($"Exception type: {ex.GetType().FullName}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"✗ Database initialization error: {ex.Message}");
                Debug.WriteLine($"Exception type: {ex.GetType().FullName}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
