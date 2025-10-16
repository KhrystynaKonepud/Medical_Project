using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Потрібно для .FirstOrDefault()
using System.Threading.Tasks;

[ApiController]
[Authorize] // Доступ тільки для авторизованих
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: /api/account/session
    // Нова точка, яка повертає інформацію про поточну сесію (включно з роллю)
    [HttpGet("session")]
    public async Task<IActionResult> GetSessionInfo()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(); // Якщо користувача чомусь не знайдено
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault(); // Беремо першу роль (зазвичай вона одна)

        // Відправляємо відповідь у форматі JSON
        return Ok(new
        {
            email = user.Email,
            role = userRole ?? "Patient" // Якщо ролі немає, за замовчуванням пацієнт
        });
    }
}