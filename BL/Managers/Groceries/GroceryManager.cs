using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using DAL.Accounts;
using DAL.Groceries;
using DAL.Recipes;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using DOM.Results;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Groceries;

public class GroceryManager : IGroceryManager
{
    private readonly IGroceryRepository _groceryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GroceryManager> _logger;

    public GroceryManager(IGroceryRepository groceryRepository, IMapper mapper, ILogger<GroceryManager> logger,
        IAccountRepository accountRepository, IIngredientRepository ingredientRepository, IGroupRepository groupRepository)
    {
        _groceryRepository = groceryRepository;
        _mapper = mapper;
        _logger = logger;
        _accountRepository = accountRepository;
        _ingredientRepository = ingredientRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<GroceryListDto>> GetGroceryListWithNextWeek(Guid accountId)
    {
        var accountResult = await _accountRepository.ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking(accountId);
        if (!accountResult.IsSuccess)
        {
            return Result<GroceryListDto>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value;
        
        // Check if user is in group-mode
        if (account!.ChosenGroupId.HasValue)
        {
            var groupResult = await _groupRepository.ReadGroupWithMealPlannerNextWeekAndWithGroceryListNoTracking(account.ChosenGroupId.Value);
            if (!groupResult.IsSuccess)
            {
                return Result<GroceryListDto>.Failure(groupResult.ErrorMessage!, groupResult.FailureType);
            }
            var group = groupResult.Value;
            var completeGroceryList = group!.GroceryList;

            foreach (var plannedMeal in group.MealPlanner!.NextWeek)
            {
                foreach (var ingredient in plannedMeal.Ingredients)
                {
                    completeGroceryList!.Ingredients.Add(ingredient);
                }
            }
        
            return  Result<GroceryListDto>.Success(_mapper.Map<GroceryListDto>(completeGroceryList));
        }
        else
        {
            var completeGroceryList = account!.GroceryList;
            
                    foreach (var plannedMeal in account.Planner!.NextWeek)
                    {
                        foreach (var ingredient in plannedMeal.Ingredients)
                        {
                            completeGroceryList!.Ingredients.Add(ingredient);
                        }
                    }
                    
                    return  Result<GroceryListDto>.Success(_mapper.Map<GroceryListDto>(completeGroceryList));
        }
    }

    public async Task<Result<GroceryListDto>> GetGroceryListByAccountId(string accountId)
    {
        Guid id = Guid.Parse(accountId);
        var groceryListResult = await _groceryRepository.ReadGroceryListByAccountId(id);
        if (!groceryListResult.IsSuccess)
        {
            return Result<GroceryListDto>.Failure(groceryListResult.ErrorMessage!, groceryListResult.FailureType);
        }
        var groceryList = groceryListResult.Value!;
        
        return Result<GroceryListDto>.Success(_mapper.Map<GroceryListDto>(groceryList));
    }

    public async Task<Result<Unit>> AddItemToGroceryList(Guid userId, ItemQuantityDto newListItem)
    {
        var accountResult = await _accountRepository.ReadAccount(userId);
        if (!accountResult.IsSuccess)
        {
            return Result<Unit>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value!;

        GroceryList groceryList;
        
        if (account.ChosenGroupId.HasValue)
        {
            var groupGroceryListResult = await _groceryRepository.ReadGroceryListByGroupId(account.ChosenGroupId.Value);
            if (!groupGroceryListResult.IsSuccess)
            {
                return Result<Unit>.Failure(groupGroceryListResult.ErrorMessage!, groupGroceryListResult.FailureType);
            }
            groceryList = groupGroceryListResult.Value!;
        }
        else
        {
            // Retrieve the user's personal grocery list
            var groceryListResult = await _groceryRepository.ReadGroceryListByAccountId(userId);
            if (!groceryListResult.IsSuccess)
            {
                return Result<Unit>.Failure(groceryListResult.ErrorMessage!, groceryListResult.FailureType);
            }
            groceryList = groceryListResult.Value!;
        }
        
        // if newListItem has quantityId: add item to gl, else update existing row
        // when a new item is passed into the endpoint, it will not have an existing ItemQuantity, thus its Guid will be 00000000-0000-0000-0000-000000000000
        if (newListItem.ItemQuantityId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
        {
            // a new item to be added cannot be empty
            if (newListItem.GroceryItem == null)
            {
                return Result<Unit>.Failure("Item does not exist.", ResultFailureType.Error);
            }
            var name = newListItem.GroceryItem.GroceryItemName;
            var createNewItemResult = await CreateNewItemInGroceryList(groceryList, name, newListItem);
            if (!createNewItemResult.IsSuccess)
            {
                return Result<Unit>.Failure(createNewItemResult.ErrorMessage!, createNewItemResult.FailureType);
            }
        }
        else
        {
            var updateExistingGroceryListItemResult = await UpdateExistingGroceryListItem(newListItem);
            if (!updateExistingGroceryListItemResult.IsSuccess)
            {
                return Result<Unit>.Failure(updateExistingGroceryListItemResult.ErrorMessage!, updateExistingGroceryListItemResult.FailureType);
            }
        }
        var updateGroceryListResult = await _groceryRepository.UpdateGroceryList(groceryList);
        if (!updateGroceryListResult.IsSuccess)
        {
            return Result<Unit>.Failure(updateGroceryListResult.ErrorMessage!, updateGroceryListResult.FailureType);
        }
        
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> RemoveItemFromGroceryList(Guid userId, ItemQuantityDto removeItem)
    {
        var accountResult = await _accountRepository.ReadAccount(userId);
        if (!accountResult.IsSuccess)
        {
            return Result<Unit>.Failure(accountResult.ErrorMessage!, accountResult.FailureType);
        }
        var account = accountResult.Value!;

        if (account.ChosenGroupId.HasValue)
        {
            if (removeItem.IsIngredient)
            {
                return await _ingredientRepository.DeleteIngredientQuantityFromGroup(account.ChosenGroupId.Value, removeItem.ItemQuantityId);
            }

            return await _groceryRepository.DeleteItemQuantityByGroupId(account.ChosenGroupId.Value, removeItem.ItemQuantityId);
        }
        
        if (removeItem.IsIngredient)
        {
            return await _ingredientRepository.DeleteIngredientQuantity(userId, removeItem.ItemQuantityId);
        }

        return await _groceryRepository.DeleteItemQuantityByUserId(userId, removeItem.ItemQuantityId);
    }

    private async Task<Result<Unit>> CreateNewItemInGroceryList(GroceryList groceryList, string name, ItemQuantityDto newListItem)
    {
        MeasurementType measurementType = newListItem.GroceryItem.Measurement;
        var ingredientResult = await _ingredientRepository.ReadIngredientByNameAndMeasurementType(name, measurementType);

        // if no ingredient is found with that id, it is an item
        if (!ingredientResult.IsSuccess && ingredientResult.FailureType == ResultFailureType.NotFound)
        {
            var groceryItemResult = await _groceryRepository.ReadGroceryItemByNameAndMeasurement(name, measurementType);
            ItemQuantity newItemQuantity;
            if (!groceryItemResult.IsSuccess && groceryItemResult.FailureType == ResultFailureType.NotFound)
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
                var groceryItem = groceryItemResult.Value!;
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
            var ingredient = ingredientResult.Value!;
            groceryList.Ingredients.Add(new IngredientQuantity()
            {
                Ingredient = ingredient,
                Quantity = newListItem.Quantity,
            });
        }
        return Result<Unit>.Success(new Unit());
    }

    private async Task<Result<Unit>> UpdateExistingGroceryListItem(ItemQuantityDto newListItem)
    {
        if (newListItem.IsIngredient)
        {
            var ingredientQuantityResult = await _ingredientRepository.ReadIngredientQuantityById(newListItem.ItemQuantityId);
            if (!ingredientQuantityResult.IsSuccess)
            {
                return Result<Unit>.Failure(ingredientQuantityResult.ErrorMessage!, ingredientQuantityResult.FailureType);
            }
            var ingredientQuantity = ingredientQuantityResult.Value!;
            ingredientQuantity.Quantity = newListItem.Quantity;
        }
        else
        {
            var itemQuantityResult = await _groceryRepository.ReadItemQuantityById(newListItem.ItemQuantityId);
            if (!itemQuantityResult.IsSuccess)
            {
                return Result<Unit>.Failure(itemQuantityResult.ErrorMessage!, itemQuantityResult.FailureType);
            }
            var itemQuantity = itemQuantityResult.Value!;
            itemQuantity.Quantity = newListItem.Quantity;
        }
        return Result<Unit>.Success(new Unit());
    }
}
