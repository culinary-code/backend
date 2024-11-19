using AutoMapper;
using BL.DTOs.Accounts;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Accounts;

public class AccountManager : IAccountManager
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountManager> _logger;
    private readonly IMapper _mapper;

    public AccountManager(IAccountRepository accountRepository, ILogger<AccountManager> logger, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _mapper = mapper;
    }
    
    public AccountDto GetAccountById(string id)
    {
        id = "6ceed686-8784-4386-9a0b-899dd7fde3e3";
        Guid parsedGuid = Guid.Parse(id);
        var account = _accountRepository.ReadAccount(parsedGuid);
        return _mapper.Map<AccountDto>(account);
    }

    public AccountDto UpdateAccount(AccountDto updatedAccount)
    {
        var account = _accountRepository.ReadAccount(updatedAccount.AccountId);
        if (account == null)
        {
            _logger.LogError("Account not found");
            throw new AccountNotFoundException("Account not found");
        }
        account.Name = updatedAccount.Name;
        _accountRepository.UpdateAccount(account);
        _logger.LogInformation($"Updating user: {updatedAccount.AccountId}, new username: {updatedAccount.Name}");
        
        return _mapper.Map<AccountDto>(account);
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