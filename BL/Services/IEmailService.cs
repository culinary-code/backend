using DOM.Exceptions;

namespace BL.Services;

public interface IEmailService
{
    Task<Result<Unit>> SendInvitationEmailAsync(string email, string token, string invitedUser, string inviterName);
}