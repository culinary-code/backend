using DOM.Accounts;
using DOM.Results;

namespace DAL.Accounts;

public interface IInvitationRepository
{
    Task<Result<Unit>> SaveInvitationAsync(Invitation invitation);
    Task<Result<Invitation>> ReadInvitationByTokenAsync(string token);
    Task<Result<Unit>> DeleteInvitationAsync(Invitation invitation);
}