using BL.DTOs.MealPlanning;
using BL.Managers.MealPlanning;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MealPlannerController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IMealPlannerManager _mealPlannerManager;

    public MealPlannerController(ILogger<AccountController> logger, IIdentityProviderService identityProviderService, IMealPlannerManager mealPlannerManager)
    {
        _logger = logger;
        _identityProviderService = identityProviderService;
        _mealPlannerManager = mealPlannerManager;
    }

    [HttpPost("PlannedMeal/Create")]
    public async Task<IActionResult> CreateNewPlannedMeal([FromBody] PlannedMealDto plannedMealDto)
    {
        
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers.Authorization.ToString().Substring(7));
        
        try
        {
            await _mealPlannerManager.CreateNewPlannedMeal(userId, plannedMealDto);

            return Ok();
        }
        catch (MealPlannerNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("{dateTime}")]
    public async Task<IActionResult> GetRecipeCollectionByName(DateTime dateTime)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers.Authorization.ToString().Substring(7));
        try
        {
            var plannedMeals = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);
            return Ok(plannedMeals);
        }
        catch (MealPlannerNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    [HttpGet("NextWeekIngredients")]
    public async Task<IActionResult> GetNextWeekIngredients()
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers.Authorization.ToString().Substring(7));
        try
        {
            var ingredients = await _mealPlannerManager.GetNextWeekIngredients(userId);
            return Ok(ingredients);
        }
        catch (MealPlannerNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
}