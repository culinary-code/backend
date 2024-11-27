using AutoMapper;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using DAL.Accounts;
using DAL.Groceries;
using DAL.MealPlanning;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace BL.Testing
{
    public class GroceryManagerTests
    {
        private readonly Mock<IGroceryRepository> _mockGroceryRepository;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<IMealPlannerRepository> _mockMealPlannerRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GroceryManager _groceryManager;
        private readonly Mock<ILogger<GroceryManager>> _mockLogger;

        public GroceryManagerTests()
        {
            _mockLogger = new Mock<ILogger<GroceryManager>>();
            _mockGroceryRepository = new Mock<IGroceryRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockMapper = new Mock<IMapper>();

            _groceryManager = new GroceryManager(
                _mockGroceryRepository.Object,
                _mockMapper.Object,
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
            var account = CreateSampleAccount(accountId);

            var groceryList = new GroceryList()
            {
                GroceryListId = groceryListId
            };
            
            account.GroceryList = groceryList;

            _mockAccountRepository
                .Setup(repo => repo.CreateAccount(It.IsAny<Account>()))
                .Callback<Account>(createdAccount =>
                {
                    Assert.NotNull(createdAccount.GroceryList);

                    groceryList = createdAccount.GroceryList;
                    Assert.NotNull(groceryList);
                    Assert.Empty(groceryList.Ingredients);
                    Assert.Empty(groceryList.Items);

                    Assert.Equal(groceryListId, groceryList.GroceryListId);
                });

            _mockAccountRepository.Object.CreateAccount(account);
            _mockAccountRepository.Verify(repo => repo.CreateAccount(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public void GettingGroceryList_ShouldReturnGroceryList()
        {
            var groceryListId = Guid.NewGuid();
            var groceryList = new GroceryList { GroceryListId = groceryListId };

            _mockGroceryRepository.Setup(repo => repo.ReadGroceryListById(groceryListId)).Returns(groceryList);

            var result = _mockGroceryRepository.Object.ReadGroceryListById(groceryListId);

            Assert.NotNull(result);
            Assert.Equal(groceryListId, result.GroceryListId);
        }
        
        [Fact]
        public void ReadGroceryListByAccountId_ShouldReturnEmptyGroceryList_WhenNoItemsOrIngredients()
        {
            var accountId = Guid.Parse("d1ec841b-9646-4ca7-a1ef-eda7354547f3");
            var groceryListId = Guid.NewGuid();
            var account = CreateSampleAccount(accountId);

            var groceryList = new GroceryList
            {
                GroceryListId = groceryListId,
                Ingredients = new List<IngredientQuantity>(),
                Items = new List<ItemQuantity>(),
            };
            
            account.GroceryList = groceryList;

            _mockGroceryRepository
                .Setup(repo => repo.ReadGroceryListByAccountId(accountId))
                .Returns(groceryList);

            var result = _mockGroceryRepository.Object.ReadGroceryListByAccountId(accountId);
            
            Assert.NotNull(result); 
            Assert.Equal(groceryListId, result.GroceryListId); 
            Assert.Empty(result.Ingredients);
            Assert.Empty(result.Items); 
            Assert.NotNull(account.GroceryList); 
            Assert.Equal(accountId, account.AccountId);
            Assert.Equal("nis", account.Name); 
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
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity { Ingredient = ingredient1, Quantity = 2 },
                    new IngredientQuantity { Ingredient = ingredient2, Quantity = 150 }
                },
                Items = new List<ItemQuantity>()
            };
            
            account.GroceryListId = groceryList.GroceryListId;
            
            var newIngredient = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Potato", Measurement = MeasurementType.Kilogram };
            var newIngredient2 = new Ingredient { IngredientId = ingredient1.IngredientId, IngredientName = "Carrot", Measurement = MeasurementType.Kilogram };
            var newIngredient3 = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Apple", Measurement = MeasurementType.Gram };
            var newItem = new GroceryItem { GroceryItemId = Guid.NewGuid(), GroceryItemName = "Waspoeder" };
            
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
            
            var addItemDto4 = new ItemQuantityDto
            {
                Ingredient = new IngredientDto
                {
                    IngredientId = newItem.GroceryItemId,
                    IngredientName = newItem.GroceryItemName,
                },
                Quantity = 1
            };
            
            _mockAccountRepository.Setup(repo => repo.ReadAccount(accountId)).Returns(account);
            _mockGroceryRepository.Setup(repo => repo.ReadGroceryListById(groceryList.GroceryListId)).Returns(groceryList);
            
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto);
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto2);
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto3);
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto4);
            
            var addedItem = groceryList.Items.FirstOrDefault(i => i.GroceryItem.GroceryItemId == newIngredient.IngredientId);
            
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
            var newItem = new GroceryItem { GroceryItemId = Guid.NewGuid(), GroceryItemName = "Waspoeder" };
            
            var groceryList = new GroceryList
            {
                GroceryListId = Guid.NewGuid(),
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity { Ingredient = ingredient, Quantity = 1 }
                }
            };
            
            account.GroceryListId = groceryList.GroceryListId;
            
            _mockAccountRepository.Setup(repo => repo.ReadAccount(accountId)).Returns(account);
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

            var addItemDto2 = new ItemQuantityDto
            {
                Ingredient = new IngredientDto
                {
                    IngredientId = newItem.GroceryItemId,
                    IngredientName = newItem.GroceryItemName,
                },
                Quantity = 1
            };

            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto);
            _groceryManager.AddItemToGroceryList(groceryList.GroceryListId, addItemDto2);
            
            var updatedGroceryList = groceryList.Ingredients.FirstOrDefault(i => i.Ingredient.IngredientId == ingredient.IngredientId);
            Assert.NotNull(updatedGroceryList);
            Assert.Equal(3, updatedGroceryList.Quantity);
        }
        
        [Fact]
        public async Task RemoveItemFromGroceryList_ShouldCallRepositoryMethod_WhenItemExists()
        {
            var groceryListId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            _mockGroceryRepository
                .Setup(repo => repo.DeleteItemFromGroceryList(groceryListId, itemId))
                .Returns(Task.CompletedTask);

            await _groceryManager.RemoveItemFromGroceryList(groceryListId, itemId);

            _mockGroceryRepository.Verify(
                repo => repo.DeleteItemFromGroceryList(groceryListId, itemId),
                Times.Once // Ensure the method was called exactly once
            );
        }
        
        [Fact]
        public async Task RemoveItemFromGroceryList_ShouldThrowException_WhenGroceryListNotFound()
        {
            var groceryListId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            _mockGroceryRepository
                .Setup(repo => repo.DeleteItemFromGroceryList(groceryListId, itemId))
                .ThrowsAsync(new GroceryListNotFoundException("Grocery list not found."));

            var exception = await Assert.ThrowsAsync<GroceryListNotFoundException>(() =>
                _groceryManager.RemoveItemFromGroceryList(groceryListId, itemId)
            );

            Assert.Equal("Grocery list not found.", exception.Message);

            _mockGroceryRepository.Verify(
                repo => repo.DeleteItemFromGroceryList(groceryListId, itemId),
                Times.Once
            );
        }

        [Fact]
        public async Task RemoveItemFromGroceryList_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            var groceryListId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            _mockGroceryRepository
                .Setup(repo => repo.DeleteItemFromGroceryList(groceryListId, itemId))
                .ThrowsAsync(new Exception("Unexpected error."));

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _groceryManager.RemoveItemFromGroceryList(groceryListId, itemId)
            );

            Assert.Equal("Unexpected error.", exception.Message);

            _mockGroceryRepository.Verify(
                repo => repo.DeleteItemFromGroceryList(groceryListId, itemId),
                Times.Once
            );
        }

    }
}