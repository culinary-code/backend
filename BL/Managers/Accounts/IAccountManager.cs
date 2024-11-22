using BL.DTOs.Accounts;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    AccountDto GetAccountById(string id);
    List<PreferenceDto> GetPreferencesByUserId(Guid userId);
    AccountDto UpdateAccount(AccountDto account);
    public AccountDto UpdateFamilySize(AccountDto updatedAccount);
    public AccountDto UpdatePreferences(Guid userId, List<PreferenceDto> preferences);
    void CreateAccount(string username, string email, Guid userId);
}