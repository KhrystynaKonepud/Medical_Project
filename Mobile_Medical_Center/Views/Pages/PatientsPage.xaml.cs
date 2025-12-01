using Mobile_Medical_Center.ViewModels;
using System.Diagnostics;

namespace Mobile_Medical_Center.Views.Pages;

public partial class PatientsPage : ContentPage
{
    private bool _isLoaded = false;

    public PatientsPage()
    {
        try
        {
            InitializeComponent();
            BindingContext = new PatientsViewModel();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in PatientsPage constructor: {ex.Message}");
            Console.WriteLine($"Error in PatientsPage constructor: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded) return;

        try
        {
            Console.WriteLine("PatientsPage OnAppearing - starting to load data");
            var viewModel = (PatientsViewModel)BindingContext;
            if (viewModel != null)
            {
                await viewModel.LoadPatients();
                _isLoaded = true;
                Console.WriteLine("PatientsPage data loaded successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading patients data: {ex.Message}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
