
using BL.DTOs.Accounts;
using DOM.Accounts;

public interface IPreferenceManager
{
    Task<List<PreferenceDto>> GetStandardPreferences();
}