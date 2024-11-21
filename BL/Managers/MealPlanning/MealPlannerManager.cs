using AutoMapper;
using BL.DTOs.MealPlanning;
using DAL.Accounts;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace BL.Managers.MealPlanning;

public class MealPlannerManager : IMealPlannerManager
{
    private readonly IMealPlannerRepository _mealPlannerRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IMapper _mapper;

    public MealPlannerManager(IMealPlannerRepository mealPlannerRepository, IMapper mapper,
        IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository)
    {
        _mealPlannerRepository = mealPlannerRepository;
        _mapper = mapper;
        _recipeRepository = recipeRepository;
        _ingredientRepository = ingredientRepository;
    }

    public async Task CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto)
    {
        // get mealplanner for user with userid

        MealPlanner mealPlanner = _mealPlannerRepository.ReadMealPlannerByIdWithNextWeek(userId);
        
        // check if planned meal exists for date
        var alreadyPlannedMeal = mealPlanner.NextWeek.FirstOrDefault(pm => pm.PlannedDate.ToUniversalTime().Date == plannedMealDto.PlannedDate.ToUniversalTime().Date);
        if (alreadyPlannedMeal != null)
        {
            await _mealPlannerRepository.DeletePlannedMeal(alreadyPlannedMeal);
        };

        var linkedRecipe = _recipeRepository.ReadRecipeById(plannedMealDto.Recipe.RecipeId);
        var linkedIngredientQuantities = new List<IngredientQuantity>();

        foreach (var ingredientQuantityDto in plannedMealDto.Ingredients)
        {
            var ingredientQuantity = new IngredientQuantity()
            {
                Quantity = ingredientQuantityDto.Quantity,
                // Ingredient = _ingredientRepository.ReadIngredientByName(ingredientQuantityDto.Ingredient.IngredientName)
                Ingredient = _ingredientRepository.ReadIngredientById(ingredientQuantityDto.Ingredient.IngredientId)
            };
            linkedIngredientQuantities.Add(ingredientQuantity);
        }
        
        // create new planned meal in db
        PlannedMeal plannedMeal = new PlannedMeal()
        {
            PlannedDate = DateTime.SpecifyKind(plannedMealDto.PlannedDate.Date, DateTimeKind.Utc),
            Recipe = linkedRecipe,
            AmountOfPeople = plannedMealDto.AmountOfPeople,
            Ingredients = linkedIngredientQuantities,
            NextWeekMealPlanner = mealPlanner
            
        };
        plannedMeal = await _mealPlannerRepository.CreatePlannedMeal(plannedMeal);
        
       
    }
}