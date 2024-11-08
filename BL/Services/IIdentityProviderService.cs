namespace BL.Services;

public interface IIdentityProviderService
{ 
    Task RegisterUserAsync(string username, string email, string password);
}