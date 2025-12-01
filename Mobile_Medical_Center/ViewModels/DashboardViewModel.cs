using Mobile_Medical_Center.Models;
using Mobile_Medical_Center.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Mobile_Medical_Center.ViewModels
{
    public class ChartDataPoint
    {
        public string DoctorName { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class DashboardViewModel : ViewModelBase
    {
        private ObservableCollection<ChartDataPoint> _chartData;
        private int _totalDoctors;
        private int _totalPatients;
        private int _totalAppointments;
        private int _completedAppointments;
        private readonly IDatabaseService _databaseService;

        public ObservableCollection<ChartDataPoint> ChartData
        {
            get => _chartData;
            set => SetProperty(ref _chartData, value);
        }

        public int TotalDoctors
        {
            get => _totalDoctors;
            set => SetProperty(ref _totalDoctors, value);
        }

        public int TotalPatients
        {
            get => _totalPatients;
            set => SetProperty(ref _totalPatients, value);
        }

        public int TotalAppointments
        {
            get => _totalAppointments;
            set => SetProperty(ref _totalAppointments, value);
        }

        public int CompletedAppointments
        {
            get => _completedAppointments;
            set => SetProperty(ref _completedAppointments, value);
        }

        public ICommand LoadDashboardCommand { get; }

        public DashboardViewModel()
        {
            _databaseService = ServiceHelper.GetService<IDatabaseService>();
            ChartData = new ObservableCollection<ChartDataPoint>();
            LoadDashboardCommand = new AsyncCommand(LoadDashboard);
        }

        public async Task LoadDashboard()
        {
            IsLoading = true;
            try
            {
                var doctors = await _databaseService.GetAllDoctorsAsync();
                var patients = await _databaseService.GetAllPatientsAsync();
                var appointments = await _databaseService.GetAllAppointmentsAsync();

                TotalDoctors = doctors.Count;
                TotalPatients = patients.Count;
                TotalAppointments = appointments.Count;
                CompletedAppointments = appointments.Count(a => a.Status == "Completed");

                // Prepare chart data - appointments per doctor
                var chartDataPoints = new List<ChartDataPoint>();

                foreach (var doctor in doctors)
                {
                    var appointmentCount = appointments.Count(a => a.DoctorId == doctor.Id);
                    chartDataPoints.Add(new ChartDataPoint
                    {
                        DoctorName = doctor.FullName,
                        AppointmentCount = appointmentCount
                    });
                }

                // Sort by appointment count descending
                chartDataPoints = chartDataPoints.OrderByDescending(x => x.AppointmentCount).ToList();

                ChartData = new ObservableCollection<ChartDataPoint>(chartDataPoints);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
