using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // ⬅️ Потрібно
using Medical_center.Data;            // ⬅️ Потрібно

[ApiController]
[Authorize(Roles = "Patient")] // Доступ тільки для пацієнтів
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context; // ⬅️ Додано DbContext

    public PatientController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context; // ⬅️ Додано DbContext
    }

    // Оновлена модель, що приймає EmergencyContact (як ти просив)
    public record CompleteProfileModel(
        string? FullName,
        string? Address,
        DateTime? DateOfBirth,
        Gender? Gender,
        string? EmergencyContact // ⬅️ Додано (це твій "звичайний номер телефону")
    );

    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // ================== Оновлення полів AspNetUsers ==================
        if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName.Trim();
        if (!string.IsNullOrEmpty(model.Address)) user.Address = model.Address.Trim();
        if (model.DateOfBirth.HasValue) user.DateOfBirth = model.DateOfBirth.Value;
        if (model.Gender.HasValue) user.Gender = model.Gender.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // ================== Оновлення / створення Patient (Tbl) ==================
        var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (patientProfile == null)
        {
            // Якщо профілю Patient ще немає, створюємо його
            patientProfile = new Patient
            {
                UserId = user.Id,
                EmergencyContact = model.EmergencyContact
            };
            _context.Patients.Add(patientProfile);
        }
        else
        {
            // Якщо профіль вже є, оновлюємо
            if (!string.IsNullOrEmpty(model.EmergencyContact))
            {
                patientProfile.EmergencyContact = model.EmergencyContact;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Профіль успішно оновлено." });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        // Знаходимо профіль Patient (Tbl)
        var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

        // Перевіряємо, чи заповнені ключові поля
        bool isProfileComplete = user.DateOfBirth.HasValue &&
                                 user.Gender.HasValue &&
                                 !string.IsNullOrEmpty(user.Address) &&
                                 patientProfile != null && // Чи є запис в Patient
                                 !string.IsNullOrEmpty(patientProfile.EmergencyContact);

        return Ok(new
        {
            user.FullName,
            user.Email,
            user.Address,
            user.DateOfBirth,
            user.Gender,
            // Використовуємо EmergencyContact, як ти просив
            PhoneNumber = patientProfile?.EmergencyContact,
            // Додаємо прапор, чи профіль заповнений
            IsProfileComplete = isProfileComplete
        });
    }
}