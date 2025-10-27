using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations; // Додано
using System.Threading.Tasks;

[ApiController]
[Authorize(Roles = "Patient")]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // === ОНОВЛЕНО: Модель прийому даних ===
    // Додаємо PhoneNumber, який вже існує в ApplicationUser
    public record CompleteProfileModel(
        [Required] string FullName,
        [Required] string Address,
        [Required] DateTime DateOfBirth,
        [Required] Gender Gender,

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^\+?380\d{9}$", ErrorMessage = "Phone must be in +380XXXXXXXXX format.")]
        string PhoneNumber
    );


    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Оновлюємо всі поля
        user.FullName = model.FullName;
        user.Address = model.Address;
        user.DateOfBirth = model.DateOfBirth;
        user.Gender = model.Gender;
        user.PhoneNumber = model.PhoneNumber; // Оновлюємо існуюче поле

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Повертаємо повний оновлений профіль
        return Ok(new
        {
            user.FullName,
            user.Email,
            user.Address,
            user.DateOfBirth,
            user.Gender,
            user.PhoneNumber // Повертаємо оновлений номер
        });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Повертаємо всі необхідні поля, включно з PhoneNumber
        return Ok(new
        {
            user.FullName,
            user.Email,
            user.Address,
            user.DateOfBirth,
            user.Gender,
            user.PhoneNumber //
        });
    }
}