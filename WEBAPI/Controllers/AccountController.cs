using BL.Managers.Accounts;
using DOM.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController: ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountManager _accountManager;

    public AccountController(IAccountManager accountManager, ILogger<AccountController> logger)
    {
        _accountManager = accountManager;
        _logger = logger;
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
    public IActionResult UpdateAccount([FromBody] Account updatedAccount)
    {
        try
        {
            var account = _accountManager.UpdateAccount(updatedAccount);
            return Ok(account);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}