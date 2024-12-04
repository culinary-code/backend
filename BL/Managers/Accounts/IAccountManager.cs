using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DOM.Recipes;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    Task<AccountDto> GetAccountById(string id);
    Task<List<PreferenceDto>> GetPreferencesByUserId(Guid userId);
    Task<List<RecipeDto>> GetFavoriteRecipesByUserId(Guid userId);
    Task<AccountDto> UpdateAccount(AccountDto account);
    Task<AccountDto> UpdateFamilySize(AccountDto updatedAccount);
    Task CreateAccount(string username, string email, Guid userId);
    Task<AccountDto> AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto);
    Task<AccountDto> AddFavoriteRecipeToAccount(Guid accountId, Guid recipeId);
    Task RemovePreferenceFromAccount(Guid accountId, Guid preferenceId);
    Task RemoveFavoriteRecipeFromAccount(Guid accountId, Guid recipeId);
}