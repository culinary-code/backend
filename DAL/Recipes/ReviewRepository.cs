using DAL.EF;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DAL.Recipes;

public class ReviewRepository : IReviewRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public ReviewRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<Review>> ReadReviewWithAccountByReviewIdNoTracking(Guid id)
    {
        var review = await _ctx.Reviews
            .Include(r => r.Account)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(r => r.ReviewId == id);
        if (review is null)
        {
            return Result<Review>.Failure($"No review found with id {id}", ResultFailureType.NotFound);
        }

        return Result<Review>.Success(review);
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<ICollection<Review>>> ReadReviewsWithAccountByRecipeIdNoTracking(Guid recipeId)
    {
        var reviews = await _ctx.Reviews
            .Where(r => r.Recipe != null && r.Recipe.RecipeId == recipeId)
            .Include(r => r.Account)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();

        return Result<ICollection<Review>>.Success(reviews);
    }

    public async Task<Result<Unit>> CreateReview(Review review)
    {
        _ctx.Reviews.Add(review);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }
    
    public async Task<Result<bool>> ReviewExistsForAccountAndRecipe(Guid accountId, Guid recipeId)
    {
        var reviewExists = await _ctx.Reviews
            .AsNoTrackingWithIdentityResolution()
            .AnyAsync(r => r.Account!.AccountId == accountId && r.Recipe!.RecipeId == recipeId);
        
        return Result<bool>.Success(reviewExists);
    }
}