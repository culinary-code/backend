using BL.DTOs.Accounts;
using DOM.Accounts;
using DOM.Results;

namespace BL.Managers.Accounts;

public interface IInvitationManager
{
    Task<Result<Unit>> SendInvitationAsync(SendInvitationRequestDto request);
    Task<Result<Invitation>> ValidateInvitationTokenAsync(string token);
    Task<Result<Unit>> AcceptInvitationAsync(Invitation invitation);
}