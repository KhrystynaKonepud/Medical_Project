using Mobile_Medical_Center.Models;
using Mobile_Medical_Center.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Mobile_Medical_Center.ViewModels
{
    public class PatientsViewModel : ViewModelBase
    {
        private ObservableCollection<Patient> _patients;
        private Patient _selectedPatient;
        private string _newPatientName = "";
        private string _newPatientEmail = "";
        private string _newPatientPhone = "";
        private DateTime _newPatientDOB = DateTime.Now.AddYears(-30);
        private readonly IDatabaseService _databaseService;

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set => SetProperty(ref _selectedPatient, value);
        }

        public string NewPatientName
        {
            get => _newPatientName;
            set => SetProperty(ref _newPatientName, value);
        }

        public string NewPatientEmail
        {
            get => _newPatientEmail;
            set => SetProperty(ref _newPatientEmail, value);
        }

        public string NewPatientPhone
        {
            get => _newPatientPhone;
            set => SetProperty(ref _newPatientPhone, value);
        }

        public DateTime NewPatientDOB
        {
            get => _newPatientDOB;
            set => SetProperty(ref _newPatientDOB, value);
        }

        public ICommand LoadPatientsCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand DeletePatientCommand { get; }

        public PatientsViewModel()
        {
            _databaseService = ServiceHelper.GetService<IDatabaseService>();
            Patients = new ObservableCollection<Patient>();

            LoadPatientsCommand = new AsyncCommand(LoadPatients);
            AddPatientCommand = new AsyncCommand(OnAddPatient);
            DeletePatientCommand = new AsyncCommand<Patient>(OnDeletePatient);
        }

        public async Task LoadPatients()
        {
            IsLoading = true;
            try
            {
                var patients = await _databaseService.GetAllPatientsAsync();
                Patients = new ObservableCollection<Patient>(patients);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading patients: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnAddPatient()
        {
            IsLoading = true;
            try
            {
                if (string.IsNullOrWhiteSpace(NewPatientName) || string.IsNullOrWhiteSpace(NewPatientEmail) || string.IsNullOrWhiteSpace(NewPatientPhone))
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Заповніть всі поля", "OK");
                    return;
                }

                var patient = new Patient
                {
                    FullName = NewPatientName,
                    Email = NewPatientEmail,
                    PhoneNumber = NewPatientPhone,
                    DateOfBirth = NewPatientDOB,
                    Gender = "Невказано",
                    Address = "",
                    EmergencyContact = ""
                };

                await _databaseService.AddPatientAsync(patient);

                NewPatientName = "";
                NewPatientEmail = "";
                NewPatientPhone = "";
                NewPatientDOB = DateTime.Now.AddYears(-30);

                await LoadPatients();
                await Application.Current.MainPage.DisplayAlert("Успіх", "Пацієнт додано", "OK");
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

        private async Task OnDeletePatient(Patient patient)
        {
            if (patient == null) return;

            var confirm = await Application.Current.MainPage.DisplayAlert("Підтвердження",
                $"Видалити пацієнта {patient.FullName}?", "Так", "Ні");

            if (confirm)
            {
                IsLoading = true;
                try
                {
                    await _databaseService.DeletePatientAsync(patient.Id);
                    await LoadPatients();
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
    }
}
