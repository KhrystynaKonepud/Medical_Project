using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Authorize(Roles = "Doctor")]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Тут можна буде додати специфічну для лікаря інформацію
        return Ok(new { user.FullName, user.Email });
    }
}