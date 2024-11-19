using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
using DAL.Recipes;
using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Groceries;

public class GroceryManager : IGroceryManager
{
    private readonly IGroceryRepository _groceryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMealPlannerRepository _mealPlannerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GroceryManager> _logger;
    
    public GroceryManager(IGroceryRepository groceryRepository, IAccountRepository accountRepository, IMapper mapper, IMealPlannerRepository mealPlannerRepository, ILogger<GroceryManager> logger)
    {
        _groceryRepository = groceryRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _mealPlannerRepository = mealPlannerRepository;
        _logger = logger;
    }
    
    public void CreateNewGroceryList(GroceryList groceryList)
    {
        groceryList = new GroceryList();
        _groceryRepository.CreateGroceryList(groceryList);
    }

    public GroceryListDto GetGroceryList(string id)
    {
        Guid groceryId = Guid.Parse(id);
        var groceryList = _groceryRepository.ReadGroceryListById(groceryId);
        return _mapper.Map<GroceryListDto>(groceryList);
    }

    public GroceryListDto GetGroceryListByAccountId(string accountId)
    {
        Guid id = Guid.Parse(accountId);
        var groceryList = _groceryRepository.ReadGroceryListByAccountId(id);

        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("No grocery list found for this account.");
        }

        return _mapper.Map<GroceryListDto>(groceryList);
    }

    public void AddItemToGroceryList(Guid groceryListId, ItemQuantityDto newListItem)
    {
        if (newListItem == null || newListItem.Ingredient == null)
        {
            throw new GroceryListNotFoundException("New item is empty");
        }
        
        var groceryList = _groceryRepository.ReadGroceryListById(groceryListId);

        if (groceryList == null)
        {
            throw new GroceryListNotFoundException("Grocery list not found!");
        }

        var existingIngredient = groceryList.Ingredients
            .FirstOrDefault(i => i.Ingredient.IngredientName == newListItem.Ingredient.IngredientName || i.Ingredient.IngredientName == newListItem.Ingredient.IngredientName);

        if (existingIngredient != null)
        {
            existingIngredient.Quantity += newListItem.Quantity;
            _logger.LogInformation($"{existingIngredient} has been updated");
        }
        else 
        {
            var newItem = new ItemQuantity
            {
                IngredientQuantityId = Guid.NewGuid(),
                Quantity = newListItem.Quantity,
                GroceryList = groceryList,
                GroceryItem = new GroceryItem()
                {
                    GroceryItemId = newListItem.Ingredient.IngredientId,
                    GroceryItemName = newListItem.Ingredient.IngredientName,
                    Measurement = newListItem.Ingredient.Measurement,
                }
            };
            groceryList.Items = groceryList.Items.Append(newItem).ToList();
            //groceryList.Items.ToList().Add(newItem);
            Console.WriteLine("KIP:" + newListItem.Ingredient.IngredientName + newItem.GroceryItem.GroceryItemName);
            
            //_groceryRepository.UpdateGroceryList(groceryList);
            _groceryRepository.AddGroceryListItem(groceryList, newItem);
            
            _logger.LogInformation(newListItem.Ingredient.IngredientId + " has been added to grocery list");
        }
    }

    public GroceryListDto? UpdateGroceryList(Guid accountId, GroceryListDto groceryList)
    {
        var mealPlanner = _mealPlannerRepository.ReadMealPlannerById(accountId);
        var plannedMeals = mealPlanner.NextWeek;
        return groceryList;
    }
    
    
    
   /* public GroceryListDto CreateGroceryList(Guid accountId)
    {
        var account = _accountRepository.ReadAccount(accountId);
        
        var mealplanner = _mealPlannerRepository.ReadMealPlannerById(accountId);
        
        var allIngredientQuantities = mealplanner.NextWeek
            .SelectMany(plannedMeal => plannedMeal.Ingredients)
            .Where(ingredient => ingredient.Quantity > 0)
            .ToList();

        var arrangedIngredients = allIngredientQuantities
            .Where(iq => iq.Ingredient != null)
            .GroupBy(iq => iq.Ingredient.IngredientId)
            .Select(group => new IngredientQuantityDto()
            {
                IngredientQuantityId = Guid.NewGuid(),
                Quantity = group.Sum(ingredientQuantity => ingredientQuantity.Quantity),
                Ingredient = new IngredientDto
                {
                    IngredientId = group.First().Ingredient.IngredientId,
                    IngredientName = group.First().Ingredient.IngredientName,
                    Measurement = group.First().Ingredient.Measurement
                }
            })
            .ToList();

        var items = new List<ItemQuantityDto>();
        
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

        GroceryList groceryList = new GroceryList
        {
            GroceryListId = groceryListDto.GroceryListId,
            Account = account,
            Items = new List<ItemQuantity>(),
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
        
        _logger.LogInformation($"{groceryListDto.GroceryListId} has been added.");
        _groceryRepository.CreateGroceryList(groceryList);
        return groceryListDto;
    }
    */
}