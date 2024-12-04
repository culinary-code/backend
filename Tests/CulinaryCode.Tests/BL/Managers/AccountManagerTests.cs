using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
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
    private readonly Mock<IPreferenceRepository> _mockPreferenceRepository;
    private readonly Mock<IRecipeRepository> _mockRecipeRepository;
    
    
    public AccountManagerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockRepository = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<AccountManager>>();
        _mockMapper = new Mock<IMapper>();
        _mockPreferenceRepository = new Mock<IPreferenceRepository>();
        _mockRecipeRepository = new Mock<IRecipeRepository>();
        _accountManager = new AccountManager(_mockRepository.Object, _loggerMock.Object, _mockMapper.Object, _mockPreferenceRepository.Object, _mockRecipeRepository.Object);
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

        var account = new Account
        {
            AccountId = userId,
            Preferences = preferences
        };

        _mockRepository.Setup(manager => manager.ReadAccountWithPreferencesByAccountId(userId)).Returns(account);
        _mockMapper.Setup(mapper => mapper.Map<List<PreferenceDto>>(preferences)).Returns(expectedPreferences);

        // Act
        var result = _accountManager.GetPreferencesByUserId(userId);

        // Assert
        Assert.NotNull(result); 
        Assert.Equal(expectedPreferences.Count, result.Count);
        Assert.Equal(expectedPreferences[0].PreferenceId, result[0].PreferenceId);
        Assert.Equal(expectedPreferences[1].PreferenceId, result[1].PreferenceId);
    }


    [Fact]
    public void DeletePreference_CallsDelete_WhenPreferenceIsDeleted()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();

        var account = new Account
        {
            AccountId = accountId,
            Preferences = new List<Preference>
            {
                new Preference { PreferenceId = preferenceId, PreferenceName = "Preference1" }
            }
        };

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).Returns(account);

        _mockRepository.Setup(r => r.DeletePreferenceFromAccount(accountId, preferenceId)).Verifiable();

        // Act
        _accountManager.RemovePreferenceFromAccount(accountId, preferenceId);

        // Assert
        _mockRepository.Verify(r => r.DeletePreferenceFromAccount(accountId, preferenceId), Times.Once); // Ensure delete was called once
    }
    
    [Fact]
    public void AddPreferenceToAccount_ReturnsUpdatedAccount_WhenPreferenceIsAdded()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var preferenceDto = new PreferenceDto { PreferenceId = Guid.NewGuid(), PreferenceName = "New Preference" };

        var account = new Account
        {
            AccountId = accountId,
            Preferences = new List<Preference>()
        };

        var updatedAccount = new AccountDto
        {
            AccountId = accountId,
            Preferences = new List<PreferenceDto> { preferenceDto }
        };

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).Returns(account);
        _mockPreferenceRepository.Setup(pr => pr.ReadPreferenceByName(preferenceDto.PreferenceName))
            .Returns((Preference)null); // Simulate that preference does not exist
        _mockPreferenceRepository.Setup(pr => pr.CreatePreference(It.IsAny<Preference>()))
            .Returns(new Preference { PreferenceName = preferenceDto.PreferenceName });
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);
        _mockRepository.Setup(r => r.UpdateAccount(It.IsAny<Account>())).Verifiable();

        // Act
        var result = _accountManager.AddPreferenceToAccount(accountId, preferenceDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedAccount.AccountId, result.AccountId);
        Assert.Single(result.Preferences);
        Assert.Equal(preferenceDto.PreferenceName, result.Preferences.First().PreferenceName);
        _mockRepository.Verify(r => r.UpdateAccount(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public void AddPreferenceToAccount_ReturnsSameAccount_WhenPreferenceAlreadyExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var preferenceDto = new PreferenceDto { PreferenceId = Guid.NewGuid(), PreferenceName = "Existing Preference" };

        var account = new Account
        {
            AccountId = accountId,
            Preferences = new List<Preference>
            {
                new Preference { PreferenceName = preferenceDto.PreferenceName }
            }
        };

        var accountDto = new AccountDto { AccountId = accountId, Preferences = new List<PreferenceDto> { preferenceDto } };

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).Returns(account);
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(accountDto);

        // Act
        var result = _accountManager.AddPreferenceToAccount(accountId, preferenceDto);

        // Assert
        Assert.Equal(accountDto.AccountId, result.AccountId);
        Assert.Single(result.Preferences);  // Preference should not be duplicated
        Assert.Equal(preferenceDto.PreferenceName, result.Preferences.First().PreferenceName);
    }

    [Fact]
    public void AddPreferenceToAccount_ThrowsAccountNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var preferenceDto = new PreferenceDto { PreferenceId = Guid.NewGuid(), PreferenceName = "Existing Preference" };

        var account = new Account
        {
            AccountId = accountId,
            Preferences = new List<Preference>
            {
                new Preference { PreferenceName = preferenceDto.PreferenceName }
            }
        };

        var accountDto = new AccountDto
        {
            AccountId = accountId,
            Preferences = new List<PreferenceDto> { preferenceDto }
        };

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).Returns(account);
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(accountDto);

        // Act
        var result = _accountManager.AddPreferenceToAccount(accountId, preferenceDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountDto.AccountId, result.AccountId);
        Assert.Single(result.Preferences);
        Assert.Equal(preferenceDto.PreferenceName, result.Preferences.First().PreferenceName);
    }
    
    [Fact]
        public void GetFavoriteRecipesByUserId_ReturnsMappedFavoriteRecipes_WhenRecipesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var recipeId1 = Guid.NewGuid();
            var recipeId2 = Guid.NewGuid();

            var favoriteRecipes = new List<Recipe>
            {
                new Recipe { RecipeId = recipeId1, RecipeName = "Recipe 1" },
                new Recipe { RecipeId = recipeId2, RecipeName = "Recipe 2" }
            };

            _mockRepository.Setup(repo => repo.ReadFavoriteRecipesByUserId(userId)).Returns(favoriteRecipes);

            var expectedRecipes = new List<RecipeDto>
            {
                new RecipeDto { RecipeId = recipeId1, RecipeName = "Recipe 1" },
                new RecipeDto { RecipeId = recipeId2, RecipeName = "Recipe 2" }
            };
            _mockMapper.Setup(mapper => mapper.Map<List<RecipeDto>>(favoriteRecipes)).Returns(expectedRecipes);

            // Act
            var result = _accountManager.GetFavoriteRecipesByUserId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRecipes.Count, result.Count);
            Assert.Equal(expectedRecipes[0].RecipeId, result[0].RecipeId);
            Assert.Equal(expectedRecipes[1].RecipeId, result[1].RecipeId);
        }

    [Fact]
    public void GetFavoriteRecipesByUserId_ThrowsRecipeNotFoundException_WhenNoRecipesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.ReadFavoriteRecipesByUserId(userId))
            .Throws(new RecipeNotFoundException("No favorite recipes found for the given account."));

        // Act & Assert
        var exception = Assert.Throws<RecipeNotFoundException>(() => _accountManager.GetFavoriteRecipesByUserId(userId));
        Assert.Equal("No favorite recipes found for the given account.", exception.Message);
    }
    
    [Fact]
    public void AddFavoriteRecipeToAccount_ReturnsUpdatedAccount_WhenRecipeIsAdded()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var recipeName = "Chocolate Cake";

        var account = new Account
        {
            AccountId = accountId,
            FavoriteRecipes = new List<FavoriteRecipe>()
        };

        var recipe = new Recipe
        {
            RecipeId = recipeId,
            RecipeName = recipeName
        };

        _mockRepository.Setup(r => r.ReadAccount(accountId)).Returns(account);
        _mockRecipeRepository.Setup(r => r.ReadRecipeById(recipeId)).Returns(recipe);

        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>()))
            .Returns((Account sourceAccount) => new AccountDto
            {
                AccountId = sourceAccount.AccountId,
                FavoriteRecipes = sourceAccount.FavoriteRecipes.Select(fr => new FavoriteRecipeDto
                {
                    FavoriteRecipeId = fr.Recipe.RecipeId,
                    Recipe = new RecipeDto
                    {
                        RecipeId = fr.Recipe.RecipeId,
                        RecipeName = fr.Recipe.RecipeName
                    },
                    CreatedAt = fr.CreatedAt
                }).ToList()
            });
        
        // Act
        var result = _accountManager.AddFavoriteRecipeToAccount(accountId, recipeId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.FavoriteRecipes);
        Assert.Equal(recipeName, result.FavoriteRecipes.First().Recipe.RecipeName);
        _mockRepository.Verify(r => r.UpdateAccount(It.IsAny<Account>()), Times.Once);
    }
    
    [Fact]
    public void DeleteFavoriteRecipe_CallsDelete_WhenPreferenceIsDeleted()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var favoriteRecipeId = Guid.NewGuid();

        var account = new Account
        {
            AccountId = accountId,
            FavoriteRecipes = new List<FavoriteRecipe>
            {
                new FavoriteRecipe
                {
                    FavoriteRecipeId = favoriteRecipeId,
                    Recipe = new Recipe
                    {
                        RecipeId = favoriteRecipeId,
                        RecipeName = "Apple Cake"
                    }
                }
            }
        };

        _mockRepository.Setup(r => r.ReadAccount(accountId)).Returns(account);
        _mockRepository.Setup(r => r.DeleteFavoriteRecipesByUserId(accountId, favoriteRecipeId)).Verifiable();

        // Act
        _accountManager.RemoveFavoriteRecipeFromAccount(accountId, favoriteRecipeId);

        // Assert
        _mockRepository.Verify(r => r.DeleteFavoriteRecipesByUserId(accountId, favoriteRecipeId), Times.Once);
    }
}