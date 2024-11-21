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

    public async Task<MealPlanner> ReadMealPlannerByIdWithNextWeek(Guid accountId)
    {
        return await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
    }
    
    public async Task<MealPlanner> ReadMealPlannerByIdWithNextWeekWithRecipe(Guid accountId)
    {
        return await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .ThenInclude(p => p.Recipe)
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
    }

    public async Task<MealPlanner> ReadMealPlannerByIdWithNextWeekAndHistory(Guid accountId)
    {
        return await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .Include(planner => planner.History)
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
    }
    
    public async Task<MealPlanner> ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipe(Guid accountId)
    {
        return await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .ThenInclude(p => p.Recipe)
            .Include(planner => planner.History)
            .ThenInclude(p => p.Recipe)
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
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

    public async Task<List<PlannedMeal>> ReadNextWeekPlannedMeals(DateTime dateTime, Guid userId)
    {
        var mealPlanner = await ReadMealPlannerByIdWithNextWeekWithRecipe(userId);
        return mealPlanner.NextWeek.ToList();
    }

    public async Task<List<PlannedMeal>> ReadPlannedMealsAfterDate(DateTime dateTime, Guid userId)
    {
        var mealPlanner = await ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipe(userId);
        var startDate = dateTime.Date;  // Ensures we are only comparing the date part
        var endDate = startDate.AddDays(7); // 7 days after the start date

        // Combine history and next week meals (adjust based on your data structure)
        var allPlannedMeals = mealPlanner.History.Concat(mealPlanner.NextWeek);

        // Filter planned meals to include only those between startDate and endDate
        var plannedMeals = allPlannedMeals
            .Where(m => m.PlannedDate >= startDate && m.PlannedDate <= endDate)
            .ToList();

        return plannedMeals;
    }
}