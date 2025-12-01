using Mobile_Medical_Center.Models;
using Mobile_Medical_Center.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Mobile_Medical_Center.ViewModels
{
    public class AppointmentsViewModel : ViewModelBase
    {
        private ObservableCollection<Appointment> _appointments;
        private ObservableCollection<Doctor> _doctors;
        private ObservableCollection<Patient> _patients;
        private Appointment _selectedAppointment;
        private Doctor _selectedDoctor;
        private Patient _selectedPatient;
        private DateTime _appointmentDate = DateTime.Now.AddDays(1);
        private string _reason = "";
        private readonly IDatabaseService _databaseService;

        public ObservableCollection<Appointment> Appointments
        {
            get => _appointments;
            set => SetProperty(ref _appointments, value);
        }

        public ObservableCollection<Doctor> Doctors
        {
            get => _doctors;
            set => SetProperty(ref _doctors, value);
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set => SetProperty(ref _selectedAppointment, value);
        }

        public Doctor SelectedDoctor
        {
            get => _selectedDoctor;
            set => SetProperty(ref _selectedDoctor, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set => SetProperty(ref _selectedPatient, value);
        }

        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set => SetProperty(ref _appointmentDate, value);
        }

        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand BookAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand ChangeStatusCommand { get; }

        public AppointmentsViewModel()
        {
            _databaseService = ServiceHelper.GetService<IDatabaseService>();
            Appointments = new ObservableCollection<Appointment>();
            Doctors = new ObservableCollection<Doctor>();
            Patients = new ObservableCollection<Patient>();

            LoadAppointmentsCommand = new AsyncCommand(LoadAppointments);
            BookAppointmentCommand = new AsyncCommand(OnBookAppointment);
            DeleteAppointmentCommand = new AsyncCommand<Appointment>(OnDeleteAppointment);
            ChangeStatusCommand = new AsyncCommand<Appointment>(OnChangeStatus);
        }

        public async Task LoadAppointments()
        {
            IsLoading = true;
            try
            {
                var appointments = await _databaseService.GetAllAppointmentsAsync();
                Appointments = new ObservableCollection<Appointment>(appointments);

                var doctors = await _databaseService.GetAllDoctorsAsync();
                Doctors = new ObservableCollection<Doctor>(doctors);

                var patients = await _databaseService.GetAllPatientsAsync();
                Patients = new ObservableCollection<Patient>(patients);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnBookAppointment()
        {
            IsLoading = true;
            try
            {
                if (SelectedDoctor == null || SelectedPatient == null || string.IsNullOrWhiteSpace(Reason))
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Виберіть лікаря, пацієнта та вкажіть причину", "OK");
                    return;
                }

                var appointment = new Appointment
                {
                    DoctorId = SelectedDoctor.Id,
                    PatientId = SelectedPatient.Id,
                    AppointmentDate = AppointmentDate,
                    Reason = Reason,
                    Status = "Scheduled"
                };

                await _databaseService.AddAppointmentAsync(appointment);

                SelectedDoctor = null;
                SelectedPatient = null;
                Reason = "";
                AppointmentDate = DateTime.Now.AddDays(1);

                await LoadAppointments();
                await Application.Current.MainPage.DisplayAlert("Успіх", "Запис на прийом зроблено", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Помилка при бронюванні: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnDeleteAppointment(Appointment appointment)
        {
            if (appointment == null) return;

            var confirm = await Application.Current.MainPage.DisplayAlert("Підтвердження",
                "Видалити цей запис на прийом?", "Так", "Ні");

            if (confirm)
            {
                IsLoading = true;
                try
                {
                    await _databaseService.DeleteAppointmentAsync(appointment.Id);
                    await LoadAppointments();
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

        private async Task OnChangeStatus(Appointment appointment)
        {
            if (appointment == null) return;

            var action = await Application.Current.MainPage.DisplayActionSheet(
                "Змінити статус",
                "Скасувати",
                null,
                new[] { "Запланований", "Виконаний", "Скасований" });

            if (action != null && action != "Скасувати")
            {
                IsLoading = true;
                try
                {
                    appointment.Status = action == "Запланований" ? "Scheduled" :
                                        action == "Виконаний" ? "Completed" : "Canceled";

                    await _databaseService.UpdateAppointmentAsync(appointment);
                    await LoadAppointments();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", $"Помилка при оновленні: {ex.Message}", "OK");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
