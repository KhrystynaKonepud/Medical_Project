using Medical_center.Controllers;
using Medical_center.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Medical_center.Controllers.AuthController;

namespace Medical_center.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object,
                null, null, null, null);

            _controller = new AuthController(_userManagerMock.Object, _signInManagerMock.Object);
        }

        [Fact]
        public void Ping_ReturnsOkResult()
        {
            var result = _controller.Ping();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Register_WithValidModel_ReturnsOkResult()
        {
            var model = new RegisterModel(
                FullName: "Test User",
                Email: "test@test.com",
                Password: "Test@1234",
                PhoneNumber: "+380123456789"
            );

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            var result = await _controller.Register(model);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            var model = new RegisterModel(
                FullName: "Test User",
                Email: "test@test.com",
                Password: "Test@1234",
                PhoneNumber: "+380123456789"
            );

            var existingUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(existingUser);

            var result = await _controller.Register(model);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Register_WithEmailTooLong_ReturnsBadRequest()
        {
            var model = new RegisterModel(
                FullName: "Test User",
                Email: "very_long_email_address_that_exceeds_fifty_characters@test.com",
                Password: "Test@1234",
                PhoneNumber: "+380123456789"
            );

            var result = await _controller.Register(model);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Register_WhenUserCreationFails_ReturnsBadRequest()
        {
            var model = new RegisterModel(
                FullName: "Test User",
                Email: "test@test.com",
                Password: "weak",
                PhoneNumber: "+380123456789"
            );

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            var identityErrors = new[]
            {
                new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" }
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var result = await _controller.Register(model);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            var model = new LoginModel(Email: "test@test.com", Password: "Test@1234");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, model.Password, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _signInManagerMock.Setup(x => x.SignInAsync(user, false, null))
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Patient" });

            var result = await _controller.Login(model);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            var model = new LoginModel(Email: "nonexistent@test.com", Password: "Test@1234");

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _controller.Login(model);

            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            var model = new LoginModel(Email: "test@test.com", Password: "WrongPassword");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, model.Password, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _controller.Login(model);

            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.StatusCode.Should().Be(401);
        }
    }
}
