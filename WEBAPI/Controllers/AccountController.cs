using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountManager _accountManager;
    private readonly IIdentityProviderService _identityProviderService;

    public AccountController(IAccountManager accountManager, ILogger<AccountController> logger,
        IIdentityProviderService identityProviderService)
    {
        _accountManager = accountManager;
        _logger = logger;
        _identityProviderService = identityProviderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserById()
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;
        var user = await _accountManager.GetAccountById(userId);

        return user.ToActionResult();
    }

    [HttpPut("updateAccount")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountDto accountDto, [FromQuery] string actionType)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
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
                    var updatedUsernameResult = await _accountManager.UpdateAccount(accountDto);
                    if (!updatedUsernameResult.IsSuccess)
                    {
                        return BadRequest(updatedUsernameResult.ErrorMessage);
                    }

                    var updatedUsername = updatedUsernameResult.Value!;
                    var result = await _identityProviderService.UpdateUsername(updatedUsername, accountDto.Name);
                    if (result.IsSuccess)
                    {
                        return updatedUsernameResult.ToActionResult();
                    }
                    return BadRequest(result.ErrorMessage);

                case "updatefamilysize":
                    var updatedFamilySize = await _accountManager.UpdateFamilySize(accountDto);
                    return updatedFamilySize.ToActionResult();

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
    
    [HttpDelete("deleteAccount")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var deleteResult = await _identityProviderService.DeleteUser(userId);
        return deleteResult.ToActionResult();
    }

    [HttpGet("getPreferences")]
    public async Task<IActionResult> GetUserPreferences()
    {
        try
        {
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
            if (!userIdResult.IsSuccess)
            {
                return BadRequest(userIdResult.ErrorMessage);
            }

            var userId = userIdResult.Value;

            var preferences = await _accountManager.GetPreferencesByUserId(userId);
            return preferences.ToActionResult();
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
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        try
        {
            var updatedAccount = await _accountManager.AddPreferenceToAccount(userId, preferenceDto);
            return updatedAccount.ToActionResult();
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
            var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
            if (!userIdResult.IsSuccess)
            {
                return BadRequest(userIdResult.ErrorMessage);
            }

            var userId = userIdResult.Value;

            var removePreferenceResult = await _accountManager.RemovePreferenceFromAccount(userId, preferenceId);
            return removePreferenceResult.ToActionResult();
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
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var favoriteRecipes = await _accountManager.GetFavoriteRecipesByUserId(userId);
        return favoriteRecipes.ToActionResult();
    }

    [HttpPost("addFavoriteRecipe")]
    public async Task<IActionResult> AddFavoriteRecipeToUser([FromBody] RecipeDto recipeDto)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var updatedAccount = await _accountManager.AddFavoriteRecipeToAccount(userId, recipeDto.RecipeId);
        return updatedAccount.ToActionResult();
    }

    [HttpDelete("deleteFavoriteRecipe/{recipeId}")]
    public async Task<IActionResult> DeleteFavoriteRecipe(Guid recipeId)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        var userIdResult = _identityProviderService.GetGuidFromAccessToken(token);
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;

        var removeFavoriteRecipeResult = await _accountManager.RemoveFavoriteRecipeFromAccount(userId, recipeId);

        return removeFavoriteRecipeResult.ToActionResult();
    }
    
    [HttpPut("setChosenGroup")]
    public async Task<IActionResult> SetChosenGroup([FromBody] AccountDto accountDto)
    {
        var userIdResult =
            _identityProviderService.GetGuidFromAccessToken(Request.Headers["Authorization"].ToString().Substring(7));
        if (!userIdResult.IsSuccess)
        {
            return BadRequest(userIdResult.ErrorMessage);
        }

        var userId = userIdResult.Value;
        
        // Check if ChosenGroupId is null (for user mode)
        Guid? chosenGroupId = accountDto.ChosenGroupId;

        if (chosenGroupId == null)
        {
            // Handle logic when ChosenGroupId is null (back to user mode)
            accountDto.ChosenGroupId = null;  // Explicitly set to null if needed
        }
        
        var account = await _accountManager.UpdateChosenGroup(userId, accountDto.ChosenGroupId);
        
        return account.ToActionResult();
    }
}