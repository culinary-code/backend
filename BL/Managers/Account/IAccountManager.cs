using BL.DTOs.Accounts;
using DOM.Accounts;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    Account GetAccountById(string id);
    Account UpdateAccount(Account account);
    void CreateAccount(string username, string email, Guid userId);
}