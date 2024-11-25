using BL.DTOs.Accounts;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    AccountDto GetAccountById(string id);
    List<PreferenceDto> GetPreferencesByUserId(Guid userId);
    AccountDto UpdateAccount(AccountDto account);
    public AccountDto UpdateFamilySize(AccountDto updatedAccount);
    void CreateAccount(string username, string email, Guid userId);

    public AccountDto AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto);
}