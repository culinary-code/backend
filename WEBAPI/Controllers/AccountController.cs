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
}