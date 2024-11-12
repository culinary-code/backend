using BL.DTOs.Accounts;
using DAL.Accounts;
using DOM.Accounts;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Accounts;

public class AccountManager : IAccountManager
{
    private readonly IAccountRepository _accountRepository;

    public AccountManager(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }


    public Account UpdateAccount(Account account)
    {
        _accountRepository.UpdateAccount(account);
        return account;
    }
}