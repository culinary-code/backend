using BL.DTOs.Accounts;

namespace CulinaryCode.Tests.Util;

public class PreferenceListDtoUtil
{
    public static List<PreferenceDto> CreatePreferenceListDto ()
    {
        var pref1 = new PreferenceDto
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Preference1",
            StandardPreference = false
        };
        
        var pref2 = new PreferenceDto
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Preference2",
            StandardPreference = true
        };
        
        List<PreferenceDto> preferences = [pref1, pref2];
        return preferences;
    }

    public static List<PreferenceDto>? CreateEmptyPreferenceListDto()
    {
        return null;
    }
}