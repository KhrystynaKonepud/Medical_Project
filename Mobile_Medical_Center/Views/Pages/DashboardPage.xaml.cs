using Mobile_Medical_Center.ViewModels;
using System.Diagnostics;

namespace Mobile_Medical_Center.Views.Pages;

public partial class DashboardPage : ContentPage
{
    private bool _isLoaded = false;

    public DashboardPage()
    {
        try
        {
            InitializeComponent();
            BindingContext = new DashboardViewModel();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DashboardPage constructor: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded) return;

        try
        {
            Console.WriteLine("DashboardPage OnAppearing - starting to load data");
            var viewModel = (DashboardViewModel)BindingContext;
            if (viewModel != null)
            {
                await viewModel.LoadDashboard();
                _isLoaded = true;
                Console.WriteLine("DashboardPage data loaded successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
