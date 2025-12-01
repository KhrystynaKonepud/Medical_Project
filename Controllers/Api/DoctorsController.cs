using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Medical_center.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // DTO для реєстрації лікаря з мобільного застосунку
        public class RegisterDoctorDto
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Specialization { get; set; }
            public int ExperienceYears { get; set; }
            public string? Bio { get; set; }
            public decimal? Rating { get; set; }
        }

        // DTO для відповіді - плоска структура для мобільного застосунку
        public class DoctorResponseDto
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Specialization { get; set; }
            public int ExperienceYears { get; set; }
            public string Bio { get; set; }
            public decimal Rating { get; set; }
        }

        // POST: api/doctors/register - для мобільного застосунку
        [HttpPost("register")]
        public async Task<ActionResult<DoctorResponseDto>> RegisterDoctor([FromBody] RegisterDoctorDto dto)
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
                    FullName = dto.FullName,
                    EmailConfirmed = true // Автоматично підтверджуємо email для мобільного застосунку
                };

                // Генеруємо випадковий пароль (можна також отримувати з DTO якщо потрібно)
                var defaultPassword = "Doctor@" + Guid.NewGuid().ToString().Substring(0, 8);
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Помилка при створенні користувача", errors = result.Errors });
                }

                // Призначаємо роль Doctor
                await _userManager.AddToRoleAsync(user, "Doctor");

                // Створюємо профіль Doctor
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = dto.Specialization,
                    ExperienceYears = dto.ExperienceYears,
                    Bio = dto.Bio ?? "Новий лікар",
                    Rating = dto.Rating ?? 5.0m
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                // Повертаємо відповідь у форматі, сумісному з мобільним додатком
                var response = new DoctorResponseDto
                {
                    Id = doctor.Id,
                    FullName = dto.FullName,
                    Specialization = doctor.Specialization,
                    ExperienceYears = doctor.ExperienceYears,
                    Bio = doctor.Bio,
                    Rating = doctor.Rating ?? 5.0m
                };

                return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Внутрішня помилка сервера", error = ex.Message });
            }
        }

        // GET: api/doctors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetAll()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();

            var response = doctors.Select(d => new DoctorResponseDto
            {
                Id = d.Id,
                FullName = d.User?.FullName ?? "Невідомо",
                Specialization = d.Specialization,
                ExperienceYears = d.ExperienceYears,
                Bio = d.Bio ?? "",
                Rating = d.Rating ?? 0m
            }).ToList();

            return response;
        }

        // GET: api/doctors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorResponseDto>> GetById(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            var response = new DoctorResponseDto
            {
                Id = doctor.Id,
                FullName = doctor.User?.FullName ?? "Невідомо",
                Specialization = doctor.Specialization,
                ExperienceYears = doctor.ExperienceYears,
                Bio = doctor.Bio ?? "",
                Rating = doctor.Rating ?? 0m
            };

            return response;
        }

        // POST: api/doctors
        [HttpPost]
        public async Task<ActionResult<Doctor>> Create(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
        }

        // PUT: api/doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return BadRequest();
            }

            _context.Entry(doctor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Doctors.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
