using Medical_center.Controllers;
using Medical_center.Data;
using Medical_center.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_center.UnitTests.Controllers
{
    public class AdminControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            _controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task CreateDoctor_WithValidData_ReturnsOkResult()
        {
            var dto = new DoctorDto
            {
                Email = "newdoctor@test.com",
                FullName = "Dr. New Doctor",
                Password = "Doctor@1234",
                Specialization = "Cardiology",
                ExperienceYears = 5,
                Bio = "Experienced cardiologist"
            };

            var user = new ApplicationUser
            {
                Id = "new-doctor-id",
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, p) =>
                {
                    u.Id = user.Id;
                });

            _roleManagerMock.Setup(x => x.RoleExistsAsync("Doctor"))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Doctor"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.CreateDoctor(dto);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var doctor = await _context.Doctors.FirstOrDefaultAsync();
            doctor.Should().NotBeNull();
            doctor!.Specialization.Should().Be(dto.Specialization);
        }

        [Fact]
        public async Task CreateDoctor_WhenUserCreationFails_ReturnsBadRequest()
        {
            var dto = new DoctorDto
            {
                Email = "newdoctor@test.com",
                FullName = "Dr. New Doctor",
                Password = "weak",
                Specialization = "Cardiology",
                ExperienceYears = 5,
                Bio = "Experienced cardiologist"
            };

            var identityErrors = new[]
            {
                new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" }
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var result = await _controller.CreateDoctor(dto);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetDoctors_ReturnsAllDoctors()
        {
            var user1 = new ApplicationUser
            {
                Id = "doctor-1",
                UserName = "doctor1@test.com",
                Email = "doctor1@test.com",
                FullName = "Dr. One",
                PhoneNumber = "+380111111111",
                Address = "100 Medical Plaza"
            };

            var user2 = new ApplicationUser
            {
                Id = "doctor-2",
                UserName = "doctor2@test.com",
                Email = "doctor2@test.com",
                FullName = "Dr. Two",
                PhoneNumber = "+380222222222",
                Address = "200 Health Avenue"
            };

            var doctor1 = new Doctor
            {
                Id = 1,
                UserId = "doctor-1",
                Specialization = "Cardiology",
                ExperienceYears = 10,
                User = user1
            };

            var doctor2 = new Doctor
            {
                Id = 2,
                UserId = "doctor-2",
                Specialization = "Neurology",
                ExperienceYears = 8,
                User = user2
            };

            _context.Users.AddRange(user1, user2);
            _context.Doctors.AddRange(doctor1, doctor2);
            await _context.SaveChangesAsync();

            var result = await _controller.GetDoctors();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
       
        public async Task DeleteDoctor_WithNonExistentDoctor_ReturnsNotFound()
        {
            var result = await _controller.DeleteDoctor(999);

            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
