using System;
using System.Collections.Generic;
using DOM.Accounts;
using DOM.Exceptions;

namespace DAL.Recipes;

public interface IPreferenceRepository
{
    Task<Result<Preference>> ReadPreferenceByNameNoTracking(string name);
    Task<Result<ICollection<Preference>>> ReadStandardPreferences();
    Task<Result<Preference>> CreatePreference(Preference preference);
}