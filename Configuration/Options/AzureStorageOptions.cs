using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class AzureStorageOptions
{
    [Required(ErrorMessage = "Azure Storage connection string is required")]
    public string ConnectionString { get; set; }
    [Required(ErrorMessage = "Azure Storage container name is required")]
    public string ContainerName { get; set; }
}