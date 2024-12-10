using BL.DTOs.Accounts;
using DOM.Results;

namespace BL.Managers.Recipes;

public interface IReviewManager
{
    Task<Result<ReviewDto>> GetReviewDtoById(Guid id);
    Task<Result<ICollection<ReviewDto>>> GetReviewDtosByRecipeId(Guid recipeId);
    Task<Result<ReviewDto>> CreateReview(Guid accountId, Guid recipeId, string description, int amountOfStars);
}