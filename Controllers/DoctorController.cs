using Microsoft.AspNetCore.Mvc;

namespace Medical_center.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}
