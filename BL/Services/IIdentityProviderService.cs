using DOM.Accounts;

namespace BL.Services;

public interface IIdentityProviderService
{ 
    Task RegisterUserAsync(string username, string email, string password);
    Task UpdateUsernameAsync(Account account, string username);
}