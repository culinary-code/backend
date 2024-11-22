using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace CulinaryCode.Tests.BL.Managers;

public class AccountManagerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IAccountRepository> _mockRepository;
    private readonly Mock<ILogger<AccountManager>> _loggerMock;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AccountManager _accountManager;
    
    public AccountManagerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockRepository = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<AccountManager>>();
        _mockMapper = new Mock<IMapper>();
        _accountManager = new AccountManager(_mockRepository.Object, _loggerMock.Object, _mockMapper.Object);
    }
    
    [Fact]
    public void GetAccountById_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accountIdString = accountId.ToString();
        var expectedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns(new Account());
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(expectedAccount);

        // Act
        var result = _accountManager.GetAccountById(accountIdString);

        // Assert
        Assert.Equal(expectedAccount, result);
    }
    
    [Fact]
    public void GetAccountById_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid().ToString();
        _mockRepository.Setup(manager => manager.ReadAccount(It.IsAny<Guid>())).Returns((Account)null);

        // Act
        var result = _accountManager.GetAccountById(accountId);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void UpdateAccount_ReturnsUpdatedAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accountIdString = accountId.ToString();
        var updatedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns(new Account());
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);

        // Act
        var result = _accountManager.UpdateAccount(updatedAccount);

        // Assert
        Assert.Equal(updatedAccount, result);
    }
    
    [Fact]
    public void UpdateAccount_ReturnsUpdatedAccountWithFamilySize_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updatedAccount = new AccountDto { AccountId = accountId, FamilySize = 14 };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns(new Account());
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);

        // Act
        var result = _accountManager.UpdateAccount(updatedAccount);

        // Assert
        Assert.Equal(updatedAccount, result);
    }
    
    [Fact]
    public void UpdateAccount_ThrowsAccountNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updatedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns((Account)null);

        // Act & Assert
        Assert.Throws<AccountNotFoundException>(() => _accountManager.UpdateAccount(updatedAccount));
    }

    [Fact]
    public void CreateAccount_CreatesAccount_WhenAccountDoesNotExist()
    {
        // Arrange
        var username = "JohnDoe";
        var email = "johndoe@example.org";
        var userId = Guid.NewGuid();
        _mockRepository.Setup(manager => manager.ReadAccount(userId)).Returns((Account)null);

        // Act
        _accountManager.CreateAccount(username, email, userId);

        // Assert
        _mockRepository.Verify(manager => manager.CreateAccount(It.IsAny<Account>()), Times.Once);
    }
    
     [Fact]
    public void GetPreferencesByUserId_ReturnsMappedPreferences_WhenPreferencesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();
        var preferenceId2 = Guid.NewGuid();

        var preferences = new List<Preference>
        {
            new Preference { PreferenceId = preferenceId, PreferenceName = "Preference1" },
            new Preference { PreferenceId = preferenceId2, PreferenceName = "Preference2" }
        };

        var expectedPreferences = new List<PreferenceDto>
        {
            new PreferenceDto { PreferenceId = preferenceId, PreferenceName = "Preference1" },
            new PreferenceDto { PreferenceId = preferenceId2, PreferenceName = "Preference2" }
        };

        _mockRepository.Setup(manager => manager.ReadAccount(userId)).Returns(new Account { Preferences = preferences });
        _mockMapper.Setup(mapper => mapper.Map<List<PreferenceDto>>(preferences)).Returns(expectedPreferences);

        // Act
        var result = _accountManager.GetPreferencesByUserId(userId);

        // Assert
        Assert.Equal(expectedPreferences.Count, result.Count);
        Assert.Equal(expectedPreferences[0].PreferenceId, result[0].PreferenceId);
    }

    [Fact]
    public void UpdatePreferences_ReturnsUpdatedAccount_WhenPreferencesAreUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();

        var preferencesDto = new List<PreferenceDto>
        {
            new PreferenceDto { PreferenceId = preferenceId, PreferenceName = "UpdatedPreference1" }
        };

        var preferences = new List<Preference>
        {
            new Preference { PreferenceId = preferenceId, PreferenceName = "UpdatedPreference1" }
        };

        var existingAccount = new Account 
        { 
            AccountId = userId, 
            Preferences = new List<Preference>()
        };
        var updatedAccount = new Account 
        { 
            AccountId = userId, 
            Preferences = preferences 
        };
        var expectedAccountDto = new AccountDto 
        { 
            AccountId = userId 
        };

        foreach (var preference in updatedAccount.Preferences)
        {
            _testOutputHelper.WriteLine(preference.PreferenceId + " - " + preference.PreferenceName);
        }

        _mockRepository.Setup(manager => manager.ReadAccount(userId)).Returns(existingAccount);
        _mockMapper.Setup(mapper => mapper.Map<List<Preference>>(preferencesDto)).Returns(preferences);
        _mockRepository.Setup(manager => manager.UpdateAccount(It.IsAny<Account>())); // UpdateAccount mock setup
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(expectedAccountDto);

        // Act
        var result = _accountManager.UpdatePreferences(userId, preferencesDto);

        // Assert
        Assert.NotNull(result); // Ensure result is not null
        Assert.Equal(expectedAccountDto.AccountId, result.AccountId);
        _mockRepository.Verify(manager => manager.UpdateAccount(It.IsAny<Account>()), Times.Once);
    }

}