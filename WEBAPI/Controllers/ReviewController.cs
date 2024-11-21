using System.Security.Claims;
using BL.DTOs.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize] 
public class ReviewController : ControllerBase
{
    private readonly IReviewManager _reviewManager;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewManager reviewManager, ILogger<ReviewController> logger, IIdentityProviderService identityProviderService)
    {
        _reviewManager = reviewManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReviewById(string id)
    {
        try
        {
            var review = await _reviewManager.GetReviewDtoById(Guid.Parse(id));
            return Ok(review);
        }
        catch (ReviewNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    [HttpGet("ByRecipeId/{recipeId}")]
    public async Task<IActionResult> GetReviewsByRecipeId(string recipeId)
    {
        try
        {
            var reviews = await _reviewManager.GetReviewDtosByRecipeId(Guid.Parse(recipeId));
            return Ok(reviews);
        }
        catch (ReviewNotFoundException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return NotFound(e.Message);
        }
    }
    
    [HttpPost("CreateReview")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var review = await _reviewManager.CreateReview(userId, createReviewDto.RecipeId, createReviewDto.Description, createReviewDto.AmountOfStars);
            return Ok(review);
        }
        catch (ReviewAlreadyExistsException e)
        {
            _logger.LogError("An error occurred: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
}