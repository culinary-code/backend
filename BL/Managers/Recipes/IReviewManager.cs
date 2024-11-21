using BL.DTOs.Accounts;

namespace BL.Managers.Recipes;

public interface IReviewManager
{
    Task<ReviewDto> GetReviewDtoById(Guid id);
    Task<ICollection<ReviewDto>> GetReviewDtosByRecipeId(Guid recipeId);
    Task<ReviewDto> CreateReview(Guid accountId, Guid recipeId, string description, int amountOfStars);
}