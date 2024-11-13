using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
using DAL.Recipes;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace BL.Managers.Groceries;

public class GroceryManager : IGroceryManager
{
    private readonly IGroceryRepository _groceryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMealPlannerRepository _mealPlannerRepository;
    private readonly IMapper _mapper;


    public GroceryManager(IGroceryRepository groceryRepository, IAccountRepository accountRepository, IMapper mapper, IMealPlannerRepository mealPlannerRepository)
    {
        _groceryRepository = groceryRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _mealPlannerRepository = mealPlannerRepository;
    }

    public GroceryListDto CreateGroceryList(Guid accountId)
    {
        var account = _accountRepository.ReadAccount(accountId);

        if (account == null)
        {
            throw new Exception("Account not found");
        }
        
        var mealplanner = _mealPlannerRepository.ReadMealPlannerById(accountId);
        
        if (!mealplanner.NextWeek.Any())
        {
            throw new Exception("No planned meals found for next week");
        }
        
        var allIngredientQuantities = mealplanner.NextWeek
            .SelectMany(plannedMeal => plannedMeal.Ingredients)
            .Where(ingredient => ingredient.Quantity > 0)
            .ToList();

        var arrangedIngredients = allIngredientQuantities
            .Where(iq => iq.Ingredient != null)
            .GroupBy(iq => iq.Ingredient.IngredientId)
            .Select(group => new IngredientQuantity
            {
                IngredientQuantityId = Guid.NewGuid(),
                Quantity = group.Sum(ingredientQuantity => ingredientQuantity.Quantity),
                Ingredient = group.First().Ingredient
            })
            .ToList();
        
        var items = arrangedIngredients
            .Select(iq => new ItemQuantityDto
            {
                IngredientQuantityId = iq.IngredientQuantityId,
                Quantity = iq.Quantity,
                Ingredient = new IngredientDto
                {
                    IngredientId = iq.Ingredient.IngredientId,
                    IngredientName = iq.Ingredient.IngredientName,
                    Measurement = iq.Ingredient.Measurement
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
            Account = account,
            Ingredients = arrangedIngredients.Select(iq => new IngredientQuantity
            {
                IngredientQuantityId = iq.IngredientQuantityId,
                Quantity = iq.Quantity,
                Ingredient = new Ingredient
                {
                    IngredientId = iq.Ingredient.IngredientId,
                    IngredientName = iq.Ingredient.IngredientName,
                    Measurement = iq.Ingredient.Measurement
                }
            }).ToList(),
        };
        
        Console.WriteLine($"Arranged Ingredients Count: {arrangedIngredients.Count}");
        foreach (var ingredient in arrangedIngredients)
        {
            Console.WriteLine($"Ingredient: {ingredient.Ingredient?.IngredientName}, Quantity: {ingredient.Quantity}");
        }
        
        _groceryRepository.CreateGroceryList(groceryList);
        
        return groceryListDto;
    }
}