﻿using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Recipes;

public interface IReviewRepository
{
    Task<Review> ReadReviewWithAccountByReviewIdNoTracking(Guid id);
    Task<ICollection<Review>> ReadReviewsWithAccountByRecipeIdNoTracking(Guid recipeId);
    Task CreateReview(Review review);
    Task<bool> ReviewExistsForAccountAndRecipe(Guid accountId, Guid recipeId);
}