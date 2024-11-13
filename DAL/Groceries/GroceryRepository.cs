using DAL.EF;
using DOM.MealPlanning;
using Microsoft.EntityFrameworkCore;

namespace DAL.Groceries;

public class GroceryRepository : IGroceryRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public GroceryRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public MealPlanner GetMealPlannerById(Guid accountId)
    {
        return _ctx.Accounts
            .Where(a => a.AccountId == accountId)
            .Include(a => a.Planner)
            .ThenInclude(mp => mp.NextWeek)
            .ThenInclude(pm => pm.Ingredients)
            .ThenInclude(iq => iq.Ingredient)
            .Select(a => a.Planner)
            .FirstOrDefault();
    }

    public void CreateGroceryList(GroceryList groceryList)
    {
        _ctx.GroceryLists.Add(groceryList);
        _ctx.SaveChanges();
    }

    /*public MealPlanner GetMealPlannerById(Guid accountId)
    {
        return _ctx.Accounts
            .Where(a => a.AccountId == accountId)
            .Include(a => a.Planner)                // Include Planner first
            .ThenInclude(mp => mp.NextWeek)         // Then include NextWeek navigation property
            .ThenInclude(pm => pm.Ingredients)     // Include Ingredients within NextWeek
            .Select(a => a.Planner)                 // Now project the Planner after including related entities
            .FirstOrDefault();
    }*/
}