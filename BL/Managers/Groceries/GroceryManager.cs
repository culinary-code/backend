using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Groceries;

public class GroceryManager : IGroceryManager
{
    private readonly IGroceryRepository _groceryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GroceryManager> _logger;

    public GroceryManager(IGroceryRepository groceryRepository, IMapper mapper, ILogger<GroceryManager> logger,
        IAccountRepository accountRepository)
    {
        _groceryRepository = groceryRepository;
        _mapper = mapper;
        _logger = logger;
        _accountRepository = accountRepository;
    }

    public void CreateNewGroceryList(GroceryList groceryList)
    {
        _groceryRepository.CreateGroceryList(groceryList);
    }

    public GroceryListDto getGroceryListWithNextWeek(Guid accountId)
    {
        var account = _accountRepository.ReadAccountWithMealPlannerNextWeekAndWithGroceryList(accountId);
        var completeGroceryList = account.GroceryList;

        foreach (var plannedMeal in account.Planner.NextWeek)
        {
            completeGroceryList.Ingredients = completeGroceryList.Ingredients.Concat(plannedMeal.Ingredients);
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

    public void AddItemToGroceryList(Guid groceryListId, ItemQuantityDto newListItem)
    {
        if (newListItem == null || newListItem.GroceryItem == null)
        {
            throw new GroceryListItemNotFoundException("Item does not exist.");
        }

        var groceryList = _groceryRepository.ReadGroceryListById(groceryListId);

        var existingIngredient = groceryList.Ingredients
            .FirstOrDefault(i =>
                i.Ingredient.IngredientName.ToLower() == newListItem.GroceryItem.GroceryItemName.ToLower());
        
        if (existingIngredient != null)
        {
            existingIngredient.Quantity = newListItem.Quantity;
            _logger.LogInformation($"{existingIngredient} has been updated");
            _groceryRepository.UpdateGroceryList(groceryList);
        }
        else if (existingIngredient == null)
        {
            var existingItem = groceryList.Items.FirstOrDefault(i =>
                i.GroceryItem.GroceryItemName.ToLower() == newListItem.GroceryItem.GroceryItemName.ToLower());
            if (existingItem != null)
            {
                existingItem.Quantity = newListItem.Quantity;
                _logger.LogInformation($"{existingItem} has been updated");
                _groceryRepository.UpdateGroceryList(groceryList);
            }
            else
            {
                var newItem = new ItemQuantity
                {
                    ItemQuantityId = Guid.NewGuid(),
                    Quantity = newListItem.Quantity,
                    GroceryList = groceryList,
                    GroceryItem = new GroceryItem()
                    {
                        GroceryItemId = newListItem.GroceryItem.GroceryItemId,
                        GroceryItemName = newListItem.GroceryItem.GroceryItemName,
                        Measurement = newListItem.GroceryItem.Measurement,
                    }
                };
                groceryList.Items = groceryList.Items.Append(newItem).ToList();
                _groceryRepository.AddGroceryListItem(groceryList, newItem);
                _logger.LogInformation(newListItem.GroceryItem.GroceryItemId + " has been added to grocery list");
            }
        }
    }
    
    public async Task RemoveItemFromGroceryList(Guid groceryListId, Guid itemId)
    {
        await _groceryRepository.DeleteItemFromGroceryList(groceryListId, itemId);
    }
}