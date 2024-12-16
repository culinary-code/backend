using DAL.Accounts;
using DAL.EF;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CulinaryCode.Tests.DAL.Accounts;

public class GroupRepositoryTests
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly GroupRepository _groupRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    
    public GroupRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            // force unique database for each test so data is isolated
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CulinaryCodeDbContext(options);
        _accountRepository = new Mock<IAccountRepository>();
        _groupRepository = new GroupRepository(_dbContext, _accountRepository.Object);
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