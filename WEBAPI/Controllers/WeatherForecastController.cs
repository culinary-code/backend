using BL.ExternalSources.ChatGPT;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    
    private readonly AzureOpenAIService _azureOpenAIService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, AzureOpenAIService azureOpenAiService)
    {
        _logger = logger;
        _azureOpenAIService = azureOpenAiService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet("getchat")]
    public string GetChat()
    {
        var message = _azureOpenAIService.GetChatMessage("Geef mij een recept voor stoofvlees en frietjes.");
        return message;
    }
}