﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Recipes;

namespace DOM.Accounts;

public class Preference
{
    public Guid PreferenceId { get; set; }
    public string PreferenceName { get; set; } = "Default Preference Name";
    public bool StandardPreference { get; set; }
    
    // navigation properties
    public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();
    public IEnumerable<Account> Accounts { get; set; } = new List<Account>();
}