using CulinaryCode.Tests.util;
using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Results;

namespace CulinaryCode.Tests.DAL.Recipes;

public class ReviewRepositoryTests : IClassFixture<TestPostgresContainerFixture>
{
    private IReviewRepository _recipeRepository;
    private readonly CulinaryCodeDbContext _dbContext;

    public ReviewRepositoryTests(TestPostgresContainerFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _recipeRepository = new ReviewRepository(_dbContext);
    }

    [Fact]
    public async Task ReadReviewById_ReviewExists_ReturnsReview()
    {
        // Arrange
        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            Account = new Account { AccountId = Guid.NewGuid() },
            Recipe = new Recipe { RecipeId = Guid.NewGuid() }
        };
        await _dbContext.Reviews.AddAsync(review);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.ReadReviewWithAccountByReviewIdNoTracking(review.ReviewId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(review.ReviewId, result.Value!.ReviewId);
        Assert.Equal(review.Account.AccountId, result.Value.Account.AccountId);
    }

    [Fact]
    public async Task ReadReviewById_ReviewDoesNotExist_ThrowsReviewNotFoundException()
    {
        // Arrange
        var reviewId = Guid.NewGuid();

        // Act
        var result = await _recipeRepository.ReadReviewWithAccountByReviewIdNoTracking(reviewId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task ReadReviewsByRecipeId_ReviewsExist_ReturnsReviews()
    {
        // Arrange
        var recipe = new Recipe { RecipeId = Guid.NewGuid() };
        
        var review1 = new Review
        {
            ReviewId = Guid.NewGuid(),
            Account = new Account { AccountId = Guid.NewGuid() },
            Recipe = recipe
        };
        var review2 = new Review
        {
            ReviewId = Guid.NewGuid(),
            Account = new Account { AccountId = Guid.NewGuid() },
            Recipe = recipe
        };
        await _dbContext.Reviews.AddRangeAsync(review1, review2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.ReadReviewsWithAccountByRecipeIdNoTracking(recipe.RecipeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, r => r.ReviewId == review1.ReviewId);
        Assert.Contains(result.Value, r => r.ReviewId == review2.ReviewId);
    }
    
    [Fact]
    public async Task ReadReviewsByRecipeId_ReviewsDoNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var recipeId = Guid.NewGuid();

        // Act
        var result = await _recipeRepository.ReadReviewsWithAccountByRecipeIdNoTracking(recipeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
    
    [Fact]
    public async Task CreateReview_ReviewIsCreated()
    {
        // Arrange
        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            Account = new Account { AccountId = Guid.NewGuid() },
            Recipe = new Recipe { RecipeId = Guid.NewGuid() }
        };

        // Act
        await _recipeRepository.CreateReview(review);

        // Assert
        var result = await _dbContext.Reviews.FindAsync(review.ReviewId);
        Assert.NotNull(result);
        Assert.Equal(review.ReviewId, result.ReviewId);
        Assert.Equal(review.Account.AccountId, result.Account.AccountId);
        Assert.Equal(review.Recipe.RecipeId, result.Recipe.RecipeId);
    }
}