using BL.DTOs.Accounts;
using DOM.Exceptions;

namespace BL.Managers.Accounts;

public interface IPreferenceManager
{
    Task<Result<List<PreferenceDto>>> GetStandardPreferences();
}