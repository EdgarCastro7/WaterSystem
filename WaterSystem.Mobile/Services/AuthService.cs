
using System.Net.Http.Json;
using System.Text.Json;

namespace WaterSystem.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    private const string ApiUrl = "https://10.0.2.2:7276/";

    public AuthService()
    {
#if DEBUG
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, certificate, chain, errors) => true
        };

        _httpClient = new HttpClient(handler);
#else
        _httpClient = new HttpClient();
#endif

        _httpClient.BaseAddress = new Uri(ApiUrl);
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        try
        {
            var loginData = new
            {
                email,
                password
            };

            var response = await _httpClient.PostAsJsonAsync(
                "api/AuthApi/login",
                loginData);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result?.Token != null)
            {
                await SecureStorage.Default.SetAsync("auth_token", result.Token);
            }

            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}
