using AutoMapper;
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
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GroceryManager> _logger;

    public GroceryManager(IGroceryRepository groceryRepository, IMapper mapper, ILogger<GroceryManager> logger,
        IAccountRepository accountRepository, IIngredientRepository ingredientRepository)
    {
        _groceryRepository = groceryRepository;
        _mapper = mapper;
        _logger = logger;
        _accountRepository = accountRepository;
        _ingredientRepository = ingredientRepository;
    }

    public void CreateNewGroceryList(GroceryList groceryList)
    {
        _groceryRepository.CreateGroceryList(groceryList);
    }

    public GroceryListDto GetGroceryListWithNextWeek(Guid accountId)
    {
        var account = _accountRepository.ReadAccountWithMealPlannerNextWeekAndWithGroceryList(accountId);
        var completeGroceryList = account.GroceryList;

        foreach (var plannedMeal in account.Planner.NextWeek)
        {
            foreach (var ingredient in plannedMeal.Ingredients)
            {
                completeGroceryList.Ingredients.Add(ingredient);
            }
        }
        
        
        return _mapper.Map<GroceryListDto>(completeGroceryList);
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
        return _mapper.Map<GroceryListDto>(groceryList);
    }

    public void AddItemToGroceryList(Guid userId, ItemQuantityDto newListItem)
    {
        if (newListItem == null)
        {
            throw new GroceryListItemNotFoundException("No itemquantityDto provided" );
        }
        
        var groceryList = _groceryRepository.ReadGroceryListByAccountId(userId);
        
        // if newListItem has quantityId: add item to gl, else update existing row
        // when a new item is passed into the endpoint, it will not have an existing ItemQuantity, thus its Guid will be 00000000-0000-0000-0000-000000000000
        if (newListItem.ItemQuantityId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
        {
            if (newListItem.GroceryItem == null)
            {
                throw new GroceryListItemNotFoundException("Item does not exist.");
            }
            var name = newListItem.GroceryItem.GroceryItemName;
            CreateNewItemInGroceryList(groceryList, name, newListItem);
        }
        else
        {
            UpdateExistingGroceryListItem(newListItem);
        }
        _groceryRepository.UpdateGroceryList(groceryList);
    }
    
    public async Task RemoveItemFromGroceryList(Guid userId, ItemQuantityDto removeItem)
    {
        if (removeItem.IsIngredient)
        {
            await _ingredientRepository.DeleteIngredientQuantity(userId, removeItem.ItemQuantityId);
        }
        else
        {
            await _groceryRepository.DeleteItemQuantity(userId, removeItem.ItemQuantityId);
        }
    }

    private void CreateNewItemInGroceryList(GroceryList groceryList, string name, ItemQuantityDto newListItem)
    {
        MeasurementType measurementType = newListItem.GroceryItem.Measurement;
        Ingredient? ingredient = _ingredientRepository.ReadPossibleIngredientByNameAndMeasurement(name, measurementType);

        // if no ingredient is found with that id, it is an item
        if (ingredient == null)
        {
            GroceryItem? groceryItem = _groceryRepository.ReadPossibleGroceryItemByNameAndMeasurement(name, measurementType);
            ItemQuantity newItemQuantity;
            if (groceryItem == null)
            {
                var newGroceryItem = new GroceryItem
                {
                    GroceryItemName = name,
                    Measurement = measurementType,
                };
                newItemQuantity = new ItemQuantity()
                {
                    GroceryItem = newGroceryItem,
                    Quantity = newListItem.Quantity,
                };
            }
            else
            {
                newItemQuantity = new ItemQuantity()
                {
                    GroceryItem = groceryItem,
                    Quantity = newListItem.Quantity,
                };
            }
            groceryList.Items.Add(newItemQuantity);
        }
        else
        {
            groceryList.Ingredients.Add(new IngredientQuantity()
            {
                Ingredient = ingredient,
                Quantity = newListItem.Quantity,
            });
        }
    }

    private void UpdateExistingGroceryListItem(ItemQuantityDto newListItem)
    {
        if (newListItem.IsIngredient)
        {
            IngredientQuantity ingredientQuantity = _ingredientRepository.ReadIngredientQuantityById(newListItem.ItemQuantityId);
            ingredientQuantity.Quantity = newListItem.Quantity;
        }
        else
        {
            ItemQuantity itemQuantity = _groceryRepository.ReadItemQuantityById(newListItem.ItemQuantityId);
            itemQuantity.Quantity = newListItem.Quantity;
        }
        
    }
}
