using System.Text;
using AutoMapper;
using BL.DTOs.Llm;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using DOM.Exceptions;
using DOM.Recipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class RecipeController : ControllerBase
{
    private readonly ILogger<RecipeController> _logger;
    private readonly IRecipeManager _recipeManager;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IAccountManager _accountManager;

    public RecipeController(ILogger<RecipeController> logger, IRecipeManager recipeManager, IIdentityProviderService identityProviderService, IAccountManager accountManager)
    {
        _logger = logger;
        _recipeManager = recipeManager;
        _identityProviderService = identityProviderService;
        _accountManager = accountManager;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipeById(string id)
    {
        try
        {
            var recipe = await _recipeManager.GetRecipeDtoById(id);
            return Ok(recipe);
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }

    [HttpGet("ByName/{name}")]
    public async Task<IActionResult> GetRecipeByName(string name)
    {
        try
        {
            var recipe = await _recipeManager.GetRecipeDtoByName(name);
            return Ok(recipe);
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }

    [HttpGet("Collection/ByName/{name}")]
    public async Task<IActionResult> GetRecipeCollectionByName(string name)
    {
        try
        {
            var recipes = await _recipeManager.GetRecipesCollectionByName(name);
            return Ok(recipes);
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    
    [HttpPost("Collection/Filtered")]
    public async Task<IActionResult> GetFilteredRecipeCollection([FromBody] RecipeFilterDto filter)
    {
        try
        {
            Enum.TryParse<RecipeType>(filter.MealType, out var mealTypeEnum);
            Enum.TryParse<Difficulty>(filter.Difficulty, out var difficultyEnum);
            var recipes = await _recipeManager.GetFilteredRecipeCollection(filter.RecipeName, difficultyEnum, mealTypeEnum, filter.CookTime, filter.Ingredients );
            return Ok(recipes);
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateRecipe([FromBody] RecipeFilterDto request)
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
            var preferences = await _accountManager.GetPreferencesByUserId(userId);
            var recipe = await _recipeManager.CreateRecipe(request, preferences);

            if (recipe is null)
            {
                _logger.LogError("An error occurred while creating recipe");
                return BadRequest();
            }

            return Ok(recipe);
        }
        catch (RecipeNotAllowedException ex)
        {
            return BadRequest(ex.ReasonMessage);
        }
    }

    [HttpPost("GetSuggestions")]
    public async Task<IActionResult> GetRecipeSuggestions([FromBody] RecipeFilterDto request)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
        var preferences = await _accountManager.GetPreferencesByUserId(userId);
        
        var recipeSuggestions = await _recipeManager.CreateRecipeSuggestions(request, preferences);
        
        if (!recipeSuggestions.Any())
        {
            _logger.LogError("An error occurred while creating recipe suggestions");
            return BadRequest("Recipe suggestions could not be generated");
        }
        
        return Ok(recipeSuggestions);
    }
    
    [HttpPost("BatchCreate")]
    [AllowAnonymous]
    public async Task<IActionResult> BatchCreateRecipes()
    {
        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            string jsonString = await reader.ReadToEndAsync();
            
            if (string.IsNullOrEmpty(jsonString))
            {
                _logger.LogError("Received empty JSON string");
                return BadRequest();
            }
            
            var recipes = await _recipeManager.CreateBatchRecipes(jsonString);
            
            return Ok(recipes);
        }
    }
}