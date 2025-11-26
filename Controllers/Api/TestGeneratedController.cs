using Microsoft.AspNetCore.Mvc;
using Medical_center.Models;
using Medical_center.Models.Generated;

namespace Medical_center.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestGeneratedController : ControllerBase
    {
        // Test to verify that generated code works
        [HttpGet("test")]
        public ActionResult TestGeneration()
        {
            // Test DTO generation
            var clinic = new Clinic
            {
                Id = 1,
                Name = "Test Clinic",
                Address = "123 Main St",
                PhoneNumber = "555-1234"
            };

            // Test Mapper generation
            var dto = clinic.ToDto();

            return Ok(new
            {
                Message = "Source Generator is working!",
                ClinicDto = dto,
                GeneratedTypes = new[]
                {
                    typeof(ClinicDto).FullName,
                    typeof(ClinicMapper).FullName
                }
            });
        }
    }
}
