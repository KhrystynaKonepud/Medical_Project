using Medical_center.Models;
using Medical_center.Validators;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using FluentAssertions;

namespace Medical_center.UnitTests.Validators
{
    public class MaxLengthPasswordValidatorTests
    {
        private readonly MaxLengthPasswordValidator<ApplicationUser> _validator;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public MaxLengthPasswordValidatorTests()
        {
            _validator = new MaxLengthPasswordValidator<ApplicationUser>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task ValidateAsync_PasswordWithin16Characters_ReturnsSuccess()
        {
            var password = "Test@1234567";
            var user = new ApplicationUser { UserName = "test@test.com" };

            var result = await _validator.ValidateAsync(_userManagerMock.Object, user, password);

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_PasswordExactly16Characters_ReturnsSuccess()
        {
            var password = "Test@12345678901";
            var user = new ApplicationUser { UserName = "test@test.com" };

            var result = await _validator.ValidateAsync(_userManagerMock.Object, user, password);

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_PasswordOver16Characters_ReturnsFailed()
        {
            var password = "Test@123456789012";
            var user = new ApplicationUser { UserName = "test@test.com" };

            var result = await _validator.ValidateAsync(_userManagerMock.Object, user, password);

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors.First().Code.Should().Be("PasswordTooLong");
            result.Errors.First().Description.Should().Be("Password must be at most 16 characters.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ValidateAsync_EmptyOrNullPassword_ReturnsSuccess(string password)
        {
            var user = new ApplicationUser { UserName = "test@test.com" };

            var result = await _validator.ValidateAsync(_userManagerMock.Object, user, password);

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_Password25Characters_ReturnsFailed()
        {
            var password = "Test@12345678901234567890";
            var user = new ApplicationUser { UserName = "test@test.com" };

            var result = await _validator.ValidateAsync(_userManagerMock.Object, user, password);

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle();
        }
    }
}