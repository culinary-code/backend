using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Recipes;

public interface IReviewRepository
{
    Task<Review> ReadReviewById(Guid id);
    Task<ICollection<Review>> ReadReviewsByRecipeId(Guid recipeId);
    Task CreateReview(Review review);
}