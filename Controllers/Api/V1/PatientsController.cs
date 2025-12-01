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

        // POST: api/v1/patients
        [HttpPost]
        public async Task<ActionResult<PatientDtoV1>> Create([FromBody] CreatePatientDto dto)
        {
            try
            {
                // Note: This is a simplified version for mobile app
                // In production, you should create proper user account
                var patient = new Patient
                {
                    EmergencyContact = dto.EmergencyContact ?? ""
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                var result = new PatientDtoV1
                {
                    Id = patient.Id,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    EmergencyContact = dto.EmergencyContact ?? ""
                };

                return CreatedAtAction(nameof(GetById), new { id = patient.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating patient", error = ex.Message });
            }
        }

        // PUT: api/v1/patients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            patient.EmergencyContact = dto.EmergencyContact ?? "";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/v1/patients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
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

    public class CreatePatientDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class UpdatePatientDto
    {
        public string? EmergencyContact { get; set; }
    }
}
