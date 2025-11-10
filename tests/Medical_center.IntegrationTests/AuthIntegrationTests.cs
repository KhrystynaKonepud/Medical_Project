using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Medical_center.Models;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Medical_center.IntegrationTests
{
    public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Ping_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/auth/ping");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("ok");
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOk()
        {
            var email = $"newuser{System.Guid.NewGuid()}@test.com";
            var registerData = new
            {
                FullName = "New Test User",
                Email = email,
                Password = "Test@12345",
                PhoneNumber = "+380123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<dynamic>();
            content.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            var email = $"duplicate{System.Guid.NewGuid()}@test.com";

            var registerData = new
            {
                FullName = "Test User",
                Email = email,
                Password = "Test@12345",
                PhoneNumber = "+380123456789"
            };

            // Створюємо користувача
            await _client.PostAsJsonAsync("/api/auth/register", registerData);

            // Спроба зареєструвати знову з тим самим email
            var duplicateResponse = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            var registerData = new
            {
                FullName = "Test User",
                Email = "invalid-email",
                Password = "Test@12345",
                PhoneNumber = "+380123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithShortPassword_ReturnsBadRequest()
        {
            var email = $"shortpass{System.Guid.NewGuid()}@test.com";
            var registerData = new
            {
                FullName = "Test User",
                Email = email,
                Password = "short",
                PhoneNumber = "+380123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithInvalidPhoneNumber_ReturnsBadRequest()
        {
            var email = $"invalidphone{System.Guid.NewGuid()}@test.com";
            var registerData = new
            {
                FullName = "Test User",
                Email = email,
                Password = "Test@12345",
                PhoneNumber = "123456789"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Використовуємо вже наявного тестового пацієнта з фабрики
            var loginData = new { Email = "testpatient@test.com", Password = "Patient@123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<dynamic>();
            content.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var loginData = new { Email = "testpatient@test.com", Password = "WrongPassword" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            var loginData = new { Email = "nonexistent@test.com", Password = "Test@12345" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
