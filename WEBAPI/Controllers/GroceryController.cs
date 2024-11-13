using BL.DTOs.MealPlanning;
using BL.Managers.Groceries;
using DOM.MealPlanning;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroceryController : ControllerBase
{
    private readonly IGroceryManager _groceryManager;

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
}