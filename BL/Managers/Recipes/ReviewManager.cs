using AutoMapper;
using BL.DTOs.Accounts;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Results;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Recipes;

public class ReviewManager : IReviewManager
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewManager> _logger;

    public ReviewManager(IReviewRepository reviewRepository, IMapper mapper, ILogger<ReviewManager> logger,
        IAccountRepository accountRepository, IRecipeRepository recipeRepository)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _logger = logger;
        _accountRepository = accountRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<Result<ReviewDto>> GetReviewDtoById(Guid id)
    {
        var reviewResult = await _reviewRepository.ReadReviewWithAccountByReviewIdNoTracking(id);
        if (!reviewResult.IsSuccess)
        {
            return Result<ReviewDto>.Failure(reviewResult.ErrorMessage!, reviewResult.FailureType);
        }

        var review = reviewResult.Value!;

        return Result<ReviewDto>.Success(_mapper.Map<ReviewDto>(review));
    }

    public async Task<Result<ICollection<ReviewDto>>> GetReviewDtosByRecipeId(Guid recipeId)
    {
        var reviewsResult = await _reviewRepository.ReadReviewsWithAccountByRecipeIdNoTracking(recipeId);
        if (!reviewsResult.IsSuccess)
        {
            return Result<ICollection<ReviewDto>>.Failure(reviewsResult.ErrorMessage!, reviewsResult.FailureType);
        }

        var reviews = reviewsResult.Value!;

        return Result<ICollection<ReviewDto>>.Success(_mapper.Map<ICollection<ReviewDto>>(reviews));
    }

    public async Task<Result<ReviewDto>> CreateReview(Guid accountId, Guid recipeId, string description,
        int amountOfStars)
    {
        _logger.LogInformation($"Creating review for account with id {accountId} and recipe with id {recipeId}");
        var accountResult = await _accountRepository.ReadAccount(accountId);
        if (!accountResult.IsSuccess)
        {
            return Result<ReviewDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }

        var account = accountResult.Value!;
        var recipeResult = await _recipeRepository.ReadRecipeWithReviewsById(recipeId);
        if (!recipeResult.IsSuccess)
        {
            return Result<ReviewDto>.Failure(recipeResult.ErrorMessage!, recipeResult.FailureType);
        }

        var recipe = recipeResult.Value!;

        var reviewExistsResult = await _reviewRepository.ReviewExistsForAccountAndRecipe(accountId, recipeId);
        if (!reviewExistsResult.IsSuccess)
        {
            return Result<ReviewDto>.Failure(reviewExistsResult.ErrorMessage!, reviewExistsResult.FailureType);
        }

        var reviewExists = reviewExistsResult.Value!;

        // check if account already has a review on this recipe, which is not allowed
        if (reviewExists)
        {
            return Result<ReviewDto>.Failure("Account already has a review on this recipe.", ResultFailureType.Error);
        }

        var review = new Review
        {
            Account = account,
            Recipe = recipe,
            Description = description,
            AmountOfStars = amountOfStars,
            CreatedAt = DateTime.UtcNow
        };
        recipe.LastUsedAt = DateTime.UtcNow;
        var createReviewResult = await _reviewRepository.CreateReview(review);
        if (!createReviewResult.IsSuccess)
        {
            return Result<ReviewDto>.Failure(createReviewResult.ErrorMessage!, createReviewResult.FailureType);
        }

        _logger.LogInformation($"Review created with id {review.ReviewId}");

        recipe.AmountOfRatings++;
        recipe.AverageRating = (recipe.AverageRating * (recipe.AmountOfRatings - 1) + amountOfStars) /
                               recipe.AmountOfRatings;
        var updateRecipeResult = await _recipeRepository.UpdateRecipe(recipe);
        if (!updateRecipeResult.IsSuccess)
        {
            return Result<ReviewDto>.Failure(updateRecipeResult.ErrorMessage!, updateRecipeResult.FailureType);
        }

        return Result<ReviewDto>.Success(_mapper.Map<ReviewDto>(review));
    }
}