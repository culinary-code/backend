using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
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
    public void ReadPreferenceById_PreferenceExists_ReturnsPreference()
    {
        // Arrange
        var preference = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference",
            StandardPreference = false
        };
        _dbContext.Preferences.Add(preference);
        _dbContext.SaveChanges();
        
        // Act
        var result = _preferenceRepository.ReadPreferenceById(preference.PreferenceId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(preference.PreferenceId, result.PreferenceId);
        Assert.Equal(preference.PreferenceName, result.PreferenceName);
    }
    
    [Fact]
    public void ReadPreferenceById_PreferenceDoesNotExist_ThrowsPreferenceNotFoundException()
    {
        // Arrange
        var preferenceId = Guid.NewGuid();
        
        // Act & Assert
        Assert.Throws<PreferenceNotFoundException>(() => _preferenceRepository.ReadPreferenceById(preferenceId));
    }
    
    [Fact]
    public void ReadPreferenceByName_PreferenceExists_ReturnsPreference()
    {
        // Arrange
        var preference = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference",
            StandardPreference = false
        };
        _dbContext.Preferences.Add(preference);
        _dbContext.SaveChanges();
        
        // Act
        var result = _preferenceRepository.ReadPreferenceByName(preference.PreferenceName);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(preference.PreferenceId, result.PreferenceId);
        Assert.Equal(preference.PreferenceName, result.PreferenceName);
    }
    
    [Fact]
    public void ReadPreferenceByName_DoesNotExist_ThrowsPreferenceNotFoundException()
    {
        // Arrange
        var preferenceName = "Test Preference";
        
        // Act & Assert
        Assert.Throws<PreferenceNotFoundException>(() => _preferenceRepository.ReadPreferenceByName(preferenceName));
    }
    
    [Fact]
    public void ReadStandardPreferences_PreferencesExist_ReturnsCollectionOfPreferences()
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
        _dbContext.SaveChanges();
        
        // Act
        var result = _preferenceRepository.ReadStandardPreferences();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(preference1, result);
        Assert.Contains(preference2, result);
    }
    
    [Fact]
    public void ReadStandardPreferences_NoStandardPreferencesExist_ThrowsPreferenceNotFoundException()
    {
        // Act & Assert
        Assert.Throws<PreferenceNotFoundException>(() => _preferenceRepository.ReadStandardPreferences());
    }
    
    [Fact]
    public void CreatePreference_PreferenceDoesNotExist_AddsPreference()
    {
        // Arrange
        var preference = new Preference
        {
            PreferenceId = Guid.NewGuid(),
            PreferenceName = "Test Preference",
            StandardPreference = false
        };
        
        // Act
        _preferenceRepository.CreatePreference(preference);
        
        // Assert
        Assert.Contains(preference, _dbContext.Preferences);
    }
}