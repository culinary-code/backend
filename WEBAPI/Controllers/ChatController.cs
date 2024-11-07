using BL.ExternalSources.Llm;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Controllers.Dto;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    
    private readonly ILlmService _llmService;
    //private readonly LocalLlmService _localLlmService;

    public ChatController(ILogger<ChatController> logger, ILlmService llmService/*, LocalLlmService localLlmService*/)
    {
        _logger = logger;
        _llmService = llmService;
        //_localLlmService = localLlmService;
    }

    [HttpPost("getchat")]
    public string GetChat([FromBody] ChatRequestDto request)
    {
        var message = _llmService.GenerateRecipe(request.Prompt);
        return message;
    }
    
    [HttpPost("getimage")]
    public string GetImage([FromBody] ChatRequestDto request)
    {
        var imageUri = _llmService.GenerateRecipeImage(request.Prompt);
        return imageUri?.ToString() ?? "Something went wrong";
    }
    
    /*[HttpPost("getlocalchat")]
    public string GetLocalChat([FromBody] ChatRequestDto request)
    {
        var message = _localLlmService.GenerateRecipe(request.Prompt);
        return message;
    }*/
}