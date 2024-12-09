using System;
using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Accounts;

public interface IAccountRepository
{
    Task<Account> ReadAccount(Guid id);
    Task<Account> ReadAccountWithPreferencesByAccountId(Guid id);
    Task<Account> ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking(Guid id);
    Task<List<Recipe?>> ReadFavoriteRecipesByUserIdNoTracking(Guid userId);
    Task UpdateAccount(Account account);
    Task CreateAccount(Account account);
    Task DeletePreferenceFromAccount(Guid accountId, Guid preferenceId);
    Task DeleteFavoriteRecipeByUserId(Guid userId, Guid recipeId);
}