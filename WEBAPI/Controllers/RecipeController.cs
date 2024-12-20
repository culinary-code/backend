﻿using System.Text;
using AutoMapper;
using BL.DTOs.Llm;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using DOM.Recipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

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

    public RecipeController(ILogger<RecipeController> logger, IRecipeManager recipeManager,
        IIdentityProviderService identityProviderService, IAccountManager accountManager)
    {
        _logger = logger;
        _recipeManager = recipeManager;
        _identityProviderService = identityProviderService;
        _accountManager = accountManager;
    }

    // Returns a recipe by the given recipeId
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipeById(string id)
    {
        var recipeResult = await _recipeManager.GetRecipeDtoById(id);
        return recipeResult.ToActionResult();
    }

    // Returns the first recipe containing the given name
    [HttpGet("ByName/{name}")]
    public async Task<IActionResult> GetRecipeByName(string name)
    {
        var recipeResult = await _recipeManager.GetRecipeDtoByName(name);
        return recipeResult.ToActionResult();
    }

    // Returns all recipes containing a given name
    [HttpGet("Collection/ByName/{name}")]
    public async Task<IActionResult> GetRecipeCollectionByName(string name)
    {
        var recipesResult = await _recipeManager.GetRecipesCollectionByName(name);
        return recipesResult.ToActionResult();
    }

    // Returns all recipes containing the given filter criteria
    [HttpPost("Collection/Filtered")]
    public async Task<IActionResult> GetFilteredRecipeCollection([FromBody] RecipeFilterDto filter)
    {
        Enum.TryParse<RecipeType>(filter.MealType, out var mealTypeEnum);
        Enum.TryParse<Difficulty>(filter.Difficulty, out var difficultyEnum);
        var recipesResult = await _recipeManager.GetFilteredRecipeCollection(filter.RecipeName, difficultyEnum,
            mealTypeEnum, filter.CookTime, filter.Ingredients);
        return recipesResult.ToActionResult();
    }

    // Creates a new recipe with the given filter criteria. This will send a request to the openAI services
    [HttpPost("Create")]
    public async Task<IActionResult> CreateRecipe([FromBody] RecipeFilterDto request)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }

        var userId = userIdResult.Value;

        var preferencesResult = await _accountManager.GetPreferencesByUserId(userId);
        if (!preferencesResult.IsSuccess) return preferencesResult.ToActionResult();
        var preferences = preferencesResult.Value!;
        var recipeResult = await _recipeManager.CreateRecipe(request, preferences);

        return recipeResult.ToActionResult();
    }

    // Returns a list of recipe suggestions based on the given filter criteria
    [HttpPost("GetSuggestions")]
    public async Task<IActionResult> GetRecipeSuggestions([FromBody] RecipeFilterDto request)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }
        var userId = userIdResult.Value;

        var preferencesResult = await _accountManager.GetPreferencesByUserId(userId);
        if (!preferencesResult.IsSuccess)
        {
            return preferencesResult.ToActionResult();
        }
        var preferences = preferencesResult.Value!;
        
        var recipeSuggestionsResult = await _recipeManager.CreateRecipeSuggestions(request, preferences);
        
        return recipeSuggestionsResult.ToActionResult();
    }
    
    // Creates recipes in the database based on the requestbody in json format. Used for development purposes and filling the database with a couple default recipes.
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

            var recipesResult = await _recipeManager.CreateBatchRecipes(jsonString);

            return recipesResult.ToActionResult();
        }
    }
}