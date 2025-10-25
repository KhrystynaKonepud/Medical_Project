using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_center.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Створення лікаря через API
        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync("Doctor"))
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));

            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                UserId = user.Id,
                Specialization = dto.Specialization,
                ExperienceYears = dto.ExperienceYears,
                Bio = dto.Bio
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { doctor.Id });
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    d.Specialization,
                    d.ExperienceYears,
                    d.Bio,
                    FullName = d.User.FullName,
                    Email = d.User.Email
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
            {
                return NotFound(new { message = $"Лікар з ID {id} не знайдений." });
            }

            var user = await _userManager.FindByIdAsync(doctor.UserId);

            // Важливо: Спочатку видаляємо запис лікаря, щоб уникнути проблем із зовнішніми ключами
            // якщо користувач має зв'язки з іншими таблицями, крім Doctors (наприклад, Appointments).

            // Якщо користувач існує (що має бути завжди), спробуйте його видалити
            if (user != null)
            {
                // КРОК 2: Видаляємо запис лікаря з таблиці Doctors
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync(); // Зберігаємо зміни перед видаленням користувача Identity

                // КРОК 3: Видаляємо користувача Identity
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    // Якщо видалення користувача Identity не вдалося
                    // Повертаємо 400 Bad Request із повідомленням, яке може прочитати фронтенд
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new { message = "Помилка Identity при видаленні користувача.", errors = errors });
                }
            }
            else
            {
                // КРОК 2b: Якщо користувач ApplicationUser не знайдений, видаляємо лише запис лікаря
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }

            // Успіх: Повертаємо HTTP 204 (No Content)
            return NoContent();
        }

        // Оновити дані лікаря
        [HttpPut("doctors/{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Недійсні дані або невідповідність ID." });

            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
                return NotFound(new { message = $"Лікар з ID {id} не знайдений." });

            var user = doctor.User;
            if (user != null)
            {
                user.FullName = dto.FullName;

                if (user.Email != dto.Email)
                {
                    var token = await _userManager.GenerateChangeEmailTokenAsync(user, dto.Email);
                    var emailChangeResult = await _userManager.ChangeEmailAsync(user, dto.Email, token);
                    if (!emailChangeResult.Succeeded)
                        return BadRequest(new { message = "Помилка при зміні Email.", errors = emailChangeResult.Errors });
                    user.UserName = dto.Email;
                }

                // Оновлюємо пароль тільки якщо він переданий
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (!removePasswordResult.Succeeded)
                        return BadRequest(new { message = "Не вдалося видалити старий пароль.", errors = removePasswordResult.Errors });

                    var addPasswordResult = await _userManager.AddPasswordAsync(user, dto.Password);
                    if (!addPasswordResult.Succeeded)
                        return BadRequest(new { message = "Не вдалося додати новий пароль.", errors = addPasswordResult.Errors });
                }

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                    return BadRequest(new { message = "Помилка при оновленні користувача.", errors = updateUserResult.Errors });
            }

            doctor.Specialization = dto.Specialization;
            doctor.ExperienceYears = dto.ExperienceYears;
            doctor.Bio = dto.Bio;

            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Дані лікаря успішно оновлено." });
        }
    }

    public class DoctorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string Bio { get; set; }
    }
}