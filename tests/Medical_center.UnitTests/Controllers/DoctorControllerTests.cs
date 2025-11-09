using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Medical_center.UnitTests.Controllers
{
    public class DoctorControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly DoctorController _controller;

        public DoctorControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new DoctorController(_userManagerMock.Object, _context);
        }

        [Fact]
        public async Task GetProfile_WhenDoctorExists_ReturnsOkResult()
        {
            var userId = "doctor-user-id";
            var doctor = new Doctor
            {
                Id = 1,
                UserId = userId,
                Specialization = "Cardiology",
                ExperienceYears = 10,
                Bio = "Experienced cardiologist",
                Rating = 4.5m
            };

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "doctor@test.com",
                Email = "doctor@test.com",
                FullName = "Dr. Test",
                PhoneNumber = "+380123456789",
                Address = "123 Test Street",
                DoctorProfile = doctor
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await _controller.GetProfile();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAppointments_ReturnsDoctorAppointments()
        {
            var userId = "doctor-user-id";
            var doctor = new Doctor
            {
                Id = 1,
                UserId = userId,
                Specialization = "Cardiology",
                ExperienceYears = 10
            };

            var patient = new Patient
            {
                Id = 1,
                UserId = "patient-user-id",
                EmergencyContact = "+380987654321"
            };

            var patientUser = new ApplicationUser
            {
                Id = "patient-user-id",
                UserName = "patient@test.com",
                Email = "patient@test.com",
                FullName = "Patient Test",
                PhoneNumber = "+380111111111",
                Address = "456 Patient Avenue"
            };

            var appointment = new Appointment
            {
                Id = 1,
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                Date = DateTime.UtcNow.AddDays(1),
                Reason = "Checkup",
                Status = AppointmentStatus.Scheduled
            };

            _context.Doctors.Add(doctor);
            _context.Patients.Add(patient);
            _context.Users.Add(patientUser);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "doctor@test.com",
                Email = "doctor@test.com",
                FullName = "Dr. Test Doctor",
                PhoneNumber = "+380222222222",
                Address = "789 Doctor Lane",
                DoctorProfile = doctor
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await _controller.GetAppointments();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CreateAvailability_WithValidData_ReturnsOkResult()
        {
            var userId = "doctor-user-id";
            var doctor = new Doctor
            {
                Id = 1,
                UserId = userId,
                Specialization = "Cardiology",
                ExperienceYears = 10
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "doctor@test.com",
                Email = "doctor@test.com",
                DoctorProfile = doctor
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var model = new DoctorController.AvailabilityModel(
                AvailableDate: DateTime.UtcNow.AddDays(2),
                StartTime: new TimeSpan(9, 0, 0),
                AppointmentDurationMinutes: 60
            );

            var result = await _controller.AddAvailability(model);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var availability = await _context.DoctorAvailabilities.FirstOrDefaultAsync();
            availability.Should().NotBeNull();
            availability!.DoctorId.Should().Be(doctor.Id);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
