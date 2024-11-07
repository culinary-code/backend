﻿using DOM.Accounts;

namespace DAL.Recipes;

public interface IPreferenceRepository
{
    public Preference ReadPreferenceById(Guid id);
    public Preference ReadPreferenceByName(string name);
    public ICollection<Preference> ReadStandardPreferences();
    public void CreatePreference(Preference preference);
}