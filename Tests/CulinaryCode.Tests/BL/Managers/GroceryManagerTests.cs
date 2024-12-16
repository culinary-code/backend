using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.Groceries;
using DAL.Accounts;
using DAL.Groceries;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;
using DOM.Results;
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
    private readonly Mock<IGroupRepository> _mockGroupRepository;

    public GroceryManagerTests()
    {
        _mockLogger = new Mock<ILogger<GroceryManager>>();
        _mockGroceryRepository = new Mock<IGroceryRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockIngredientRepository = new Mock<IIngredientRepository>();
        _mockGroupRepository = new Mock<IGroupRepository>();
        _mockMapper = new Mock<IMapper>();

        _groceryManager = new GroceryManager(
            _mockGroceryRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockAccountRepository.Object,
            _mockIngredientRepository.Object,
            _mockGroupRepository.Object
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
    public async Task GettingGroceryList_ShouldReturnGroceryList()
    {
        var groceryListId = Guid.NewGuid();
        var groceryList = new GroceryList { GroceryListId = groceryListId };

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByIdNoTracking(groceryListId)).ReturnsAsync(Result<GroceryList>.Success(groceryList));

        var result = await _mockGroceryRepository.Object.ReadGroceryListByIdNoTracking(groceryListId);

        Assert.True(result.IsSuccess);
        Assert.Equal(groceryListId, result.Value!.GroceryListId);
    }

    [Fact]
    public async Task ReadGroceryListByAccountId_ShouldReturnEmptyGroceryList_WhenNoItemsOrIngredients()
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
            .ReturnsAsync(Result<GroceryList>.Success(groceryList));

        var result = await _mockGroceryRepository.Object.ReadGroceryListByAccountId(accountId);

        Assert.True(result.IsSuccess);
        Assert.Equal(groceryListId, result.Value!.GroceryListId);
        Assert.Empty(result.Value.Ingredients);
        Assert.Empty(result.Value.Items);
        Assert.NotNull(account.GroceryList);
        Assert.Equal(accountId, account.AccountId);
        Assert.Equal("nis", account.Name);
    }

    [Fact]
    public async Task AddItemToGroceryList_ShouldAddItemToGroupGroceryList_WhenUserIsInGroupMode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var groupGroceryList = new GroceryList
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
                GroceryItemName = "Bread",
                Measurement = MeasurementType.Piece
            },
            Quantity = 5,
            IsIngredient = false
        };

        var account = new Account
        {
            ChosenGroupId = groupId
        };

        _mockAccountRepository.Setup(repo => repo.ReadAccount(userId)).ReturnsAsync(Result<Account>.Success(account));
        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByGroupId(groupId))
            .ReturnsAsync(Result<GroceryList>.Success(groupGroceryList));
        _mockIngredientRepository.Setup(repo => repo.ReadIngredientByNameAndMeasurementType(It.IsAny<string>(), It.IsAny<MeasurementType>()))
            .ReturnsAsync(Result<Ingredient>.Failure("", ResultFailureType.NotFound));
        _mockGroceryRepository.Setup(repo => repo.ReadGroceryItemByNameAndMeasurement(It.IsAny<string>(), It.IsAny<MeasurementType>()))
            .ReturnsAsync(Result<GroceryItem>.Failure("", ResultFailureType.NotFound));
        _mockGroceryRepository.Setup(repo => repo.UpdateGroceryList(groupGroceryList))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Act
        await _groceryManager.AddItemToGroceryList(userId, newItemDto);

        // Assert
        var addedItem = groupGroceryList.Items.FirstOrDefault(i => i.GroceryItem.GroceryItemName == "Bread");
        Assert.NotNull(addedItem);
        Assert.Equal(5, addedItem.Quantity);
    }
    
    [Fact]
    public async Task AddItemToGroceryList_ShouldReturnFailure_WhenGroupGroceryListNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var newItemDto = new ItemQuantityDto
        {
            ItemQuantityId = Guid.Empty,
            GroceryItem = new GroceryItemDto
            {
                GroceryItemName = "Juice",
                Measurement = MeasurementType.Litre
            },
            Quantity = 3,
            IsIngredient = false
        };

        var account = new Account
        {
            ChosenGroupId = groupId
        };

        _mockAccountRepository.Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(account));
        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByGroupId(groupId))
            .ReturnsAsync(Result<GroceryList>.Failure("Group grocery list not found", ResultFailureType.NotFound));

        // Act
        var result = await _groceryManager.AddItemToGroceryList(userId, newItemDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal("Group grocery list not found", result.ErrorMessage);
    }

    
    [Fact]
    public async Task AddItemToGroceryList_ShouldAddNewItem_WhenNewItemIsProvided()
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

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).ReturnsAsync(Result<GroceryList>.Success(groceryList));
        _mockIngredientRepository.Setup(repo => repo.ReadIngredientByNameAndMeasurementType(It.IsAny<string>(), It.IsAny<MeasurementType>())).ReturnsAsync(Result<Ingredient>.Failure("", ResultFailureType.NotFound));
        _mockGroceryRepository.Setup(repo => repo.ReadGroceryItemByNameAndMeasurement(It.IsAny<string>(), It.IsAny<MeasurementType>())).ReturnsAsync(Result<GroceryItem>.Failure("", ResultFailureType.NotFound));
        _mockGroceryRepository
            .Setup(repo => repo.UpdateGroceryList(groceryList))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));
        
        // Act
        await _groceryManager.AddItemToGroceryList(userId, newItemDto);

        // Assert
        var addedItem = groceryList.Items.FirstOrDefault(i => i.GroceryItem.GroceryItemName == "Milk");
        Assert.NotNull(addedItem);
        Assert.Equal(2, addedItem.Quantity);
    }

    [Fact]
    public async Task AddItemToGroceryList_ShouldAddIngredientQuantity_WhenIngredientIsFound()
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
            .ReturnsAsync(Result<GroceryList>.Success(groceryList));
        
        _mockGroceryRepository
            .Setup(repo => repo.UpdateGroceryList(groceryList))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        _mockIngredientRepository
            .Setup(repo => repo.ReadIngredientByNameAndMeasurementType("Tomato", MeasurementType.Kilogram))
            .ReturnsAsync(Result<Ingredient>.Success(existingIngredient));
        
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));

        // Act
        await _groceryManager.AddItemToGroceryList(userId, newListItem);

        // Assert
        Assert.Single(groceryList.Ingredients);
        var addedIngredient = groceryList.Ingredients.First();
        Assert.Equal(existingIngredient, addedIngredient.Ingredient);
        Assert.Equal(2, addedIngredient.Quantity);
    }


    [Fact]
    public async Task AddItemToGroceryList_ShouldAddItemQuantity_WhenNoIngredientButGroceryItemIsFound()
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
            .ReturnsAsync(Result<GroceryList>.Success(groceryList));

        _mockIngredientRepository
            .Setup(repo => repo.ReadIngredientByNameAndMeasurementType("Milk", MeasurementType.Litre))
            .ReturnsAsync(Result<Ingredient>.Failure("", ResultFailureType.NotFound));
        
        _mockGroceryRepository
            .Setup(repo => repo.UpdateGroceryList(groceryList))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        _mockGroceryRepository
            .Setup(repo => repo.ReadGroceryItemByNameAndMeasurement("Milk", MeasurementType.Litre))
            .ReturnsAsync(Result<GroceryItem>.Success(existingGroceryItem));
        
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));

        // Act
        await _groceryManager.AddItemToGroceryList(userId, newListItem);

        // Assert
        Assert.Single(groceryList.Items);
        var addedItem = groceryList.Items.First();
        Assert.Equal(existingGroceryItem, addedItem.GroceryItem);
        Assert.Equal(3, addedItem.Quantity);
    }


    [Fact]
    public async Task AddItemToGroceryList_ShouldUpdateExistingItem_WhenItemIdIsProvided()
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

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).ReturnsAsync(Result<GroceryList>.Success(groceryList));
        _mockGroceryRepository.Setup(repo => repo.ReadItemQuantityById(existingItem.GroceryItem.GroceryItemId))
            .ReturnsAsync(Result<ItemQuantity>.Success(existingItem));
        _mockGroceryRepository
            .Setup(repo => repo.UpdateGroceryList(groceryList))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));

        // Act
        await _groceryManager.AddItemToGroceryList(userId, updateItemDto);

        // Assert
        Assert.Equal(3, existingItem.Quantity);
    }

    [Fact]
    public async Task AddItemToGroceryList_ShouldUpdateExistingIngredient_WhenIngredientIdIsProvided()
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

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).ReturnsAsync(Result<GroceryList>.Success(groceryList));
        _mockIngredientRepository.Setup(repo =>
                repo.ReadIngredientQuantityById(existingIngredient.Ingredient.IngredientId))
            .ReturnsAsync(Result<IngredientQuantity>.Success(existingIngredient));
        _mockGroceryRepository
            .Setup(repo => repo.UpdateGroceryList(groceryList))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));

        // Act
        await _groceryManager.AddItemToGroceryList(userId, updateIngredientDto);

        // Assert
        Assert.Equal(5, existingIngredient.Quantity);
    }

    [Fact]
    public async Task AddItemToGroceryList_ShouldThrowException_WhenNewItemHasNullGroceryItem()
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

        _mockGroceryRepository.Setup(repo => repo.ReadGroceryListByAccountId(userId)).ReturnsAsync(Result<GroceryList>.Success(groceryList));
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));
        
        // Act
        var result = await _groceryManager.AddItemToGroceryList(userId, newItemDto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Error, result.FailureType);
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
        
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));

        // Act
        await _groceryManager.RemoveItemFromGroceryList(userId, removeItemDto);

        // Assert
        _mockIngredientRepository.Verify(repo => repo.DeleteIngredientQuantityByUserId(userId, ingredientId), Times.Once);
        _mockGroceryRepository.Verify(repo => repo.DeleteItemQuantityByUserId(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
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
        
        _mockAccountRepository
            .Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(new Account()));

        // Act
        await _groceryManager.RemoveItemFromGroceryList(userId, removeItemDto);

        // Assert
        _mockGroceryRepository.Verify(repo => repo.DeleteItemQuantityByUserId(userId, itemId), Times.Once);
        _mockIngredientRepository.Verify(repo => repo.DeleteIngredientQuantityByUserId(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemFromGroceryList_ShouldRemoveItemFromGroup_WhenUserIsInGroupMode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var removeItemDto = new ItemQuantityDto
        {
            ItemQuantityId = itemId,
            IsIngredient = false
        };
        var account = new Account
        {
            ChosenGroupId = groupId
        };

        _mockAccountRepository.Setup(repo => repo.ReadAccount(userId))
            .ReturnsAsync(Result<Account>.Success(account));
        _mockGroceryRepository.Setup(repo => repo.DeleteItemQuantityByGroupId(groupId, itemId))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Act
        await _groceryManager.RemoveItemFromGroceryList(userId, removeItemDto);

        // Assert
        _mockGroceryRepository.Verify(repo => repo.DeleteItemQuantityByGroupId(groupId, itemId), Times.Once);
    }

}