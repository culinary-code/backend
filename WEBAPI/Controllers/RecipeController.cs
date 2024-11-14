using System.Text;
using BL.DTOs.Llm;
using BL.Managers.Recipes;
using DOM.Exceptions;
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

    public RecipeController(ILogger<RecipeController> logger, IRecipeManager recipeManager)
    {
        _logger = logger;
        _recipeManager = recipeManager;
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

    [HttpPost("Create")]
    public IActionResult CreateRecipe([FromBody] CreateRecipeDto createRecipeDto)
    {
        try
        {
            var recipe = _recipeManager.CreateRecipe(createRecipeDto.Name);

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