using System;
using System.Collections.Generic;
using System.Linq;
using BL.ExternalSources.Llm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WEBAPI.Controllers.Dto;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    
    private readonly AzureOpenAIService _azureOpenAIService;
    private readonly LocalLlmService _localLlmService;

    public ChatController(ILogger<ChatController> logger, AzureOpenAIService azureOpenAiService, LocalLlmService localLlmService)
    {
        _logger = logger;
        _azureOpenAIService = azureOpenAiService;
        _localLlmService = localLlmService;
    }

    [HttpPost("getchat")]
    public string GetChat([FromBody] ChatRequestDto request)
    {
        var message = _azureOpenAIService.GetChatMessage(request.Prompt);
        return message;
    }
    
    [HttpPost("getimage")]
    public string GetImage([FromBody] ChatRequestDto request)
    {
        var imageUri = _azureOpenAIService.GetImage(request.Prompt);
        return imageUri.ToString();
    }
    
    [HttpPost("getlocalchat")]
    public string GetLocalChat([FromBody] ChatRequestDto request)
    {
        var message = _localLlmService.GetChatMessage(request.Prompt);
        return message;
    }
}