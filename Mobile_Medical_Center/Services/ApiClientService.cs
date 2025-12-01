using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Mobile_Medical_Center.Models;

namespace Mobile_Medical_Center.Services
{
    public class ApiClientService : IDatabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private const string API_BASE_URL = "https://localhost:7263";

        private static string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "medical_center_api_debug.log"
        );

        private void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
            }
            catch { }
        }

        public ApiClientService(IAuthService authService)
        {
            _authService = authService;

            var handler = new HttpClientHandler();

#if DEBUG
            // For development with self-signed certificates only
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (message?.RequestUri?.Host == "localhost" || message?.RequestUri?.Host == "127.0.0.1")
                {
                    return true;
                }
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#else
            handler.ServerCertificateCustomValidationCallback = null;
#endif

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(API_BASE_URL),
                Timeout = TimeSpan.FromSeconds(30)
            };

            LogToFile("ApiClientService initialized");
        }

        private void AddAuthorizationHeader()
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task InitializeAsync()
        {
            // No local database to initialize - thin client pattern
            LogToFile("ApiClientService initialized (thin client - no local DB)");
            await Task.CompletedTask;
        }

        // DOCTORS
        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile("Getting all doctors from API...");

                var response = await _httpClient.GetAsync("/api/doctors");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var doctors = JsonSerializer.Deserialize<List<Doctor>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Doctor>();

                LogToFile($"Loaded {doctors.Count} doctors from API");
                return doctors;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting doctors: {ex.Message}");
                Debug.WriteLine($"Error getting doctors: {ex.Message}");
                return new List<Doctor>();
            }
        }

        public async Task<Doctor> GetDoctorByIdAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Getting doctor {id} from API...");

                var response = await _httpClient.GetAsync($"/api/doctors/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var doctor = JsonSerializer.Deserialize<Doctor>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return doctor;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting doctor {id}: {ex.Message}");
                Debug.WriteLine($"Error getting doctor: {ex.Message}");
                return null;
            }
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Adding doctor {doctor.FullName} via API...");

                // Використовуємо спеціальний endpoint для мобільного застосунку
                var registerDto = new
                {
                    FullName = doctor.FullName,
                    Email = $"{doctor.FullName.Replace(" ", "").ToLower()}@medicalcenter.com", // Генеруємо email
                    Specialization = doctor.Specialization,
                    ExperienceYears = doctor.ExperienceYears,
                    Bio = doctor.Bio,
                    Rating = doctor.Rating
                };

                var json = JsonSerializer.Serialize(registerDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                LogToFile($"Sending request to /api/doctors/register with data: {json}");
                var response = await _httpClient.PostAsync("/api/doctors/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogToFile($"Error response: {errorContent}");
                    throw new Exception($"Server returned {response.StatusCode}: {errorContent}");
                }

                LogToFile("Doctor added successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error adding doctor: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Error adding doctor: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Updating doctor {doctor.Id} via API...");

                var json = JsonSerializer.Serialize(doctor);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/doctors/{doctor.Id}", content);
                response.EnsureSuccessStatusCode();

                LogToFile("Doctor updated successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error updating doctor: {ex.Message}");
                Debug.WriteLine($"Error updating doctor: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDoctorAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Deleting doctor {id} via API...");

                var response = await _httpClient.DeleteAsync($"/api/doctors/{id}");
                response.EnsureSuccessStatusCode();

                LogToFile("Doctor deleted successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error deleting doctor: {ex.Message}");
                Debug.WriteLine($"Error deleting doctor: {ex.Message}");
                throw;
            }
        }

        // PATIENTS
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile("Getting all patients from API...");

                var response = await _httpClient.GetAsync("/api/patients");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                LogToFile($"API Response: {content}");

                var patients = JsonSerializer.Deserialize<List<Patient>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Patient>();

                LogToFile($"Loaded {patients.Count} patients from API");
                return patients;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting patients: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Error getting patients: {ex.Message}");
                return new List<Patient>();
            }
        }

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Getting patient {id} from API...");

                var response = await _httpClient.GetAsync($"/api/patients/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var patient = JsonSerializer.Deserialize<Patient>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return patient;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting patient {id}: {ex.Message}");
                Debug.WriteLine($"Error getting patient: {ex.Message}");
                return null;
            }
        }

        public async Task AddPatientAsync(Patient patient)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Adding patient {patient.FullName} via API...");

                var createDto = new
                {
                    FullName = patient.FullName,
                    Email = patient.Email,
                    PhoneNumber = patient.PhoneNumber,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    Address = patient.Address,
                    EmergencyContact = patient.EmergencyContact
                };

                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                LogToFile($"Sending request to /api/patients/register with data: {json}");
                var response = await _httpClient.PostAsync("/api/patients/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogToFile($"Error response: {errorContent}");
                    throw new Exception($"Server returned {response.StatusCode}: {errorContent}");
                }

                LogToFile("Patient added successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error adding patient: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Error adding patient: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Updating patient {patient.Id} via API...");

                var json = JsonSerializer.Serialize(patient);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/patients/{patient.Id}", content);
                response.EnsureSuccessStatusCode();

                LogToFile("Patient updated successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error updating patient: {ex.Message}");
                Debug.WriteLine($"Error updating patient: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePatientAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Deleting patient {id} via API...");

                var response = await _httpClient.DeleteAsync($"/api/patients/{id}");
                response.EnsureSuccessStatusCode();

                LogToFile("Patient deleted successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error deleting patient: {ex.Message}");
                Debug.WriteLine($"Error deleting patient: {ex.Message}");
                throw;
            }
        }

        // APPOINTMENTS
        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile("Getting all appointments from API...");

                var response = await _httpClient.GetAsync("/api/appointments");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var appointments = JsonSerializer.Deserialize<List<Appointment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Appointment>();

                LogToFile($"Loaded {appointments.Count} appointments from API");
                return appointments;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting appointments: {ex.Message}");
                Debug.WriteLine($"Error getting appointments: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Getting appointment {id} from API...");

                var response = await _httpClient.GetAsync($"/api/appointments/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var appointment = JsonSerializer.Deserialize<Appointment>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return appointment;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting appointment {id}: {ex.Message}");
                Debug.WriteLine($"Error getting appointment: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Getting appointments for doctor {doctorId} from API...");

                var response = await _httpClient.GetAsync($"/api/appointments/doctor/{doctorId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var appointments = JsonSerializer.Deserialize<List<Appointment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Appointment>();

                return appointments;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting appointments for doctor: {ex.Message}");
                Debug.WriteLine($"Error getting appointments for doctor: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Getting appointments for patient {patientId} from API...");

                var response = await _httpClient.GetAsync($"/api/appointments/patient/{patientId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var appointments = JsonSerializer.Deserialize<List<Appointment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Appointment>();

                return appointments;
            }
            catch (Exception ex)
            {
                LogToFile($"Error getting appointments for patient: {ex.Message}");
                Debug.WriteLine($"Error getting appointments for patient: {ex.Message}");
                return new List<Appointment>();
            }
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Adding appointment via API...");

                var json = JsonSerializer.Serialize(appointment);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/appointments", content);
                response.EnsureSuccessStatusCode();

                LogToFile("Appointment added successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error adding appointment: {ex.Message}");
                Debug.WriteLine($"Error adding appointment: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Updating appointment {appointment.Id} via API...");

                var json = JsonSerializer.Serialize(appointment);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/appointments/{appointment.Id}", content);
                response.EnsureSuccessStatusCode();

                LogToFile("Appointment updated successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error updating appointment: {ex.Message}");
                Debug.WriteLine($"Error updating appointment: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                LogToFile($"Deleting appointment {id} via API...");

                var response = await _httpClient.DeleteAsync($"/api/appointments/{id}");
                response.EnsureSuccessStatusCode();

                LogToFile("Appointment deleted successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error deleting appointment: {ex.Message}");
                Debug.WriteLine($"Error deleting appointment: {ex.Message}");
                throw;
            }
        }
    }
}
