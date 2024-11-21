using BL.DTOs.MealPlanning;

namespace BL.Managers.MealPlanning;

public interface IMealPlannerManager
{
    Task CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto);
}