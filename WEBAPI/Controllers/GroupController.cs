using BL.Managers.Accounts;
using BL.Services;
using DAL.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

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

    public GroupController(ILogger<GroupController> logger, IGroupManager groupManager, IAccountManager accountManager,
        IIdentityProviderService identityProviderService)
    {
        _logger = logger;
        _groupManager = groupManager;
        _accountManager = accountManager;
        _identityProviderService = identityProviderService;
    }

    // Creates a group with the user requesting this method in it
    [HttpPost("createGroup")]
    public async Task<IActionResult> CreateGroup([FromQuery] string groupName)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var ownerId = userIdResult.Value;

        var result = await _groupManager.CreateGroupAsync(groupName, ownerId);
        return result.ToActionResult();
    }

    // Adds a user to the group with that groupId
    [HttpPost("{groupId}/addUserToGroup")]
    public async Task<IActionResult> AddUserToGroup(Guid groupId)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(
                Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var result = await _groupManager.AddUserToGroupAsync(groupId, userId);
        return result.ToActionResult();
    }

    // Gets all groups the user is connected to
    [HttpGet("getGroups")]
    public async Task<IActionResult> GetGroups()
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(
                Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;
        var groups = await _groupManager.GetAllGroupsByUserIdAsync(userId);
        return groups.ToActionResult();
    }

    // Removes a user from a group with that groupId
    [HttpPost("{groupId}/removeUser")]
    public async Task<IActionResult> RemoveUserFromGroup(Guid groupId)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(
                Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var result = await _groupManager.RemoveUserFromGroup(groupId, userId);
        return result.ToActionResult();
    }
}