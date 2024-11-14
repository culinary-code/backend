using AutoMapper;
using BL.DTOs.Recipes.Ingredients;
using BL.DTOs.MealPlanning;
using BL.Managers.Groceries;
using DAL.Groceries;
using DAL.Recipes;
using DOM.Recipes.Ingredients;
using DOM.MealPlanning;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Accounts;
using DOM.Accounts;
using Xunit.Abstractions;

namespace BL.Testing
{
    public class GroceryManagerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IGroceryRepository> _mockGroceryRepository;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<IMealPlannerRepository> _mockMealPlannerRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GroceryManager _groceryManager;

        public GroceryManagerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _mockGroceryRepository = new Mock<IGroceryRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockMealPlannerRepository = new Mock<IMealPlannerRepository>();
            _mockMapper = new Mock<IMapper>();

            _groceryManager = new GroceryManager(
                _mockGroceryRepository.Object,
                _mockAccountRepository.Object,
                _mockMapper.Object,
                _mockMealPlannerRepository.Object
            );
        }

        private static Account CreateSampleAccount(Guid accountId)
        {
            return new Account
            {
                AccountId = accountId,
                Name = "John Doe",
                Email = "john.doe@example.com",
                FamilySize = 4
            };
        }

        private static MealPlanner CreateSampleMealPlanner()
        {
            var ingredient1 = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Carrot", Measurement = MeasurementType.Kilogram };
            var ingredient2 = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Apple", Measurement = MeasurementType.Gram };

            var meal1 = new PlannedMeal
            {
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity { Ingredient = ingredient1, Quantity = 1 },
                    new IngredientQuantity { Ingredient = ingredient2, Quantity = 150 }
                }
            };

            var meal2 = new PlannedMeal
            {
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity { Ingredient = ingredient1, Quantity = 2 },
                    new IngredientQuantity { Ingredient = ingredient2, Quantity = 100 }
                }
            };

            return new MealPlanner
            {
                NextWeek = new List<PlannedMeal> { meal1, meal2 }
            };
        }

        [Fact]
        public void CreateGroceryList_ShouldReturnCorrectGroceryList_WhenValidDataIsProvided()
        {
            var accountId = Guid.NewGuid();
            var account = CreateSampleAccount(accountId);
            var mealPlanner = CreateSampleMealPlanner();

            _mockAccountRepository
                .Setup(repo => repo.ReadAccount(accountId))
                .Returns(account);

            _mockMealPlannerRepository
                .Setup(repo => repo.ReadMealPlannerById(accountId))
                .Returns(mealPlanner);

            _mockMapper
                .Setup(mapper => mapper.Map<GroceryListDto>(It.IsAny<GroceryList>()))
                .Returns(new GroceryListDto());  

            var result = _groceryManager.CreateGroceryList(accountId);

            _testOutputHelper.WriteLine("GroceryListDto:");
            _testOutputHelper.WriteLine($"GroceryListId: {result.GroceryListId}");
            _testOutputHelper.WriteLine($"AccountId: {result.Account.AccountId}");
            _testOutputHelper.WriteLine($"Account Name: {result.Account.Name}");
            _testOutputHelper.WriteLine($"Number of Items: {result.Items.Count}");

            foreach (var ingredient in result.Ingredients)
            {
                _testOutputHelper.WriteLine($"Ingredient: {ingredient.Ingredient.IngredientName}, Quantity: {ingredient.Quantity}");
            }
            
            Assert.NotNull(result);
            _mockAccountRepository.Verify(repo => repo.ReadAccount(accountId), Times.Once);
            _mockMealPlannerRepository.Verify(repo => repo.ReadMealPlannerById(accountId), Times.Once);
            _mockGroceryRepository.Verify(repo => repo.CreateGroceryList(It.IsAny<GroceryList>()), Times.Once);
        }

        [Fact]
        public void CreateGroceryList_ShouldThrowException_WhenAccountIsNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _mockAccountRepository
                .Setup(repo => repo.ReadAccount(accountId))
                .Returns((Account)null);

            Assert.Throws<Exception>(() => _groceryManager.CreateGroceryList(accountId));
        }

        [Fact]
        public void CreateGroceryList_ShouldThrowException_WhenNoPlannedMeals()
        {
            var accountId = Guid.NewGuid();
            var account = CreateSampleAccount(accountId);
            var mealPlanner = new MealPlanner(); // No planned meals

            _mockAccountRepository
                .Setup(repo => repo.ReadAccount(accountId))
                .Returns(account);

            _mockMealPlannerRepository
                .Setup(repo => repo.ReadMealPlannerById(accountId))
                .Returns(mealPlanner);

            Assert.Throws<Exception>(() => _groceryManager.CreateGroceryList(accountId));
        }
    }
}
