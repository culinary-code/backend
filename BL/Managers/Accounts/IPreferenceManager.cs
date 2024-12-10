using BL.DTOs.Accounts;
using DOM.Results;

namespace BL.Managers.Accounts;

public interface IPreferenceManager
{
    Task<Result<List<PreferenceDto>>> GetStandardPreferences();
}