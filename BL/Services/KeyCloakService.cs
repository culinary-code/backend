using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using DOM.Accounts;
using Microsoft.Extensions.Configuration;
using DOM.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace BL.Services;

public class KeyCloakService : IIdentityProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _realm;
    private readonly string _adminUsername;
    private readonly string _adminPassword;
    private readonly IAccountManager _accountManager;

    public KeyCloakService(HttpClient httpClient, IConfiguration configuration, IAccountManager accountManager)
    {
        _httpClient = httpClient;
        _accountManager = accountManager;
        
        _baseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_BASE_URL environment variable is not set.");
        _clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_CLIENT_ID environment variable is not set.");
        _realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_REALM environment variable is not set.");
        _adminUsername = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_USERNAME") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_ADMIN_USERNAME environment variable is not set.");
        _adminPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_ADMIN_PASSWORD environment variable is not set.");
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
            throw new LoginAdminException("Failed to get access_token");

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        return jsonDocument.RootElement.GetProperty("access_token").GetString();
    }

    // Create a new user in Keycloak
    public async Task RegisterUserAsync(string username, string email, string password)
    {
        var accessToken = await LoginAsync(_adminUsername, _adminPassword);

        if (string.IsNullOrEmpty(accessToken))
            throw new LoginAdminException("Failed to read admin accesstoken when registering a new user");
        
        var userPayload = new
        {
            username,
            enabled = true,
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
        
        accessToken = await LoginAsync(username, password);

        if (accessToken.IsNullOrEmpty())
        {
            throw new RegisterUserException($"Failed to create user: access token is empty");
        }
        var userId = GetGuidFromAccessToken(accessToken);
        await _accountManager.CreateAccount(username, email, userId);
        
    }
    

    public Guid GetGuidFromAccessToken(string accessToken)
    {
        // Initialize the JWT token handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Validate the token format and decode it if it's a valid JWT
        if (tokenHandler.CanReadToken(accessToken))
        {
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            // Extract the "sub" claim (which usually contains the GUID in Keycloak tokens)
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            // Convert the "sub" claim to GUID if it exists
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
        }
        throw new JwtTokenException("Failed to get userId from account token");
    }

    public (string, string) GetUsernameAndEmailFromAccessToken(string accessToken)
    {
        // Initialize the JWT token handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Validate the token format and decode it if it's a valid JWT
        if (tokenHandler.CanReadToken(accessToken))
        {
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            // Extract the "preferred_username" claim (which usually contains the username in Keycloak tokens)
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            // Extract the "email" claim (which usually contains the email in Keycloak tokens)
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            return (usernameClaim, emailClaim)!;
        }
        throw new JwtTokenException("Failed to get username and email from account token");
    }

    public async Task UpdateUsername(AccountDto account, string newUsername)
    {
        string accessToken = "";
        try
        {
            accessToken = await LoginAsync(_adminUsername, _adminPassword);

        }
        catch (Exception e)
        {
            throw new LoginAdminException("Failed to read admin access token", e);
        }
        
        if (string.IsNullOrEmpty(accessToken!))
            throw new LoginAdminException("Failed to read admin access token.");
        
        var userPayload = new
        {
            username = newUsername,
            
        };
        var jsonPayload = JsonSerializer.Serialize(userPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/admin/realms/{_realm}/users/{account.AccountId}")
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
            throw new RegisterUserException($"Failed to change username: {errorContent}");
        }
    }
}