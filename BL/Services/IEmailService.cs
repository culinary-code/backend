namespace BL.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string email, string token, string invitedUser, string inviterName);
}