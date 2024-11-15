using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroceryController : ControllerBase
{
    private readonly IGroceryManager _groceryManager;
    private readonly IMapper _mapper;
    private readonly Logger<GroceryController> _logger;

    public GroceryController(IGroceryManager groceryManager, IMapper mapper, Logger<GroceryController> logger)
    {
        _groceryManager = groceryManager;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("{accountId}/grocery-list")]
    public async Task<IActionResult> GetGroceryList(Guid accountId)
    {
        try
        { 
            GroceryListDto groceryList =  _groceryManager.CreateGroceryList(accountId);
            _logger.LogInformation($"Grocery list for {accountId} has been created.");
            return Ok(groceryList);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("{groceryListId}/add-item")]
    public IActionResult AddItemToGroceryList(Guid groceryListId, [FromBody] ItemQuantityDto newItem)
    {
        if (newItem == null)
        {
            _logger.LogError($"Item was null for {groceryListId}");
            return BadRequest("ItemQuantity is required!");
        }

        try
        {
            _groceryManager.AddItemToGroceryList(groceryListId, newItem);
            _logger.LogInformation("Item added to grocery list.");
            return Ok($"{newItem} added to {groceryListId} grocery list.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError($"Item was not found for {groceryListId}");
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            _logger.LogError($"Item was not found for {groceryListId}");
            return BadRequest(ex.Message);
        }
    }
}