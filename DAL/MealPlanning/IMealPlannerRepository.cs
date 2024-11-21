using DOM.MealPlanning;

namespace DAL.MealPlanning;

public interface IMealPlannerRepository
{ 
    Task<MealPlanner> ReadMealPlannerByIdWithNextWeek(Guid accountId);
    Task<MealPlanner> ReadMealPlannerByIdWithNextWeekWithRecipe(Guid accountId);
    Task<MealPlanner> ReadMealPlannerByIdWithNextWeekAndHistory(Guid accountId);
    Task<MealPlanner> ReadMealPlannerByIdWithNextWeekAndHistoryWithRecipe(Guid accountId);
    Task DeletePlannedMeal(PlannedMeal plannedMeal);
    Task<PlannedMeal> CreatePlannedMeal(PlannedMeal plannedMeal);
    Task<List<PlannedMeal>> ReadNextWeekPlannedMeals(DateTime dateTime, Guid userId);
    Task<List<PlannedMeal>> ReadPlannedMealsAfterDate(DateTime dateTime, Guid mealPlannerId);
}