using BL.DTOs.Accounts;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace BL.Managers.Groceries;

public class GroceryManager : IGroceryManager
{
    private readonly IGroceryRepository _groceryRepository;
    private readonly IAccountRepository _accountRepository;

    public GroceryManager(IGroceryRepository groceryRepository, IAccountRepository accountRepository)
    {
        _groceryRepository = groceryRepository;
        _accountRepository = accountRepository;
    }

    public GroceryListDto CreateGroceryList(Guid accountId)
    {
        var account = _accountRepository.ReadAccount(accountId);

        if (account == null)
        {
            throw new Exception("Account not found");
        }
        
        var mealplanner = _groceryRepository.GetMealPlannerById(accountId);
        
        if (!mealplanner.NextWeek.Any())
        {
            throw new Exception("No planned meals found for next week");
        }
        
        var allIngredientQuantities = mealplanner.NextWeek
            .SelectMany(plannedMeal => plannedMeal.Ingredients)
            .Where(ingredient => ingredient.Quantity > 0)
            .ToList();

        var arrangedIngredients = allIngredientQuantities
            //.Where(iq => iq.Quantity > 0)
            .GroupBy(ingredientQuantity => new
            {
                ingredientQuantity.Ingredient?.IngredientName,
                ingredientQuantity.Ingredient?.Measurement
            })
            .Select(group => new IngredientQuantityDto
            {
                IngredientQuantityId = Guid.NewGuid(),
                Quantity = group.Sum(ingredientQuantity => ingredientQuantity.Quantity),
                Ingredient = new IngredientDto()
                {
                    IngredientId = group.First().Ingredient.IngredientId,
                    IngredientName = group.First().Ingredient.IngredientName,
                    Measurement = group.First().Ingredient.Measurement
                }
            })
            .ToList();
        
        var items = mealplanner.NextWeek
            .Select(plannedMeal => new ItemQuantityDto
            {
                IngredientQuantityId = Guid.NewGuid(),  // Example: generate or map actual IngredientQuantityId
                Quantity = plannedMeal.Ingredients.Sum(iq => iq.Quantity),  // Example: summing quantities of ingredients in each planned meal
                Ingredient = new IngredientDto
                {
                    IngredientId = plannedMeal.Ingredients.First().Ingredient.IngredientId,  // Example: assume at least one ingredient exists
                    IngredientName = plannedMeal.Ingredients.First().Ingredient.IngredientName,
                    Measurement = plannedMeal.Ingredients.First().Ingredient.Measurement
                }
            })
            .ToList();
        
        var groceryListDto = new GroceryListDto
        {
            GroceryListId = Guid.NewGuid(),
            Ingredients = arrangedIngredients,
            Items = items,
            Account  = new AccountDto 
            {
                AccountId = account.AccountId,
                Name = account.Name,
                Email = account.Email,
                FamilySize = account.FamilySize
            }
        };

        GroceryList groceryList = new GroceryList()
        {
            GroceryListId = groceryListDto.GroceryListId,
            Ingredients = allIngredientQuantities,
            Account = account,
        };
        
        _groceryRepository.CreateGroceryList(groceryList);
        
        return groceryListDto;
    }
}