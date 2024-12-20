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
    
    // Returns a link with an invitation token
    [HttpPost("sendInvitation")]
    public async Task<IActionResult> SendInvitation([FromBody] SendInvitationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(
                Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var inviterResult = await _accountManager.GetAccountById(userId);
        if (!inviterResult.IsSuccess) return inviterResult.ToActionResult();
        var inviter = inviterResult.Value!;

        var invitationRequest = new SendInvitationRequestDto
        {
            GroupId = request.GroupId,
            InviterId = userId,
            InviterName = inviter.Name,
        };

        var result = await _invitationManager.SendInvitationAsync(invitationRequest);
        if (!result.IsSuccess) return result.ToActionResult();

        return Ok(new { Link = result.Value });
    }
    
    // Accepts a user into a group through the invitation token
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

        var acceptInvitationResult = await _invitationManager.RemoveInvitationAsync(invitation);
        return acceptInvitationResult.ToActionResult();
    }
    
    // Validates the given token to see if it is used or outdated
    [HttpGet("validateInvitation/{token}")]
    public async Task<IActionResult> ValidateInvitation(string token)
    {
        var invitationResult = await _invitationManager.ValidateInvitationTokenAsync(token);
        return Ok(invitationResult.ToActionResult());
    }
}