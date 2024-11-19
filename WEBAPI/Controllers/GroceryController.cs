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
    private readonly IMapper _mapper;
    //private readonly Logger<GroceryController> _logger;

    public GroceryController(IGroceryManager groceryManager, IMapper mapper, IIdentityProviderService identityProviderService)
    {
        _groceryManager = groceryManager;
        _mapper = mapper;
        _identityProviderService = identityProviderService;
        //_logger = logger;
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
            //_logger.LogError("An error occured trying to fetch user: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    [Authorize]
    [HttpGet("/account/grocery-list")]
    public IActionResult GetGroceryListByAccessToken([FromHeader] string accessToken)
    {
        try
        {
            // Extract AccountId from the access token
            var accountId = _identityProviderService.GetGuidFromAccessToken(accessToken);

            // Fetch the grocery list for this account
            var groceryListDto = _groceryManager.GetGroceryListByAccountId(accountId.ToString());
        
            return Ok(groceryListDto);
        }
        catch (JwtTokenException e)
        {
            return Unauthorized("Invalid access token: " + e.Message);
        }
        catch (GroceryListNotFoundException e)
        {
            return NotFound(e.Message);
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
            //_logger.LogError("An error occured while creating grocerylist: {ErrorMessage}", e.Message);
            return BadRequest("Failed to create new grocery list.");
        }
    }

    [HttpPut("{groceryListId}/add-item")]
    [Authorize]
    public IActionResult AddItemToGroceryList(Guid groceryListId, [FromBody] ItemQuantityDto newItem)
    {
        if (newItem == null)
        {
            //_logger.LogError($"Item was null for {groceryListId}");
            return BadRequest("ItemQuantity is required!");
        }

        try
        {
            _groceryManager.AddItemToGroceryList(groceryListId, newItem);
            //_logger.LogInformation("Item added to grocery list.");
            return Ok($"{newItem} added to {groceryListId} grocery list.");
        }
        catch (GroceryListNotFoundException ex)
        {
            //_logger.LogError($"Grocery list was not found for {groceryListId}");
            return BadRequest(ex.Message);
        }
    }
    
    
    [HttpGet]
    [Route("grocerylist")]
    [Authorize]
    public IActionResult GetGroceryList([FromHeader(Name = "Authorization")] string accessToken)
    {
        try
        {
            // Extract the GUID from the access token
            var userId = _identityProviderService.GetGuidFromAccessToken(accessToken);
            // Fetch the grocery list for the user
            var groceryList = _groceryManager.GetGroceryListByAccountId(userId.ToString());

            if (groceryList == null)
            {
                return NotFound(new { Message = "Grocery list not found for the specified user." });
            }

            return Ok(groceryList);
        }
        catch (JwtTokenException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An unexpected error occurred.", Details = ex.Message });
        }
    }

    /*private Guid GetGuidFromAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (tokenHandler.CanReadToken(accessToken))
        {
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
        }

        throw new JwtTokenException("Failed to get userId from access token");
    }*/
    
    /*[HttpGet("{accountId}/grocery-list")]
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
}*/
}