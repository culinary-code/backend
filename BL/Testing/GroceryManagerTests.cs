using BL.Managers.Groceries;
using DAL.Accounts;
using DAL.Groceries;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using FluentAssertions;
using Moq;
using Xunit;

namespace BL.Testing;

public class GroceryManagerTests
{
    private readonly Mock<IGroceryRepository> _mockGroceryRepository;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly GroceryManager _groceryManager;

        public GroceryManagerTests()
        {
            // Initialize mock dependencies
            _mockGroceryRepository = new Mock<IGroceryRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();

            // Create the service instance to test
            _groceryManager = new GroceryManager(_mockGroceryRepository.Object, _mockAccountRepository.Object);
        }

        [Fact]
        public void CreateGroceryList_WhenNoMealsArePlanned_ShouldReturnEmptyGroceryList()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, Name = "Test User" };

            var mealPlanner = new MealPlanner
            {
                NextWeek = new List<PlannedMeal>() // No meals planned for next week
            };

            _mockAccountRepository.Setup(r => r.ReadAccount(accountId)).Returns(account);
            _mockGroceryRepository.Setup(r => r.GetMealPlannerById(accountId)).Returns(mealPlanner);

            // Act
            var groceryList = _groceryManager.CreateGroceryList(accountId);

            // Assert
            groceryList.Should().NotBeNull();
            groceryList.Ingredients.Should().BeEmpty();
            groceryList.Account.Should().Be(account);
        }

        [Fact]
        public void CreateGroceryList_WhenMealsArePlanned_ShouldGroupAndSumIngredients()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, Name = "Test User" };

            var mealPlanner = new MealPlanner
            {
                NextWeek = new List<PlannedMeal>
                {
                    new PlannedMeal
                    {
                        Ingredients = new List<IngredientQuantity>
                        {
                            new IngredientQuantity
                            {
                                Ingredient = new Ingredient { IngredientName = "Tomato", Measurement = MeasurementType.Kilogram },
                                Quantity = 2
                            },
                            new IngredientQuantity
                            {
                                Ingredient = new Ingredient { IngredientName = "Potato", Measurement = MeasurementType.Kilogram },
                                Quantity = 5
                            }
                        }
                    },
                    new PlannedMeal
                    {
                        Ingredients = new List<IngredientQuantity>
                        {
                            new IngredientQuantity
                            {
                                Ingredient = new Ingredient { IngredientName = "Tomato", Measurement = MeasurementType.Kilogram },
                                Quantity = 1
                            }
                        }
                    }
                }
            };

            _mockAccountRepository.Setup(r => r.ReadAccount(accountId)).Returns(account);
            _mockGroceryRepository.Setup(r => r.GetMealPlannerById(accountId)).Returns(mealPlanner);

            // Act
            var groceryList = _groceryManager.CreateGroceryList(accountId);

            // Assert
            groceryList.Should().NotBeNull();
            groceryList.Ingredients.Should().HaveCount(2);

            // Check Tomato grouping
            var tomato = groceryList.Ingredients.FirstOrDefault(i => i.Ingredient.IngredientName == "Tomato");
            tomato.Should().NotBeNull();
            tomato.Quantity.Should().Be(3); // 2 (from first meal) + 1 (from second meal)

            // Check Potato
            var potato = groceryList.Ingredients.FirstOrDefault(i => i.Ingredient.IngredientName == "Potato");
            potato.Should().NotBeNull();
            potato.Quantity.Should().Be(5); // Only one entry for Potato
        }

        [Fact]
        public void CreateGroceryList_WhenIngredientsHaveZeroQuantity_ShouldNotIncludeInList()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, Name = "Test User" };

            var mealPlanner = new MealPlanner
            {
                NextWeek = new List<PlannedMeal>
                {
                    new PlannedMeal
                    {
                        Ingredients = new List<IngredientQuantity>
                        {
                            new IngredientQuantity
                            {
                                Ingredient = new Ingredient { IngredientName = "Tomato", Measurement = MeasurementType.Kilogram },
                                Quantity = 0 // Zero quantity, should not be included
                            },
                            new IngredientQuantity
                            {
                                Ingredient = new Ingredient { IngredientName = "Potato", Measurement = MeasurementType.Kilogram },
                                Quantity = 5
                            }
                        }
                    }
                }
            };

            _mockAccountRepository.Setup(r => r.ReadAccount(accountId)).Returns(account);
            _mockGroceryRepository.Setup(r => r.GetMealPlannerById(accountId)).Returns(mealPlanner);

            // Act
            var groceryList = _groceryManager.CreateGroceryList(accountId);

            // Assert
            groceryList.Should().NotBeNull();
            groceryList.Ingredients.Should().HaveCount(1);
            var potato = groceryList.Ingredients.FirstOrDefault(i => i.Ingredient.IngredientName == "Potato");
            potato.Should().NotBeNull();
            potato.Quantity.Should().Be(5);
        }
        
        [Fact]
        public void CreateGroceryList_WhenAccountDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var invalidAccountId = Guid.NewGuid();
    
            // Simulate that no account is found for the given ID
            _mockAccountRepository.Setup(r => r.ReadAccount(invalidAccountId)).Returns((Account)null);

            // Act & Assert
            Action act = () => _groceryManager.CreateGroceryList(invalidAccountId);
            act.Should().Throw<Exception>().WithMessage("Account not found");
        }


        [Fact]
        public void CreateGroceryList_WhenMealPlannerIsNull_ShouldThrowException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, Name = "Test User" };

            // Simulate that the account is found, but the meal planner is null
            _mockAccountRepository.Setup(r => r.ReadAccount(accountId)).Returns(account);
            _mockGroceryRepository.Setup(r => r.GetMealPlannerById(accountId)).Returns((MealPlanner)null);

            // Act & Assert
            Action act = () => _groceryManager.CreateGroceryList(accountId);
            act.Should().Throw<Exception>().WithMessage("Meal planner not found");
        }


}
