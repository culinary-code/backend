using System;
using System.Collections.Generic;
using System.Linq;
using DAL.EF;
using DOM.Accounts;
using DOM.Results;
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
    public async Task<Result<Preference>> ReadPreferenceByNameNoTracking(string name)
    {
        var preference = await _ctx.Preferences.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(p => p.PreferenceName == name);

        if (preference == null)
        {
            return Result<Preference>.Failure($"No preference found with name: {name}", ResultFailureType.NotFound);
        }
        
        return Result<Preference>.Success(preference);
    }

    // Returned as dto but also used to add preferences to recipes at creation, needs to be tracked
    public async Task<Result<ICollection<Preference>>> ReadStandardPreferences()
    {
        ICollection<Preference> preferences = await _ctx.Preferences.Where(p => p.StandardPreference).ToListAsync();
        if (preferences.Count <= 0)
        {
            return Result<ICollection<Preference>>.Failure($"No standard preferences found", ResultFailureType.NotFound);
        }
        
        return Result<ICollection<Preference>>.Success(preferences);
    }

    public async Task<Result<Preference>> CreatePreference(Preference preference)
    {
        await _ctx.Preferences.AddAsync(preference);
        await _ctx.SaveChangesAsync();
        return Result<Preference>.Success(preference);
    }
}