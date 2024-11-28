using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PreferenceController : ControllerBase
{
    private readonly ILogger<PreferenceController> _logger;
    private readonly IPreferenceManager _preferenceManager;

    public PreferenceController(ILogger<PreferenceController> logger, IPreferenceManager preferenceManager)
    {
        _logger = logger;
        _preferenceManager = preferenceManager;
    }

    [HttpGet("getStandardPreference")]
    public  IActionResult GetPreferences()
    {
        try
        {
            var standardPreferences = _preferenceManager.GetPreferences();
            return Ok(standardPreferences);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}