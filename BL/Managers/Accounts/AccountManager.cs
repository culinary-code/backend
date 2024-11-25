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
        Guid parsedGuid = Guid.Parse(id);
        var account = _accountRepository.ReadAccount(parsedGuid);
        return _mapper.Map<AccountDto>(account);
    }

    public List<PreferenceDto> GetPreferencesByUserId(Guid userId)
    {
        var account = _accountRepository.ReadAccountPreferencesByAccountId(userId);
        var preferences = account.Preferences ?? new List<Preference>();
        return _mapper.Map<List<PreferenceDto>>(preferences);
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
    
    public AccountDto UpdateFamilySize(AccountDto updatedAccount)
    {
        var account = _accountRepository.ReadAccount(updatedAccount.AccountId);
        account.FamilySize = updatedAccount.FamilySize;
        _accountRepository.UpdateAccount(account);
        _logger.LogInformation($"Updating user: {updatedAccount.AccountId}, new familySize: {updatedAccount.FamilySize}");
        
        return _mapper.Map<AccountDto>(account);
    }


    public void CreateAccount(string username, string email, Guid userId)
    {
        _accountRepository.CreateAccount(new Account()
            {
                AccountId = userId,
                Name = username,
                Email = email,
                
            }
        );
    }

    public AccountDto AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto)
    {
        var account = _accountRepository.ReadAccountPreferencesByAccountId(accountId);
        if (account == null)
        {
            _logger.LogError("Account not found");
            throw new AccountNotFoundException("Account not found");
        }

        if (account.Preferences.Any(p => p.PreferenceName.ToLower() == preferenceDto.PreferenceName.ToLower()))
        {
            _logger.LogError("Account already has this preference");
            return _mapper.Map<AccountDto>(account);
        }
        
        var preference = _mapper.Map<Preference>(preferenceDto);

        var preferences = account.Preferences.ToList();
        preferences.Add(preference);
        account.Preferences = preferences;

        _accountRepository.UpdateAccount(account);
        _logger.LogInformation($"Added preference '{preference.PreferenceName}' to account {accountId}");

        return _mapper.Map<AccountDto>(account);
    }

    public void RemovePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        var account = _accountRepository.ReadAccountPreferencesByAccountId(accountId);
        if (account == null)
        {
            _logger.LogError("Account not found");
            throw new AccountNotFoundException("Account not found");
        }
        _accountRepository.DeletePreferenceFromAccount(accountId, preferenceId);
        _logger.LogInformation($"Removed preference with ID {preferenceId} from account {accountId}");
    }
}