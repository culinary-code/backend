using AutoMapper;
using BL.DTOs.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BL.Managers.Accounts;

public class PreferenceManager : IPreferenceManager
{
    private readonly IPreferenceRepository _preferenceRepository;
    private readonly ILogger<PreferenceManager> _logger;
    private readonly IMapper _mapper;

    public PreferenceManager(IPreferenceRepository preferenceRepository, ILogger<PreferenceManager> logger, IMapper mapper)
    {
        _preferenceRepository = preferenceRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<List<PreferenceDto>> GetStandardPreferences()
    {
        var preferences = await _preferenceRepository.ReadStandardPreferences();
        return _mapper.Map<List<PreferenceDto>>(preferences);
    }
}