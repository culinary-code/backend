using AutoMapper;
using BL.DTOs.Accounts;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Recipes;

public class ReviewManager : IReviewManager
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewManager> _logger;

    public ReviewManager(IReviewRepository reviewRepository, IMapper mapper, ILogger<ReviewManager> logger, IAccountRepository accountRepository, IRecipeRepository recipeRepository)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _logger = logger;
        _accountRepository = accountRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<ReviewDto> GetReviewDtoById(Guid id)
    {
        return _mapper.Map<ReviewDto>(await _reviewRepository.ReadReviewById(id));
    }

    public async Task<ICollection<ReviewDto>> GetReviewDtosByRecipeId(Guid recipeId)
    {
        return _mapper.Map<ICollection<ReviewDto>>(await _reviewRepository.ReadReviewsByRecipeId(recipeId));
    }

    public async Task<ReviewDto> CreateReview(Guid accountId, Guid recipeId, string description, int amountOfStars)
    {
        _logger.LogInformation($"Creating review for account with id {accountId} and recipe with id {recipeId}");
        var account = _accountRepository.ReadAccount(accountId);
        var recipe = _recipeRepository.ReadRecipeById(recipeId);
        
        
        // check if account already has a review on this recipe, which is not allowed
        var existingReview = await _reviewRepository.ReadReviewsByRecipeId(recipeId);
        foreach (var checkReview in existingReview)
        {
            if (checkReview.Account!.AccountId == accountId)
            {
                throw new ReviewAlreadyExistsException($"Account with id {accountId} already has a review on recipe with id {recipeId}");
            }
        }
        
        var review = new Review
        {
            Account = account,
            Recipe = recipe,
            Description = description,
            AmountOfStars = amountOfStars,
            CreatedAt = DateTime.UtcNow
        };
        await _reviewRepository.CreateReview(review);
        _logger.LogInformation($"Review created with id {review.ReviewId}");
        
        recipe.AmountOfRatings++;
        recipe.AverageRating = (recipe.AverageRating * (recipe.AmountOfRatings - 1) + amountOfStars) / recipe.AmountOfRatings;
        _recipeRepository.UpdateRecipe(recipe);
        
        return _mapper.Map<ReviewDto>(review);
    }
}