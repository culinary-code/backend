using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
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

    public List<RecipeDto> GetFavoriteRecipesByUserId(Guid userId)
    {
        var favoriteRecipes = _accountRepository.ReadFavoriteRecipesByUserId(userId);
        return _mapper.Map<List<RecipeDto>>(favoriteRecipes);
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

    var preference = _preferenceRepository.ReadPreferenceByName(preferenceDto.PreferenceName);

    Preference newPreference;
    
    if (preference == null)
    {
        newPreference = _preferenceRepository.CreatePreference(new Preference()
        {
            PreferenceName = preferenceDto.PreferenceName,
            StandardPreference = false
        });
    }
    else
    {
        newPreference = preference;
    }
    
    account.Preferences.Add(newPreference);
    _accountRepository.UpdateAccount(account);
    _logger.LogInformation($"Added custom preference '{newPreference.PreferenceName}' to account {accountId}");
    
    return _mapper.Map<AccountDto>(account);
}
    

    public void RemovePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        _accountRepository.DeletePreferenceFromAccount(accountId, preferenceId);
        _logger.LogInformation($"Removed preference with ID {preferenceId} from account {accountId}");
    }
}