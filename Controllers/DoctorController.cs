using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Authorize(Roles = "Doctor")] // Доступ тільки для користувачів із роллю Doctor
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // =================== Профіль лікаря ===================
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Беремо поточного користувача з токена
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Перевірка, чи користувач дійсно має роль Doctor
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Doctor"))
            return Forbid(); // 403, якщо немає ролі

        // Повертаємо дані профілю та специфічні дані лікаря
        return Ok(new
        {
            user.FullName,
            user.Email,
            Specialization = user.DoctorProfile?.Specialization,
            ExperienceYears = user.DoctorProfile?.ExperienceYears,
            Rating = user.DoctorProfile?.Rating
        });
    }
}
