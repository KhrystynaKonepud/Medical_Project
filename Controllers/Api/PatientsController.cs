using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // DTO для реєстрації пацієнта з мобільного застосунку
        public class RegisterPatientDto
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public string? EmergencyContact { get; set; }
        }

        // DTO для відповіді - плоска структура для мобільного застосунку
        public class PatientResponseDto
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string Gender { get; set; }
            public string Address { get; set; }
            public string EmergencyContact { get; set; }
        }

        // POST: api/patients/register - для мобільного застосунку
        [HttpPost("register")]
        public async Task<ActionResult<PatientResponseDto>> RegisterPatient([FromBody] RegisterPatientDto dto)
        {
            try
            {
                // Перевіряємо чи email вже існує
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Користувач з таким email вже існує" });
                }

                // Генеруємо унікальний username на основі email
                var username = dto.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 8);

                // Створюємо ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = ParseGender(dto.Gender),
                    Address = dto.Address ?? "",
                    EmailConfirmed = true
                };

                // Генеруємо випадковий пароль
                var defaultPassword = "Patient@" + Guid.NewGuid().ToString().Substring(0, 8);
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Помилка при створенні користувача", errors = result.Errors });
                }

                // Призначаємо роль Patient
                await _userManager.AddToRoleAsync(user, "Patient");

                // Створюємо профіль Patient
                var patient = new Patient
                {
                    UserId = user.Id,
                    EmergencyContact = dto.EmergencyContact ?? ""
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Повертаємо відповідь
                var response = new PatientResponseDto
                {
                    Id = patient.Id,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender ?? "Невказано",
                    Address = dto.Address ?? "",
                    EmergencyContact = dto.EmergencyContact ?? ""
                };

                return CreatedAtAction(nameof(GetById), new { id = patient.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Внутрішня помилка сервера", error = ex.Message });
            }
        }

        // GET: api/patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetAll()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .ToListAsync();

            var response = patients.Select(p => new PatientResponseDto
            {
                Id = p.Id,
                FullName = p.User?.FullName ?? "Невідомо",
                Email = p.User?.Email ?? "",
                PhoneNumber = p.User?.PhoneNumber ?? "",
                DateOfBirth = p.User?.DateOfBirth ?? DateTime.MinValue,
                Gender = p.User?.Gender.HasValue == true ? p.User.Gender.Value.ToString() : "Невказано",
                Address = p.User?.Address ?? "",
                EmergencyContact = p.EmergencyContact ?? ""
            }).ToList();

            return response;
        }

        // GET: api/patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientResponseDto>> GetById(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            var response = new PatientResponseDto
            {
                Id = patient.Id,
                FullName = patient.User?.FullName ?? "Невідомо",
                Email = patient.User?.Email ?? "",
                PhoneNumber = patient.User?.PhoneNumber ?? "",
                DateOfBirth = patient.User?.DateOfBirth ?? DateTime.MinValue,
                Gender = patient.User?.Gender.HasValue == true ? patient.User.Gender.Value.ToString() : "Невказано",
                Address = patient.User?.Address ?? "",
                EmergencyContact = patient.EmergencyContact ?? ""
            };

            return response;
        }

        // DELETE: api/patients/5
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

        private Medical_center.Models.Gender? ParseGender(string? gender)
        {
            if (string.IsNullOrEmpty(gender) || gender == "Невказано")
                return null;

            if (Enum.TryParse<Medical_center.Models.Gender>(gender, out var result))
                return result;

            return null;
        }
    }
}
