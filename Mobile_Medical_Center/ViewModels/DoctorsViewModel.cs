using Mobile_Medical_Center.Models;
using Mobile_Medical_Center.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Mobile_Medical_Center.ViewModels
{
    public class DoctorsViewModel : ViewModelBase
    {
        private ObservableCollection<Doctor> _doctors;
        private Doctor _selectedDoctor;
        private string _newDoctorName = "";
        private string _newDoctorSpecialization = "";
        private int _newDoctorExperience;
        private readonly IDatabaseService _databaseService;

        private static string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "medical_center_debug.log"
        );

        private void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [DoctorsViewModel] {message}\n");
            }
            catch { }
        }

        public ObservableCollection<Doctor> Doctors
        {
            get => _doctors;
            set => SetProperty(ref _doctors, value);
        }

        public Doctor SelectedDoctor
        {
            get => _selectedDoctor;
            set => SetProperty(ref _selectedDoctor, value);
        }

        public string NewDoctorName
        {
            get => _newDoctorName;
            set => SetProperty(ref _newDoctorName, value);
        }

        public string NewDoctorSpecialization
        {
            get => _newDoctorSpecialization;
            set => SetProperty(ref _newDoctorSpecialization, value);
        }

        public int NewDoctorExperience
        {
            get => _newDoctorExperience;
            set => SetProperty(ref _newDoctorExperience, value);
        }

        public ICommand LoadDoctorsCommand { get; }
        public ICommand AddDoctorCommand { get; }
        public ICommand DeleteDoctorCommand { get; }
        public ICommand SelectionChangedCommand { get; }

        public DoctorsViewModel()
        {
            _databaseService = ServiceHelper.GetService<IDatabaseService>();
            Doctors = new ObservableCollection<Doctor>();

            LoadDoctorsCommand = new AsyncCommand(LoadDoctors);
            AddDoctorCommand = new AsyncCommand(OnAddDoctor);
            DeleteDoctorCommand = new AsyncCommand<Doctor>(OnDeleteDoctor);
            SelectionChangedCommand = new AsyncCommand<Doctor>(OnSelectionChanged);
        }

        public async Task LoadDoctors()
        {
            IsLoading = true;
            try
            {
                LogToFile("LoadDoctors() started");
                LogToFile($"DatabaseService is null: {_databaseService == null}");

                var doctors = await _databaseService.GetAllDoctorsAsync();

                LogToFile($"Loaded {doctors?.Count ?? 0} doctors from database");

                if (doctors != null && doctors.Count > 0)
                {
                    foreach (var doc in doctors.Take(3))
                    {
                        LogToFile($"  - Doctor: {doc.FullName} ({doc.Specialization})");
                    }
                }

                LogToFile($"Before setting Doctors collection, current count: {_doctors?.Count ?? 0}");
                Doctors = new ObservableCollection<Doctor>(doctors ?? new List<Doctor>());
                LogToFile($"After setting Doctors collection, count: {Doctors?.Count ?? 0}");
                LogToFile($"Doctors property value is null: {Doctors == null}");
            }
            catch (Exception ex)
            {
                LogToFile($"Error loading doctors: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Error loading doctors: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnAddDoctor()
        {
            IsLoading = true;
            try
            {
                if (string.IsNullOrWhiteSpace(NewDoctorName) || string.IsNullOrWhiteSpace(NewDoctorSpecialization))
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Заповніть всі поля", "OK");
                    return;
                }

                var doctor = new Doctor
                {
                    FullName = NewDoctorName,
                    Specialization = NewDoctorSpecialization,
                    ExperienceYears = NewDoctorExperience,
                    Bio = "Новий лікар",
                    Rating = 5.0m
                };

                await _databaseService.AddDoctorAsync(doctor);

                NewDoctorName = "";
                NewDoctorSpecialization = "";
                NewDoctorExperience = 0;

                await LoadDoctors();
                await Application.Current.MainPage.DisplayAlert("Успіх", "Лікар додано", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Помилка при додаванні: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnDeleteDoctor(Doctor doctor)
        {
            if (doctor == null) return;

            var confirm = await Application.Current.MainPage.DisplayAlert("Підтвердження",
                $"Видалити лікаря {doctor.FullName}?", "Так", "Ні");

            if (confirm)
            {
                IsLoading = true;
                try
                {
                    await _databaseService.DeleteDoctorAsync(doctor.Id);
                    await LoadDoctors();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", $"Помилка при видаленні: {ex.Message}", "OK");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task OnSelectionChanged(Doctor doctor)
        {
            if (doctor != null)
            {
                SelectedDoctor = doctor;
                // Selection handling - currently just stores the selected doctor
            }
            await Task.CompletedTask;
        }
    }
}
