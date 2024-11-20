﻿using AutoMapper;
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
    private readonly IMealPlannerRepository _mealPlannerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GroceryManager> _logger;
    
    public GroceryManager(IGroceryRepository groceryRepository, IMapper mapper, IMealPlannerRepository mealPlannerRepository, ILogger<GroceryManager> logger)
    {
        _groceryRepository = groceryRepository;
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
            .FirstOrDefault(i => i.Ingredient.IngredientName.ToLower() == newListItem.Ingredient.IngredientName.ToLower());
        var existingItem = groceryList.Items.FirstOrDefault(i => i.GroceryItem.GroceryItemName.ToLower() == newListItem.Ingredient.IngredientName.ToLower());
        
        if (existingIngredient != null)
        {
            existingIngredient.Quantity += newListItem.Quantity;
            _logger.LogInformation($"{existingIngredient} has been updated");
            _groceryRepository.UpdateGroceryList(groceryList);
        }  
        else if (existingItem != null)
        {
            existingItem.Quantity += newListItem.Quantity;
            _logger.LogInformation($"{existingItem} has been updated");
            _groceryRepository.UpdateGroceryList(groceryList);
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
}