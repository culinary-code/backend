using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DOM.Recipes;
using DOM.Results;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    Task<Result<AccountDto>> GetAccountById(Guid id);
    Task<Result<List<PreferenceDto>>> GetPreferencesByUserId(Guid userId);
    Task<Result<List<RecipeDto>>> GetFavoriteRecipesByUserId(Guid userId);
    Task<Result<AccountDto>> UpdateAccount(AccountDto account);
    Task<Result<AccountDto>> UpdateChosenGroup(Guid userId, Guid? chosenGroupId);
    Task<Result<Unit>> DeleteAccount(Guid accountId);
    Task<Result<AccountDto>> UpdateFamilySize(AccountDto updatedAccount);
    Task<Result<Unit>> CreateAccount(string username, string email, Guid userId);
    Task<Result<AccountDto>> AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto);
    Task<Result<AccountDto>> AddFavoriteRecipeToAccount(Guid accountId, Guid recipeId);
    Task<Result<Unit>> RemovePreferenceFromAccount(Guid accountId, Guid preferenceId);
    Task<Result<Unit>> RemoveFavoriteRecipeFromAccount(Guid accountId, Guid recipeId);
}