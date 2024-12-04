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
    public async Task<IActionResult> GetStandardPreference()
    {
        try
        {
            var standardPreferences = await _preferenceManager.GetStandardPreferences();
            return Ok(standardPreferences);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}