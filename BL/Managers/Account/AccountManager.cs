using BL.DTOs.Accounts;
using DAL.Accounts;
using DOM.Accounts;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Accounts;

public class AccountManager : IAccountManager
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountManager> _logger;

    public AccountManager(IAccountRepository accountRepository, ILogger<AccountManager> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }


    public Account GetAccountById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var account = _accountRepository.ReadAccount(parsedGuid);
        return account;
    }

    public Account UpdateAccount(Account updatedAccount)
    {
        var account = _accountRepository.ReadAccount(updatedAccount.AccountId);
        if (account == null)
        {
            throw new Exception("Account not found");
        }
        account.Name = updatedAccount.Name;
        _accountRepository.UpdateAccount(account);
        
        Console.WriteLine("old: " + account.Name + " updated" + updatedAccount.AccountId);
        
        _logger.LogInformation($"Updating user: {updatedAccount.AccountId}, new username: {updatedAccount.Name}");
        
        return account;
    }

    public void CreateAccount(string username, string email, Guid userId)
    {
        _accountRepository.CreateAccount(new Account()
            {
                AccountId = userId,
                Name = username,
                Email = email
            }
        );
    }
}