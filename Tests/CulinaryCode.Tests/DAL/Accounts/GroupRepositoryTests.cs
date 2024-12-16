using CulinaryCode.Tests.util;
using DAL.Accounts;
using DAL.EF;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Moq;

namespace CulinaryCode.Tests.DAL.Accounts;

public class GroupRepositoryTests : IClassFixture<TestPostgresContainerFixture>, IAsyncLifetime
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly GroupRepository _groupRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly TestPostgresContainerFixture _fixture;
    
    public GroupRepositoryTests(TestPostgresContainerFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _accountRepository = new Mock<IAccountRepository>();
        _groupRepository = new GroupRepository(_dbContext, _accountRepository.Object);
        _fixture = fixture;
    }
    
    // Ensure the database is reset before each test
    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task ReadGroupWithMealPlannerNextWeekAndWithGroceryListNoTracking_AccountExists_ReturnsAccount()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        var group = new Group
        {
            GroupId = groupId,
            GroupName = "Test Group"
        };
        
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org",
            ChosenGroupId = groupId,
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

        _dbContext.Groups.Add(group);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result =
            await _groupRepository.ReadGroupWithMealPlannerNextWeekAndWithGroceryListNoTracking(groupId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(account.ChosenGroupId, result.Value!.GroupId);
    }
}