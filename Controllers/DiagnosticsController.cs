using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Medical_center.Data;
using Medical_center.Models;
using Bogus;
using Microsoft.AspNetCore.Identity;

namespace Medical_center.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private static readonly ActivitySource ActivitySource = new("Medical_center");
        private readonly ApplicationDbContext _context;

        public DiagnosticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("simulate-work")]
        public async Task<IActionResult> SimulateWork()
        {
            using var activity = ActivitySource.StartActivity("LongRunningReportGeneration");

            activity?.SetTag("lab.student", "StudentName");
            activity?.SetTag("lab.task", "6");
            activity?.SetTag("report.type", "full_statistics");

            await Task.Delay(2000);

            activity?.AddEvent(new ActivityEvent("ReportGenerated"));

            return Ok(new
            {
                Message = "Simulated work completed",
                TraceId = activity?.TraceId.ToString(),
                Duration = "2000ms"
            });
        }

        [HttpPost("seed-patients")]
        public async Task<IActionResult> SeedPatients([FromQuery] int count = 10000)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var faker = new Faker("uk");

                var newUsers = new List<ApplicationUser>();
                var newPatients = new List<Patient>();

                for (int i = 0; i < count; i++)
                {
                    var uniqueId = Guid.NewGuid().ToString();
                    var email = faker.Internet.Email(uniqueSuffix: uniqueId);

                    var user = new ApplicationUser
                    {
                        Id = uniqueId,
                        UserName = email,
                        NormalizedUserName = email.ToUpper(), 
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        EmailConfirmed = true,
                        FullName = faker.Name.FullName(),
                        DateOfBirth = faker.Date.Past(50, DateTime.Now.AddYears(-18)),
                        Gender = faker.PickRandom<Gender>(),
                        Address = faker.Address.FullAddress(),
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    newUsers.Add(user);

                    var patient = new Patient
                    {
                        UserId = user.Id,
                        EmergencyContact = faker.Phone.PhoneNumber()
                    };
                    newPatients.Add(patient);
                }

                await _context.Users.AddRangeAsync(newUsers);
                await _context.Patients.AddRangeAsync(newPatients);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                return Ok(new
                {
                    Message = $"Successfully created {count} users and patients",
                    TimeTaken = $"{stopwatch.ElapsedMilliseconds} ms"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Помилка при створенні записів",
                    Details = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}