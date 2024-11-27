using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using BL.Services;
using DOM.Exceptions;
using DOM.MealPlanning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroceryController : ControllerBase
{
    private readonly IGroceryManager _groceryManager;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly ILogger<GroceryController> _logger;

    public GroceryController(IGroceryManager groceryManager, IIdentityProviderService identityProviderService, ILogger<GroceryController> logger)
    {
        _groceryManager = groceryManager;
        _identityProviderService = identityProviderService;
        _logger = logger;
    }

    [HttpGet("{groceryListId}")]
    public IActionResult GetGroceryListById(string groceryListId)
    {
        try
        {
            var groceryList = _groceryManager.GetGroceryList(groceryListId);
            return Ok(groceryList);
        }
        catch (GroceryListNotFoundException e)
        {
            _logger.LogError("An error occured trying to fetch user: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    [HttpGet("account/grocery-list")]
    public IActionResult GetGroceryListByAccessToken([FromHeader(Name = "Authorization")] string accessToken)
    {
        try
        {
            Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
            var groceryListDto = _groceryManager.GetGroceryListByAccountId(userId.ToString());
            return Ok(groceryListDto);
        }
        catch (JwtTokenException e)
        {
            _logger.LogError("An error occurred while trying to get user: {ErrorMessage}", e.Message);
            return Unauthorized("Invalid access token: " + e.Message);
        }
        catch (GroceryListNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{groceryListId}/add-item")]
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
    
    [HttpDelete("{groceryListId}/items/{itemQuantityId}")]
    public async Task<IActionResult> DeleteItemFromList(Guid groceryListId, Guid itemQuantityId)
    {
        try
        {
            await _groceryManager.RemoveItemFromGroceryList(groceryListId, itemQuantityId);
            _logger.LogInformation("Item {ItemQuantityId} deleted from grocery list {GroceryListId}.", itemQuantityId, groceryListId);
            return Ok(new { message = "Item deleted successfully." });
        }
        catch (GroceryListNotFoundException)
        {
            _logger.LogWarning("Grocery list {GroceryListId} or item {ItemQuantityId} not found.", groceryListId, itemQuantityId);
            return NotFound(new { message = "Grocery list or item not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while deleting item {ItemQuantityId} from grocery list {GroceryListId}: {ErrorMessage}", itemQuantityId, groceryListId, ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }


}