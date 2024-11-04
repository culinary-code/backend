using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Recipes;

namespace DOM.Accounts;

public class Preference
{
    [Key] public int PreferenceId { get; set; }
    public string PreferenceName { get; set; } = "Default Preference Name";
    public bool StandardPreference { get; set; }
    
    // navigation properties
    private IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();
    private IEnumerable<Account> Accounts { get; set; } = new List<Account>();
}