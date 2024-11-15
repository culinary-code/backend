using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using DOM.Exceptions;
using DOM.MealPlanning;
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

    [HttpPost("/add-grocery-list/{accountId}")]
    public IActionResult CreateNewGroceryList([FromBody] GroceryList groceryList)
    {
        try
        {
            _groceryManager.CreateNewGroceryList(groceryList);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError("An error occured while creating grocerylist: {ErrorMessage}", e.Message);
            return BadRequest("Failed to create new grocery list.");
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
        catch (GroceryListNotFoundException ex)
        {
            _logger.LogError($"Grocery list was not found for {groceryListId}");
            return BadRequest(ex.Message);
        }
    }
}