using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class AzureOpenAIOptions
{
    [Required(ErrorMessage = "Azure OpenAI API key is required")]
    public string ApiKey { get; set; }
    [Required(ErrorMessage = "Azure OpenAI endpoint is required")]
    public string Endpoint { get; set; }
}