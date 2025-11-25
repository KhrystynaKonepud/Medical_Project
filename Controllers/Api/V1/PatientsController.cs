using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace Medical_center.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDtoV1>>> GetAll()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Select(p => new PatientDtoV1
                {
                    Id = p.Id,
                    FullName = p.User.FullName ?? "",
                    Email = p.User.Email ?? "",
                    PhoneNumber = p.User.PhoneNumber ?? "",
                    EmergencyContact = p.EmergencyContact ?? ""
                })
                .ToListAsync();

            return Ok(patients);
        }

        // GET: api/v1/patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDtoV1>> GetById(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.Id == id)
                .Select(p => new PatientDtoV1
                {
                    Id = p.Id,
                    FullName = p.User.FullName ?? "",
                    Email = p.User.Email ?? "",
                    PhoneNumber = p.User.PhoneNumber ?? "",
                    EmergencyContact = p.EmergencyContact ?? ""
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }
    }

    // DTO для версії 1.0 - базова інформація
    public class PatientDtoV1
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyContact { get; set; }
    }
}
