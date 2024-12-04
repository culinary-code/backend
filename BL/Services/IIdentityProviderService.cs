using BL.DTOs.Accounts;
using DOM.Accounts;

namespace BL.Services;

public interface IIdentityProviderService
{ 
    Task RegisterUserAsync(string username, string email, string password);
    Task UpdateUsernameAsync(AccountDto account, string username);
    Guid GetGuidFromAccessToken(string accessToken);
    (string, string) GetUsernameAndEmailFromAccessToken(string accessToken);
}