using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Results;
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

    public async Task<Result<AccountDto>> GetAccountById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var accountResult = await _accountRepository.ReadAccount(parsedGuid);
        if (!accountResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;
        
        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }

    public async Task<Result<List<PreferenceDto>>> GetPreferencesByUserId(Guid userId)
    {
        var accountResult = await _accountRepository.ReadAccountWithPreferencesByAccountId(userId);
        if (!accountResult.IsSuccess)
        {
            return Result<List<PreferenceDto>>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;
        
        var preferences = account!.Preferences;
        return Result<List<PreferenceDto>>.Success(_mapper.Map<List<PreferenceDto>>(preferences));
    }

    public async Task<Result<List<RecipeDto>>> GetFavoriteRecipesByUserId(Guid userId)
    {
        var favoriteRecipesResult = await _accountRepository.ReadFavoriteRecipesByUserIdNoTracking(userId);
        if (!favoriteRecipesResult.IsSuccess)
        {
            return Result<List<RecipeDto>>.Failure(favoriteRecipesResult.ErrorMessage!, favoriteRecipesResult.FailureType);
        }
        var favoriteRecipes = favoriteRecipesResult.Value;
        
        return Result<List<RecipeDto>>.Success(_mapper.Map<List<RecipeDto>>(favoriteRecipes));
    }

    public async Task<Result<AccountDto>> UpdateAccount(AccountDto updatedAccount)
    {
        var accountResult = await _accountRepository.ReadAccount(updatedAccount.AccountId);
        if (!accountResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;

        account!.Name = updatedAccount.Name;

        await _accountRepository.UpdateAccount(account);
        _logger.LogInformation($"Updating user: {updatedAccount.AccountId}, new username: {updatedAccount.Name}");

        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }

    public Task<Result<Unit>> DeleteAccount(Guid userId)
    {
        return _accountRepository.DeleteAccount(userId);
    }

    public async Task<Result<AccountDto>> UpdateFamilySize(AccountDto updatedAccount)
    {
        var accountResult = await _accountRepository.ReadAccount(updatedAccount.AccountId);
        if (!accountResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;
        
        var updateResult = await _accountRepository.UpdateAccount(account!);
        if (!updateResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(updateResult.ErrorMessage!, updateResult.FailureType);
        }
        _logger.LogInformation(
            $"Updating user: {updatedAccount.AccountId}, new familySize: {updatedAccount.FamilySize}");

        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }
    
    public async Task<Result<Unit>> CreateAccount(string username, string email, Guid userId)
    {
        var createResult = await _accountRepository.CreateAccount(new Account()
            {
                AccountId = userId,
                Name = username,
                Email = email,
            }
        );
        return createResult;
    }

    public async Task<Result<AccountDto>> AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto)
    {
        var accountResult = await _accountRepository.ReadAccountWithPreferencesByAccountId(accountId);
        if (!accountResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;

        if (account!.Preferences.Any(p => p.PreferenceName.ToLower() == preferenceDto.PreferenceName.ToLower()))
        {
            _logger.LogError("Account already has this preference");
            return Result<AccountDto>.Failure("Account already has this preference", ResultFailureType.Error);
        }

        var preferenceResult = await _preferenceRepository.ReadPreferenceByNameNoTracking(preferenceDto.PreferenceName);
        
        Preference newPreference;

        if (!preferenceResult.IsSuccess)
        {
            var newPreferenceResult = await _preferenceRepository.CreatePreference(new Preference()
            {
                PreferenceName = preferenceDto.PreferenceName,
                StandardPreference = false
            });
            if (!newPreferenceResult.IsSuccess)
            {
                return Result<AccountDto>.Failure(newPreferenceResult.ErrorMessage!, newPreferenceResult.FailureType);
            }
            newPreference = newPreferenceResult.Value!;
        }
        else
        {
            newPreference = preferenceResult.Value!;
        }

        account.Preferences.Add(newPreference);
        var updateResult = await _accountRepository.UpdateAccount(account);
        if (!updateResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(updateResult.ErrorMessage!, updateResult.FailureType);
        }
        _logger.LogInformation($"Added custom preference '{newPreference.PreferenceName}' to account {accountId}");

        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }

    public async Task<Result<AccountDto>> AddFavoriteRecipeToAccount(Guid accountId, Guid recipeId)
    {
        var accountResult = await _accountRepository.ReadAccount(accountId);
        if (!accountResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;

        var recipeResult = await _recipeRepository.ReadRecipeById(recipeId);
        if (!recipeResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(recipeResult.ErrorMessage!, recipeResult.FailureType);
        }
        var recipe = recipeResult.Value;

        var favoriteRecipe = new FavoriteRecipe()
        {
            Recipe = recipe,
            Account = account,
            CreatedAt = DateTime.UtcNow
        };

        account!.FavoriteRecipes.Add(favoriteRecipe);
        var updateAccountResult = await _accountRepository.UpdateAccount(account);
        if (!updateAccountResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(updateAccountResult.ErrorMessage!, updateAccountResult.FailureType);
        }
        
        recipe!.LastUsedAt = DateTime.UtcNow;
        var updateRecipeResult = await _recipeRepository.UpdateRecipe(recipe);
        if (!updateRecipeResult.IsSuccess)
        {
            return Result<AccountDto>.Failure(updateRecipeResult.ErrorMessage!, updateRecipeResult.FailureType);
        }

        _logger.LogInformation($"Added new favorite recipe '{recipe.RecipeName}' to account {accountId}");
        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }

    public async Task<Result<Unit>> RemovePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        await _accountRepository.DeletePreferenceFromAccount(accountId, preferenceId);
        _logger.LogInformation($"Removed preference with ID {preferenceId} from account {accountId}");
        return Result<Unit>.Success(new Unit());
    }
    public async Task<Result<Unit>> RemoveFavoriteRecipeFromAccount(Guid accountId, Guid recipeId)
    {
        await _accountRepository.DeleteFavoriteRecipeByUserId(accountId, recipeId);
        _logger.LogInformation($"Removed favorite recipe with ID {recipeId} from account {accountId}");
        return Result<Unit>.Success(new Unit());
    }
}