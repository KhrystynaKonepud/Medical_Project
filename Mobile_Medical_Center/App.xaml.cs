using System.Diagnostics;
using Microsoft.Maui.Controls;
using Mobile_Medical_Center.Services;
using Mobile_Medical_Center.Views.Pages;

namespace Mobile_Medical_Center
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                Console.WriteLine("🟢 App constructor started");
                InitializeComponent();
                Console.WriteLine("🟢 InitializeComponent completed");

                // Initialize AppShell with full navigation
                MainPage = new AppShell();
                Console.WriteLine("🟢 MainPage set to AppShell");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 App constructor ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            Console.WriteLine("🟢 OnStart called");
            // Navigation disabled for testing - just show the shell with all tabs visible
        }
    }
}
