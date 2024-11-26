using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly ILogger<ConnectionController> _logger;

    public ConnectionController(ILogger<ConnectionController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Connection request received.");
        string? keycloakUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL") ?? null;
        if (keycloakUrl == null)
        {
            _logger.LogError("Keycloak URL was not found, ensure the environment variable KEYCLOAK_BASE_URL is set");
            return NotFound("Keycloak URL werd niet gevonden");
        }
        return Ok(keycloakUrl);
    }
}
