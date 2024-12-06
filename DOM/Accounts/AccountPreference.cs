namespace DOM.Accounts;

public class AccountPreference
{
    public Guid AccountPreferenceId { get; set; }
    public Account Account { get; set; }
    public Preference Preference { get; set; }
    
    //foreign keys
    public Guid PreferenceId { get; set; }
    public Guid AccountId { get; set; }
}