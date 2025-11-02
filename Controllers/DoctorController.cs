using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Medical_center.Data;

[ApiController]
[Authorize(Roles = "Doctor")]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public DoctorController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // =================== Моделі ===================
    public record CompleteDoctorProfileModel(
        string? FullName,
        string? Address,
        DateTime? DateOfBirth,
        Gender? Gender,
        string? PhoneNumber,
        string? Specialization,
        int? ExperienceYears,
        string? Bio
    );

    public record AvailabilityModel(
        DateTime AvailableDate,
        TimeSpan StartTime,
        int? AppointmentDurationMinutes
    );

    // =================== ОНОВЛЕННЯ ПРОФІЛЮ ===================
    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteDoctorProfileModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // ================== Оновлення полів AspNetUsers ==================
        if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName.Trim();
        if (!string.IsNullOrEmpty(model.Address)) user.Address = model.Address.Trim();
        if (model.DateOfBirth.HasValue) user.DateOfBirth = model.DateOfBirth.Value;
        if (model.Gender.HasValue) user.Gender = model.Gender.Value;
        if (!string.IsNullOrEmpty(model.PhoneNumber)) user.PhoneNumber = model.PhoneNumber.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // ================== Оновлення / створення DoctorProfile ==================
        var doctorProfile = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctorProfile == null)
        {
            doctorProfile = new Doctor
            {
                UserId = user.Id,
                Specialization = model.Specialization,
                ExperienceYears = model.ExperienceYears ?? 0,
                Bio = model.Bio
            };
            _context.Doctors.Add(doctorProfile);
        }
        else
        {
            if (!string.IsNullOrEmpty(model.Specialization)) doctorProfile.Specialization = model.Specialization;
            if (model.ExperienceYears.HasValue) doctorProfile.ExperienceYears = model.ExperienceYears.Value;
            if (!string.IsNullOrEmpty(model.Bio)) doctorProfile.Bio = model.Bio;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Профіль лікаря успішно оновлено." });
    }

    // =================== ОТРИМАННЯ ПРОФІЛЮ ===================
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _userManager.GetUserId(User);

        var doctorProfile = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        return Ok(new
        {
            user.FullName,
            user.Email,
            user.Address,
            user.DateOfBirth,
            user.Gender,
            user.PhoneNumber,
            Specialization = doctorProfile?.Specialization,
            ExperienceYears = doctorProfile?.ExperienceYears ?? 0,
            Bio = doctorProfile?.Bio,
            Rating = doctorProfile?.Rating
        });
    }

    // =================== ДОСТУПНІСТЬ ЛІКАРЯ ===================
    [HttpPost("availability/add")]
    public async Task<IActionResult> AddAvailability([FromBody] AvailabilityModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Знаходимо профіль лікаря
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        try
        {
            var availability = new DoctorAvailability
            {
                DoctorId = doctor.Id,
                AvailableDate = model.AvailableDate.Date,
                StartTime = model.StartTime,
                AppointmentDurationMinutes = model.AppointmentDurationMinutes ?? 60,
                IsActive = true
            };

            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Слот доступності додано." });
        }
        catch (Exception ex)
        {
            // Повертаємо реальну помилку для дебагу
            return BadRequest(new { message = "Сталася помилка при додаванні слоту", error = ex.Message });
        }
    }

    [HttpGet("availability/all")]
    public async Task<IActionResult> GetAvailabilities()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var availabilities = await _context.DoctorAvailabilities
            .Where(a => a.DoctorId == doctor.Id && a.IsActive)
            .OrderBy(a => a.AvailableDate).ThenBy(a => a.StartTime)
            .Select(a => new
            {
                a.Id,
                a.AvailableDate,
                a.StartTime,
                a.AppointmentDurationMinutes,
                a.IsActive
            })
            .ToListAsync();

        return Ok(availabilities);
    }

    [HttpPost("availability/deactivate/{id}")]
    public async Task<IActionResult> DeactivateSlot(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);
        if (slot == null) return NotFound(new { message = "Слот не знайдено." });

        slot.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Слот деактивовано." });
    }
}