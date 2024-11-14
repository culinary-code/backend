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

    public GroceryController(IGroceryManager groceryManager)
    {
        _groceryManager = groceryManager;
    }

    [HttpGet("{accountId}/grocery-list")]
    public async Task<IActionResult> GetGroceryList(Guid accountId)
    {
        try
        { 
            GroceryListDto groceryList =  _groceryManager.CreateGroceryList(accountId);
            return Ok(groceryList);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("{groceryListId}/add-item")]
    public IActionResult AddItemToGroceryList(Guid groceryListId, [FromBody] ItemQuantityDto addItemDto)
    {
        if (addItemDto == null)
        {
            return BadRequest("ItemQuantityDto is required.");
        }

        try
        {
            _groceryManager.AddItemToGroceryList(groceryListId, addItemDto);
            return Ok("Item added to the grocery list.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}