using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using DOM.Recipes;

namespace BL.Managers.Accounts;

public interface IAccountManager
{
    AccountDto GetAccountById(string id);
    List<PreferenceDto> GetPreferencesByUserId(Guid userId);
    List<RecipeDto> GetFavoriteRecipesByUserId(Guid userId);
    AccountDto UpdateAccount(AccountDto account);
    public AccountDto UpdateFamilySize(AccountDto updatedAccount);
    void CreateAccount(string username, string email, Guid userId);
    AccountDto AddPreferenceToAccount(Guid accountId, PreferenceDto preferenceDto);
    void RemovePreferenceFromAccount(Guid accountId, Guid preferenceId);
}