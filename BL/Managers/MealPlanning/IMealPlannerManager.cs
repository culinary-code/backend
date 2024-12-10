using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DOM.Results;

namespace BL.Managers.MealPlanning;

public interface IMealPlannerManager
{
    Task<Result<Unit>> CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto);
    Task<Result<List<PlannedMealDto>>> GetPlannedMealsFromUserAfterDate(DateTime dateTime, Guid userId);
    Task<Result<List<IngredientQuantityDto>>> GetNextWeekIngredients(Guid userId);
}