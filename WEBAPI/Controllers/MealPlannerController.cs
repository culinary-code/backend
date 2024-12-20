using BL.DTOs.MealPlanning;
using BL.Managers.MealPlanning;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MealPlannerController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IMealPlannerManager _mealPlannerManager;

    public MealPlannerController(ILogger<AccountController> logger, IIdentityProviderService identityProviderService,
        IMealPlannerManager mealPlannerManager)
    {
        _logger = logger;
        _identityProviderService = identityProviderService;
        _mealPlannerManager = mealPlannerManager;
    }

    // Creates a new planned meal for a given date or overwrites an existing planned meal for that date
    [HttpPost("PlannedMeal/Create")]
    public async Task<IActionResult> CreateNewPlannedMeal([FromBody] PlannedMealDto plannedMealDto)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }

        var userId = userIdResult.Value;

        var createPlannedMealResult = await _mealPlannerManager.CreateNewPlannedMeal(userId, plannedMealDto);

        return createPlannedMealResult.ToActionResult();
    }

    // Returns a collection of planned meals based on a given datetime
    [HttpGet("{dateTime}")]
    public async Task<IActionResult> GetRecipeCollectionByName(DateTime dateTime)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }

        var userId = userIdResult.Value;

        var plannedMealsResult = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);
        return plannedMealsResult.ToActionResult();
    }

    // Returns the ingredients of the next week of planned meals
    [HttpGet("NextWeekIngredients")]
    public async Task<IActionResult> GetNextWeekIngredients()
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }

        var userId = userIdResult.Value;

        var ingredientsResult = await _mealPlannerManager.GetNextWeekIngredients(userId);
        return ingredientsResult.ToActionResult();
    }
}