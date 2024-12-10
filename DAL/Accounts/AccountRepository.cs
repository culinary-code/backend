using DAL.EF;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace DAL.Accounts;

public class AccountRepository : IAccountRepository
{
    
    private readonly CulinaryCodeDbContext _ctx;

    public AccountRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }
    
    // used to update account, needs to be tracked
    public async Task<Result<Account>> ReadAccount(Guid id)
    {
        Account? account = await _ctx.Accounts.FindAsync(id);
        if (account == null)
        {
            return Result<Account>.Failure($"No account found with id {id}", ResultFailureType.NotFound);
        }
        return Result<Account>.Success(account);
    }

    // used to update preferences, needs to be tracked
    public async Task<Result<Account>> ReadAccountWithPreferencesByAccountId(Guid id)
    {
        var account = await _ctx.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefaultAsync(a => a.AccountId == id);
        
        if (account == null)
        {
            return Result<Account>.Failure($"No account found with id {id}", ResultFailureType.NotFound);
        }
        
        return Result<Account>.Success(account);
    }

    // used to create a grocerylistdto, does not need to be tracked
    public async Task<Result<Account>> ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking(Guid id)
    {
        var account = await _ctx.Accounts
            .Include(a => a.GroceryList)
                .ThenInclude(gl => gl!.Ingredients)
                .ThenInclude(iq => iq.Ingredient)
            .Include(a => a.GroceryList)
                .ThenInclude(gl => gl!.Ingredients)
                .ThenInclude(iq => iq.PlannedMeal)
                .ThenInclude(p => p.Recipe )
            .Include(a => a.GroceryList)
                .ThenInclude(gl => gl!.Items)
                .ThenInclude(i => i.GroceryItem)
            .Include(a => a.Planner)
                .ThenInclude(p => p!.NextWeek)
                .ThenInclude(n => n.Ingredients)
                .ThenInclude(iq => iq.Ingredient)
            .Include(a => a.Planner)
                .ThenInclude(p => p!.NextWeek)
                .ThenInclude(p => p.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(a => a.AccountId == id);
        
        if (account == null)
        {
            return Result<Account>.Failure($"No account found with id {id}", ResultFailureType.NotFound);
        }
        
        return Result<Account>.Success(account);
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<List<Recipe?>>> ReadFavoriteRecipesByUserIdNoTracking(Guid userId)
    {
        var favoriteRecipes = await _ctx.FavoriteRecipes
            .Where(fr => fr.Account != null && fr.Account.AccountId == userId)
            .Include(favoriteRecipe => favoriteRecipe.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
        
        var recipes = favoriteRecipes.Select(fr => fr.Recipe).ToList();

        return Result<List<Recipe?>>.Success(recipes);

    }

    public async Task<Result<Unit>> UpdateAccount(Account account)
    {
        _ctx.Accounts.Update(account);
        await _ctx.SaveChangesAsync();    
        return Result<Unit>.Success(new Unit());

    }

    public async Task<Result<Unit>> CreateAccount(Account account)
    {
        await _ctx.Accounts.AddAsync(account);
        await _ctx.SaveChangesAsync(); 
        return Result<Unit>.Success(new Unit());

    }

    public async Task<Result<Unit>> DeletePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        var accountResult = await ReadAccountWithPreferencesByAccountId(accountId);
        if (!accountResult.IsSuccess)
        {
            return Result<Unit>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;
        
        var preferenceToRemove = account!.Preferences.FirstOrDefault(p => p.PreferenceId == preferenceId);
        if (preferenceToRemove == null)
        {
            return Result<Unit>.Failure($"No preference found with id {preferenceId}", ResultFailureType.NotFound);
        } 
        
        account.Preferences = account.Preferences.Where(p => p.PreferenceId != preferenceId).ToList(); 
        await _ctx.SaveChangesAsync();
        
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> DeleteFavoriteRecipeByUserId(Guid accountId, Guid recipeId)
    { 
        var favoriteRecipeToRemove = await _ctx.FavoriteRecipes.FirstOrDefaultAsync(r => r.Recipe.RecipeId == recipeId && r.Account.AccountId == accountId); 
        
        if (favoriteRecipeToRemove == null)
        {
            return Result<Unit>.Failure($"No favorite recipe found with id {recipeId}", ResultFailureType.NotFound);
        } 
        
        _ctx.FavoriteRecipes.Remove(favoriteRecipeToRemove);
        await _ctx.SaveChangesAsync();
        
        return Result<Unit>.Success(new Unit());
    }
}