using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
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
    private readonly IGroceryRepository _groceryRepository;
    private readonly IMapper _mapper;

    public MealPlannerManager(IMealPlannerRepository mealPlannerRepository, IMapper mapper,
        IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository, IGroceryRepository groceryRepository)
    {
        _mealPlannerRepository = mealPlannerRepository;
        _mapper = mapper;
        _recipeRepository = recipeRepository;
        _ingredientRepository = ingredientRepository;
        _groceryRepository = groceryRepository;
    }

    public async Task CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto)
    {
        // get mealplanner for user with userid

        MealPlanner mealPlanner = await _mealPlannerRepository.ReadMealPlannerByIdWithNextWeek(userId);
        
        // check if planned meal exists for date
        var alreadyPlannedMeal = mealPlanner.NextWeek.FirstOrDefault(pm => pm.PlannedDate.ToUniversalTime().Date == plannedMealDto.PlannedDate.Date);
        if (alreadyPlannedMeal != null)
        {
            await _mealPlannerRepository.DeletePlannedMeal(alreadyPlannedMeal);
        }

        var linkedRecipe = _recipeRepository.ReadRecipeById(plannedMealDto.Recipe.RecipeId);
        var linkedIngredientQuantities = new List<IngredientQuantity>();
        
        GroceryList groceryList = _groceryRepository.ReadGroceryListByAccountId(userId);

        foreach (var ingredientQuantityDto in plannedMealDto.Ingredients)
        {
            var ingredientQuantity = new IngredientQuantity()
            {
                GroceryList = groceryList,
                Quantity = ingredientQuantityDto.Quantity,
                Ingredient = _ingredientRepository.ReadIngredientById(ingredientQuantityDto.Ingredient.IngredientId)
            };
            linkedIngredientQuantities.Add(ingredientQuantity);
        }
        
        PlannedMeal plannedMeal = new PlannedMeal()
        {
            PlannedDate = DateTime.SpecifyKind(plannedMealDto.PlannedDate.Date, DateTimeKind.Utc),
            Recipe = linkedRecipe,
            AmountOfPeople = plannedMealDto.AmountOfPeople,
            Ingredients = linkedIngredientQuantities,
            NextWeekMealPlanner = mealPlanner
        };
        
        linkedRecipe.LastUsedAt = DateTime.UtcNow;
        
        await _mealPlannerRepository.CreatePlannedMeal(plannedMeal);
        _groceryRepository.UpdateGroceryList(groceryList);
        
    }

    public async Task<List<PlannedMealDto>> GetPlannedMealsFromUserAfterDate(DateTime dateTime, Guid userId)
    {
        List<PlannedMeal> plannedMeals;
        if (dateTime.Date == DateTime.Now.Date)
        {
            plannedMeals = await _mealPlannerRepository.ReadNextWeekPlannedMeals(userId);
        }
        else
        {
            plannedMeals = await _mealPlannerRepository.ReadPlannedMealsAfterDate(dateTime, userId);
        }
        return _mapper.Map<List<PlannedMealDto>>(plannedMeals);
    }
    
    public async Task<List<IngredientQuantityDto>> GetNextWeekIngredients(Guid userId)
    {
        // Get all planned meals for next week
        List<PlannedMeal> plannedMeals = await _mealPlannerRepository.ReadNextWeekPlannedMeals(userId);
    
        // Aggregate the ingredients
        var aggregatedIngredients = new Dictionary<Guid, IngredientQuantityDto>();
    
        foreach (var meal in plannedMeals)
        {
            foreach (var ingredientQuantity in meal.Ingredients)
            {
                // If ingredient already exists in the dictionary, combine the quantities
                if (aggregatedIngredients.ContainsKey(ingredientQuantity.Ingredient.IngredientId))
                {
                    aggregatedIngredients[ingredientQuantity.Ingredient.IngredientId].Quantity += ingredientQuantity.Quantity;
                }
                else
                {
                    aggregatedIngredients.Add(ingredientQuantity.Ingredient.IngredientId, new IngredientQuantityDto
                    {
                        Ingredient = new IngredientDto
                        {
                            IngredientId = ingredientQuantity.Ingredient.IngredientId,
                            IngredientName = ingredientQuantity.Ingredient.IngredientName
                        },
                        Quantity = ingredientQuantity.Quantity
                    });
                }
            }
        }
        return aggregatedIngredients.Values.ToList();
    }
}