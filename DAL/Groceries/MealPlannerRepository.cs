using DAL.EF;
using DOM.MealPlanning;
using Microsoft.EntityFrameworkCore;

namespace DAL.Groceries;

public class MealPlannerRepository : IMealPlannerRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public MealPlannerRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public MealPlanner ReadMealPlannerById(Guid accountId)
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
}