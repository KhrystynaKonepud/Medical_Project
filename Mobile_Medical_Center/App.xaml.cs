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
                InitializeComponent();
                MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 App constructor ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Show error page
                MainPage = new ContentPage
                {
                    BackgroundColor = Colors.Red,
                    Content = new Label
                    {
                        Text = $"ERROR: {ex.Message}",
                        TextColor = Colors.White,
                        FontSize = 20,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                };
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            Console.WriteLine("🟢 OnStart called");
        }
    }
}
