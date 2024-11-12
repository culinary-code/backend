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

    [HttpPut("/updateAccount")]
    public IActionResult UpdateAccount([FromBody] Account updatedAccount)
    {
        try
        {
            var account = _accountManager.UpdateAccount(updatedAccount);
            if (account is null)
            {
                return BadRequest();
            }
            return Ok(account);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}