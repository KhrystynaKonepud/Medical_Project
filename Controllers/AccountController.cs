using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Medical_center.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Єдина точка після входу/реєстрації/Google
        [HttpGet]
        public async Task<IActionResult> RedirectByRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                return RedirectToAction("Dashboard", "Admin");

            if (roles.Contains("Doctor"))
                return RedirectToAction("Dashboard", "Doctor");

            if (roles.Contains("Patient"))
                return RedirectToAction("Dashboard", "Patient");

            // дефолт — якщо немає ролі
            return RedirectToAction("Index", "Home");
        }
    }
}
