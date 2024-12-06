using DOM.MealPlanning;

namespace DAL.MealPlanning;

public interface IMealPlannerRepository
{ 
    Task<MealPlanner> ReadMealPlannerByIdWithNextWeekNoTracking(Guid accountId);
    Task DeletePlannedMeal(PlannedMeal plannedMeal);
    Task<PlannedMeal> CreatePlannedMeal(PlannedMeal plannedMeal);
    Task<List<PlannedMeal>> ReadNextWeekPlannedMealsNoTracking(Guid userId);
    Task<List<PlannedMeal>> ReadPlannedMealsAfterDateNoTracking(DateTime dateTime, Guid mealPlannerId);
}