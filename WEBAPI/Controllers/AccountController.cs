using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController: ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountManager _accountManager;
    private readonly IIdentityProviderService _identityProviderService;

    public AccountController(IAccountManager accountManager, ILogger<AccountController> logger, IIdentityProviderService identityProviderService)
    {
        _accountManager = accountManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
    }
    
    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetUserById(string accountId)
    {
        var user = await _accountManager.GetAccountById(accountId); 
            
        return user.ToActionResult();
    }

    [HttpPut("updateAccount")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountDto accountDto, [FromQuery] string actionType)
    {
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }
        var userId = userIdResult.Value;
        accountDto.AccountId = userId;
        try
        {
            if (string.IsNullOrEmpty(actionType))
            {
                return BadRequest("Action type is required.");
            }

            switch (actionType.ToLowerInvariant())
            {
                case "updateusername":
                    var updatedUsername = await _accountManager.UpdateAccount(accountDto);
                    await _identityProviderService.UpdateUsername(updatedUsername, accountDto.Name);
                    return Ok(updatedUsername);
                
                case "updatefamilysize":
                    var updatedFamilySize = await _accountManager.UpdateFamilySize(accountDto);
                    return Ok(updatedFamilySize);
                
                default:
                    return BadRequest("Invalid action type.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while updating account {AccountId}: {ErrorMessage}", userId, e.Message);
            return BadRequest("Failed to update account.");
        }
    }
    
    [HttpGet("getPreferences")]
    public async Task<IActionResult> GetUserPreferences()
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
            var preferences = await _accountManager.GetPreferencesByUserId(userId);
            return Ok(preferences);
        }
        catch (AccountNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {ErrorMessage}", ex.Message);
            return NotFound("Account not found.");
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred trying to fetch user preferences: {ErrorMessage}", e.Message);
            return BadRequest("Failed to get user preferences.");
        }
    }
    
    
    [HttpPost("addPreference")]
    public async Task<IActionResult> AddPreference([FromBody] PreferenceDto preferenceDto)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));

        try
        {
            var updatedAccount = await _accountManager.AddPreferenceToAccount(userId, preferenceDto);
            return Ok(updatedAccount);
        }
        catch (AccountNotFoundException e)
        {
            _logger.LogError("An error occurred while adding preference to account {AccountId}: {ErrorMessage}", userId, e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while adding preference: {ErrorMessage}", e.Message);
            return BadRequest("Failed to add preference.");
        }
    }
    
    [HttpDelete("deletePreference/{preferenceId}")]
    public async Task<IActionResult> DeletePreference(Guid preferenceId)
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7); 
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
            
            await _accountManager.RemovePreferenceFromAccount(userId, preferenceId);

            return Ok("Preference deleted successfully.");
        }
        catch (AccountNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {ErrorMessage}", ex.Message);
            return NotFound("Account not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while deleting preference: {ErrorMessage}", ex.Message);
            return BadRequest("Failed to delete preference.");
        }
    }

    [HttpGet("getFavoriteRecipes")]
    public async Task<IActionResult> GetFavoriteRecipes()
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
            var favoriteRecipes = await _accountManager.GetFavoriteRecipesByUserId(userId);
            return Ok(favoriteRecipes);
        }
        catch (AccountNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {ErrorMessage}", ex.Message);
            return NotFound("Account not found.");
        }
        catch (RecipeNotFoundException e)
        {
            _logger.LogError("An error occurred trying to fetch favorite recipes: {ErrorMessage}", e.Message);
            return BadRequest("Failed to get favorite recipes.");
        }
    }

    [HttpPost("addFavoriteRecipe")]
    public async Task<IActionResult> AddFavoriteRecipeToUser([FromBody] RecipeDto recipeDto)
    {
        Guid userId = _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));

        try
        {
            var updatedAccount = await _accountManager.AddFavoriteRecipeToAccount(userId, recipeDto.RecipeId);
            return Ok(updatedAccount);
        }
        catch (AccountNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {ErrorMessage}", ex.Message);
            return NotFound("Account not found.");
        }
        catch (RecipeNotFoundException ex)
        {
            _logger.LogError("Error occurred while adding favorite recipe: {ErrorMessage}", ex.Message);
            return BadRequest("Failed to add favorite recipe.");
        }
    }
    
    [HttpDelete("deleteFavoriteRecipe/{recipeId}")]
    public async Task<IActionResult> DeleteFavoriteRecipe(Guid recipeId)
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7); 
            Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
            
            await _accountManager.RemoveFavoriteRecipeFromAccount(userId, recipeId);
            
            return Ok("Favorite recipe deleted successfully.");
        }
        catch (AccountNotFoundException ex)
        {
            _logger.LogWarning("Account not found: {ErrorMessage}", ex.Message);
            return NotFound("Account not found.");
        }
        catch (RecipeNotFoundException ex)
        {
            _logger.LogError("Error occurred while deleting favorite recipe: {ErrorMessage}", ex.Message);
            return BadRequest("Failed to delete favorite recipe.");
        }
    }
}