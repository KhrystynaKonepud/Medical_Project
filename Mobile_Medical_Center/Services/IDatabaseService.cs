using Mobile_Medical_Center.Models;

namespace Mobile_Medical_Center.Services
{
    public interface IDatabaseService
    {
        Task InitializeAsync();
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task<List<Patient>> GetAllPatientsAsync();
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task AddDoctorAsync(Doctor doctor);
        Task AddPatientAsync(Patient patient);
        Task AddAppointmentAsync(Appointment appointment);
        Task UpdateDoctorAsync(Doctor doctor);
        Task UpdatePatientAsync(Patient patient);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task DeleteDoctorAsync(int id);
        Task DeletePatientAsync(int id);
        Task DeleteAppointmentAsync(int id);
        Task<Doctor> GetDoctorByIdAsync(int id);
        Task<Patient> GetPatientByIdAsync(int id);
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId);
    }
}
