using DOM.Accounts;

namespace DAL.Accounts;

public interface IInvitationRepository
{
    Task SaveInvitationAsync(Invitation invitation);
    Task<Invitation> ReadInvitationByTokenAsync(string token);
    Task DeleteInvitationAsync(Invitation invitation);
}