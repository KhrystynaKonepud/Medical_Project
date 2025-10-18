using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace Medical_center.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // ВАЖЛИВО: явно вказуємо схему Bearer, інакше за замовчуванням буде cookie
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TestTokenController : ControllerBase
    {
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                authType = User.Identity?.AuthenticationType,
                name = User.Identity?.Name,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
