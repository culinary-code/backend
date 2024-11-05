using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.Recipes;

namespace DOM.Accounts;

public class Preference
{
    [Key] public int PreferenceId { get; set; }
    public string PreferenceName { get; set; } = "Default Preference Name";
    public bool StandardPreference { get; set; }
    
    // navigation properties
    [JsonIgnore]
    public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();
    [JsonIgnore]
    public IEnumerable<Account> Accounts { get; set; } = new List<Account>();
}