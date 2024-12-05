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

    public InvitationController(IInvitationManager invitationManager, ILogger<InvitationController> logger, IIdentityProviderService identityProviderService, IGroupManager groupManager)
    {
        _invitationManager = invitationManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
        _groupManager = groupManager;
    }
    
    [HttpPost("sendInvitation")]
    public async Task<IActionResult> SendInvitation([FromBody] SendInvitationRequestDto request)
    {
        try
        {
            // TODO: kijk wanneer je frontend gaat doe, miss validatie ofzo toevoegen hier eventueel

            await _invitationManager.SendInvitationAsync(request);
            return Ok(new { Message = "Invitation sent successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending invitation: {ex.Message}");
            return StatusCode(500, "An error occurred while sending the invitation.");
        }
    }

    // TODO: for testing without frontend
    [HttpGet("acceptInvitation/{token}")]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        try
        {
            var userId = Guid.Parse("718a1b80-7ae4-4ae0-a26c-87770f54d517");

            // Validate the invitation token
            var invitation = await _invitationManager.ValidateInvitationTokenAsync(token);
            if (invitation == null)
            {
                return BadRequest("Invalid or expired invitation token.");
            }

            await _groupManager.AddUserToGroupAsync(invitation.GroupId, userId);

            // Mark the invitation as accepted
            await _invitationManager.AcceptInvitationAsync(invitation);

            return Ok(new { Message = "You have successfully joined the group." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error accepting invitation: {ex.Message}");
            return StatusCode(500, "An error occurred while processing the invitation.");
        }
    }
}