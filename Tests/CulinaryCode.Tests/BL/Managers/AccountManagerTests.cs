using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace CulinaryCode.Tests.BL.Managers;

public class AccountManagerTests
{
    private readonly Mock<IAccountRepository> _mockRepository;
    private readonly Mock<ILogger<AccountManager>> _loggerMock;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AccountManager _accountManager;
    private readonly Mock<IPreferenceRepository> _mockPreferenceRepository;
    private readonly Mock<IRecipeRepository> _mockRecipeRepository;
    
    
    public AccountManagerTests()
    {
        _mockRepository = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<AccountManager>>();
        _mockMapper = new Mock<IMapper>();
        _mockPreferenceRepository = new Mock<IPreferenceRepository>();
        _mockRecipeRepository = new Mock<IRecipeRepository>();
        _accountManager = new AccountManager(_mockRepository.Object, _loggerMock.Object, _mockMapper.Object, _mockPreferenceRepository.Object, _mockRecipeRepository.Object);
    }
    
    [Fact]
    public async Task GetAccountById_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var expectedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).ReturnsAsync(Result<Account>.Success(new Account()));
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(expectedAccount);

        // Act
        var result = await _accountManager.GetAccountById(accountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAccount, result.Value);
    }
    
    [Fact]
    public async Task GetAccountById_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockRepository.Setup(manager => manager.ReadAccount(It.IsAny<Guid>())).ReturnsAsync(Result<Account>.Failure("", ResultFailureType.NotFound));

        // Act
        var result = await _accountManager.GetAccountById(accountId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task UpdateAccount_ReturnsUpdatedAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updatedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).ReturnsAsync(Result<Account>.Success(new Account()));
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);

        // Act
        var result = await _accountManager.UpdateAccount(updatedAccount);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updatedAccount, result.Value);
    }
    
    [Fact]
    public async Task UpdateAccount_ReturnsUpdatedAccountWithFamilySize_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updatedAccount = new AccountDto { AccountId = accountId, FamilySize = 14 };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).ReturnsAsync(Result<Account>.Success(new Account()));
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);

        // Act
        var result = await _accountManager.UpdateAccount(updatedAccount);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updatedAccount, result.Value);
    }
    
    [Fact]
    public async Task UpdateAccount_ThrowsAccountNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updatedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).ReturnsAsync(Result<Account>.Failure("", ResultFailureType.NotFound));

        // Act & Assert
        var result = await _accountManager.UpdateAccount(updatedAccount);
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task CreateAccount_CreatesAccount_WhenAccountDoesNotExist()
    {
        // Arrange
        var username = "JohnDoe";
        var email = "johndoe@example.org";
        var userId = Guid.NewGuid();
        Account account = new Account()
        {
            AccountId = userId,
            Email = email,
            Name = username,
        };
        
        _mockRepository.Setup(accountRepository => accountRepository.CreateAccount(It.IsAny<Account>()))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Act
        var result = await _accountManager.CreateAccount(username, email, userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(accountRepository => accountRepository.CreateAccount(It.IsAny<Account>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAccount_DeletesAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockRepository.Setup(manager => manager.DeleteAccount(accountId)).ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Act
        var result = await _accountManager.DeleteAccount(accountId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepository.Verify(manager => manager.DeleteAccount(accountId), Times.Once);
    }
    
    [Fact]
    public async Task GetPreferencesByUserId_ReturnsMappedPreferences_WhenPreferencesExist()
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

        _mockRepository.Setup(manager => manager.ReadAccountWithPreferencesByAccountId(userId)).ReturnsAsync(Result<Account>.Success(account));
        _mockMapper.Setup(mapper => mapper.Map<List<PreferenceDto>>(preferences)).Returns(expectedPreferences);

        // Act
        var result = await _accountManager.GetPreferencesByUserId(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPreferences.Count, result.Value!.Count);
        Assert.Equal(expectedPreferences[0].PreferenceId, result.Value[0].PreferenceId);
        Assert.Equal(expectedPreferences[1].PreferenceId, result.Value[1].PreferenceId);
    }


    [Fact]
    public async Task DeletePreference_CallsDelete_WhenPreferenceIsDeleted()
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

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).ReturnsAsync(Result<Account>.Success(account));

        _mockRepository.Setup(r => r.DeletePreferenceFromAccount(accountId, preferenceId)).Verifiable();

        // Act
        await _accountManager.RemovePreferenceFromAccount(accountId, preferenceId);

        // Assert
        _mockRepository.Verify(r => r.DeletePreferenceFromAccount(accountId, preferenceId), Times.Once); // Ensure delete was called once
    }
    
    [Fact]
    public async Task AddPreferenceToAccount_ReturnsUpdatedAccount_WhenPreferenceIsAdded()
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

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).ReturnsAsync(Result<Account>.Success(account));
        _mockPreferenceRepository.Setup(pr => pr.ReadPreferenceByNameNoTracking(preferenceDto.PreferenceName))
            .ReturnsAsync(Result<Preference>.Failure("", ResultFailureType.NotFound)); // Simulate that preference does not exist
        _mockPreferenceRepository.Setup(pr => pr.CreatePreference(It.IsAny<Preference>()))
            .ReturnsAsync(Result<Preference>.Success(new Preference { PreferenceName = preferenceDto.PreferenceName }));
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);
        _mockRepository.Setup(r => r.UpdateAccount(account)).ReturnsAsync(Result<Unit>.Success(new Unit())).Verifiable();;


        // Act
        var result = await _accountManager.AddPreferenceToAccount(accountId, preferenceDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updatedAccount.AccountId, result.Value!.AccountId);
        Assert.Single(result.Value.Preferences);
        Assert.Equal(preferenceDto.PreferenceName, result.Value.Preferences.First().PreferenceName);
        _mockRepository.Verify(r => r.UpdateAccount(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task AddPreferenceToAccount_ReturnsSameAccount_WhenPreferenceAlreadyExists()
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

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).ReturnsAsync(Result<Account>.Success(account));
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(accountDto);

        // Act
        var result = await _accountManager.AddPreferenceToAccount(accountId, preferenceDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Error, result.FailureType);
    }

    [Fact]
    public async Task AddPreferenceToAccount_ThrowsAccountNotFoundException_WhenAccountDoesNotExist()
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

        _mockRepository.Setup(r => r.ReadAccountWithPreferencesByAccountId(accountId)).ReturnsAsync(Result<Account>.Failure("", ResultFailureType.NotFound));
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(accountDto);

        // Act
        var result = await _accountManager.AddPreferenceToAccount(accountId, preferenceDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
        public async Task GetFavoriteRecipesByUserId_ReturnsMappedFavoriteRecipes_WhenRecipesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var recipeId1 = Guid.NewGuid();
            var recipeId2 = Guid.NewGuid();

            var favoriteRecipes = new List<Recipe?>
            {
                new Recipe { RecipeId = recipeId1, RecipeName = "Recipe 1" },
                new Recipe { RecipeId = recipeId2, RecipeName = "Recipe 2" }
            };

            _mockRepository.Setup(repo => repo.ReadFavoriteRecipesByUserIdNoTracking(userId)).ReturnsAsync(Result<List<Recipe?>>.Success(favoriteRecipes));

            var expectedRecipes = new List<RecipeDto>
            {
                new RecipeDto { RecipeId = recipeId1, RecipeName = "Recipe 1" },
                new RecipeDto { RecipeId = recipeId2, RecipeName = "Recipe 2" }
            };
            _mockMapper.Setup(mapper => mapper.Map<List<RecipeDto>>(favoriteRecipes)).Returns(expectedRecipes);

            // Act
            var result = await _accountManager.GetFavoriteRecipesByUserId(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedRecipes.Count, result.Value!.Count);
            Assert.Equal(expectedRecipes[0].RecipeId, result.Value[0].RecipeId);
            Assert.Equal(expectedRecipes[1].RecipeId, result.Value[1].RecipeId);
        }

    [Fact]
    public async Task GetFavoriteRecipesByUserId_ThrowsRecipeNotFoundException_WhenNoRecipesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
         _mockRepository.Setup(repo => repo.ReadFavoriteRecipesByUserIdNoTracking(userId))
            .ReturnsAsync(Result<List<Recipe?>>.Failure("No favorite recipes found for the given account.", ResultFailureType.NotFound));

        // Act & Assert
        var result = await _accountManager.GetFavoriteRecipesByUserId(userId);
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task AddFavoriteRecipeToAccount_ReturnsUpdatedAccount_WhenRecipeIsAdded()
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

        _mockRepository.Setup(r => r.ReadAccount(accountId)).ReturnsAsync(Result<Account>.Success(account));
        _mockRepository.Setup(r => r.UpdateAccount(account)).ReturnsAsync(Result<Unit>.Success(new Unit()));
        _mockRecipeRepository.Setup(r => r.ReadRecipeById(recipeId)).ReturnsAsync(Result<Recipe>.Success(recipe));
        _mockRecipeRepository.Setup(r => r.UpdateRecipe(recipe)).ReturnsAsync(Result<Unit>.Success(new Unit()));

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
        var result = await _accountManager.AddFavoriteRecipeToAccount(accountId, recipeId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.FavoriteRecipes);
        Assert.Equal(recipeName, result.Value.FavoriteRecipes.First().Recipe.RecipeName);
        _mockRepository.Verify(r => r.UpdateAccount(It.IsAny<Account>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteFavoriteRecipe_CallsDelete_WhenPreferenceIsDeleted()
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

        _mockRepository.Setup(r => r.ReadAccount(accountId)).ReturnsAsync(Result<Account>.Success(account));
        _mockRepository.Setup(r => r.DeleteFavoriteRecipeByUserId(accountId, favoriteRecipeId)).Verifiable();

        // Act
        await _accountManager.RemoveFavoriteRecipeFromAccount(accountId, favoriteRecipeId);

        // Assert
        _mockRepository.Verify(r => r.DeleteFavoriteRecipeByUserId(accountId, favoriteRecipeId), Times.Once);
    }
}