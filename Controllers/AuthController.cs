using Medical_center.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;                // AuthenticateAsync/SignOutAsync
using Microsoft.AspNetCore.Authentication.JwtBearer;      // ⬅️ ДОДАНО: для вказання схеми Bearer
using Microsoft.IdentityModel.Tokens;                     // JWT створення
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Medical_center.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // =================== Тестовий ping ===================
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok(new { ok = true, at = DateTime.UtcNow });

        // =================== РЕЄСТРАЦІЯ (локальна) ===================
        public record RegisterModel(
            [Required][StringLength(500, ErrorMessage = "Full name must be ≤ 500 characters.")] string FullName,
            [Required][EmailAddress(ErrorMessage = "Email is invalid.")] string Email,
            [Required][StringLength(16, MinimumLength = 8, ErrorMessage = "Password length must be between 8 and 16 characters.")] string Password,
            [Required][RegularExpression(@"^\+?380\d{9}$", ErrorMessage = "Phone must be in +380XXXXXXXXX format.")] string PhoneNumber
        );

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Username = Email → не більше 50 символів
            if (model.Email.Length > 50)
                return BadRequest(new { field = "Email", message = "Username (Email) must be ≤ 50 characters." });

            // Унікальність email
            var exists = await _userManager.FindByEmailAsync(model.Email);
            if (exists != null)
                return BadRequest(new { field = "Email", message = "Email is already taken." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var create = await _userManager.CreateAsync(user, model.Password);
            if (!create.Succeeded)
                return BadRequest(new { message = "Registration failed.", errors = create.Errors });

            // За замовчуванням — роль Patient
            await _userManager.AddToRoleAsync(user, "Patient");

            // Сесія (кукі)
            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(new { message = "Registration successful", email = user.Email, role = "Patient" });
        }

        // =================== ЛОГІН (локальний) ===================
        public record LoginModel([Required][EmailAddress] string Email, [Required] string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized(new { message = "Invalid login attempt." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            if (!result.Succeeded) return Unauthorized(new { message = "Invalid login attempt." });

            await _signInManager.SignInAsync(user, isPersistent: false);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new { email = user.Email, role = roles.FirstOrDefault() ?? "Patient" });
        }

        // =================== GOOGLE SIGN-IN (OAuth2) ===================

        [HttpGet("google/login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
            };
            return Challenge(props, "Google");
        }

        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback([FromQuery] string returnUrl = "/")
        {
            // 1) Основний шлях
            var info = await _signInManager.GetExternalLoginInfoAsync();

            // 2) Fallback, якщо info == null (читаємо зовнішній кукі вручну)
            if (info == null)
            {
                var ext = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
                if (ext?.Succeeded == true && ext.Principal != null)
                {
                    var provider = ext.Properties?.Items.TryGetValue(".AuthScheme", out var s) == true ? s : "Google";
                    var providerKey = ext.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                                      ?? ext.Principal.FindFirstValue("sub");

                    if (!string.IsNullOrEmpty(providerKey))
                    {
                        info = new ExternalLoginInfo(ext.Principal, provider, providerKey, provider);
                    }
                }

                if (info == null)
                    return Redirect("/login?error=google-no-info");
            }

            // 3) Якщо зовнішній логін уже прив’язано — просто увійдемо
            var extSignIn = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (extSignIn.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
            }

            // 4) Створюємо/знаходимо локального користувача
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            if (string.IsNullOrEmpty(email))
                return Redirect("/login?error=google-no-email");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = name
                };

                var create = await _userManager.CreateAsync(user);
                if (!create.Succeeded)
                    return Redirect("/login?error=create-failed");

                await _userManager.AddToRoleAsync(user, "Patient");
            }

            // 5) Прив’язуємо зовнішній логін (якщо ще не прив’язаний)
            var addLogin = await _userManager.AddLoginAsync(user, info);
            // Якщо вже прив’язано — помилка не критична

            // 6) Входимо
            await _signInManager.SignInAsync(user, isPersistent: false);

            // 7) Чистимо тимчасовий external-cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // 8) Назад у SPA
            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        // =================== JWT видача (DEV) ===================
        public record TokenDevRequest([Required][EmailAddress] string Email, [Required] string Password);

        [HttpPost("token-dev")]
        [AllowAnonymous]
        public async Task<IActionResult> TokenDev([FromBody] TokenDevRequest req)
        {
            // Перевіряємо облікові дані
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            var passOk = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!passOk)
                return Unauthorized(new { message = "Invalid credentials." });

            var roles = await _userManager.GetRolesAsync(user);

            // Читаємо конфіг для JWT
            var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var issuer = cfg["Jwt:Issuer"];
            var audience = cfg["Jwt:Audience"];
            var key = cfg["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(key))
                return StatusCode(500, new { message = "JWT settings are missing in configuration." });

            // Формуємо клейми і токен
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email ?? "")
            };
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { access_token = jwt, token_type = "Bearer", expires_in = 7200 });
        }

        // =================== ВИХІД ===================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }

        // =================== ХТО Я (працює з Bearer АБО cookie) ===================
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { email = user.Email, fullName = user.FullName, roles });
        }


        // =================== 401-хендлер ===================
        [HttpGet("unauthorized")]
        [AllowAnonymous]
        public IActionResult UnauthorizedEndpoint() => Unauthorized(new { message = "Unauthorized" });
    }
}
