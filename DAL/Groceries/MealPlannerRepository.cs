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
        return _ctx.MealPlanners.Where(m => m.Account.AccountId == accountId).FirstOrDefault();
    }
}