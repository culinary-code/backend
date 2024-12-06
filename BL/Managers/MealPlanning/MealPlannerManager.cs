using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.Exceptions;
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
        IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository,
        IGroceryRepository groceryRepository)
    {
        _mealPlannerRepository = mealPlannerRepository;
        _mapper = mapper;
        _recipeRepository = recipeRepository;
        _ingredientRepository = ingredientRepository;
        _groceryRepository = groceryRepository;
    }

    public async Task<Result<Unit>> CreateNewPlannedMeal(Guid userId, PlannedMealDto plannedMealDto)
    {
        // get mealplanner for user with userid

        var mealPlannerResult = await _mealPlannerRepository.ReadMealPlannerByIdWithNextWeekNoTracking(userId);
        if (!mealPlannerResult.IsSuccess)
        {
            return Result<Unit>.Failure(mealPlannerResult.ErrorMessage!, mealPlannerResult.FailureType);
        }

        var mealPlanner = mealPlannerResult.Value!;

        // check if planned meal exists for date
        var alreadyPlannedMeal = mealPlanner.NextWeek.FirstOrDefault(pm =>
            pm.PlannedDate.ToUniversalTime().Date == plannedMealDto.PlannedDate.Date);
        if (alreadyPlannedMeal != null)
        {
            var deletePlannedMealResult = await _mealPlannerRepository.DeletePlannedMeal(alreadyPlannedMeal);
            if (!deletePlannedMealResult.IsSuccess)
            {
                return Result<Unit>.Failure(deletePlannedMealResult.ErrorMessage!, deletePlannedMealResult.FailureType);
            }
        }

        var linkedRecipeResult = await _recipeRepository.ReadRecipeById(plannedMealDto.Recipe.RecipeId);
        if (!linkedRecipeResult.IsSuccess)
        {
            return Result<Unit>.Failure(linkedRecipeResult.ErrorMessage!, linkedRecipeResult.FailureType);
        }

        var linkedRecipe = linkedRecipeResult.Value!;

        var linkedIngredientQuantities = new List<IngredientQuantity>();

        foreach (var ingredientQuantityDto in plannedMealDto.Ingredients)
        {
            var ingredientId = ingredientQuantityDto.Ingredient.IngredientId;
            
            var ingredientResult = await _ingredientRepository.ReadIngredientById(ingredientId);
            if (!ingredientResult.IsSuccess)
            {
                return Result<Unit>.Failure(ingredientResult.ErrorMessage!, ingredientResult.FailureType);
            }
            var ingredient = ingredientResult.Value!;

            var ingredientQuantity = new IngredientQuantity()
            {
                Quantity = ingredientQuantityDto.Quantity,
                Ingredient = ingredient,
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

        var createPlannedMealResult = await _mealPlannerRepository.CreatePlannedMeal(plannedMeal);
        if (!createPlannedMealResult.IsSuccess)
        {
            return Result<Unit>.Failure(createPlannedMealResult.ErrorMessage!, createPlannedMealResult.FailureType);
        }

        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<List<PlannedMealDto>>> GetPlannedMealsFromUserAfterDate(DateTime dateTime, Guid userId)
    {
        List<PlannedMeal> plannedMeals;
        if (dateTime.Date == DateTime.Now.Date)
        {
            var plannedMealsResult = await _mealPlannerRepository.ReadNextWeekPlannedMealsNoTracking(userId);
            if (!plannedMealsResult.IsSuccess)
            {
                return Result<List<PlannedMealDto>>.Failure(plannedMealsResult.ErrorMessage!, plannedMealsResult.FailureType);
            }
            plannedMeals = plannedMealsResult.Value!;
        }
        else
        {
            var plannedMealsResult = await _mealPlannerRepository.ReadPlannedMealsAfterDateNoTracking(dateTime, userId);
            if (!plannedMealsResult.IsSuccess)
            {
                return Result<List<PlannedMealDto>>.Failure(plannedMealsResult.ErrorMessage!, plannedMealsResult.FailureType);
            }
            plannedMeals = plannedMealsResult.Value!;
        }

        return Result<List<PlannedMealDto>>.Success(_mapper.Map<List<PlannedMealDto>>(plannedMeals));
    }

    public async Task<Result<List<IngredientQuantityDto>>> GetNextWeekIngredients(Guid userId)
    {
        // Get all planned meals for next week
        var plannedMealsResult = await _mealPlannerRepository.ReadNextWeekPlannedMealsNoTracking(userId);
        if (!plannedMealsResult.IsSuccess)
        {
            return Result<List<IngredientQuantityDto>>.Failure(plannedMealsResult.ErrorMessage!, plannedMealsResult.FailureType);
        }
        var plannedMeals = plannedMealsResult.Value!;

        // Aggregate the ingredients
        var aggregatedIngredients = new Dictionary<Guid, IngredientQuantityDto>();

        foreach (var meal in plannedMeals)
        {
            foreach (var ingredientQuantity in meal.Ingredients)
            {
                // If ingredient already exists in the dictionary, combine the quantities
                if (aggregatedIngredients.ContainsKey(ingredientQuantity.Ingredient.IngredientId))
                {
                    aggregatedIngredients[ingredientQuantity.Ingredient.IngredientId].Quantity +=
                        ingredientQuantity.Quantity;
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

        return Result<List<IngredientQuantityDto>>.Success(aggregatedIngredients.Values.ToList());
    }
}