using System;
using System.Linq;
using System.Threading.Tasks;
using BL.DTOs.Accounts;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Controllers;

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
                return BadRequest("Password is required.");
            }

            var email = request.Email;
            if (email == null)
            {
                return BadRequest("Email is required.");
            }

            var username = request.Username;
            if (username == null)
            {
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
        catch (Exception ex) when (ex is LoginException or RegisterUserException or ArgumentNullException)
        {
            _logger.LogError($"Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }
}