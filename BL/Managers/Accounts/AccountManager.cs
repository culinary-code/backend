using AutoMapper;
using BL.DTOs.Accounts;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Accounts;

public class AccountManager : IAccountManager
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPreferenceRepository _preferenceRepository;
    private readonly ILogger<AccountManager> _logger;
    private readonly IMapper _mapper;

    public AccountManager(IAccountRepository accountRepository, ILogger<AccountManager> logger, IMapper mapper, IPreferenceRepository preferenceRepository)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _mapper = mapper;
        _preferenceRepository = preferenceRepository;
    }
    
    public AccountDto GetAccountById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var account = _accountRepository.ReadAccount(parsedGuid);
        return _mapper.Map<AccountDto>(account);
    }

    public List<PreferenceDto> GetPreferencesByUserId(Guid userId)
    {
        var account = _accountRepository.ReadAccountWithPreferencesByAccountId(userId);
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
    var account = _accountRepository.ReadAccountWithPreferencesByAccountId(accountId);
    
    if (account.Preferences.Any(p => p.PreferenceName.ToLower() == preferenceDto.PreferenceName.ToLower()))
    {
        _logger.LogError("Account already has this preference");
        return _mapper.Map<AccountDto>(account);
    }

    var standardPreferences = _preferenceRepository.ReadStandardPreferences();

    var preference = _mapper.Map<Preference>(preferenceDto);
    
    var preferences = account.Preferences.ToList();

    if (standardPreferences.Any(p => p.PreferenceName.ToLower() == preferenceDto.PreferenceName.ToLower()))
    {
        var standardPreference = standardPreferences.First(p => p.PreferenceName.ToLower() == preferenceDto.PreferenceName.ToLower());
        preferences.Add(standardPreference); 

        account.Preferences = preferences;

        _accountRepository.UpdateAccount(account);
        
        _logger.LogInformation($"Added standard preference '{standardPreference.PreferenceName}' to account {accountId}");

        return _mapper.Map<AccountDto>(account);
    }

    preferences.Add(preference);
    account.Preferences = preferences;

    _accountRepository.UpdateAccount(account);

    _logger.LogInformation($"Added custom preference '{preference.PreferenceName}' to account {accountId}");

    return _mapper.Map<AccountDto>(account);
}
    

    public void RemovePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        var account = _accountRepository.ReadAccountWithPreferencesByAccountId(accountId);
        if (account == null)
        {
            _logger.LogError("Account not found");
            throw new AccountNotFoundException("Account not found");
        }
        _accountRepository.DeletePreferenceFromAccount(accountId, preferenceId);
        _logger.LogInformation($"Removed preference with ID {preferenceId} from account {accountId}");
    }
}