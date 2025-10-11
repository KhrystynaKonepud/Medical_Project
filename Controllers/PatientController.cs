using Microsoft.AspNetCore.Mvc;

namespace Medical_center.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}
