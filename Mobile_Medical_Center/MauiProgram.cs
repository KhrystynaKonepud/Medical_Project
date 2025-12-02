using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Mobile_Medical_Center.Services;
using Mobile_Medical_Center.Views.Pages;
using SkiaSharp.Views.Maui.Controls.Hosting;

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
                    .UseSkiaSharp()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                LogToFile("Services registration starting...");
                // Register Services - Thin client pattern (API-based, no local database)
                builder.Services.AddSingleton<IAuthService, AuthService>();
                builder.Services.AddSingleton<IDatabaseService, ApiClientService>();

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

                // No local database to initialize - thin client uses API
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

    }
}
