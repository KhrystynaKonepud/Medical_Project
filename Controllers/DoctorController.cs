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

    // =================== Моделі для записів ===================
    public record AppointmentDto(
        int Id,
        DateTime Date,
        string Reason,
        string Status,
        int PatientId,
        string PatientName,
        string PatientPhone,
        string PatientEmail,
        bool HasMedicalRecord
    );

    public record CreateMedicalRecordDto(
        int AppointmentId,
        int PatientId,
        string Title,
        string Diagnosis,
        string Treatment,
        string? Notes
    );

    public record UpdateMedicalRecordDto(
    string Diagnosis,
    string Treatment,
    string? Notes
    );

    public record PrescriptionDto(
    int Id,
    DateTime DateIssued,
    string Medication,
    string Dosage,
    string? Notes,
    int PatientId,
    string PatientName,
    int DoctorId
);

    public record CreatePrescriptionDto(
        int PatientId,
        string Medication,
        string Dosage,
        string? Notes
    );

    public record PatientInfoDto(
        int Id,
        string Name,
        string Phone,
        string Email,
        DateTime? LastAppointment,
        int CompletedAppointmentsCount
    );

    // =================== ОНОВЛЕННЯ ПРОФІЛЮ ===================
    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteDoctorProfileModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName.Trim();
        if (!string.IsNullOrEmpty(model.Address)) user.Address = model.Address.Trim();
        if (model.DateOfBirth.HasValue) user.DateOfBirth = model.DateOfBirth.Value;
        if (model.Gender.HasValue) user.Gender = model.Gender.Value;
        if (!string.IsNullOrEmpty(model.PhoneNumber)) user.PhoneNumber = model.PhoneNumber.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

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

    // =================== 4. ПЕРЕГЛЯД ЗАПИСІВ ===================

    /// <summary>
    /// Отримати всі записи на прийом для поточного лікаря
    /// </summary>
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        // Базовий запит
        var query = _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctor.Id);

        // Фільтрація по статусу
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var appointmentStatus))
        {
            query = query.Where(a => a.Status == appointmentStatus);
        }

        // Фільтрація по даті від
        if (dateFrom.HasValue)
        {
            query = query.Where(a => a.Date >= dateFrom.Value.Date);
        }

        // Фільтрація по даті до
        if (dateTo.HasValue)
        {
            query = query.Where(a => a.Date <= dateTo.Value.Date.AddDays(1).AddTicks(-1));
        }

        var appointments = await query
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        // Перевіряємо наявність медичних записів
        // Використовуємо PatientId та перевіряємо Title на формат "Appointment_{id}"
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var medicalRecordsMap = await _context.MedicalRecords
            .Where(mr => appointmentIds.Select(aid => $"Appointment_{aid}").Contains(mr.Title))
            .Select(mr => mr.Title)
            .ToListAsync();

        var result = appointments.Select(a => new AppointmentDto(
            a.Id,
            a.Date,
            a.Reason,
            a.Status.ToString(),
            a.PatientId,
            a.Patient.User.FullName ?? "Не вказано",
            a.Patient.User.PhoneNumber ?? "Не вказано",
            a.Patient.User.Email ?? "Не вказано",
            medicalRecordsMap.Contains($"Appointment_{a.Id}")
        )).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Отримати конкретний запис на прийом за ID
    /// </summary>
    [HttpGet("appointments/{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Where(a => a.Id == id && a.DoctorId == doctor.Id)
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return NotFound(new { message = "Запис не знайдено або у вас немає доступу до нього." });
        }

        // Перевіряємо наявність медичного запису
        var hasMedicalRecord = await _context.MedicalRecords
            .AnyAsync(mr => mr.Title == $"Appointment_{id}" && mr.PatientId == appointment.PatientId);

        var result = new AppointmentDto(
            appointment.Id,
            appointment.Date,
            appointment.Reason,
            appointment.Status.ToString(),
            appointment.PatientId,
            appointment.Patient.User.FullName ?? "Не вказано",
            appointment.Patient.User.PhoneNumber ?? "Не вказано",
            appointment.Patient.User.Email ?? "Не вказано",
            hasMedicalRecord
        );

        return Ok(result);
    }

    /// <summary>
    /// Отримати статистику по записах
    /// </summary>
    [HttpGet("appointments/statistics")]
    public async Task<IActionResult> GetAppointmentStatistics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var today = DateTime.Today;
        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
        var thisMonthStart = new DateTime(today.Year, today.Month, 1);

        var statistics = new
        {
            TotalAppointments = await _context.Appointments.CountAsync(a => a.DoctorId == doctor.Id),
            ScheduledAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Scheduled),
            CompletedAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed),
            CanceledAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Canceled),
            TodayAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Date.Date == today),
            ThisWeekAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Date >= thisWeekStart),
            ThisMonthAppointments = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctor.Id && a.Date >= thisMonthStart)
        };

        return Ok(statistics);
    }

    // =================== 5. СТВОРЕННЯ МЕДИЧНОГО ЗАПИСУ ===================

    // ЗАМІНІТЬ метод CreateMedicalRecord в DoctorController.cs на цей:

    /// <summary>
    /// Створити медичний запис для пацієнта
    /// Автоматично змінює статус appointment на Completed
    /// </summary>
    [HttpPost("medical-records")]
    public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null)
            return BadRequest(new { message = "Профіль лікаря не знайдено." });

        // Перевіряємо, чи існує прийом
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.DoctorId == doctor.Id); // ← ДОДАНО перевірку лікаря

        if (appointment == null)
            return NotFound(new { message = "Прийом не знайдено або у вас немає доступу до нього." });

        // Перевіряємо, що пацієнт збігається
        if (appointment.PatientId != dto.PatientId)
            return BadRequest(new { message = "Пацієнт не відповідає запису прийому." });

        // Перевіряємо чи вже існує медичний запис для цього прийому
        var existingRecord = await _context.MedicalRecords
            .FirstOrDefaultAsync(mr => mr.Title == $"Appointment_{dto.AppointmentId}");

        if (existingRecord != null)
            return BadRequest(new { message = "Медичний запис для цього прийому вже існує." });

        // Створюємо медичний запис
        var record = new MedicalRecord
        {
            PatientId = dto.PatientId,
            Title = dto.Title,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            RecordDate = DateTime.Now
        };

        _context.MedicalRecords.Add(record);

        // ✅ ГОЛОВНЕ ВИПРАВЛЕННЯ: Змінюємо статус appointment на Completed
        appointment.Status = AppointmentStatus.Completed;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Медичний запис створено та статус прийому оновлено.",
            recordId = record.Id,
            appointmentId = appointment.Id,
            newStatus = appointment.Status.ToString()
        });
    }

    /// <summary>
    /// Отримати медичний запис за ID appointment
    /// </summary>
    [HttpGet("medical-records/appointment/{appointmentId}")]
    public async Task<IActionResult> GetMedicalRecordByAppointment(int appointmentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        // Перевіряємо що appointment належить цьому лікарю
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

        if (appointment == null)
        {
            return NotFound(new { message = "Запис не знайдено." });
        }

        // Знаходимо медичний запис
        var medicalRecord = await _context.MedicalRecords
            .Include(mr => mr.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(mr => mr.Title == $"Appointment_{appointmentId}");

        if (medicalRecord == null)
        {
            return NotFound(new { message = "Медичний запис не знайдено." });
        }

        return Ok(new
        {
            medicalRecord.Id,
            AppointmentId = appointmentId,
            medicalRecord.PatientId,
            PatientName = medicalRecord.Patient.User.FullName,
            medicalRecord.Title,
            medicalRecord.Diagnosis,
            medicalRecord.Treatment,
            medicalRecord.Notes,
            medicalRecord.RecordDate
        });
    }

    [HttpPut("medical-records/{id}")]
    public async Task<IActionResult> UpdateMedicalRecord(int id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var record = await _context.MedicalRecords
            .Include(r => r.Patient)
            .Include(r => r.Patient.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record == null)
            return NotFound(new { message = "Медичний запис не знайдено." });

        // Редагувати можна тільки той запис, який належить лікарю
        var appointmentId = int.Parse(record.Title.Replace("Appointment_", ""));
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

        if (appointment == null)
            return Forbid(); // Немає доступу

        // Оновлення
        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Медичний запис успішно оновлено." });
    }

    // =================== 6. ВИПИСКА РЕЦЕПТУ ===================

    [HttpGet("patients")]
    public async Task<IActionResult> GetMyPatients()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

            // Крок 1: Завантажуємо ВСІ appointments в пам'ять
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .ToListAsync();

            // Крок 2: Групуємо в пам'яті
            var patients = appointments
                .Where(a => a.Patient != null && a.Patient.User != null) // Фільтруємо тільки валідні
                .GroupBy(a => a.PatientId)
                .Select(g => new PatientInfoDto(
                    g.Key,
                    g.First().Patient.User?.FullName ?? "Невідомий пацієнт",
                    g.First().Patient.User?.PhoneNumber ?? "-",
                    g.First().Patient.User?.Email ?? "-",
                    g.Max(a => a.Date),
                    g.Count()
                ))
                .OrderByDescending(p => p.LastAppointment)
                .ToList();

            return Ok(patients);
        }
        catch (Exception ex)
        {
            // Детальне логування помилки
            Console.WriteLine($"Error in GetMyPatients: {ex.Message}");
            Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            return StatusCode(500, new
            {
                message = "Помилка при завантаженні пацієнтів",
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Отримати всі рецепти лікаря
    /// </summary>
    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions([FromQuery] int? patientId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var query = _context.Prescriptions
            .Include(p => p.Patient)
            .ThenInclude(pt => pt.User)
            .Where(p => p.DoctorId == doctor.Id);

        // Фільтр по пацієнту
        if (patientId.HasValue)
        {
            query = query.Where(p => p.PatientId == patientId.Value);
        }

        var prescriptions = await query
            .OrderByDescending(p => p.DateIssued)
            .Select(p => new PrescriptionDto(
                p.Id,
                p.DateIssued,
                p.Medication,
                p.Dosage,
                p.Notes,
                p.PatientId,
                p.Patient.User.FullName ?? "Не вказано",
                p.DoctorId
            ))
            .ToListAsync();

        return Ok(prescriptions);
    }

    /// <summary>
    /// Отримати конкретний рецепт
    /// </summary>
    [HttpGet("prescriptions/{id}")]
    public async Task<IActionResult> GetPrescription(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var prescription = await _context.Prescriptions
            .Include(p => p.Patient)
            .ThenInclude(pt => pt.User)
            .Where(p => p.Id == id && p.DoctorId == doctor.Id)
            .Select(p => new PrescriptionDto(
                p.Id,
                p.DateIssued,
                p.Medication,
                p.Dosage,
                p.Notes,
                p.PatientId,
                p.Patient.User.FullName ?? "Не вказано",
                p.DoctorId
            ))
            .FirstOrDefaultAsync();

        if (prescription == null)
        {
            return NotFound(new { message = "Рецепт не знайдено або у вас немає доступу до нього." });
        }

        return Ok(prescription);
    }

    /// <summary>
    /// Створити новий рецепт
    /// </summary>
    [HttpPost("prescriptions")]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        // Перевіряємо, чи пацієнт був у цього лікаря
        var hasAppointment = await _context.Appointments
            .AnyAsync(a => a.PatientId == dto.PatientId && a.DoctorId == doctor.Id);

        if (!hasAppointment)
        {
            return BadRequest(new { message = "Ви можете виписувати рецепти тільки для своїх пацієнтів." });
        }

        // Створюємо рецепт
        var prescription = new Prescription
        {
            PatientId = dto.PatientId,
            DoctorId = doctor.Id,
            Medication = dto.Medication,
            Dosage = dto.Dosage,
            Notes = dto.Notes,
            DateIssued = DateTime.Now
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Рецепт успішно створено.",
            prescriptionId = prescription.Id
        });
    }

    /// <summary>
    /// Видалити рецепт
    /// </summary>
    [HttpDelete("prescriptions/{id}")]
    public async Task<IActionResult> DeletePrescription(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor == null) return BadRequest(new { message = "Профіль лікаря не знайдено." });

        var prescription = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id);

        if (prescription == null)
        {
            return NotFound(new { message = "Рецепт не знайдено." });
        }

        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Рецепт успішно видалено." });
    }
}