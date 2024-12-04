using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using DAL.Accounts;
using DAL.Groceries;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;
using Moq;

namespace CulinaryCode.Tests.BL.Managers;

public class GroceryManagerTests
{
    private readonly Mock<IGroceryRepository> _mockGroceryRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IIngredientRepository> _mockIngredientRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GroceryManager _groceryManager;
    private readonly Mock<ILogger<GroceryManager>> _mockLogger;

    public GroceryManagerTests()
    {
        _mockLogger = new Mock<ILogger<GroceryManager>>();
        _mockGroceryRepository = new Mock<IGroceryRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockIngredientRepository = new Mock<IIngredientRepository>();
        _mockMapper = new Mock<IMapper>();

        _groceryManager = new GroceryManager(
            _mockGroceryRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockAccountRepository.Object,
            _mockIngredientRepository.Object
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
    public void AddItemToGroceryList_ShouldAddNewItem_WhenNewItemIsProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groceryList = new GroceryList
        {
            GroceryListId = Guid.NewGuid(),
            Ingredients = new List<IngredientQuantity>(),
            Items = new List<ItemQuantity>()
        };
        var newItemDto = new ItemQuantityDto
        {
            ItemQuantityId = Guid.Empty,
            GroceryItem = new GroceryItemDto
            {
                GroceryItemId = Guid.NewGuid(),
                GroceryItemName = "Milk",
                Measurement = MeasurementType.Litre
            },
            Quantity = 2,
            IsIngredient = false
        };

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).Returns(groceryList);

        // Act
        _groceryManager.AddItemToGroceryList(userId, newItemDto);

        // Assert
        var addedItem = groceryList.Items.FirstOrDefault(i => i.GroceryItem.GroceryItemName == "Milk");
        Assert.NotNull(addedItem);
        Assert.Equal(2, addedItem.Quantity);
    }

    [Fact]
    public void AddItemToGroceryList_ShouldAddIngredientQuantity_WhenIngredientIsFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groceryList = new GroceryList
        {
            Ingredients = new List<IngredientQuantity>(),
            Items = new List<ItemQuantity>()
        };
        var existingIngredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Tomato",
            Measurement = MeasurementType.Kilogram
        };
        var newListItem = new ItemQuantityDto
        {
            ItemQuantityId = Guid.Empty,
            GroceryItem = new GroceryItemDto
            {
                GroceryItemName = "Tomato",
                Measurement = MeasurementType.Kilogram
            },
            Quantity = 2,
            IsIngredient = true
        };

        _mockGroceryRepository
            .Setup(repo => repo.ReadGroceryListByAccountId(userId))
            .Returns(groceryList);

        _mockIngredientRepository
            .Setup(repo => repo.ReadPossibleIngredientByNameAndMeasurement("Tomato", MeasurementType.Kilogram))
            .Returns(existingIngredient);

        // Act
        _groceryManager.AddItemToGroceryList(userId, newListItem);

        // Assert
        Assert.Single(groceryList.Ingredients);
        var addedIngredient = groceryList.Ingredients.First();
        Assert.Equal(existingIngredient, addedIngredient.Ingredient);
        Assert.Equal(2, addedIngredient.Quantity);
    }


    [Fact]
    public void AddItemToGroceryList_ShouldAddItemQuantity_WhenNoIngredientButGroceryItemIsFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groceryList = new GroceryList
        {
            Ingredients = new List<IngredientQuantity>(),
            Items = new List<ItemQuantity>()
        };
        var existingGroceryItem = new GroceryItem
        {
            GroceryItemId = Guid.NewGuid(),
            GroceryItemName = "Milk",
            Measurement = MeasurementType.Litre
        };
        var newListItem = new ItemQuantityDto
        {
            ItemQuantityId = Guid.Empty,
            GroceryItem = new GroceryItemDto
            {
                GroceryItemName = "Milk",
                Measurement = MeasurementType.Litre
            },
            Quantity = 3,
            IsIngredient = false
        };

        _mockGroceryRepository
            .Setup(repo => repo.ReadGroceryListByAccountId(userId))
            .Returns(groceryList);

        _mockIngredientRepository
            .Setup(repo => repo.ReadPossibleIngredientByNameAndMeasurement("Milk", MeasurementType.Litre))
            .Returns((Ingredient)null);

        _mockGroceryRepository
            .Setup(repo => repo.ReadPossibleGroceryItemByNameAndMeasurement("Milk", MeasurementType.Litre))
            .Returns(existingGroceryItem);

        // Act
        _groceryManager.AddItemToGroceryList(userId, newListItem);

        // Assert
        Assert.Single(groceryList.Items);
        var addedItem = groceryList.Items.First();
        Assert.Equal(existingGroceryItem, addedItem.GroceryItem);
        Assert.Equal(3, addedItem.Quantity);
    }


    [Fact]
    public void AddItemToGroceryList_ShouldUpdateExistingItem_WhenItemIdIsProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingItem = new ItemQuantity
        {
            GroceryItem = new GroceryItem
            {
                GroceryItemId = Guid.NewGuid(),
                GroceryItemName = "Sugar"
            },
            Quantity = 1
        };
        var groceryList = new GroceryList
        {
            GroceryListId = Guid.NewGuid(),
            Items = new List<ItemQuantity> { existingItem },
            Ingredients = new List<IngredientQuantity>()
        };

        var updateItemDto = new ItemQuantityDto
        {
            ItemQuantityId = existingItem.GroceryItem.GroceryItemId,
            Quantity = 3,
            IsIngredient = false
        };

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).Returns(groceryList);
        _mockGroceryRepository.Setup(repo => repo.ReadItemQuantityById(existingItem.GroceryItem.GroceryItemId))
            .Returns(existingItem);

        // Act
        _groceryManager.AddItemToGroceryList(userId, updateItemDto);

        // Assert
        Assert.Equal(3, existingItem.Quantity);
    }

    [Fact]
    public void AddItemToGroceryList_ShouldUpdateExistingIngredient_WhenIngredientIdIsProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingIngredient = new IngredientQuantity
        {
            Ingredient = new Ingredient
            {
                IngredientId = Guid.NewGuid(),
                IngredientName = "Salt"
            },
            Quantity = 1
        };
        var groceryList = new GroceryList
        {
            GroceryListId = Guid.NewGuid(),
            Items = new List<ItemQuantity>(),
            Ingredients = new List<IngredientQuantity> { existingIngredient }
        };

        var updateIngredientDto = new ItemQuantityDto
        {
            ItemQuantityId = existingIngredient.Ingredient.IngredientId,
            Quantity = 5,
            IsIngredient = true
        };

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).Returns(groceryList);
        _mockIngredientRepository.Setup(repo =>
                repo.ReadIngredientQuantityById(existingIngredient.Ingredient.IngredientId))
            .Returns(existingIngredient);

        // Act
        _groceryManager.AddItemToGroceryList(userId, updateIngredientDto);

        // Assert
        Assert.Equal(5, existingIngredient.Quantity);
    }

    [Fact]
    public void AddItemToGroceryList_ShouldThrowException_WhenItemQuantityDtoIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<GroceryListItemNotFoundException>(() => _groceryManager.AddItemToGroceryList(userId, null));
    }

    [Fact]
    public void AddItemToGroceryList_ShouldThrowException_WhenNewItemHasNullGroceryItem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groceryList = new GroceryList
        {
            GroceryListId = Guid.NewGuid(),
            Items = new List<ItemQuantity>(),
            Ingredients = new List<IngredientQuantity>()
        };
        var newItemDto = new ItemQuantityDto
        {
            ItemQuantityId = Guid.Empty,
            GroceryItem = null,
            Quantity = 2,
            IsIngredient = false
        };

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).Returns(groceryList);

        // Act & Assert
        Assert.Throws<GroceryListItemNotFoundException>(() =>
            _groceryManager.AddItemToGroceryList(userId, newItemDto));
    }
    
    [Fact]
    public async Task RemoveItemFromGroceryList_ShouldRemoveIngredientQuantity_WhenIsIngredientIsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        var removeItemDto = new ItemQuantityDto
        {
            ItemQuantityId = ingredientId,
            IsIngredient = true
        };

        // Act
        await _groceryManager.RemoveItemFromGroceryList(userId, removeItemDto);

        // Assert
        _mockIngredientRepository.Verify(repo => repo.DeleteIngredientQuantity(userId, ingredientId), Times.Once);
        _mockGroceryRepository.Verify(repo => repo.DeleteItemQuantity(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemFromGroceryList_ShouldRemoveItemQuantity_WhenIsIngredientIsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var removeItemDto = new ItemQuantityDto
        {
            ItemQuantityId = itemId,
            IsIngredient = false
        };

        // Act
        await _groceryManager.RemoveItemFromGroceryList(userId, removeItemDto);

        // Assert
        _mockGroceryRepository.Verify(repo => repo.DeleteItemQuantity(userId, itemId), Times.Once);
        _mockIngredientRepository.Verify(repo => repo.DeleteIngredientQuantity(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

}