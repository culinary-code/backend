using System;
using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Accounts;

public interface IAccountRepository
{
    Task<Account> ReadAccount(Guid id);
    Task<Account> ReadAccountWithPreferencesByAccountId(Guid id);
    Task<Account> ReadAccountWithMealPlannerNextWeekAndWithGroceryList(Guid id);
    Task<List<Recipe?>> ReadFavoriteRecipesByUserId(Guid userId);
    Task UpdateAccount(Account account);
    Task CreateAccount(Account account);
    Task DeletePreferenceFromAccount(Guid accountId, Guid preferenceId);
}