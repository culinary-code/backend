using AutoMapper;
using BL.DTOs.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Results;
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

    public async Task<Result<List<PreferenceDto>>> GetStandardPreferences()
    {
        var preferencesResult = await _preferenceRepository.ReadStandardPreferences();
        if (!preferencesResult.IsSuccess)
        {
            return Result<List<PreferenceDto>>.Failure(preferencesResult.ErrorMessage!, preferencesResult.FailureType);
        }
        var preferences = preferencesResult.Value;
        return Result<List<PreferenceDto>>.Success(_mapper.Map<List<PreferenceDto>>(preferences));
    }
}