namespace BL.DTOs.Accounts;

public class PreferenceDto
{
    public Guid PreferenceId { get; set; }
    
    public string PreferenceName { get; set; } = "Default Preference Name";
    
    public bool StandardPreference { get; set; }

    // Navigation properties omitted as they are not needed for the DTO
    
}