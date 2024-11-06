using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
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
        _baseUrl = configuration["Keycloak:BaseUrl"] ?? throw new ArgumentNullException("Keycloak:BaseUrl");
        _clientId = configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException("Keycloak:ClientId");
        _realm = configuration["Keycloak:Realm"] ?? throw new ArgumentNullException("Keycloak:Realm");
        _adminUsername = configuration["Keycloak:AdminUsername"] ?? throw new ArgumentNullException("Keycloak:AdminUsername");
        _adminPassword = configuration["Keycloak:AdminPassword"] ?? throw new ArgumentNullException("Keycloak:AdminPassword");
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
            throw new LoginException("Failed to log in as admin");

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        return jsonDocument.RootElement.GetProperty("access_token").GetString();
    }

    // Create a new user in Keycloak
    public async Task RegisterUserAsync(string username, string? firstName, string? lastName, string email, string password)
    {
        var accessToken = await LoginAsync(_adminUsername, _adminPassword);

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