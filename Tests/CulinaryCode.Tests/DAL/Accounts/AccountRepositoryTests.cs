using DAL.Accounts;
using DAL.EF;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Accounts;

public class AccountRepositoryTests
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly IAccountRepository _accountRepository;

    public AccountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            // force unique database for each test so data is isolated
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CulinaryCodeDbContext(options);
        _accountRepository = new AccountRepository(_dbContext);
    }

    [Fact]
    public async Task ReadAccount_AccountExists_ReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accountRepository.ReadAccount(account.AccountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(account.AccountId, result.Value!.AccountId);
        Assert.Equal(account.Name, result.Value.Name);
        Assert.Equal(account.Email, result.Value.Email);
    }

    [Fact]
    public async Task ReadAccount_AccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.ReadAccount(accountId);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking_AccountExists_ReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org",
            GroceryList = new GroceryList
            {
                GroceryListId = Guid.NewGuid(),
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity
                    {
                        IngredientQuantityId = Guid.NewGuid(),
                        Ingredient = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Test Ingredient" }
                    }
                },
                Items = new List<ItemQuantity>
                {
                    new ItemQuantity
                    {
                        ItemQuantityId = Guid.NewGuid(),
                        GroceryItem = new GroceryItem { GroceryItemId = Guid.NewGuid(), GroceryItemName = "Test Item" }
                    }
                }
            },
            Planner = new MealPlanner
            {
                MealPlannerId = Guid.NewGuid(),
                NextWeek = new List<PlannedMeal>
                {
                    new PlannedMeal
                    {
                        PlannedMealId = Guid.NewGuid(),
                        Ingredients = new List<IngredientQuantity>
                        {
                            new IngredientQuantity
                            {
                                IngredientQuantityId = Guid.NewGuid(),
                                Ingredient = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Test Ingredient" }
                            }
                        },
                        Recipe = new Recipe { RecipeId = Guid.NewGuid(), RecipeName = "Test Recipe" }
                    }
                }
            }
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result =
            await _accountRepository.ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking(account.AccountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(account.AccountId, result.Value!.AccountId);
    }

    [Fact]
    public async Task
        ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking_AccountDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.ReadAccountWithMealPlannerNextWeekAndWithGroceryListNoTracking(accountId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task ReadFavoriteRecipesByUserIdNoTracking_AccountExists_ReturnsFavoriteRecipes()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        var favoriteRecipe = new FavoriteRecipe
        {
            AccountId = account.AccountId,
            Recipe = new Recipe { RecipeId = Guid.NewGuid(), RecipeName = "Test Recipe" }
        };

        _dbContext.Accounts.Add(account);
        _dbContext.FavoriteRecipes.Add(favoriteRecipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accountRepository.ReadFavoriteRecipesByUserIdNoTracking(account.AccountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(favoriteRecipe.Recipe.RecipeId, result.Value.First().RecipeId);
    }

    [Fact]
    public async Task ReadFavoriteRecipesByUserIdNoTracking_AccountDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.ReadFavoriteRecipesByUserIdNoTracking(accountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task UpdateAccount_AccountExists_UpdatesAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        _dbContext.SaveChanges();

        // Act
        account.Name = "Updated Account";
        await _accountRepository.UpdateAccount(account);

        // Assert
        var result = await _dbContext.Accounts.FindAsync(account.AccountId);
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal("Updated Account", result.Name);
    }

    [Fact]
    public async Task CreateAccount_ValidAccount_CreatesAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        // Act
        await _accountRepository.CreateAccount(account);

        // Assert
        var result = await _dbContext.Accounts.FindAsync(account.AccountId);
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(account.Name, result.Name);
        Assert.Equal(account.Email, result.Email);
    }

    [Fact]
    public async Task DeleteAccount_AccountExists_DeletesAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@test.test"
        };

        var accountPreference = new AccountPreference
        {
            AccountId = account.AccountId,
            PreferenceId = Guid.NewGuid()
        };

        var accountFavoriteRecipe = new FavoriteRecipe
        {
            AccountId = account.AccountId,
            RecipeId = Guid.NewGuid()
        };

        var groceryList = new GroceryList
        {
            AccountId = account.AccountId,
            GroceryListId = Guid.NewGuid()
        };

        var mealplanner = new MealPlanner
        {
            AccountId = account.AccountId,
            MealPlannerId = Guid.NewGuid()
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        await _accountRepository.DeleteAccount(account.AccountId);

        // Assert
        var result = await _dbContext.Accounts.FindAsync(account.AccountId);
        Assert.Null(result);
        Assert.Null(await _dbContext.AccountPreferences.FindAsync(accountPreference.AccountPreferenceId));
        Assert.Null(await _dbContext.FavoriteRecipes.FindAsync(accountFavoriteRecipe.FavoriteRecipeId));
        Assert.Null(await _dbContext.GroceryLists.FindAsync(groceryList.GroceryListId));
        Assert.Null(await _dbContext.MealPlanners.FindAsync(mealplanner.MealPlannerId));
    }

    [Fact]
    public async Task DeleteAccount_AccountDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.DeleteAccount(nonExistentAccountId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal($"No account found with id {nonExistentAccountId}", result.ErrorMessage);
    }

    [Fact]
    public async Task DeletePreferenceFromAccount_AccountDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        var preferenceId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.DeletePreferenceFromAccount(nonExistentAccountId, preferenceId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task DeletePreferenceFromAccount_PreferenceDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var nonExistentPreferenceId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.DeletePreferenceFromAccount(account.AccountId, nonExistentPreferenceId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task DeletePreferenceFromAccount_PreferenceExists_DeletesPreference()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org",
            Preferences = new List<Preference>
            {
                new Preference { PreferenceId = Guid.NewGuid() }
            }
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var preferenceId = account.Preferences.First().PreferenceId;

        // Act
        var result = await _accountRepository.DeletePreferenceFromAccount(account.AccountId, preferenceId);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedAccount = await _dbContext.Accounts.Include(a => a.Preferences)
            .FirstOrDefaultAsync(a => a.AccountId == account.AccountId);
        Assert.Empty(updatedAccount.Preferences);
    }

    [Fact]
    public async Task DeleteFavoriteRecipeByUserId_AccountDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.DeleteFavoriteRecipeByUserId(nonExistentAccountId, recipeId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task DeleteFavoriteRecipeByUserId_FavoriteRecipeDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var nonExistentRecipeId = Guid.NewGuid();

        // Act
        var result = await _accountRepository.DeleteFavoriteRecipeByUserId(account.AccountId, nonExistentRecipeId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task DeleteFavoriteRecipeByUserId_FavoriteRecipeExists_DeletesFavoriteRecipe()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        var favoriteRecipe = new FavoriteRecipe
        {
            AccountId = account.AccountId,
            Recipe = new Recipe { RecipeId = Guid.NewGuid(), RecipeName = "Test Recipe" }
        };

        _dbContext.Accounts.Add(account);
        _dbContext.FavoriteRecipes.Add(favoriteRecipe);
        await _dbContext.SaveChangesAsync();

        var recipeId = favoriteRecipe.Recipe.RecipeId;

        // Act
        var result = await _accountRepository.DeleteFavoriteRecipeByUserId(account.AccountId, recipeId);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedFavoriteRecipe = await _dbContext.FavoriteRecipes.FirstOrDefaultAsync(fr =>
            fr.Recipe.RecipeId == recipeId && fr.Account.AccountId == account.AccountId);
        Assert.Null(deletedFavoriteRecipe);
    }
}