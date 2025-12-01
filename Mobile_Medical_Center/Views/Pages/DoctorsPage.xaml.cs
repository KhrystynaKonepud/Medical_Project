using Mobile_Medical_Center.ViewModels;
using System.Diagnostics;

namespace Mobile_Medical_Center.Views.Pages;

public partial class DoctorsPage : ContentPage
{
    private bool _isLoaded = false;

    private static string _logFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "medical_center_debug.log"
    );

    private void LogToFile(string message)
    {
        try
        {
            File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DoctorsPage] {message}\n");
        }
        catch { }
    }

    public DoctorsPage()
    {
        try
        {
            LogToFile("Constructor called");
            InitializeComponent();
            BindingContext = new DoctorsViewModel();
            LogToFile("BindingContext set successfully");
        }
        catch (Exception ex)
        {
            LogToFile($"Error in constructor: {ex.Message}");
            Debug.WriteLine($"Error in DoctorsPage constructor: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LogToFile("========== OnAppearing CALLED ==========");
        LogToFile($"_isLoaded = {_isLoaded}");

        if (_isLoaded)
        {
            LogToFile("Already loaded, skipping");
            return;
        }

        try
        {
            LogToFile("OnAppearing - starting to load data");
            var viewModel = (DoctorsViewModel)BindingContext;
            LogToFile($"ViewModel is null: {viewModel == null}");

            if (viewModel != null)
            {
                LogToFile("About to call viewModel.LoadDoctors()");
                await viewModel.LoadDoctors();
                _isLoaded = true;
                LogToFile("Data loaded successfully");
            }
            else
            {
                LogToFile("ERROR: ViewModel is null!");
            }
        }
        catch (Exception ex)
        {
            LogToFile($"Error loading data: {ex.Message}");
            LogToFile($"Exception Type: {ex.GetType().Name}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
        LogToFile("========== OnAppearing FINISHED ==========");
    }
}
