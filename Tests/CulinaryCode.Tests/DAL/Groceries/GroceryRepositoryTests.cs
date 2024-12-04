using CulinaryCode.Tests.util;
using CulinaryCode.Tests.Util;
using DAL.EF;
using DAL.Groceries;
using DAL.Recipes;
using DOM.Exceptions;
using DOM.MealPlanning;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Groceries;

public class GroceryRepositoryTests : IClassFixture<TestPostgresContainerFixture>, IAsyncLifetime
{
    private CulinaryCodeDbContext _dbContext;
    private IGroceryRepository _groceryRepository;
    private readonly TestPostgresContainerFixture _fixture;

    public GroceryRepositoryTests(TestPostgresContainerFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _groceryRepository = new GroceryRepository(_dbContext);
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
    public void ReadGroceryListById_GroceryListExists_ReturnsGroceryList()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        _dbContext.GroceryLists.Add(groceryList);
        _dbContext.SaveChanges();

        // Act
        var result = _groceryRepository.ReadGroceryListById(groceryList.GroceryListId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groceryList.GroceryListId, result.GroceryListId);
        Assert.Equal(groceryList.Account.AccountId, result.Account.AccountId);
    }

    [Fact]
    public void ReadItemQuantityById_ItemQuantityExists_ReturnsItemQuantity()
    {
        // Arrange
        var itemQuantity = GroceryUtil.CreateItemQuantity();
        _dbContext.ItemQuantities.Add(itemQuantity);
        _dbContext.SaveChanges();

        // Act
        var result = _groceryRepository.ReadItemQuantityById(itemQuantity.ItemQuantityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemQuantity.ItemQuantityId, result.ItemQuantityId);
        Assert.Equal(itemQuantity.Quantity, result.Quantity);
    }

    [Fact]
    public void ReadGroceryListByAccountId_AccountHasGroceryList_ReturnsGroceryList()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        _dbContext.GroceryLists.Add(groceryList);
        _dbContext.SaveChanges();

        // Act
        var result = _groceryRepository.ReadGroceryListByAccountId(groceryList.Account.AccountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groceryList.GroceryListId, result.GroceryListId);
        Assert.Equal(groceryList.Account.AccountId, result.Account.AccountId);
    }

    [Fact]
    public void ReadPossibleGroceryItemByNameAndMeasurement_ItemExists_ReturnsGroceryItem()
    {
        // Arrange
        var groceryItem = GroceryUtil.CreateGroceryItem();
        _dbContext.GroceryItems.Add(groceryItem);
        _dbContext.SaveChanges();

        // Act
        var result =
            _groceryRepository.ReadPossibleGroceryItemByNameAndMeasurement(groceryItem.GroceryItemName,
                groceryItem.Measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groceryItem.GroceryItemName, result.GroceryItemName);
        Assert.Equal(groceryItem.Measurement, result.Measurement);
    }

    [Fact]
    public void CreateGroceryList_ValidGroceryList_AddsToDatabase()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();

        // Act
        _groceryRepository.CreateGroceryList(groceryList);

        // Assert
        var result = _dbContext.GroceryLists.Find(groceryList.GroceryListId);
        Assert.NotNull(result);
        Assert.Equal(groceryList.GroceryListId, result.GroceryListId);
    }

    [Fact]
    public void AddGroceryListItem_ValidItem_AddsToGroceryList()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        var itemQuantity = GroceryUtil.CreateItemQuantity();
        _dbContext.GroceryLists.Add(groceryList);
        _dbContext.SaveChanges();

        // Act
        _groceryRepository.AddGroceryListItem(groceryList, itemQuantity);

        // Assert
        var result = _dbContext.GroceryLists
            .Include(gl => gl.Items)
            .FirstOrDefault(gl => gl.GroceryListId == groceryList.GroceryListId);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(itemQuantity.ItemQuantityId, result.Items.First().ItemQuantityId);
    }

    [Fact]
    public async Task DeleteItemQuantity_ItemExists_RemovesFromDatabase()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        var itemQuantity = GroceryUtil.CreateItemQuantity();
        groceryList.Items.Add(itemQuantity);
        _dbContext.GroceryLists.Add(groceryList);
        _dbContext.SaveChanges();

        // Act
        await _groceryRepository.DeleteItemQuantity(groceryList.Account.AccountId, itemQuantity.ItemQuantityId);

        // Assert
        var result = _dbContext.ItemQuantities.Find(itemQuantity.ItemQuantityId);
        Assert.Null(result);
    }

    [Fact]
    public void ReadGroceryListById_GroceryListDoesNotExist_ThrowsGroceryListNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<GroceryListNotFoundException>(() =>
            _groceryRepository.ReadGroceryListById(nonExistentId));
        Assert.Equal("Grocery list not found!", exception.Message);
    }

    [Fact]
    public void ReadItemQuantityById_ItemQuantityDoesNotExist_ThrowsItemQuantityNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ItemQuantityNotFoundException>(() =>
            _groceryRepository.ReadItemQuantityById(nonExistentId));
        Assert.Equal($"No itemQuantity found with id {nonExistentId}", exception.Message);
    }
    
    [Fact]
    public void AddGroceryListItem_NullGroceryList_ThrowsGroceryListNotFoundException()
    {
        // Arrange
        GroceryList? groceryList = null;
        var itemQuantity = GroceryUtil.CreateItemQuantity();

        // Act & Assert
        var exception = Assert.Throws<GroceryListNotFoundException>(() =>
            _groceryRepository.AddGroceryListItem(groceryList, itemQuantity));
        Assert.Equal("Grocery list or new item cannot be null.", exception.Message);
    }
    
    [Fact]
    public void ReadGroceryListByAccountId_AccountDoesNotHaveGroceryList_ThrowsGroceryListNotFoundException()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<GroceryListNotFoundException>(() =>
            _groceryRepository.ReadGroceryListByAccountId(nonExistentAccountId));
        Assert.Equal("No grocery list found for this account.", exception.Message);
    }
}