using System;

namespace DAL.Accounts;

public interface IAccountRepository
{
    DOM.Accounts.Account ReadAccount(Guid id);
    void UpdateAccount(DOM.Accounts.Account account);
    void CreateAccount(DOM.Accounts.Account account);
}