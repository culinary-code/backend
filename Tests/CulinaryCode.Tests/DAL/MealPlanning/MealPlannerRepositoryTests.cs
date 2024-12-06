using CulinaryCode.Tests.util;
using DAL.EF;
using DAL.MealPlanning;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CulinaryCode.Tests.DAL.MealPlanning;

public class MealPlannerRepositoryTests : IClassFixture<TestPostgresContainerFixture>, IAsyncLifetime
{
    private readonly TestPostgresContainerFixture _fixture;
    private CulinaryCodeDbContext _dbContext;
    private IMealPlannerRepository _mealPlannerRepository;

    public MealPlannerRepositoryTests(TestPostgresContainerFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _mealPlannerRepository = new MealPlannerRepository(_dbContext);
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ReadMealPlannerByIdWithNextWeek_Exists_ReturnsMealPlannerWithNextWeek()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var mealPlanner = new MealPlanner
        {
            Account = new Account { AccountId = accountId },
            NextWeek = new List<PlannedMeal> { new PlannedMeal { PlannedDate = DateTime.UtcNow } }
        };
        _dbContext.MealPlanners.Add(mealPlanner);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mealPlannerRepository.ReadMealPlannerByIdWithNextWeekNoTracking(accountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.Account.AccountId);
        Assert.NotEmpty(result.NextWeek);
    }

    [Fact]
    public async Task DeletePlannedMeal_DeletesMeal()
    {
        // Arrange
        var plannedMeal = new PlannedMeal { PlannedDate = DateTime.UtcNow };
        _dbContext.PlannedMeals.Add(plannedMeal);
        await _dbContext.SaveChangesAsync();

        // Act
        await _mealPlannerRepository.DeletePlannedMeal(plannedMeal);

        // Assert
        Assert.Null(await _dbContext.PlannedMeals.FindAsync(plannedMeal.PlannedMealId));
    }

    [Fact]
    public async Task CreatePlannedMeal_CreatesMeal()
    {
        // Arrange
        var plannedMeal = new PlannedMeal { PlannedDate = DateTime.UtcNow };

        // Act
        var result = await _mealPlannerRepository.CreatePlannedMeal(plannedMeal);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.PlannedMealId);
        Assert.NotNull(await _dbContext.PlannedMeals.FindAsync(result.PlannedMealId));
    }

    [Fact]
    public async Task ReadNextWeekPlannedMeals_ReturnsMeals()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plannedMeals = new List<PlannedMeal>
        {
            new PlannedMeal { PlannedDate = DateTime.UtcNow, Recipe = new Recipe { RecipeName = "Test Recipe" } }
        };
        var mealPlanner = new MealPlanner
        {
            Account = new Account { AccountId = userId },
            NextWeek = plannedMeals
        };
        _dbContext.MealPlanners.Add(mealPlanner);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mealPlannerRepository.ReadNextWeekPlannedMealsNoTracking(userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(plannedMeals.Count, result.Count);
    }

    [Fact]
    public async Task ReadPlannedMealsAfterDate_ReturnsFilteredMeals()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plannedMealsHistory = new List<PlannedMeal>
        {
            new PlannedMeal
                { PlannedDate = DateTime.UtcNow.AddDays(-5), Recipe = new Recipe { RecipeName = "History Recipe" } }
        };
        var plannedMealsNextWeek = new List<PlannedMeal>
        {
            new PlannedMeal
                { PlannedDate = DateTime.UtcNow.AddDays(2), Recipe = new Recipe { RecipeName = "Next Week Recipe" } }
        };
        var mealPlanner = new MealPlanner
        {
            Account = new Account { AccountId = userId },
            History = plannedMealsHistory,
            NextWeek = plannedMealsNextWeek
        };
        _dbContext.MealPlanners.Add(mealPlanner);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mealPlannerRepository.ReadPlannedMealsAfterDateNoTracking(DateTime.UtcNow, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Should only return the meal in NextWeek
        Assert.Equal("Next Week Recipe", result.First().Recipe?.RecipeName);
    }
}