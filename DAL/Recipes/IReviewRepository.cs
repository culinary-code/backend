using DOM.Accounts;
using DOM.Recipes;
using DOM.Results;

namespace DAL.Recipes;

public interface IReviewRepository
{
    Task<Result<Review>> ReadReviewWithAccountByReviewIdNoTracking(Guid id);
    Task<Result<ICollection<Review>>> ReadReviewsWithAccountByRecipeIdNoTracking(Guid recipeId);
    Task<Result<Unit>> CreateReview(Review review);
    Task<Result<bool>> ReviewExistsForAccountAndRecipe(Guid accountId, Guid recipeId);
}