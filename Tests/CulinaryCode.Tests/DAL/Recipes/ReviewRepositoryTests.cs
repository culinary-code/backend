using CulinaryCode.Tests.util;
using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;

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
        var result = await _recipeRepository.ReadReviewWithAccountByReviewId(review.ReviewId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(review.ReviewId, result.ReviewId);
        Assert.Equal(review.Account.AccountId, result.Account.AccountId);
        Assert.Equal(review.Recipe.RecipeId, result.Recipe.RecipeId);
    }

    [Fact]
    public async Task ReadReviewById_ReviewDoesNotExist_ThrowsReviewNotFoundException()
    {
        // Arrange
        var reviewId = Guid.NewGuid();

        // Act
        async Task Act() => await _recipeRepository.ReadReviewWithAccountByReviewId(reviewId);

        // Assert
        await Assert.ThrowsAsync<ReviewNotFoundException>(Act);
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
        var result = await _recipeRepository.ReadReviewsWithAccountByRecipeId(recipe.RecipeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.ReviewId == review1.ReviewId);
        Assert.Contains(result, r => r.ReviewId == review2.ReviewId);
    }
    
    [Fact]
    public async Task ReadReviewsByRecipeId_ReviewsDoNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var recipeId = Guid.NewGuid();

        // Act
        var result = await _recipeRepository.ReadReviewsWithAccountByRecipeId(recipeId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
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