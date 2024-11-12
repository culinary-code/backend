using BL.DTOs.Accounts;
using DOM.Accounts;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    AccountDto GetAccountById(string id);
    AccountDto UpdateAccount(AccountDto account);
    void CreateAccount(string username, string email, Guid userId);
}