using BL.DTOs.Accounts;
using DOM.Accounts;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    Account UpdateAccount(Account account);
}