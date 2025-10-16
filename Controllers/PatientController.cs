using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Authorize(Roles = "Patient")] // Доступ тільки для пацієнтів
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public record CompleteProfileModel(string FullName, string Address, DateTime DateOfBirth, Gender Gender);

    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        user.FullName = model.FullName;
        user.Address = model.Address;
        user.DateOfBirth = model.DateOfBirth;
        user.Gender = model.Gender;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? Ok(new { message = "Profile completed successfully." }) : BadRequest(result.Errors);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        return Ok(new { user.FullName, user.Email, user.Address, user.DateOfBirth, user.Gender });
    }
}
