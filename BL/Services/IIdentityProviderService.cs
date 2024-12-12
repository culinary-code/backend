using BL.DTOs.Accounts;
using DOM.Accounts;
using DOM.Results;

namespace BL.Services;

public interface IIdentityProviderService
{ 
    Task<Result<Unit>> RegisterUserAsync(string username, string email, string password);
    Task<Result<Unit>> UpdateUsername(AccountDto account, string username);
    Result<Guid> GetGuidFromAccessToken(string accessToken);
    Result<(string, string)> GetUsernameAndEmailFromAccessToken(string accessToken);
    Result<Unit> DeleteUser(Guid accountId);
}