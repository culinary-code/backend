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

    [HttpGet("grocery-list")]
    public IActionResult GetGroceryListById()
    {
        try
        {
            Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
            var groceryList = _groceryManager.GetGroceryListWithNextWeek(userId);
            return Ok(groceryList);
        }
        catch (GroceryListNotFoundException e)
        {
            _logger.LogError("An error occured trying to fetch user: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    [HttpGet("account/grocery-list")]
    public IActionResult GetGroceryListByAccessToken()
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

    [HttpPut("grocery-list/add-item")]
    public IActionResult AddItemToGroceryList([FromBody] ItemQuantityDto newItem)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));

        try
        {
            _groceryManager.AddItemToGroceryList(userId, newItem);
            _logger.LogInformation("Item added to grocery list.");
            return Ok($"{newItem} added to grocery list of account: {userId}.");
        }
        catch (GroceryListNotFoundException ex)
        {
            _logger.LogError($"Grocery list was not found for account with id: {userId}");
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("grocery-list/items")]
    public async Task<IActionResult> DeleteItemFromList([FromBody] ItemQuantityDto removeItem)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        try
        {
            await _groceryManager.RemoveItemFromGroceryList(userId, removeItem);
            _logger.LogInformation("Item {ItemQuantityId} deleted from grocery list of account {UserId}.", removeItem.ItemQuantityId, userId);
            return Ok(new { message = "Item deleted successfully." });
        }
        catch (GroceryListNotFoundException)
        {
            _logger.LogWarning("Grocery list of account {UserId} or item {ItemQuantityId} not found.", userId, removeItem.ItemQuantityId);
            return NotFound(new { message = "Grocery list or item not found." });
        }
    }
}