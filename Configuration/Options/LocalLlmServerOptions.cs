using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class LocalLlmServerOptions
{
    [Required(ErrorMessage = "Local LLM server URL is required")]
    public string ServerUrl { get; set; }
}