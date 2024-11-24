using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController: ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountManager _accountManager;
    private readonly IIdentityProviderService _identityProviderService;

    public AccountController(IAccountManager accountManager, ILogger<AccountController> logger, IIdentityProviderService identityProviderService)
    {
        _accountManager = accountManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
    }
    
    [HttpGet("{accountId}")]
    public IActionResult GetUserById(string accountId)
    {
        try
        {
            var user = _accountManager.GetAccountById(accountId); 
            return Ok(user);
        }
        catch (AccountNotFoundException e)
        {
            _logger.LogError("An error occured trying to fetch user: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }

    [HttpPut("updateAccount")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountDto accountDto, [FromQuery] string actionType)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        accountDto.AccountId = userId;
        
        try
        {
            if (string.IsNullOrEmpty(actionType))
            {
                return BadRequest("Action type is required.");
            }

            switch (actionType.ToLowerInvariant())
            {
                case "updateusername":
                    var updatedUsername = _accountManager.UpdateAccount(accountDto);
                    await _identityProviderService.UpdateUsernameAsync(updatedUsername, accountDto.Name);
                    return Ok(updatedUsername);
                
                case "updatefamilysize":
                    var updatedFamilySize = _accountManager.UpdateFamilySize(accountDto);
                    return Ok(updatedFamilySize);
                
                default:
                    return BadRequest("Invalid action type.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while updating account {AccountId}: {ErrorMessage}", userId, e.Message);
            return BadRequest("Failed to update account.");
        }
    }
    
    [HttpGet("getPreferences")]
    public IActionResult GetUserPreferences()
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
            var preferences = _accountManager.GetPreferencesByUserId(userId);
            return Ok(preferences);
        }
        catch (AccountNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {ErrorMessage}", ex.Message);
            return NotFound("Account not found.");
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred trying to fetch user preferences: {ErrorMessage}", e.Message);
            return BadRequest("Failed to get user preferences.");
        }
    }

    
    /*[HttpGet("getPreferences")]
    public IActionResult GetUserPreferences()
    {
        try
        {
            Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
            var preferences = _accountManager.GetPreferencesByUserId(userId);
            return Ok(preferences);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occured trying to fetch user preferences: {ErrorMessage}", e.Message);
            return BadRequest("Failed to get user preferences.");
        }
    }*/

    [HttpPut("updatePreferences")]
    public IActionResult UpdateUserPreferences([FromBody] List<PreferenceDto> preferences)
    {
        try
        {
            Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
            var updatedAccount = _accountManager.UpdatePreferences(userId, preferences);
            return Ok(updatedAccount);
        }
        catch (Exception e)
        {
            _logger.LogError("Error updating user preferences: {Message}", e.Message);
            return BadRequest("Failed to update user preferences.");
        }
    } 
    
    
    [HttpPost("addPreference")]
    public async Task<IActionResult> AddPreference([FromBody] PreferenceDto preferenceDto)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));

        try
        {
            var updatedAccount = _accountManager.AddPreferenceToAccount(userId, preferenceDto);
            return Ok(updatedAccount);
        }
        catch (AccountNotFoundException e)
        {
            _logger.LogError("An error occurred while adding preference to account {AccountId}: {ErrorMessage}", userId, e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while adding preference: {ErrorMessage}", e.Message);
            return BadRequest("Failed to add preference.");
        }
    }

}