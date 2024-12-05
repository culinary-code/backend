using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class JobSettingsOptions
{
    [Required(ErrorMessage = "Minimum amount of recipes to generate is required")]
    public int MinAmount { get; set; }
    [Required(ErrorMessage = "Maximum amount of recipes to generate is required")]
    public string CronSchedule { get; set; }
}