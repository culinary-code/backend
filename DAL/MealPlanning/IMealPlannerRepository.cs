using DOM.MealPlanning;
using DOM.Results;

namespace DAL.MealPlanning;

public interface IMealPlannerRepository
{ 
    Task<Result<MealPlanner>> ReadMealPlannerByUserIdWithNextWeekNoTracking(Guid accountId);
    Task<Result<MealPlanner>> ReadMealPlannerByGroupIdWithNextWeekNoTracking(Guid groupId);
    Task<Result<Unit>> DeletePlannedMeal(PlannedMeal plannedMeal);
    Task<Result<PlannedMeal>> CreatePlannedMeal(PlannedMeal plannedMeal);
    Task<Result<List<PlannedMeal>>> ReadNextWeekPlannedMealsNoTrackingByUserId(Guid userId);
    Task<Result<List<PlannedMeal>>> ReadNextWeekPlannedMealsNoTrackingByGroupId(Guid groupId);
    Task<Result<List<PlannedMeal>>> ReadPlannedMealsAfterDateNoTrackingByUserId(DateTime dateTime, Guid userId); 
    Task<Result<List<PlannedMeal>>> ReadPlannedMealsAfterDateNoTrackingByGroupId(DateTime dateTime, Guid groupId);
}