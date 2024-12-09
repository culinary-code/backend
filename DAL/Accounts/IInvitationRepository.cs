using DOM.Accounts;

namespace DAL.Accounts;

public interface IInvitationRepository
{
    Task SaveInvitationAsync(Invitation invitation);
    Task<Invitation> GetInvitationByTokenAsync(string token);
    Task DeleteInvitationAsync(Invitation invitation);
}