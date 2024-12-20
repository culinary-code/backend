using BL.Managers.Accounts;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.ResultExtension;

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

    // Returns all preferences that have been marked as standard preferences
    [HttpGet("getStandardPreference")]
    public async Task<IActionResult> GetStandardPreference()
    {
        var standardPreferencesResult = await _preferenceManager.GetStandardPreferences();
        return standardPreferencesResult.ToActionResult();
    }
}