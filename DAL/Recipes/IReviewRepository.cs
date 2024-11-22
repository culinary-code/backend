using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Recipes;

public interface IReviewRepository
{
    Task<Review> ReadReviewWithAccountByReviewId(Guid id);
    Task<ICollection<Review>> ReadReviewsWithAccountByRecipeId(Guid recipeId);
    Task CreateReview(Review review);
    Task<bool> ReviewExistsForAccountAndRecipe(Guid accountId, Guid recipeId);
}