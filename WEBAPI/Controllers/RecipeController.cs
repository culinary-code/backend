﻿using System.Text;
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
    public IActionResult GetRecipeById(string id)
    {
        try
        {
            var recipe = _recipeManager.GetRecipeDtoById(id);
            return Ok(recipe);
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }

    [HttpGet("ByName/{name}")]
    public IActionResult GetRecipeByName(string name)
    {
        try
        {
            var recipe = _recipeManager.GetRecipeDtoByName(name);
            return Ok(recipe);
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }

    [HttpGet("Collection/ByName/{name}")]
    public IActionResult GetRecipeCollectionByName(string name)
    {
        try
        {
            var recipes = _recipeManager.GetRecipesCollectionByName(name);
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
    public IActionResult CreateRecipe([FromBody] RecipeFilterDto request)
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
            var preferences = _accountManager.GetPreferencesByUserId(userId);
            var recipe = _recipeManager.CreateRecipe(request, preferences);

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
            
            var recipes = _recipeManager.CreateBatchRecipes(jsonString);
            
            return Ok(recipes);
        }
    }
}