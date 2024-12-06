using DAL.EF;
using DOM.Exceptions;
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

    // used to create a new planned meal, needs to be tracked
    public async Task<MealPlanner> ReadMealPlannerByIdWithNextWeekNoTracking(Guid accountId)
    {
        var mealPlanner = await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
        if (mealPlanner == null) throw new MealPlannerNotFoundException();
        return mealPlanner;
    }
    
    // used to return a dto, doesn't need to be tracked
    private async Task<MealPlanner> ReadMealPlannerByIdWithNextWeekWithRecipeNoTracking(Guid accountId)
    {
        var mealPlanner = await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .ThenInclude(p => p.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
        
        if (mealPlanner == null) throw new MealPlannerNotFoundException();
        return mealPlanner;
    }
    
    // used to return a dto, doesn't need to be tracked
    private async Task<MealPlanner> ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipeNoTracking(Guid accountId)
    {
        return await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .ThenInclude(p => p.Recipe)
            .Include(planner => planner.History)
            .ThenInclude(p => p.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
    }
    
    public async Task DeletePlannedMeal(PlannedMeal plannedMeal)
    {
        _ctx.PlannedMeals.Remove(plannedMeal);
        await _ctx.SaveChangesAsync();
    }

    public async Task<PlannedMeal> CreatePlannedMeal(PlannedMeal plannedMeal)
    {
        await _ctx.PlannedMeals.AddAsync(plannedMeal);
        await _ctx.SaveChangesAsync();
        return plannedMeal;
    }

    // used to return dto, doesn't need to be tracked
    public async Task<List<PlannedMeal>> ReadNextWeekPlannedMealsNoTracking(Guid userId)
    {
        var mealPlanner = await ReadMealPlannerByIdWithNextWeekWithRecipeNoTracking(userId);
        return mealPlanner.NextWeek.ToList();
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<List<PlannedMeal>> ReadPlannedMealsAfterDateNoTracking(DateTime dateTime, Guid userId)
    {
        var mealPlanner = await ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipeNoTracking(userId);
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