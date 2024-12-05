using BL.DTOs.Llm;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Accounts;
using BL.Services;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Obsolete]
// Class used to test out Azure functionality
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    
    private readonly ILlmService _llmService;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IAccountManager _accountManager;

    public ChatController(ILogger<ChatController> logger, ILlmService llmService, IIdentityProviderService identityProviderService, IAccountManager accountManager)
    {
        _logger = logger;
        _llmService = llmService;
        _identityProviderService = identityProviderService;
        _accountManager = accountManager;
    }

    [HttpPost("getchat")]
    public async Task<IActionResult> GetChat([FromBody] RecipeFilterDto request)
    {
        string token = Request.Headers["Authorization"].ToString().Substring(7);
        Guid userId = _identityProviderService.GetGuidFromAccessToken(token);
        
        var preferences = await _accountManager.GetPreferencesByUserId(userId);
        var prompt = LlmSettingsService.BuildPrompt(request, preferences);
        var message = _llmService.GenerateRecipe(prompt);
        return Ok(message);
    }
    
    [HttpPost("getmultiplechat")]
    public IActionResult GetMultipleChat([FromBody] ChatRequestDto request)
    {
        var message = _llmService.GenerateMultipleRecipes(request.Prompt, request.Amount);
        return Ok(message);
    }
    
    [HttpPost("getimage")]
    public IActionResult GetImage([FromBody] ChatRequestDto request)
    {
        var imageUri = _llmService.GenerateRecipeImage(request.Prompt);
        return Ok(imageUri?.ToString() ?? "Something went wrong");
    }
}