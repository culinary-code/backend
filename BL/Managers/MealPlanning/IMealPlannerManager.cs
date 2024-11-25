using BL.DTOs.MealPlanning;

namespace BL.Managers.MealPlanning;

public interface IMealPlannerManager
{
    Task CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto);
    Task<List<PlannedMealDto>> GetPlannedMealsFromUserAfterDate(DateTime dateTime, Guid userId);
}