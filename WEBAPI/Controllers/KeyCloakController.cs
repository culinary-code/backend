using BL.DTOs.Accounts;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class KeyCloakController : ControllerBase, IIdentityProviderController
{
    private readonly IIdentityProviderService _identityProviderService;
    private readonly ILogger<KeyCloakController> _logger;


    public KeyCloakController(IIdentityProviderService identityProviderService, ILogger<KeyCloakController> logger)
    {
        _identityProviderService = identityProviderService;
        _logger = logger;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationRequestDto request)
    {
        try
        {
            // Extract credentials and pass them to the service
            var password = request.Password;
            if (string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("RegisterUser input missing: Password is empty");
                return BadRequest("Password is required.");
            }

            var email = request.Email;
            if (email == null)
            {
                _logger.LogWarning("RegisterUser input missing: Email is empty");
                return BadRequest("Email is required.");
            }

            var username = request.Username;
            if (username == null)
            {
                _logger.LogWarning("RegisterUser input missing: Username is empty");
                return BadRequest("Username is required.");
            }

            await _identityProviderService.RegisterUserAsync(
                username,
                request.FirstName,
                request.LastName,
                email,
                password
            );

            return Ok("User created successfully.");
        }
        // exceptions directly readable for end user
        catch (Exception e) when (e is RegisterUserException)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return BadRequest($"Error: {e.Message}");
        }
        // exceptions made for dev purposes, translate to readable string for end user
        catch (Exception e) when (e is ArgumentNullException or LoginAdminException)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return BadRequest($"Could not register new user. Try again later.");
        }
    }
}