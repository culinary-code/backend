using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class KeyCloakController : ControllerBase, IIdentityProviderController
{
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IAccountManager _accountManager;
    private readonly ILogger<KeyCloakController> _logger;


    public KeyCloakController(IIdentityProviderService identityProviderService, ILogger<KeyCloakController> logger, IAccountManager accountManager)
    {
        _identityProviderService = identityProviderService;
        _logger = logger;
        _accountManager = accountManager;
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
    
    [HttpPost("login")]
    [Authorize]
    public async Task<IActionResult> CheckIfUserAccountExistsOrCreate()
    {
        var token = Request.Headers["Authorization"].ToString().Substring(7);
        Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
        // Check if user exists in our database
        try
        {
            var account = _accountManager.GetAccountById(userId.ToString());
            
            return Ok("User account exists.");
        } catch (AccountNotFoundException)
        {
            // If user does not exist, create a new account
            var (username, email) = _identityProviderService.GetUsernameAndEmailFromAccessToken(token);
            await _accountManager.CreateAccount(username, email, userId);
            
            return Created("", "User account created.");
        }
    }
}