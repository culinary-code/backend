using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DAL.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public InvitationController(IInvitationManager invitationManager, ILogger<InvitationController> logger, IIdentityProviderService identityProviderService, IGroupManager groupManager, IAccountManager accountManager)
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
        try
        {
            Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
            Guid groupId = request.GroupId;
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                AccountDto inviter = await _accountManager.GetAccountById(userId.ToString());
                var invitation = new SendInvitationRequestDto
                {
                    GroupId = groupId,
                    InviterId = userId,
                    Email = request.Email, 
                    InviterName = inviter.Name, 
                    InvitedUserName = request.InvitedUserName
                };

                await _invitationManager.SendInvitationAsync(invitation);
                return Ok(invitation);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error sending invitation: {ex.Message}");
                return StatusCode(500, "An error occurred while sending the invitation.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending invitation: {ex.Message}");
        }
        return Ok("Success");
    }

    [HttpGet("acceptInvitation/{token}")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        try
        {
            var userId = Guid.Parse("718a1b80-7ae4-4ae0-a26c-87770f54d517"); 

            var invitation = await _invitationManager.ValidateInvitationTokenAsync(token);
            if (invitation == null)
            {
                return BadRequest("Invalid or expired invitation token.");
            }

            await _groupManager.AddUserToGroupAsync(invitation.GroupId, userId);

            await _invitationManager.AcceptInvitationAsync(invitation);

            return Ok(invitation);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error accepting invitation: {ex.Message}");
            return StatusCode(500, "An error occurred while processing the invitation.");
        }
    }

}