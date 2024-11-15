using AutoMapper;
using BL.DTOs.Recipes.Ingredients;
using BL.DTOs.MealPlanning;
using BL.Managers.Groceries;
using DAL.Groceries;
using DOM.Recipes.Ingredients;
using DOM.MealPlanning;
using Moq;
using Xunit;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;
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
        private readonly Mock<ILogger<GroceryManager>> _mockLogger;

        public GroceryManagerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _mockLogger = new Mock<ILogger<GroceryManager>>();
            _mockGroceryRepository = new Mock<IGroceryRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockMealPlannerRepository = new Mock<IMealPlannerRepository>();
            _mockMapper = new Mock<IMapper>();

            _groceryManager = new GroceryManager(
                _mockGroceryRepository.Object,
                _mockAccountRepository.Object,
                _mockMapper.Object,
                _mockMealPlannerRepository.Object,
                _mockLogger.Object
            );
        }

        private static Account CreateSampleAccount(Guid accountId)
        {
            return new Account
            {
                AccountId = Guid.Parse("d1ec841b-9646-4ca7-a1ef-eda7354547f3"),
                Name = "nis",
                Email = "nis@n.n",
                FamilySize = 4
            };
        }

        private static MealPlanner CreateSampleMealPlanner()
        {
            var ingredient1 = new Ingredient
                { IngredientId = Guid.NewGuid(), IngredientName = "Carrot", Measurement = MeasurementType.Kilogram };
            var ingredient2 = new Ingredient
                { IngredientId = Guid.NewGuid(), IngredientName = "Apple", Measurement = MeasurementType.Gram };

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
        public void CreateAccount_ShouldInitializeWithEmptyGroceryList()
        {
            var accountId = Guid.NewGuid();
            var groceryListId = Guid.NewGuid();
    
            var account = new Account
            {
                AccountId = accountId,
                Name = "Nieuw",
                Email = "nieuw@account.com",
                FamilySize = 2,
                GroceryList = new GroceryList
                {
                    GroceryListId = groceryListId
                }
            };

            _mockAccountRepository
                .Setup(repo => repo.CreateAccount(It.IsAny<Account>()))
                .Callback<Account>(createdAccount =>
                {
                    Assert.NotNull(createdAccount.GroceryList);

                    var groceryList = createdAccount.GroceryList;
                    Assert.NotNull(groceryList);
                    Assert.Empty(groceryList.Ingredients);
                    Assert.Empty(groceryList.Items);

                    Assert.Equal(groceryListId, groceryList.GroceryListId);
                });

            _mockAccountRepository.Object.CreateAccount(account);

            _mockAccountRepository.Verify(repo => repo.CreateAccount(It.IsAny<Account>()), Times.Once);

            _testOutputHelper.WriteLine($"Account ID: {account.AccountId} created with Grocery List ID: {account.GroceryList.GroceryListId}");
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
                _testOutputHelper.WriteLine(
                    $"Ingredient: {ingredient.Ingredient.IngredientName}, Quantity: {ingredient.Quantity}");
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

            Assert.Throws<NullReferenceException>(() => _groceryManager.CreateGroceryList(accountId));
        }

        [Fact]
        public void CreateGroceryList_ShouldReturnEmptyList_WhenNoPlannedMeals()
        {
            var accountId = Guid.NewGuid();
            var account = CreateSampleAccount(accountId);
            var mealPlanner = new MealPlanner();

            _mockAccountRepository
                .Setup(repo => repo.ReadAccount(accountId))
                .Returns(account);

            _mockMealPlannerRepository
                .Setup(repo => repo.ReadMealPlannerById(accountId))
                .Returns(mealPlanner);
            
            Assert.Empty(mealPlanner.NextWeek);
        }


        [Fact]
        public void AddItemToGroceryList_ShouldAddNewItem_WhenItemIsNotInGroceryList()
        {
            var accountId = Guid.NewGuid();
            var account = CreateSampleAccount(accountId);
            var mealPlanner = CreateSampleMealPlanner();
            
            var ingredient1 = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Carrot", Measurement = MeasurementType.Kilogram };
            var ingredient2 = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Apple", Measurement = MeasurementType.Gram };
            
            var groceryList = new GroceryList
            {
                GroceryListId = Guid.NewGuid(),
                Account = account,
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity { Ingredient = ingredient1, Quantity = 2 },
                    new IngredientQuantity { Ingredient = ingredient2, Quantity = 150 }
                },
                Items = new List<ItemQuantity>()
            };
            
            var newIngredient = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Potato", Measurement = MeasurementType.Kilogram };
            var newIngredient2 = new Ingredient { IngredientId = ingredient1.IngredientId, IngredientName = "Carrot", Measurement = MeasurementType.Kilogram };
            var newIngredient3 = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Apple", Measurement = MeasurementType.Kilogram };

            var addItemDto = new ItemQuantityDto
            {
                Ingredient = new IngredientDto
                {
                    IngredientId = newIngredient.IngredientId,
                    IngredientName = newIngredient.IngredientName,
                    Measurement = newIngredient.Measurement
                },
                Quantity = 3
            };
            
            var addItemDto2 = new ItemQuantityDto
            {
                Ingredient = new IngredientDto
                {
                    IngredientId = newIngredient2.IngredientId,
                    IngredientName = newIngredient2.IngredientName,
                    Measurement = newIngredient2.Measurement
                },
                Quantity = 3
            };
            
            var addItemDto3 = new ItemQuantityDto
            {
                Ingredient = new IngredientDto
                {
                    IngredientId = newIngredient3.IngredientId,
                    IngredientName = newIngredient3.IngredientName,
                    Measurement = newIngredient3.Measurement
                },
                Quantity = 111
            };
            
            _mockAccountRepository.Setup(repo => repo.ReadAccount(accountId)).Returns(account);
            _mockMealPlannerRepository.Setup(repo => repo.ReadMealPlannerById(accountId)).Returns(mealPlanner);
            _mockGroceryRepository.Setup(repo => repo.ReadGroceryListById(groceryList.GroceryListId)).Returns(groceryList);

            _testOutputHelper.WriteLine("Initial Grocery List ingredients:");
            foreach (var item in groceryList.Ingredients)
            {
                _testOutputHelper.WriteLine($"- Ingredient: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            _testOutputHelper.WriteLine("Initial Grocery List items:");
            foreach (var item in groceryList.Items)
            {
                _testOutputHelper.WriteLine($"- Ingredient: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto);
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto2);
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto3);
            
            var addedItem = groceryList.Items.FirstOrDefault(i => i.Ingredient.IngredientId == newIngredient.IngredientId);
            
            _testOutputHelper.WriteLine("Updated Grocery ListItems:");
            foreach (var item in groceryList.Items)
            {
                _testOutputHelper.WriteLine($"- Ingredient: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            _testOutputHelper.WriteLine("After Grocery ListIngredients:");
            foreach (var item in groceryList.Ingredients)
            {
                _testOutputHelper.WriteLine($"- Ingredient: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            Assert.NotNull(addedItem); 
            Assert.Equal(3, addedItem.Quantity);

            var existingIngredientInIngredients = groceryList.Ingredients.FirstOrDefault(i => i.Ingredient.IngredientId == newIngredient.IngredientId);
            Assert.Null(existingIngredientInIngredients);
        }

        
        [Fact]
        public void AddItemToGroceryList_ShouldAddItem_WhenValidItemIsProvided()
        {
            var accountId = Guid.NewGuid();
            var account = CreateSampleAccount(accountId);
            var mealPlanner = CreateSampleMealPlanner();
            
            var ingredient = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Potato", Measurement = MeasurementType.Kilogram };
            var groceryList = new GroceryList
            {
                GroceryListId = Guid.NewGuid(),
                Account = account,
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity { Ingredient = ingredient, Quantity = 1 }
                }
            };
            
            _testOutputHelper.WriteLine($"Initial Grocery List (ID: {groceryList.GroceryListId}):");
            foreach (var item in groceryList.Ingredients)
            {
                _testOutputHelper.WriteLine($"- Ingredient: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            _mockAccountRepository.Setup(repo => repo.ReadAccount(accountId)).Returns(account);
            _mockMealPlannerRepository.Setup(repo => repo.ReadMealPlannerById(accountId)).Returns(mealPlanner);
            _mockGroceryRepository.Setup(repo => repo.ReadGroceryListById(accountId)).Returns(groceryList);
            _mockGroceryRepository.Setup(repo => repo.ReadGroceryListById(groceryList.GroceryListId)).Returns(groceryList);

            var addItemDto = new ItemQuantityDto
            {
                Ingredient = new IngredientDto
                {
                    IngredientId = ingredient.IngredientId,
                    IngredientName = ingredient.IngredientName,
                    Measurement = ingredient.Measurement
                },
                Quantity = 2
            };

            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto);
            
            _testOutputHelper.WriteLine($"Updated Grocery List (ID: {groceryList.GroceryListId}):");
            foreach (var item in groceryList.Ingredients)
            {
                _testOutputHelper.WriteLine($"- Ingredient: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            _testOutputHelper.WriteLine($"Test Grocery List (ID: {groceryList.GroceryListId}):");
            foreach (var item in groceryList.Items)
            {
                _testOutputHelper.WriteLine($"- Item: {item.Ingredient.IngredientName}, Quantity: {item.Quantity}");
            }
            
            var updatedGroceryList = groceryList.Ingredients.FirstOrDefault(i => i.Ingredient.IngredientId == ingredient.IngredientId);
            Assert.NotNull(updatedGroceryList);
            Assert.Equal(3, updatedGroceryList.Quantity);
        }
    }
}