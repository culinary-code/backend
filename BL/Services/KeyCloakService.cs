using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using Configuration.Options;
using Microsoft.Extensions.Configuration;
using DOM.Results;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BL.Services;

public class KeyCloakService : IIdentityProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _realm;
    private readonly string _adminUsername;
    private readonly string _adminPassword;
    private readonly IAccountManager _accountManager;

    public KeyCloakService(IConfiguration configuration, IAccountManager accountManager,
        IOptions<KeycloakOptions> keycloakOptions, IHttpClientFactory httpFactory)
    {
        _httpClient = httpFactory.CreateClient("Keycloak");

        _accountManager = accountManager;

        var keycloakOptionsValue = keycloakOptions.Value;
        _clientId = keycloakOptionsValue.ClientId;
        _realm = keycloakOptionsValue.Realm;
        _adminUsername = keycloakOptionsValue.AdminUsername;
        _adminPassword = keycloakOptionsValue.AdminPassword;
    }

    // Login as admin and store the access token
    private async Task<Result<string>> LoginAsync(string username, string password)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

        var response = await _httpClient.PostAsync($"/realms/{_realm}/protocol/openid-connect/token", content);

        if (!response.IsSuccessStatusCode)
        {
            Result<string>.Failure("Failed to get access_token", ResultFailureType.None);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        var accessToken = jsonDocument.RootElement.GetProperty("access_token").GetString();
        if (accessToken == null) return Result<string>.Failure("Failed to get access_token", ResultFailureType.None);
        return Result<string>.Success(accessToken);
    }

    // Create a new user in Keycloak
    public async Task<Result<Unit>> RegisterUserAsync(string username, string email, string password)
    {
        var accessTokenResult = await LoginAsync(_adminUsername, _adminPassword);
        if (!accessTokenResult.IsSuccess)
            return Result<Unit>.Failure(accessTokenResult.ErrorMessage!, accessTokenResult.FailureType);
        var accessToken = accessTokenResult.Value!;

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/admin/realms/{_realm}/users")
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
            return Result<Unit>.Failure($"Failed to create user: {errorContent}", ResultFailureType.None);
        }

        accessTokenResult = await LoginAsync(username, password);
        if (!accessTokenResult.IsSuccess)
            return Result<Unit>.Failure(accessTokenResult.ErrorMessage!, accessTokenResult.FailureType);
        accessToken = accessTokenResult.Value!;

        var userIdResult = GetGuidFromAccessToken(accessToken);
        if (!userIdResult.IsSuccess) return Result<Unit>.Failure(userIdResult.ErrorMessage!, userIdResult.FailureType);
        var userId = userIdResult.Value!;

        var createAccountResult = await _accountManager.CreateAccount(username, email, userId);
        if (!createAccountResult.IsSuccess)
        {
            return Result<Unit>.Failure(createAccountResult.ErrorMessage!, createAccountResult.FailureType);
        }

        return Result<Unit>.Success(new Unit());
    }


    public Result<Guid> GetGuidFromAccessToken(string accessToken)
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
                return Result<Guid>.Success(userId);
            }
        }

        return Result<Guid>.Failure("Failed to get guid from access token", ResultFailureType.Error);
    }

    public Result<(string, string)> GetUsernameAndEmailFromAccessToken(string accessToken)
    {
        // Initialize the JWT token handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Validate the token format and decode it if it's a valid JWT
        if (tokenHandler.CanReadToken(accessToken))
        {
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            // Extract the "preferred_username" claim (which usually contains the username in Keycloak tokens)
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            if (usernameClaim == null)
                return Result<(string, string)>.Failure("Username not found in access token", ResultFailureType.Error);

            // Extract the "email" claim (which usually contains the email in Keycloak tokens)
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            if (emailClaim == null)
                return Result<(string, string)>.Failure("email not found in access token", ResultFailureType.Error);

            return Result<(string, string)>.Success((usernameClaim, emailClaim));
        }

        return Result<(string, string)>.Failure("Failed to get username and email from account token", ResultFailureType.Error);
    }

    public async Task<Result<Unit>> UpdateUsername(AccountDto account, string newUsername)
    {
        string accessToken = "";

        var accessTokenResult = await LoginAsync(_adminUsername, _adminPassword);
        if (!accessTokenResult.IsSuccess)
            return Result<Unit>.Failure(accessTokenResult.ErrorMessage!, accessTokenResult.FailureType);
        accessToken = accessTokenResult.Value!;

        var userPayload = new
        {
            username = newUsername,
        };
        var jsonPayload = JsonSerializer.Serialize(userPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, $"/admin/realms/{_realm}/users/{account.AccountId}")
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
            return Result<Unit>.Failure($"Failed to change username: {errorContent}", ResultFailureType.Error);
        }
        return Result<Unit>.Success(new Unit());
    }
}