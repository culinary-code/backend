using Configuration.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WEBAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly KeycloakOptions _keycloakOptions;
    private readonly ILogger<ConnectionController> _logger;

    public ConnectionController(ILogger<ConnectionController> logger, IOptions<KeycloakOptions> keycloakOptions)
    {
        _logger = logger;
        _keycloakOptions = keycloakOptions.Value;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Connection request received.");
        string keycloakUrl = _keycloakOptions.BaseUrl;
        return Ok(new { keycloakUrl, verifier = "Culinary Code" });
    }
}