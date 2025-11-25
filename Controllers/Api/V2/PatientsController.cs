using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace Medical_center.Controllers.Api.V2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [AllowAnonymous]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/v2/patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDtoV2>>> GetAll()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .Include(p => p.MedicalRecords)
                .Include(p => p.Prescriptions)
                .Include(p => p.TestResults)
                .Include(p => p.Vaccinations)
                .Include(p => p.Reviews)
                .Select(p => new PatientDtoV2
                {
                    Id = p.Id,
                    FullName = p.User.FullName ?? "",
                    Email = p.User.Email ?? "",
                    PhoneNumber = p.User.PhoneNumber ?? "",
                    EmergencyContact = p.EmergencyContact ?? "",
                    // Нова інформація в V2
                    Statistics = new PatientStatistics
                    {
                        TotalAppointments = p.Appointments.Count,
                        TotalMedicalRecords = p.MedicalRecords.Count,
                        TotalPrescriptions = p.Prescriptions.Count,
                        TotalTestResults = p.TestResults.Count,
                        TotalVaccinations = p.Vaccinations.Count,
                        TotalReviews = p.Reviews.Count,
                        LastAppointmentDate = p.Appointments
                            .OrderByDescending(a => a.Date)
                            .Select(a => (DateTime?)a.Date)
                            .FirstOrDefault()
                    }
                })
                .ToListAsync();

            return Ok(patients);
        }

        // GET: api/v2/patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDtoV2>> GetById(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .Include(p => p.MedicalRecords)
                .Include(p => p.Prescriptions)
                .Include(p => p.TestResults)
                .Include(p => p.Vaccinations)
                .Include(p => p.Reviews)
                .Where(p => p.Id == id)
                .Select(p => new PatientDtoV2
                {
                    Id = p.Id,
                    FullName = p.User.FullName ?? "",
                    Email = p.User.Email ?? "",
                    PhoneNumber = p.User.PhoneNumber ?? "",
                    EmergencyContact = p.EmergencyContact ?? "",
                    Statistics = new PatientStatistics
                    {
                        TotalAppointments = p.Appointments.Count,
                        TotalMedicalRecords = p.MedicalRecords.Count,
                        TotalPrescriptions = p.Prescriptions.Count,
                        TotalTestResults = p.TestResults.Count,
                        TotalVaccinations = p.Vaccinations.Count,
                        TotalReviews = p.Reviews.Count,
                        LastAppointmentDate = p.Appointments
                            .OrderByDescending(a => a.Date)
                            .Select(a => (DateTime?)a.Date)
                            .FirstOrDefault()
                    }
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }
    }

    // DTO для версії 2.0 - розширена інформація зі статистикою
    public class PatientDtoV2
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyContact { get; set; }
        public PatientStatistics Statistics { get; set; }
    }

    public class PatientStatistics
    {
        public int TotalAppointments { get; set; }
        public int TotalMedicalRecords { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TotalTestResults { get; set; }
        public int TotalVaccinations { get; set; }
        public int TotalReviews { get; set; }
        public DateTime? LastAppointmentDate { get; set; }
    }
}
