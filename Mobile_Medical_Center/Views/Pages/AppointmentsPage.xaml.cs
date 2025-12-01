using Mobile_Medical_Center.ViewModels;
using System.Diagnostics;

namespace Mobile_Medical_Center.Views.Pages;

public partial class AppointmentsPage : ContentPage
{
    private bool _isLoaded = false;

    public AppointmentsPage()
    {
        try
        {
            InitializeComponent();
            BindingContext = new AppointmentsViewModel();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in AppointmentsPage constructor: {ex.Message}");
            Console.WriteLine($"Error in AppointmentsPage constructor: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded) return;

        try
        {
            Console.WriteLine("AppointmentsPage OnAppearing - starting to load data");
            var viewModel = (AppointmentsViewModel)BindingContext;
            if (viewModel != null)
            {
                await viewModel.LoadAppointments();
                _isLoaded = true;
                Console.WriteLine("AppointmentsPage data loaded successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading appointments data: {ex.Message}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
