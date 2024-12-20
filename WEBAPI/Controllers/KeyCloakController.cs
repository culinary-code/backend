using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class KeyCloakController : ControllerBase, IIdentityProviderController
{
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IAccountManager _accountManager;
    private readonly ILogger<KeyCloakController> _logger;


    public KeyCloakController(IIdentityProviderService identityProviderService, ILogger<KeyCloakController> logger,
        IAccountManager accountManager)
    {
        _identityProviderService = identityProviderService;
        _logger = logger;
        _accountManager = accountManager;
    }

    // Registers a user. Used in the development version of the back-end
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationRequestDto request)
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

        var registerUserResult = await _identityProviderService.RegisterUserAsync(
            username,
            email,
            password
        );

        return registerUserResult.ToActionResult();
    }

    // Logs a user into their account. Checks if the user already has an account or creates a new one based on the access token
    [HttpPost("login")]
    [Authorize]
    public async Task<IActionResult> CheckIfUserAccountExistsOrCreate()
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }

        var userId = userIdResult.Value;

        // Check if user exists in our database
        var accountResult = await _accountManager.GetAccountById(userId);
        if (accountResult.IsSuccess)
        {
            return accountResult.ToActionResult();
        }

        var userNameAndEmailResult = _identityProviderService.GetUsernameAndEmailFromAccessToken(token);
        if (!userNameAndEmailResult.IsSuccess)
        {
            return userNameAndEmailResult.ToActionResult();
        }

        var (username, email) = userNameAndEmailResult.Value;
        var createAccountResult = await _accountManager.CreateAccount(username, email, userId);

        return createAccountResult.ToActionResult();
    }
}