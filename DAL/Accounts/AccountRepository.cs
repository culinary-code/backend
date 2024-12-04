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
    
    public Account ReadAccount(Guid id)
    {
        Account? account = _ctx.Accounts.Find(id);
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        return account;
    }

    public Account ReadAccountWithPreferencesByAccountId(Guid id)
    {
        var account = _ctx.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefault(a => a.AccountId == id);
        
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        
        return account;
    }

    public List<Recipe?> ReadFavoriteRecipesByUserId(Guid userId)
    {
        var favoriteRecipes = _ctx.FavoriteRecipes
            .Where(fr => fr.Account != null && fr.Account.AccountId == userId)
            .Select(fr => fr.Recipe)
            .Where(r => r != null)
            .ToList();
        if (!favoriteRecipes.Any())
        {
            throw new RecipeNotFoundException("No favorite recipes found for the given account.");
        }

        return favoriteRecipes;
    }

    public List<FavoriteRecipe> ReadFavoriteRecipeListByUserId(Guid userId)
    {
        var favoriteRecipes = _ctx.FavoriteRecipes
            .Where(fr => fr.Account != null && fr.Account.AccountId == userId)
            .Include(fr => fr.Recipe)
            .Where(r => r != null)
            .ToList();
        if (!favoriteRecipes.Any())
        {
            throw new RecipeNotFoundException("No favorite recipes found for the given account.");
        }
        return favoriteRecipes;
    }

    public void UpdateAccount(Account account)
    {
        _ctx.Accounts.Update(account);
        _ctx.SaveChanges();    
    }

    public void CreateAccount(Account account)
    {
        _ctx.Accounts.Add(account);
        _ctx.SaveChanges(); 
    }

    public void DeletePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        var account = ReadAccountWithPreferencesByAccountId(accountId);
        var preferenceToRemove = account.Preferences?.FirstOrDefault(p => p.PreferenceId == preferenceId);
        if (preferenceToRemove == null)
        {
            throw new PreferenceNotFoundException("Preference not found for this account");
        } 
        account.Preferences = account.Preferences.Where(p => p.PreferenceId != preferenceId).ToList(); 
        _ctx.SaveChanges();
    }

    public void DeleteFavoriteRecipesByUserId(Guid accountId, Guid favoriteRecipeId)
    {
        var account = ReadAccountWithPreferencesByAccountId(accountId);
        var favoriteRecipeToRemove = account.FavoriteRecipes.FirstOrDefault(r => r.FavoriteRecipeId == favoriteRecipeId);

        if (favoriteRecipeToRemove == null)
        {
            throw new RecipeNotFoundException("No favorite recipes found for the given account.");
        }
        account.FavoriteRecipes.Remove(favoriteRecipeToRemove);
        _ctx.FavoriteRecipes.Remove(favoriteRecipeToRemove);
        _ctx.SaveChanges();
    }
}