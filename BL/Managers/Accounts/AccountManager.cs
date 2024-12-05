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
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<AccountManager> _logger;
    private readonly IMapper _mapper;

    public AccountManager(IAccountRepository accountRepository, ILogger<AccountManager> logger, IMapper mapper,
        IPreferenceRepository preferenceRepository, IRecipeRepository recipeRepository)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _mapper = mapper;
        _preferenceRepository = preferenceRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<AccountDto> GetAccountById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var account = await _accountRepository.ReadAccount(parsedGuid);
        return _mapper.Map<AccountDto>(account);
    }

    public async Task<List<PreferenceDto>> GetPreferencesByUserId(Guid userId)
    {
        var account = await _accountRepository.ReadAccountWithPreferencesByAccountId(userId);
        var preferences = account.Preferences ?? new List<Preference>();
        return _mapper.Map<List<PreferenceDto>>(preferences);
    }

    public async Task<List<RecipeDto>> GetFavoriteRecipesByUserId(Guid userId)
    {
        var favoriteRecipes = await _accountRepository.ReadFavoriteRecipesByUserId(userId);
        return _mapper.Map<List<RecipeDto>>(favoriteRecipes);
    }

    public async Task<AccountDto> UpdateAccount(AccountDto updatedAccount)
    {
        var account = await _accountRepository.ReadAccount(updatedAccount.AccountId);
        if (account == null)
        {
            _logger.LogError("Account not found");
            throw new AccountNotFoundException("Account not found");
        }

        account.Name = updatedAccount.Name;

        await _accountRepository.UpdateAccount(account);
        _logger.LogInformation($"Updating user: {updatedAccount.AccountId}, new username: {updatedAccount.Name}");

        return _mapper.Map<AccountDto>(account);
    }

    public async Task<AccountDto> UpdateFamilySize(AccountDto updatedAccount)
    {
        var account = await _accountRepository.ReadAccount(updatedAccount.AccountId);
        account.FamilySize = updatedAccount.FamilySize;
        await _accountRepository.UpdateAccount(account);
        _logger.LogInformation(
            $"Updating user: {updatedAccount.AccountId}, new familySize: {updatedAccount.FamilySize}");

        return _mapper.Map<AccountDto>(account);
    }
    
    public async Task CreateAccount(string username, string email, Guid userId)
    {
        await _accountRepository.CreateAccount(new Account()
            {
                AccountId = userId,
                Name = username,
                Email = email,
            }
        );
    }

    public async Task<AccountDto> AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto)
    {
        var account = await _accountRepository.ReadAccountWithPreferencesByAccountId(accountId);

        if (account.Preferences.Any(p => p.PreferenceName.ToLower() == preferenceDto.PreferenceName.ToLower()))
        {
            _logger.LogError("Account already has this preference");
            return _mapper.Map<AccountDto>(account);
        }

        var preference = await _preferenceRepository.ReadPreferenceByName(preferenceDto.PreferenceName);

        Preference newPreference;

        if (preference == null)
        {
            newPreference = await _preferenceRepository.CreatePreference(new Preference()
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
        await _accountRepository.UpdateAccount(account);
        _logger.LogInformation($"Added custom preference '{newPreference.PreferenceName}' to account {accountId}");

        return _mapper.Map<AccountDto>(account);
    }

    public async Task<AccountDto> AddFavoriteRecipeToAccount(Guid accountId, Guid recipeId)
    {
        var account = await _accountRepository.ReadAccount(accountId);

        var recipe = await _recipeRepository.ReadRecipeById(recipeId);

        var favoriteRecipe = new FavoriteRecipe()
        {
            Recipe = recipe,
            Account = account,
            CreatedAt = DateTime.UtcNow
        };

        account.FavoriteRecipes.Add(favoriteRecipe);
        await _accountRepository.UpdateAccount(account);

        recipe.LastUsedAt = DateTime.UtcNow;
        await _recipeRepository.UpdateRecipe(recipe);

        _logger.LogInformation($"Added new favorite recipe '{recipe.RecipeName}' to account {accountId}");
        return _mapper.Map<AccountDto>(account);
    }

    public async Task RemovePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        await _accountRepository.DeletePreferenceFromAccount(accountId, preferenceId);
        _logger.LogInformation($"Removed preference with ID {preferenceId} from account {accountId}");
    }
    public async Task RemoveFavoriteRecipeFromAccount(Guid accountId, Guid recipeId)
    {
        await _accountRepository.DeleteFavoriteRecipeByUserId(accountId, recipeId);
        _logger.LogInformation($"Removed favorite recipe with ID {recipeId} from account {accountId}");
    }
}