using System;
using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Accounts;

public interface IAccountRepository
{
    Account ReadAccount(Guid id);
    Account ReadAccountWithPreferencesByAccountId(Guid id);
    IEnumerable<Recipe> ReadFavoriteRecipesByUserId(Guid userId);
    void UpdateAccount(Account account);
    void CreateAccount(Account account);
    void DeletePreferenceFromAccount(Guid accountId, Guid preferenceId);

}