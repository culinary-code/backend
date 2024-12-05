using BL.Managers.Accounts;
using BL.Services;
using DAL.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly ILogger<GroupController> _logger;
    private readonly IGroupManager _groupManager;
    private readonly IAccountManager _accountManager;
    private readonly IIdentityProviderService _identityProviderService;

    public GroupController(ILogger<GroupController> logger, IGroupManager groupManager, IAccountManager accountManager, IIdentityProviderService identityProviderService)
    {
        _logger = logger;
        _groupManager = groupManager;
        _accountManager = accountManager;
        _identityProviderService = identityProviderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromQuery] string groupName)
    {
        try
        {
            Guid ownerId =
                _identityProviderService.GetGuidFromAccessToken(
                    Request.Headers["Authorization"].ToString().Substring(7));

            await _groupManager.CreateGroupAsync(groupName, ownerId);
            return Ok(groupName);
        }
        catch
        {
            _logger.LogError($"Group {groupName} not created");
            return BadRequest();
        }
    }
}