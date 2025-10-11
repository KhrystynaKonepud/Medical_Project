using Microsoft.AspNetCore.Mvc;

namespace Medical_center.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}
