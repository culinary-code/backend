﻿using System;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;

namespace DAL.Accounts;

public interface IAccountRepository
{
    Task<Result<Account>> ReadAccount(Guid id);
    Task<Result<Account>> ReadAccountWithPreferencesByAccountId(Guid id);
    Task<Result<Account>> ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking(Guid id);
    Task<Result<List<Recipe?>>> ReadFavoriteRecipesByUserIdNoTracking(Guid userId);
    Task<Result<Unit>> UpdateAccount(Account account);
    Task<Result<Unit>> CreateAccount(Account account);
    Task<Result<Unit>> DeletePreferenceFromAccount(Guid accountId, Guid preferenceId);
    Task<Result<Unit>> DeleteFavoriteRecipeByUserId(Guid userId, Guid recipeId);
}