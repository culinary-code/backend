using DAL.EF;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
using Microsoft.EntityFrameworkCore;

namespace DAL.Accounts;

public class AccountRepository : IAccountRepository
{
    
    private readonly CulinaryCodeDbContext _ctx;

    public AccountRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }
    
    public async Task<Account> ReadAccount(Guid id)
    {
        Account? account = await _ctx.Accounts.FindAsync(id);
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        return account;
    }

    public async Task<Account> ReadAccountWithPreferencesByAccountId(Guid id)
    {
        var account = await _ctx.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefaultAsync(a => a.AccountId == id);
        
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        
        return account;
    }

    public async Task<Account> ReadAccountWithMealPlannerNextWeekAndWithGroceryList(Guid id)
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
            .FirstOrDefaultAsync(a => a.AccountId == id);
        
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        
        return account;
    }

    public async Task<List<Recipe?>> ReadFavoriteRecipesByUserId(Guid userId)
    {
        var favoriteRecipes = await _ctx.FavoriteRecipes
            .Where(fr => fr.Account != null && fr.Account.AccountId == userId)
            .Select(fr => fr.Recipe)
            .Where(r => r != null)
            .ToListAsync();
        if (!favoriteRecipes.Any())
        {
            throw new RecipeNotFoundException("No favorite recipes found for the given account.");
        }

        return favoriteRecipes;
    }

    public async Task UpdateAccount(Account account)
    {
        _ctx.Accounts.Update(account);
        await _ctx.SaveChangesAsync();    
    }

    public async Task CreateAccount(Account account)
    {
        await _ctx.Accounts.AddAsync(account);
        await _ctx.SaveChangesAsync(); 
    }

    public async Task DeletePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        var account = await ReadAccountWithPreferencesByAccountId(accountId);
        var preferenceToRemove = account.Preferences?.FirstOrDefault(p => p.PreferenceId == preferenceId);
        if (preferenceToRemove == null)
        {
            throw new PreferenceNotFoundException("Preference not found for this account");
        } 
        account.Preferences = account.Preferences.Where(p => p.PreferenceId != preferenceId).ToList(); 
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteFavoriteRecipeByUserId(Guid accountId, Guid recipeId)
    { 
        var favoriteRecipeToRemove = await _ctx.FavoriteRecipes.FirstOrDefaultAsync(r => r.Recipe.RecipeId == recipeId && r.Account.AccountId == accountId); 
        _ctx.FavoriteRecipes.Remove(favoriteRecipeToRemove);
        await _ctx.SaveChangesAsync();
    }
}