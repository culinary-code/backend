using DOM.MealPlanning;

namespace DAL.MealPlanning;

public interface IMealPlannerRepository
{ 
    MealPlanner ReadMealPlannerByIdWithNextWeek(Guid accountId);
    Task DeletePlannedMeal(PlannedMeal plannedMeal);
    Task<PlannedMeal> CreatePlannedMeal(PlannedMeal plannedMeal);
}