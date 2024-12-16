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
        var result = await _mealPlannerRepository.ReadMealPlannerByUserIdWithNextWeekNoTracking(accountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(accountId, result.Value!.Account.AccountId);
        Assert.NotEmpty(result.Value.NextWeek);
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
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.PlannedMealId);
        Assert.NotNull(await _dbContext.PlannedMeals.FindAsync(result.Value.PlannedMealId));
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
        var result = await _mealPlannerRepository.ReadNextWeekPlannedMealsNoTrackingByUserId(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(plannedMeals.Count, result.Value!.Count);
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
        var result = await _mealPlannerRepository.ReadPlannedMealsAfterDateNoTrackingByUserId(DateTime.UtcNow, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!); // Should only return the meal in NextWeek
        Assert.Equal("Next Week Recipe", result.Value!.First().Recipe?.RecipeName);
    }

    [Fact]
    public async Task ReadPlannedMealsAfterDateNoTrackingByGroupId_ReturnsFilteredPlannedMeals()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;

        // Create meal planner with history and next week planned meals
        var historyMeals = new List<PlannedMeal>
        {
            new PlannedMeal
            {
                PlannedDate = dateTime.AddDays(-2),
                Recipe = new Recipe { RecipeName = "Old Meal" }
            },
            new PlannedMeal
            {
                PlannedDate = dateTime.AddDays(-1),
                Recipe = new Recipe { RecipeName = "Another Old Meal" }
            }
        };

        var nextWeekMeals = new List<PlannedMeal>
        {
            new PlannedMeal
            {
                PlannedDate = dateTime.AddDays(2),
                Recipe = new Recipe { RecipeName = "Upcoming Meal 1" }
            },
            new PlannedMeal
            {
                PlannedDate = dateTime.AddDays(5),
                Recipe = new Recipe { RecipeName = "Upcoming Meal 2" }
            }
        };

        var group = new Group
        {
            GroupId = groupId,
            GroupName = "Test Group"
        };

        var mealPlanner = new MealPlanner
        {
            Group = group,
            History = historyMeals,
            NextWeek = nextWeekMeals
        };

        // Add meal planner to the database
        _dbContext.MealPlanners.Add(mealPlanner);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mealPlannerRepository.ReadPlannedMealsAfterDateNoTrackingByGroupId(dateTime, groupId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);

        var filteredMealNames = result.Value.Select(m => m.Recipe!.RecipeName).ToList();
        Assert.Contains("Upcoming Meal 1", filteredMealNames);
        Assert.Contains("Upcoming Meal 2", filteredMealNames);

        // Ensure old meals are excluded
        Assert.DoesNotContain("Old Meal", filteredMealNames);
        Assert.DoesNotContain("Another Old Meal", filteredMealNames);
    }

}