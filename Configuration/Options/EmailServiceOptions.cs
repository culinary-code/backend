using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

public class EmailServiceOptions
{    
    [Required(ErrorMessage = "SmtpClient is required")]
    public string SmtpClient { get; set; }
    [Required(ErrorMessage = "SmtpUserName is required")]
    public string SmtpUserName { get; set; }
    [Required(ErrorMessage = "SmtpPassword is required")]
    public string SmtpPassword { get; set; }
}