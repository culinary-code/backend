using BL.DTOs.Accounts;
using DOM.Accounts;

namespace BL.Managers.Accounts;

public interface IInvitationManager
{
    Task SendInvitationAsync(SendInvitationRequestDto request);
    Task<Invitation> ValidateInvitationTokenAsync(string token);
    Task AcceptInvitationAsync(Invitation invitation);
}