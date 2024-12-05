using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class KeycloakOptions
{
    [Required(ErrorMessage = "Keycloak base URL is required")]
    public string BaseUrl { get; set; }
    [Required(ErrorMessage = "Keycloak frontend URL is required")]
    public string FrontendUrl { get; set; }
    [Required(ErrorMessage = "Keycloak client ID is required")]
    public string ClientId { get; set; }
    [Required(ErrorMessage = "Keycloak client secret is required")]
    public string Realm { get; set; }
    [Required(ErrorMessage = "Keycloak admin username is required")]
    public string AdminUsername { get; set; }
    [Required(ErrorMessage = "Keycloak admin password is required")]
    public string AdminPassword { get; set; }
}