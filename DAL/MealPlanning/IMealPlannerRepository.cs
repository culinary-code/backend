using DOM.Exceptions;
using DOM.MealPlanning;

namespace DAL.MealPlanning;

public interface IMealPlannerRepository
{ 
    Task<Result<MealPlanner>> ReadMealPlannerByIdWithNextWeekNoTracking(Guid accountId);
    Task<Result<Unit>> DeletePlannedMeal(PlannedMeal plannedMeal);
    Task<Result<PlannedMeal>> CreatePlannedMeal(PlannedMeal plannedMeal);
    Task<Result<List<PlannedMeal>>> ReadNextWeekPlannedMealsNoTracking(Guid userId);
    Task<Result<List<PlannedMeal>>> ReadPlannedMealsAfterDateNoTracking(DateTime dateTime, Guid mealPlannerId);
}