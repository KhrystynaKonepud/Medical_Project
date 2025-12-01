using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace Mobile_Medical_Center.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string fullName, string email, string password, string phoneNumber);
        Task LogoutAsync();
        string GetToken();
        bool IsAuthenticated { get; }
        string CurrentUserEmail { get; }
    }

    public class AuthService : IAuthService
    {
        private const string TokenKey = "auth_token";
        private const string UserEmailKey = "user_email";
        private readonly HttpClient _httpClient;
        private static string ApiBaseUrl => SecureStorage.GetAsync("api_base_url").Result ?? "https://localhost:7041/api/auth";

        public bool IsAuthenticated => !string.IsNullOrEmpty(GetToken());
        public string CurrentUserEmail
        {
            get
            {
                try
                {
                    return SecureStorage.GetAsync(UserEmailKey).Result ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public AuthService()
        {
            var handler = new HttpClientHandler();

#if DEBUG
            // For development with self-signed certificates only
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // Only accept localhost/127.0.0.1 with self-signed certs in DEBUG
                if (message?.RequestUri?.Host == "localhost" || message?.RequestUri?.Host == "127.0.0.1")
                {
                    return true;
                }
                // For production, validate properly
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#else
            // Production: strict certificate validation
            handler.ServerCertificateCustomValidationCallback = null;
#endif

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new { email, password };
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("token", out var tokenElement))
                    {
                        var token = tokenElement.GetString();
                        await SecureStorage.SetAsync(TokenKey, token);
                        await SecureStorage.SetAsync(UserEmailKey, email);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string fullName, string email, string password, string phoneNumber)
        {
            try
            {
                var registerRequest = new
                {
                    fullName,
                    email,
                    password,
                    phoneNumber,
                    dateOfBirth = DateTime.Now.AddYears(-30),
                    gender = "Other",
                    address = "",
                    emergencyContact = ""
                };

                var json = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/register", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Register error: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Try to remove from SecureStorage (may not be available on all platforms)
                if (SecureStorage.Default != null)
                {
                    await SecureStorage.Default.SetAsync(TokenKey, string.Empty);
                    await SecureStorage.Default.SetAsync(UserEmailKey, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }

        public string GetToken()
        {
            try
            {
                return SecureStorage.GetAsync(TokenKey).Result ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
