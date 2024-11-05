using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.Recipes;

namespace DOM.Accounts;

public class Preference
{
    [Key] public Guid PreferenceId { get; set; }
    public string PreferenceName { get; set; } = "Default Preference Name";
    public bool StandardPreference { get; set; }
    
    // navigation properties
    public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();
    public IEnumerable<Account> Accounts { get; set; } = new List<Account>();
}