using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using BL.Services;
using DOM.MealPlanning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroceryController : ControllerBase
{
    private readonly IGroceryManager _groceryManager;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly ILogger<GroceryController> _logger;

    public GroceryController(IGroceryManager groceryManager, IIdentityProviderService identityProviderService,
        ILogger<GroceryController> logger)
    {
        _groceryManager = groceryManager;
        _identityProviderService = identityProviderService;
        _logger = logger;
    }

    [HttpGet("grocery-list")]
    public async Task<IActionResult> GetGroceryListById()
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;
        var groceryList = await _groceryManager.GetGroceryListWithNextWeek(userId);
        return groceryList.ToActionResult();
    }

    [HttpGet("account/grocery-list")]
    public async Task<IActionResult> GetGroceryListByAccessToken()
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;
        var groceryListDto = await _groceryManager.GetGroceryListByAccountId(userId.ToString());
        return groceryListDto.ToActionResult();
    }

    [HttpPut("grocery-list/add-item")]
    public async Task<IActionResult> AddItemToGroceryList([FromBody] ItemQuantityDto newItem)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var addItemToGroceryListResult = await _groceryManager.AddItemToGroceryList(userId, newItem);
        _logger.LogInformation("Item added to grocery list.");
        return addItemToGroceryListResult.ToActionResult();
    }

    [HttpDelete("grocery-list/items")]
    public async Task<IActionResult> DeleteItemFromList([FromBody] ItemQuantityDto removeItem)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var removeItemFromGroceryListResult = await _groceryManager.RemoveItemFromGroceryList(userId, removeItem);
        _logger.LogInformation("Item {ItemQuantityId} deleted from grocery list of account {UserId}.",
            removeItem.ItemQuantityId, userId);
        return removeItemFromGroceryListResult.ToActionResult();
    }
}