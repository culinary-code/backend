namespace BL.Services;

public interface IIdentityProviderService
{ 
    Task RegisterUserAsync(string username, string? firstName, string? lastName, string email, string password);
}