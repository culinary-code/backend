using BL.DTOs.MealPlanning;
using BL.Managers.MealPlanning;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;


[Route("api/[controller]")]
[ApiController]
// [Authorize]
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
        // Guid userId = Guid.Parse("d1ec841b-9646-4ca7-a1ef-eda7354547f3");
        
        try
        {
            await _mealPlannerManager.CreateNewPlannedMeal(userId, plannedMealDto);

            return Ok();
        }
        catch (Exception e) // TODO: change exception type
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("PlannedMeal/{dateTime}")]
    public async Task<IActionResult> GetRecipeCollectionByName(DateTime dateTime)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers.Authorization.ToString().Substring(7));
        try
        {
            var plannedMeals = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);
            return Ok(plannedMeals);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
}