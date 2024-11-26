using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;

namespace BL.Managers.MealPlanning;

public interface IMealPlannerManager
{
    Task CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto);
    Task<List<PlannedMealDto>> GetPlannedMealsFromUserAfterDate(DateTime dateTime, Guid userId);
    Task<List<IngredientQuantityDto>> GetNextWeekIngredients(Guid userId);
}