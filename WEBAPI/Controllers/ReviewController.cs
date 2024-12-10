using System.Security.Claims;
using BL.DTOs.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewManager _reviewManager;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewManager reviewManager, ILogger<ReviewController> logger,
        IIdentityProviderService identityProviderService)
    {
        _reviewManager = reviewManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReviewById(string id)
    {
        var reviewResult = await _reviewManager.GetReviewDtoById(Guid.Parse(id));
        return reviewResult.ToActionResult();
    }

    [HttpGet("ByRecipeId/{recipeId}")]
    public async Task<IActionResult> GetReviewsByRecipeId(string recipeId)
    {
        var reviewsResult = await _reviewManager.GetReviewDtosByRecipeId(Guid.Parse(recipeId));
        return reviewsResult.ToActionResult();
    }

    [HttpPost("CreateReview")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return userIdResult.ToActionResult();
        }

        var userId = userIdResult.Value;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var reviewResult = await _reviewManager.CreateReview(userId, createReviewDto.RecipeId,
            createReviewDto.Description, createReviewDto.AmountOfStars);
        return reviewResult.ToActionResult();
    }
}