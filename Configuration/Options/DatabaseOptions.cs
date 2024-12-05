using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class DatabaseOptions
{
    [Required(ErrorMessage = "Database connection string is required")]
    public string ConnectionString { get; set; }
}