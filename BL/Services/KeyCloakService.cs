using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using DOM.Exceptions;

namespace BL.Services;

public class KeyCloakService : IIdentityProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _realm;
    private readonly string _adminUsername;
    private readonly string _adminPassword;

    public KeyCloakService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient; 
        _baseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL") ?? throw new EnvironmentException("KEYCLOAK_BASE_URL environment variable is not set.");
        _clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? throw new EnvironmentException("KEYCLOAK_CLIENT_ID environment variable is not set.");
        _realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? throw new EnvironmentException("KEYCLOAK_REALM environment variable is not set.");
        _adminUsername = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_USERNAME") ?? throw new EnvironmentException("KEYCLOAK_ADMIN_USERNAME environment variable is not set.");
        _adminPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") ?? throw new EnvironmentException("KEYCLOAK_ADMIN_PASSWORD environment variable is not set.");
    }

    // Login as admin and store the access token
    private async Task<string?> LoginAsync(string username, string password)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

        var response = await _httpClient.PostAsync($"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token", content);
        
        if (!response.IsSuccessStatusCode)
            throw new LoginAdminException("Failed to log in as admin when registering a new user");

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        return jsonDocument.RootElement.GetProperty("access_token").GetString();
    }

    // Create a new user in Keycloak
    public async Task RegisterUserAsync(string username, string? firstName, string? lastName, string email, string password)
    {
        var accessToken = await LoginAsync(_adminUsername, _adminPassword);

        if (string.IsNullOrEmpty(accessToken))
            throw new LoginAdminException("Failed to read admin accesstoken when registering a new user");
        
        var userPayload = new
        {
            username,
            enabled = true,
            firstName,
            lastName,
            email,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(userPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/admin/realms/{_realm}/users")
        {
            Content = content,
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
            }
        };

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new RegisterUserException($"Failed to create user: {errorContent}");
        }
    }
    
    
}