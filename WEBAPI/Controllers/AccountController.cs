using AutoMapper;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Accounts;
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
            if (user == null)
            {
                return NotFound("User not found");
            }
            return Ok(user);
        }
        catch (Exception e)
        {
            return BadRequest($"Error fetching user: {e.Message}");
        }
    }

    [HttpPut("/updateAccount/{accountId}")]
    public async Task<IActionResult> UpdateAccount([FromBody] Account updatedAccount)
    {
        try
        {
            var account = _accountManager.UpdateAccount(updatedAccount);

            await _identityProviderService.UpdateUsernameAsync(account, updatedAccount.Name);
            
            return Ok(account);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}