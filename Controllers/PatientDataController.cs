using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Authorize(Roles = "Patient")] // Тільки для пацієнтів
[Route("api/patient-data")]
public class PatientDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientDataController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Отримати ID поточної сутності Patient (Tbl)
    private async Task<int?> GetCurrentPatientIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        return patient?.Id;
    }

    // 3. Можливість переглянути всіх лікарів
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Select(d => new
            {
                d.Id,
                FullName = d.User.FullName,
                d.Specialization,
                d.ExperienceYears,
                d.Bio,
                d.Rating
            })
            .ToListAsync();
        return Ok(doctors);
    }

    // 4a. Можливість обрати певну годину доступності лікаря
    [HttpGet("doctor-availability/{doctorId}")]
    public async Task<IActionResult> GetDoctorAvailability(int doctorId)
    {
        var slots = await _context.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId &&
                        a.IsActive && // ⬅️ Цей прапор гарантує, що заброньовані слоти не повернуться
                        a.AvailableDate >= DateTime.Today)
            .OrderBy(a => a.AvailableDate).ThenBy(a => a.StartTime)
            .ToListAsync();

        return Ok(slots);
    }

    // 4b. Записатися на прийом
    public record AppointmentBookingModel(int DoctorId, DateTime Date, TimeSpan StartTime, string Reason);

    [HttpPost("book-appointment")]
    public async Task<IActionResult> BookAppointment([FromBody] AppointmentBookingModel model)
    {
        var patientId = await GetCurrentPatientIdAsync();
        if (patientId == null) return BadRequest(new { message = "Профіль пацієнта не знайдено." });

        // === ПОЧАТОК ТРАНЗАКЦІЇ ===
        // Гарантує, що ми або створимо запис І деактивуємо слот, або нічого
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Знайти КОНКРЕТНИЙ активний слот
            var slot = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(a => a.DoctorId == model.DoctorId &&
                                           a.AvailableDate.Date == model.Date.Date &&
                                           a.StartTime == model.StartTime &&
                                           a.IsActive);

            // 2. Якщо слоту немає (вже заброньований/деактивований), повернути помилку
            if (slot == null)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Вибраний слот недоступний або щойно був заброньований." });
            }

            // 3. Створити запис
            var appointment = new Appointment
            {
                PatientId = patientId.Value,
                DoctorId = model.DoctorId,
                Date = model.Date.Date.Add(model.StartTime),
                Reason = model.Reason,
                Status = AppointmentStatus.Scheduled
            };
            _context.Appointments.Add(appointment);

            // 4. Деактивувати слот, щоб його не можна було забронювати знову
            slot.IsActive = false;
            _context.DoctorAvailabilities.Update(slot);

            // 5. Зберегти ОБИДВІ зміни (атомарно)
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Запис успішно створено." });
        }
        catch (Exception)
        {
            // Якщо щось пішло не так, відкотити зміни
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Помилка сервера під час бронювання." });
        }
    }

    // 5. Можливість записатись на вакцинацію (спрощено)
    public record VaccinationBookingModel(int DoctorId, string VaccineName, DateTime DateAdministered);

    [HttpPost("book-vaccination")]
    public async Task<IActionResult> BookVaccination([FromBody] VaccinationBookingModel model)
    {
        var patientId = await GetCurrentPatientIdAsync();
        if (patientId == null) return BadRequest(new { message = "Профіль пацієнта не знайдено." });

        var vaccination = new Vaccination
        {
            PatientId = patientId.Value,
            DoctorId = model.DoctorId, // Припускаємо, що пацієнт знає ID лікаря/клініки
            VaccineName = model.VaccineName,
            DateAdministered = model.DateAdministered,
            DoseNumber = 1 // Спрощено
        };

        _context.Vaccinations.Add(vaccination);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Запис на вакцинацію створено." });
    }

    // 6. Можливість залишити відгук лікарю
    public record ReviewModel(int DoctorId, decimal Rating, string Comment);

    [HttpPost("leave-review")]
    public async Task<IActionResult> LeaveReview([FromBody] ReviewModel model)
    {
        var patientId = await GetCurrentPatientIdAsync();
        if (patientId == null) return BadRequest(new { message = "Профіль пацієнта не знайдено." });

        // Перевірка, чи був візит до цього лікаря (спрощено)
        bool hadAppointment = await _context.Appointments
            .AnyAsync(a => a.PatientId == patientId.Value &&
                           a.DoctorId == model.DoctorId &&
                           a.Status == AppointmentStatus.Completed);

        if (!hadAppointment)
            return BadRequest(new { message = "Ви не можете залишити відгук, не відвідавши лікаря." });

        var review = new Review
        {
            PatientId = patientId.Value,
            DoctorId = model.DoctorId,
            Rating = model.Rating,
            Comment = model.Comment,
            DateCreated = DateTime.Now
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Оновлення середнього рейтингу лікаря
        var averageRating = await _context.Reviews
            .Where(r => r.DoctorId == model.DoctorId)
            .AverageAsync(r => r.Rating);

        var doctor = await _context.Doctors.FindAsync(model.DoctorId);
        if (doctor != null)
        {
            doctor.Rating = averageRating;
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Відгук додано." });
    }

    // 7. Перегляд результатів аналізів
    [HttpGet("test-results")]
    public async Task<IActionResult> GetTestResults()
    {
        var patientId = await GetCurrentPatientIdAsync();
        if (patientId == null) return Ok(new List<TestResult>());

        var results = await _context.TestResults
            .Where(tr => tr.PatientId == patientId.Value)
            .Include(tr => tr.Doctor.User)
            .Select(tr => new {
                tr.Id,
                tr.TestName,
                tr.Result,
                tr.Notes,
                tr.DateConducted,
                DoctorName = tr.Doctor.User.FullName
            })
            .OrderByDescending(tr => tr.DateConducted)
            .ToListAsync();

        return Ok(results);
    }

    // 8. Перегляд медичних записів (Історія записів)
    [HttpGet("medical-history")]
    public async Task<IActionResult> GetMedicalHistory()
    {
        var patientId = await GetCurrentPatientIdAsync();
        if (patientId == null) return Ok(new List<Appointment>());

        var appointments = await _context.Appointments
            .Where(a => a.PatientId == patientId.Value)
            .Include(a => a.Doctor.User)
            .Include(a => a.Doctor) // ⬅️ Додано
            .Select(a => new {
                a.Id,
                a.Date,
                DoctorName = a.Doctor.User.FullName,
                DoctorSpecialization = a.Doctor.Specialization,
                a.DoctorId, // ⬅️ Потрібно для відгуків
                a.Reason,
                a.Status
            })
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return Ok(appointments);
    }

    // 9. Можливість отримати рецепт
    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions()
    {
        var patientId = await GetCurrentPatientIdAsync();
        if (patientId == null) return Ok(new List<Prescription>());

        var prescriptions = await _context.Prescriptions
            .Where(p => p.PatientId == patientId.Value)
            .Include(p => p.Doctor.User)
            .Select(p => new {
                p.Id,
                p.DateIssued,
                p.Medication,
                p.Dosage,
                p.Notes,
                DoctorName = p.Doctor.User.FullName
            })
            .OrderByDescending(p => p.DateIssued)
            .ToListAsync();

        return Ok(prescriptions);
    }
}