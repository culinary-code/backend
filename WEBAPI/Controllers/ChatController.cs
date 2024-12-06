using BL.DTOs.Accounts;
using BL.DTOs.Llm;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Accounts;
using BL.Managers.Recipes;
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
        var prompt = LlmSettingsService.BuildPrompt(request, null);
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
    
    [HttpGet("getSuggestions")]
    public IActionResult GetSuggestions()
    {
        var suggestions = _llmService.GenerateMultipleRecipeNamesAndDescriptions("Ik wil graag een recept voor een lekker gerecht", 5);
        return Ok(suggestions);
    }
}