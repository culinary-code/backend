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
    public async Task<Result<MealPlanner>> ReadMealPlannerByIdWithNextWeekNoTracking(Guid accountId)
    {
        var mealPlanner = await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
        if (mealPlanner == null)
        {
            return Result<MealPlanner>.Failure($"No mealplanner found for account with id {accountId}", ResultFailureType.NotFound);
        }
        
        return Result<MealPlanner>.Success(mealPlanner);
    }
    
    // used to return a dto, doesn't need to be tracked
    private async Task<Result<MealPlanner>> ReadMealPlannerByIdWithNextWeekWithRecipeNoTracking(Guid accountId)
    {
        var mealPlanner = await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .ThenInclude(p => p.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
        if (mealPlanner == null)
        {
            return Result<MealPlanner>.Failure($"No mealplanner found for account with id {accountId}", ResultFailureType.NotFound);
        }
        
        return Result<MealPlanner>.Success(mealPlanner);
    }
    
    // used to return a dto, doesn't need to be tracked
    private async Task<Result<MealPlanner>> ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipeNoTracking(Guid accountId)
    {
        var mealPlanner = await _ctx.MealPlanners
            .Include(planner => planner.NextWeek)
            .ThenInclude(p => p.Recipe)
            .Include(planner => planner.History)
            .ThenInclude(p => p.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(m => m.Account.AccountId == accountId);
        
        if (mealPlanner == null)
        {
            return Result<MealPlanner>.Failure($"No mealplanner found for account with id {accountId}", ResultFailureType.NotFound);
        }
        
        return Result<MealPlanner>.Success(mealPlanner);
    }
    
    public async Task<Result<Unit>> DeletePlannedMeal(PlannedMeal plannedMeal)
    {
        _ctx.PlannedMeals.Remove(plannedMeal);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<PlannedMeal>> CreatePlannedMeal(PlannedMeal plannedMeal)
    {
        await _ctx.PlannedMeals.AddAsync(plannedMeal);
        await _ctx.SaveChangesAsync();
        return Result<PlannedMeal>.Success(plannedMeal);
    }

    // used to return dto, doesn't need to be tracked
    public async Task<Result<List<PlannedMeal>>> ReadNextWeekPlannedMealsNoTracking(Guid userId)
    {
        var mealPlannerResult = await ReadMealPlannerByIdWithNextWeekWithRecipeNoTracking(userId);
        if (!mealPlannerResult.IsSuccess)
        {
            return Result<List<PlannedMeal>>.Failure(mealPlannerResult.ErrorMessage!, mealPlannerResult.FailureType);
        }
        var mealPlanner = mealPlannerResult.Value; 
        
        return Result<List<PlannedMeal>>.Success(mealPlanner!.NextWeek.ToList());
    }

    // used to return a dto, doesn't need to be tracked
    public async Task<Result<List<PlannedMeal>>> ReadPlannedMealsAfterDateNoTracking(DateTime dateTime, Guid userId)
    {
        var mealPlannerResult = await ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipeNoTracking(userId);
        if (!mealPlannerResult.IsSuccess)
        {
            return Result<List<PlannedMeal>>.Failure(mealPlannerResult.ErrorMessage!, mealPlannerResult.FailureType);
        }
        
        var mealPlanner = mealPlannerResult.Value; 
        var startDate = dateTime.Date;  // Ensures we are only comparing the date part
        var endDate = startDate.AddDays(7); // 7 days after the start date

        // Combine history and next week meals (adjust based on your data structure)
        var allPlannedMeals = mealPlanner!.History.Concat(mealPlanner.NextWeek);

        // Filter planned meals to include only those between startDate and endDate
        var plannedMeals = allPlannedMeals
            .Where(m => m.PlannedDate >= startDate && m.PlannedDate <= endDate)
            .ToList();

        return Result<List<PlannedMeal>>.Success(plannedMeals);
    }
}