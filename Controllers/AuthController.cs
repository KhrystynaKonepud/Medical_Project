using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Модель, що приймає дані з React-форми реєстрації
    public record RegisterModel(string FullName, string Email, string Password, string PhoneNumber);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        // Всі нові користувачі автоматично отримують роль "Patient"
        await _userManager.AddToRoleAsync(user, "Patient");
        // Одразу логінимо користувача після реєстрації
        await _signInManager.SignInAsync(user, isPersistent: false);
        return Ok(new { message = "Registration successful" });
    }

    public record LoginModel(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // Логін тепер відбувається по Email, як у вашому LoginModel.cs
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid login attempt." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid login attempt." });
        }

        // Створюємо сесію для користувача
        await _signInManager.SignInAsync(user, isPersistent: false);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new { role = roles.FirstOrDefault() ?? "Patient" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logout successful" });
    }
}