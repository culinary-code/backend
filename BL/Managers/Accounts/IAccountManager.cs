using BL.DTOs.Accounts;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    AccountDto GetAccountById(string id);
    AccountDto UpdateAccount(AccountDto account);
    public AccountDto UpdateFamilySize(AccountDto updatedAccount);
    void CreateAccount(string username, string email, Guid userId);
}