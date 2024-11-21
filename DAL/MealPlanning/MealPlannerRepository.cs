using DAL.EF;
using DOM.MealPlanning;
using Microsoft.EntityFrameworkCore;

namespace DAL.MealPlanning;

public class MealPlannerRepository : IMealPlannerRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public MealPlannerRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public MealPlanner ReadMealPlannerByIdWithNextWeek(Guid accountId)
    {
        return _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .Where(m => m.Account.AccountId == accountId).FirstOrDefault();
    }

    public async Task DeletePlannedMeal(PlannedMeal plannedMeal)
    {
        _ctx.PlannedMeals.Remove(plannedMeal);
        await _ctx.SaveChangesAsync();
    }

    public async Task<PlannedMeal> CreatePlannedMeal(PlannedMeal plannedMeal)
    {
        _ctx.PlannedMeals.Add(plannedMeal);
        await _ctx.SaveChangesAsync();
        return plannedMeal;
    }
}