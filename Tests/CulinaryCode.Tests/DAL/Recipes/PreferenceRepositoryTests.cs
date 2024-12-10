using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Recipes;

public class PreferenceRepositoryTests
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly IPreferenceRepository _preferenceRepository;
    
    public PreferenceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            // force unique database for each test so data is isolated
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;
        
        _dbContext = new CulinaryCodeDbContext(options);
        _preferenceRepository = new PreferenceRepository(_dbContext);
    }
    
    [Fact]
    public async Task ReadPreferenceByName_PreferenceExists_ReturnsPreference()
    {
        // Arrange
        var preference = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference",
            StandardPreference = false
        };
        _dbContext.Preferences.Add(preference);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _preferenceRepository.ReadPreferenceByNameNoTracking(preference.PreferenceName);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(preference.PreferenceId, result.Value!.PreferenceId);
        Assert.Equal(preference.PreferenceName, result.Value.PreferenceName);
    }
    
    [Fact]
    public async Task ReadStandardPreferences_PreferencesExist_ReturnsCollectionOfPreferences()
    {
        // Arrange
        var preference1 = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference 1",
            StandardPreference = true
        };
        var preference2 = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference 2",
            StandardPreference = true
        };
        _dbContext.Preferences.Add(preference1);
        _dbContext.Preferences.Add(preference2);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _preferenceRepository.ReadStandardPreferences();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(preference1, result.Value!);
        Assert.Contains(preference2, result.Value!);
    }
    
    [Fact]
    public async Task ReadStandardPreferences_NoStandardPreferencesExist_ThrowsPreferenceNotFoundException()
    {
        // Act
        var result = await _preferenceRepository.ReadStandardPreferences();
        
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task CreatePreference_PreferenceDoesNotExist_AddsPreference()
    {
        // Arrange
        var preference = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference",
            StandardPreference = false
        };
        
        // Act
        var result = await _preferenceRepository.CreatePreference(preference);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(preference, _dbContext.Preferences);
    }
}