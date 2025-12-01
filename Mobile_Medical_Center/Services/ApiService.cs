using Mobile_Medical_Center.Models;
using System.Net.Http.Json;

namespace Mobile_Medical_Center.Services
{
    public class ApiService : IDatabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7263/api";

        // API endpoints (versioned for patients, plain for others)
        private const string DoctorsEndpoint = "/doctors";
        private const string PatientsEndpoint = "/v1/patients";
        private const string AppointmentsEndpoint = "/appointments";

        private static string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "medical_center_debug.log"
        );

        private void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [ApiService] {message}\n");
            }
            catch { }
        }

        public ApiService()
        {
            var handler = new HttpClientHandler();
            // Игнорируем ошибки SSL сертификата для локального https
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            _httpClient = new HttpClient(handler);
            LogToFile("ApiService initialized");
        }

        public async Task InitializeAsync()
        {
            try
            {
                LogToFile("InitializeAsync called - verifying connection to API");
                // Попытаемся получить список докторов для проверки соединения
                await GetAllDoctorsAsync();
                LogToFile("✓ Connection to API verified successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"✗ Error verifying API connection: {ex.Message}");
                throw;
            }
        }

        // ===== DOCTORS =====
        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            try
            {
                LogToFile("GetAllDoctorsAsync called");
                var response = await _httpClient.GetAsync($"{_baseUrl}{DoctorsEndpoint}");
                response.EnsureSuccessStatusCode();
                var doctors = await response.Content.ReadFromJsonAsync<List<Doctor>>();
                LogToFile($"Loaded {doctors?.Count ?? 0} doctors from API");
                return doctors ?? new List<Doctor>();
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAllDoctorsAsync: {ex.Message}");
                return new List<Doctor>();
            }
        }

        public async Task<Doctor> GetDoctorByIdAsync(int id)
        {
            try
            {
                LogToFile($"GetDoctorByIdAsync called with id={id}");
                var response = await _httpClient.GetAsync($"{_baseUrl}{DoctorsEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                var doctor = await response.Content.ReadFromJsonAsync<Doctor>();
                return doctor;
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetDoctorByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            try
            {
                LogToFile($"AddDoctorAsync called: {doctor.FullName}");
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}{DoctorsEndpoint}", doctor);
                response.EnsureSuccessStatusCode();
                LogToFile("Doctor added successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in AddDoctorAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            try
            {
                LogToFile($"UpdateDoctorAsync called: {doctor.FullName}");
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}{DoctorsEndpoint}/{doctor.Id}", doctor);
                response.EnsureSuccessStatusCode();
                LogToFile("Doctor updated successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in UpdateDoctorAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDoctorAsync(int id)
        {
            try
            {
                LogToFile($"DeleteDoctorAsync called with id={id}");
                var response = await _httpClient.DeleteAsync($"{_baseUrl}{DoctorsEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                LogToFile("Doctor deleted successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in DeleteDoctorAsync: {ex.Message}");
                throw;
            }
        }

        // ===== PATIENTS =====
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            try
            {
                LogToFile("GetAllPatientsAsync called");
                var response = await _httpClient.GetAsync($"{_baseUrl}{PatientsEndpoint}");
                response.EnsureSuccessStatusCode();
                var patients = await response.Content.ReadFromJsonAsync<List<Patient>>();
                LogToFile($"Loaded {patients?.Count ?? 0} patients from API");
                return patients ?? new List<Patient>();
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAllPatientsAsync: {ex.Message}");
                return new List<Patient>();
            }
        }

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            try
            {
                LogToFile($"GetPatientByIdAsync called with id={id}");
                var response = await _httpClient.GetAsync($"{_baseUrl}{PatientsEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                var patient = await response.Content.ReadFromJsonAsync<Patient>();
                return patient;
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetPatientByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task AddPatientAsync(Patient patient)
        {
            try
            {
                LogToFile($"AddPatientAsync called: {patient.FullName}");
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}{PatientsEndpoint}", patient);
                response.EnsureSuccessStatusCode();
                LogToFile("Patient added successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in AddPatientAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            try
            {
                LogToFile($"UpdatePatientAsync called: {patient.FullName}");
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}{PatientsEndpoint}/{patient.Id}", patient);
                response.EnsureSuccessStatusCode();
                LogToFile("Patient updated successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in UpdatePatientAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePatientAsync(int id)
        {
            try
            {
                LogToFile($"DeletePatientAsync called with id={id}");
                var response = await _httpClient.DeleteAsync($"{_baseUrl}{PatientsEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                LogToFile("Patient deleted successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in DeletePatientAsync: {ex.Message}");
                throw;
            }
        }

        // ===== APPOINTMENTS =====
        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                LogToFile("GetAllAppointmentsAsync called");
                var response = await _httpClient.GetAsync($"{_baseUrl}{AppointmentsEndpoint}");
                response.EnsureSuccessStatusCode();
                var appointments = await response.Content.ReadFromJsonAsync<List<Appointment>>();
                LogToFile($"Loaded {appointments?.Count ?? 0} appointments from API");
                return appointments ?? new List<Appointment>();
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAllAppointmentsAsync: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            try
            {
                LogToFile($"GetAppointmentByIdAsync called with id={id}");
                var response = await _httpClient.GetAsync($"{_baseUrl}{AppointmentsEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                var appointment = await response.Content.ReadFromJsonAsync<Appointment>();
                return appointment;
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAppointmentByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            try
            {
                LogToFile($"GetAppointmentsByDoctorAsync called with doctorId={doctorId}");
                var allAppointments = await GetAllAppointmentsAsync();
                var result = allAppointments.Where(a => a.DoctorId == doctorId).ToList();
                LogToFile($"Found {result.Count} appointments for doctor {doctorId}");
                return result;
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAppointmentsByDoctorAsync: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            try
            {
                LogToFile($"GetAppointmentsByPatientAsync called with patientId={patientId}");
                var allAppointments = await GetAllAppointmentsAsync();
                var result = allAppointments.Where(a => a.PatientId == patientId).ToList();
                LogToFile($"Found {result.Count} appointments for patient {patientId}");
                return result;
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAppointmentsByPatientAsync: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            try
            {
                LogToFile($"AddAppointmentAsync called: Doctor={appointment.DoctorId}, Patient={appointment.PatientId}");
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/appointments", appointment);
                response.EnsureSuccessStatusCode();
                LogToFile("Appointment added successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in AddAppointmentAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    LogToFile($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                LogToFile($"UpdateAppointmentAsync called with id={appointment.Id}");
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/appointments/{appointment.Id}", appointment);
                response.EnsureSuccessStatusCode();
                LogToFile("Appointment updated successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in UpdateAppointmentAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            try
            {
                LogToFile($"DeleteAppointmentAsync called with id={id}");
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/appointments/{id}");
                response.EnsureSuccessStatusCode();
                LogToFile("Appointment deleted successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in DeleteAppointmentAsync: {ex.Message}");
                throw;
            }
        }
    }
}

