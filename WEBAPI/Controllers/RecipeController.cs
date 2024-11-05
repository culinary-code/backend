using System;
using System.Collections.Generic;
using System.Linq;
using BL.Recipes;
using DOM.Exceptions;
using DOM.Recipes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Controllers;

[ApiController]
[Route("[controller]")]
public class RecipeController : ControllerBase
{
    private readonly ILogger<RecipeController> _logger;
    private readonly IRecipeManager _recipeManager;

    public RecipeController(ILogger<RecipeController> logger, IRecipeManager recipeManager)
    {
        _logger = logger;
        _recipeManager = recipeManager;
    }
    
    [HttpGet( "/{id}")]
    public IActionResult GetRecipeById(int id)
    {
        try
        {
            var recipe = _recipeManager.GetRecipeById(id);
            return Ok(recipe);
        }
        catch (RecipeNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpGet("ByName/{name}")]
    public IActionResult GetRecipeByName(string name)
    {
        try
        {
            var recipe = _recipeManager.GetRecipeByName(name);
            return Ok(recipe);
        }
        catch (RecipeNotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }
    
    [HttpGet("/Collection/ByName/{name}")]
    public IActionResult GetRecipeCollectionByName(string name)
    {
        try
        {
            var recipes = _recipeManager.GetRecipesCollectionByName(name);
            return Ok(recipes);
        }
        catch (RecipeNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}