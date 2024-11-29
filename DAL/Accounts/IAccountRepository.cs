using System;
using DOM.Accounts;

namespace DAL.Accounts;

public interface IAccountRepository
{
    Account ReadAccount(Guid id);
    Account ReadAccountWithPreferencesByAccountId(Guid id);
    Account ReadAccountWithMealPlannerNextWeekAndWithGroceryList(Guid id);
    void UpdateAccount(Account account);
    void CreateAccount(Account account);
    void DeletePreferenceFromAccount(Guid accountId, Guid preferenceId);

}