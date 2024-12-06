﻿using System;
using System.Collections.Generic;
using System.Linq;
using DAL.EF;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DAL.Recipes;

public class PreferenceRepository : IPreferenceRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public PreferenceRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    // Used to update preferences one by one, doesn't need to be tracked
    public async Task<Preference?> ReadPreferenceByNameNoTracking(string name)
    {
        Preference? preference = await _ctx.Preferences.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(p => p.PreferenceName == name);
        return preference;
    }

    // Returned as dto but also used to add preferences to recipes at creation, needs to be tracked
    public async Task<ICollection<Preference>> ReadStandardPreferences()
    {
        ICollection<Preference> preferences = await _ctx.Preferences.Where(p => p.StandardPreference).ToListAsync();
        if (preferences.Count <= 0)
        {
            throw new PreferenceNotFoundException("No standard preferences found");
        }
        return preferences;
    }

    public async Task<Preference> CreatePreference(Preference preference)
    {
        await _ctx.Preferences.AddAsync(preference);
        await _ctx.SaveChangesAsync();
        return preference;
    }
}