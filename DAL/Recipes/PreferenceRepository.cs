using DAL.EF;
using DOM.Accounts;
using DOM.Recipes;

namespace DAL.Recipes;

public class PreferenceRepository : IPreferenceRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public PreferenceRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }


    public Preference ReadPreferenceById(Guid id)
    {
        Preference? preference = _ctx.Preferences.Find(id);
        if (preference is null)
        {
            throw new Exception($"No preference found with id {id}");
        }
        return preference;
    }

    public Preference ReadPreferenceByName(string name)
    {
        Preference? preference = _ctx.Preferences.FirstOrDefault(p => p.PreferenceName == name);
        if (preference is null)
        {
            throw new Exception($"No preference found with name {name}");
        }
        return preference;
    }

    public ICollection<Preference> ReadStandardPreferences()
    {
        ICollection<Preference> preferences = _ctx.Preferences.Where(p => p.StandardPreference).ToList();
        if (preferences.Count <= 0)
        {
            throw new Exception("No standard preferences found");
        }
        return preferences;
    }

    public void CreatePreference(Preference preference)
    {
        _ctx.Preferences.Add(preference);
        _ctx.SaveChanges();
    }
}