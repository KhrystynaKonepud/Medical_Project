using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Mobile_Medical_Center.Data;
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

    public class DatabaseService : IDatabaseService
    {
        private ApplicationDbContext _context;

        private static string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "medical_center_debug.log"
        );

        private void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
            }
            catch { }
        }

        public async Task InitializeAsync()
        {
            try
            {
                LogToFile("========== DatabaseService.InitializeAsync STARTED ==========");

                _context = new ApplicationDbContext();
                LogToFile("ApplicationDbContext created successfully");

                await _context.Database.EnsureCreatedAsync();
                LogToFile("Database created/ensured successfully");

                // Check if database has doctors
                var doctorCount = await _context.Doctors.CountAsync();
                LogToFile($"Current doctor count in database: {doctorCount}");

                // Seed initial data if database is empty
                if (doctorCount == 0)
                {
                    LogToFile("Database is empty, seeding initial data...");
                    await SeedInitialDataAsync();

                    doctorCount = await _context.Doctors.CountAsync();
                    LogToFile($"After seeding, doctor count: {doctorCount}");
                }
                else
                {
                    LogToFile("Database already has data, skipping seed");
                }

                LogToFile("========== DatabaseService.InitializeAsync COMPLETED ==========");
            }
            catch (Exception ex)
            {
                LogToFile($"ERROR in DatabaseService.InitializeAsync: {ex.Message}");
                LogToFile($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task SeedInitialDataAsync()
        {
            var doctors = new List<Doctor>
            {
                new Doctor
                {
                    FullName = "Dr. Іван Петренко",
                    Specialization = "Кардіолог",
                    ExperienceYears = 10,
                    Bio = "Досвідчений кардіолог з 10 років практики",
                    Rating = 4.8m
                },
                new Doctor
                {
                    FullName = "Dr. Марія Сидоренко",
                    Specialization = "Невролог",
                    ExperienceYears = 8,
                    Bio = "Спеціаліст по захворюванням нервової системи",
                    Rating = 4.6m
                },
                new Doctor
                {
                    FullName = "Dr. Петро Коваленко",
                    Specialization = "Терапевт",
                    ExperienceYears = 12,
                    Bio = "Загальний лікар з великим досвідом",
                    Rating = 4.7m
                }
            };

            await _context.Doctors.AddRangeAsync(doctors);

            var patients = new List<Patient>
            {
                new Patient
                {
                    FullName = "Анна Іванівна",
                    Email = "anna@example.com",
                    PhoneNumber = "+380501234567",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Gender = "Жіноча",
                    Address = "Київ, вул. Хрещатик, 10",
                    EmergencyContact = "+380701234567"
                },
                new Patient
                {
                    FullName = "Василь Петрович",
                    Email = "vasyl@example.com",
                    PhoneNumber = "+380502345678",
                    DateOfBirth = new DateTime(1990, 3, 20),
                    Gender = "Чоловіча",
                    Address = "Київ, вул. Льва Толстого, 5",
                    EmergencyContact = "+380702345678"
                },
                new Patient
                {
                    FullName = "Олена Миколівна",
                    Email = "olena@example.com",
                    PhoneNumber = "+380503456789",
                    DateOfBirth = new DateTime(1988, 7, 10),
                    Gender = "Жіноча",
                    Address = "Київ, вул. Шевченка, 15",
                    EmergencyContact = "+380703456789"
                }
            };

            await _context.Patients.AddRangeAsync(patients);

            await _context.SaveChangesAsync();

            var doctorsFromDb = await _context.Doctors.ToListAsync();
            var patientsFromDb = await _context.Patients.ToListAsync();

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    DoctorId = doctorsFromDb[0].Id,
                    PatientId = patientsFromDb[0].Id,
                    AppointmentDate = DateTime.Now.AddDays(3),
                    Reason = "Регулярний прийом",
                    Status = "Scheduled"
                },
                new Appointment
                {
                    DoctorId = doctorsFromDb[0].Id,
                    PatientId = patientsFromDb[1].Id,
                    AppointmentDate = DateTime.Now.AddDays(5),
                    Reason = "Консультація",
                    Status = "Scheduled"
                },
                new Appointment
                {
                    DoctorId = doctorsFromDb[1].Id,
                    PatientId = patientsFromDb[2].Id,
                    AppointmentDate = DateTime.Now.AddDays(2),
                    Reason = "Первинний прийом",
                    Status = "Scheduled"
                },
                new Appointment
                {
                    DoctorId = doctorsFromDb[2].Id,
                    PatientId = patientsFromDb[0].Id,
                    AppointmentDate = DateTime.Now.AddDays(7),
                    Reason = "Профілактичний огляд",
                    Status = "Scheduled"
                }
            };

            await _context.Appointments.AddRangeAsync(appointments);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            try
            {
                LogToFile($"GetAllDoctorsAsync called, _context is null: {_context == null}");
                if (_context == null)
                    return new List<Doctor>();
                var doctors = await _context.Doctors.ToListAsync();
                LogToFile($"Loaded {doctors?.Count ?? 0} doctors");
                return doctors ?? new List<Doctor>();
            }
            catch (Exception ex)
            {
                LogToFile($"Error in GetAllDoctorsAsync: {ex.Message}");
                return new List<Doctor>();
            }
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients.ToListAsync();
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments.Include(a => a.Doctor).Include(a => a.Patient).ToListAsync();
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task AddPatientAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Doctor> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors.Include(d => d.Appointments).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            return await _context.Patients.Include(p => p.Appointments).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments.Include(a => a.Doctor).Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .ToListAsync();
        }
    }
}
