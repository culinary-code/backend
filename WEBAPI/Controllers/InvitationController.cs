using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DAL.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvitationController : ControllerBase
{
    private readonly IInvitationManager _invitationManager;
    private readonly ILogger<InvitationController> _logger;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IGroupManager _groupManager;
    private readonly IAccountManager _accountManager;

    public InvitationController(IInvitationManager invitationManager, ILogger<InvitationController> logger,
        IIdentityProviderService identityProviderService, IGroupManager groupManager, IAccountManager accountManager)
    {
        _invitationManager = invitationManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
        _groupManager = groupManager;
        _accountManager = accountManager;
    }

    [HttpPost("sendInvitation")]
    public async Task<IActionResult> SendInvitation([FromBody] SendInvitationRequestDto request)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(
                Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;
        var groupId = request.GroupId;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var inviterResult = await _accountManager.GetAccountById(userId.ToString());
        if (!inviterResult.IsSuccess) return inviterResult.ToActionResult();
        var inviter = inviterResult.Value!;
        var invitation = new SendInvitationRequestDto
        {
            GroupId = groupId,
            InviterId = userId,
            Email = request.Email,
            InviterName = inviter.Name,
            InvitedUserName = request.InvitedUserName
        };

        var result = await _invitationManager.SendInvitationAsync(invitation);
        return result.ToActionResult();
    }

    [HttpGet("acceptInvitation/{token}")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(
                Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var invitationResult = await _invitationManager.ValidateInvitationTokenAsync(token);
        if (!invitationResult.IsSuccess)
        {
            return invitationResult.ToActionResult();
        }

        var invitation = invitationResult.Value!;

        var addUserResult = await _groupManager.AddUserToGroupAsync(invitation.GroupId, userId);
        if (!addUserResult.IsSuccess) return addUserResult.ToActionResult();

        var acceptInvitationResult = await _invitationManager.AcceptInvitationAsync(invitation);
        return acceptInvitationResult.ToActionResult();
    }
}