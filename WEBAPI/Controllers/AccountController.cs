using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
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

    [HttpPut("/updateAccount/{accountId}")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountDto updatedAccount)
    {
        try
        {
            var account = _accountManager.UpdateAccount(updatedAccount);
            await _identityProviderService.UpdateUsernameAsync(account, updatedAccount.Name);
            return Ok(account);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while updating account {AccountId}: {ErrorMessage}", updatedAccount.AccountId, e.Message);
            return BadRequest("Failed to update account.");
        }
    }
}