using Mobile_Medical_Center.Models;
using Mobile_Medical_Center.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Microcharts;
using SkiaSharp;

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
        private int _scheduledAppointments;
        private int _canceledAppointments;
        private Chart _appointmentStatusChart;
        private Chart _appointmentsByDoctorChart;
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

        public int ScheduledAppointments
        {
            get => _scheduledAppointments;
            set => SetProperty(ref _scheduledAppointments, value);
        }

        public int CanceledAppointments
        {
            get => _canceledAppointments;
            set => SetProperty(ref _canceledAppointments, value);
        }

        public Chart AppointmentStatusChart
        {
            get => _appointmentStatusChart;
            set => SetProperty(ref _appointmentStatusChart, value);
        }

        public Chart AppointmentsByDoctorChart
        {
            get => _appointmentsByDoctorChart;
            set => SetProperty(ref _appointmentsByDoctorChart, value);
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
                ScheduledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Scheduled);
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
                CanceledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Canceled);

                // Create Donut Chart for Appointment Status
                var statusEntries = new List<ChartEntry>();

                if (ScheduledAppointments > 0)
                {
                    statusEntries.Add(new ChartEntry(ScheduledAppointments)
                    {
                        Label = "Запланований",
                        ValueLabel = ScheduledAppointments.ToString(),
                        Color = SKColor.Parse("#2196F3") // Blue
                    });
                }

                if (CompletedAppointments > 0)
                {
                    statusEntries.Add(new ChartEntry(CompletedAppointments)
                    {
                        Label = "Виконаний",
                        ValueLabel = CompletedAppointments.ToString(),
                        Color = SKColor.Parse("#4CAF50") // Green
                    });
                }

                if (CanceledAppointments > 0)
                {
                    statusEntries.Add(new ChartEntry(CanceledAppointments)
                    {
                        Label = "Скасований",
                        ValueLabel = CanceledAppointments.ToString(),
                        Color = SKColor.Parse("#F44336") // Red
                    });
                }

                AppointmentStatusChart = new DonutChart
                {
                    Entries = statusEntries,
                    LabelTextSize = 32,
                    BackgroundColor = SKColors.Transparent,
                    LabelMode = LabelMode.RightOnly
                };

                // Prepare chart data - appointments per doctor (Bar Chart)
                var doctorEntries = new List<ChartEntry>();
                var chartDataPoints = new List<ChartDataPoint>();

                var colors = new[] { "#512BD4", "#2196F3", "#4CAF50", "#FF9800", "#E91E63", "#9C27B0" };
                int colorIndex = 0;

                foreach (var doctor in doctors.Take(6)) // Top 6 doctors
                {
                    var appointmentCount = appointments.Count(a => a.DoctorId == doctor.Id);
                    if (appointmentCount > 0)
                    {
                        doctorEntries.Add(new ChartEntry(appointmentCount)
                        {
                            Label = doctor.FullName.Length > 15
                                ? doctor.FullName.Substring(0, 12) + "..."
                                : doctor.FullName,
                            ValueLabel = appointmentCount.ToString(),
                            Color = SKColor.Parse(colors[colorIndex % colors.Length])
                        });

                        chartDataPoints.Add(new ChartDataPoint
                        {
                            DoctorName = doctor.FullName,
                            AppointmentCount = appointmentCount
                        });

                        colorIndex++;
                    }
                }

                AppointmentsByDoctorChart = new BarChart
                {
                    Entries = doctorEntries,
                    LabelTextSize = 28,
                    ValueLabelTextSize = 28,
                    BackgroundColor = SKColors.Transparent,
                    LabelOrientation = Orientation.Horizontal,
                    ValueLabelOrientation = Orientation.Horizontal
                };

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
