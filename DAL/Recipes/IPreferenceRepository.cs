using System;
using System.Collections.Generic;
using DOM.Accounts;

namespace DAL.Recipes;

public interface IPreferenceRepository
{
    Task<Preference> ReadPreferenceById(Guid id);
    Task<Preference?> ReadPreferenceByName(string name);
    Task<ICollection<Preference>> ReadStandardPreferences();
    Task<Preference> CreatePreference(Preference preference);
}