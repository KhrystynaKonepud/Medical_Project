using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Medical_center.IntegrationTests
{
    public class DoctorIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public DoctorIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<HttpClient> GetAuthenticatedDoctorClient()
        {
            var loginData = new
            {
                Email = "testdoctor@test.com",
                Password = "Doctor@123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            response.EnsureSuccessStatusCode();

            var cookies = response.Headers.GetValues("Set-Cookie");
            var client = new CustomWebApplicationFactory().CreateClient();

            foreach (var cookie in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
            }

            return client;
        }

        [Fact]
        public async Task GetProfile_WithAuthenticatedDoctor_ReturnsOk()
        {
            var authenticatedClient = await GetAuthenticatedDoctorClient();

            var response = await authenticatedClient.GetAsync("/api/doctor/profile");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetProfile_WithoutAuthentication_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/doctor/profile");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateAvailability_WithValidData_ReturnsOk()
        {
            var authenticatedClient = await GetAuthenticatedDoctorClient();

            var availabilityData = new
            {
                AvailableDate = DateTime.UtcNow.AddDays(5),
                StartTime = new TimeSpan(9, 0, 0),
                AppointmentDurationMinutes = 60
            };

            var response = await authenticatedClient.PostAsJsonAsync("/api/doctor/availability", availabilityData);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetAppointments_WithAuthenticatedDoctor_ReturnsOk()
        {
            var authenticatedClient = await GetAuthenticatedDoctorClient();

            var response = await authenticatedClient.GetAsync("/api/doctor/appointments");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAvailabilities_WithAuthenticatedDoctor_ReturnsOk()
        {
            var authenticatedClient = await GetAuthenticatedDoctorClient();

            var response = await authenticatedClient.GetAsync("/api/doctor/availabilities");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateProfile_WithValidData_ReturnsOk()
        {
            var authenticatedClient = await GetAuthenticatedDoctorClient();

            var updateData = new
            {
                FullName = "Dr. Updated Name",
                Specialization = "Cardiology",
                ExperienceYears = 10,
                Bio = "Updated bio information",
                PhoneNumber = "+380333333333"
            };

            var response = await authenticatedClient.PutAsJsonAsync("/api/doctor/profile/complete", updateData);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }
    }
}
