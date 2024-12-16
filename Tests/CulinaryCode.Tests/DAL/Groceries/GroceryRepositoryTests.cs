using CulinaryCode.Tests.util;
using CulinaryCode.Tests.Util;
using DAL.EF;
using DAL.Groceries;
using DAL.Recipes;
using DOM.MealPlanning;
using DOM.Results;
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
    public async Task ReadGroceryListById_GroceryListExists_ReturnsGroceryList()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        _dbContext.GroceryLists.Add(groceryList);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groceryRepository.ReadGroceryListByIdNoTracking(groceryList.GroceryListId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(groceryList.GroceryListId, result.Value!.GroceryListId);
        Assert.Equal(groceryList.Account.AccountId, result.Value.Account.AccountId);
    }

    [Fact]
    public async Task ReadItemQuantityById_ItemQuantityExists_ReturnsItemQuantity()
    {
        // Arrange
        var itemQuantity = GroceryUtil.CreateItemQuantity();
        _dbContext.ItemQuantities.Add(itemQuantity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groceryRepository.ReadItemQuantityById(itemQuantity.ItemQuantityId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(itemQuantity.ItemQuantityId, result.Value!.ItemQuantityId);
        Assert.Equal(itemQuantity.Quantity, result.Value.Quantity);
    }

    [Fact]
    public async Task ReadGroceryListByAccountId_AccountHasGroceryList_ReturnsGroceryList()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        _dbContext.GroceryLists.Add(groceryList);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _groceryRepository.ReadGroceryListByAccountId(groceryList.Account.AccountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(groceryList.GroceryListId, result.Value!.GroceryListId);
        Assert.Equal(groceryList.Account.AccountId, result.Value.Account.AccountId);
    }

    [Fact]
    public async Task ReadPossibleGroceryItemByNameAndMeasurement_ItemExists_ReturnsGroceryItem()
    {
        // Arrange
        var groceryItem = GroceryUtil.CreateGroceryItem();
        _dbContext.GroceryItems.Add(groceryItem);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await 
            _groceryRepository.ReadGroceryItemByNameAndMeasurement(groceryItem.GroceryItemName,
                groceryItem.Measurement);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(groceryItem.GroceryItemName, result.Value!.GroceryItemName);
        Assert.Equal(groceryItem.Measurement, result.Value.Measurement);
    }

    [Fact]
    public async Task CreateGroceryList_ValidGroceryList_AddsToDatabase()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();

        // Act
        await _groceryRepository.CreateGroceryList(groceryList);

        // Assert
        var result = await _dbContext.GroceryLists.FindAsync(groceryList.GroceryListId);
        Assert.NotNull(result);
        Assert.Equal(groceryList.GroceryListId, result.GroceryListId);
    }

    [Fact]
    public async Task AddGroceryListItem_ValidItem_AddsToGroceryList()
    {
        // Arrange
        var groceryList = GroceryUtil.CreateGroceryList();
        var itemQuantity = GroceryUtil.CreateItemQuantity();
        _dbContext.GroceryLists.Add(groceryList);
        await _dbContext.SaveChangesAsync();

        // Act
        await _groceryRepository.AddGroceryListItem(groceryList, itemQuantity);

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
        await _dbContext.SaveChangesAsync();

        // Act
        await _groceryRepository.DeleteItemQuantityByUserId(groceryList.Account.AccountId, itemQuantity.ItemQuantityId);

        // Assert
        var result = await _dbContext.ItemQuantities.FindAsync(itemQuantity.ItemQuantityId);
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadGroceryListById_GroceryListDoesNotExist_ThrowsGroceryListNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act 
        var result = await _groceryRepository.ReadGroceryListByIdNoTracking(nonExistentId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task ReadItemQuantityById_ItemQuantityDoesNotExist_ThrowsItemQuantityNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act 
        var result = await _groceryRepository.ReadItemQuantityById(nonExistentId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task ReadGroceryListByAccountId_AccountDoesNotHaveGroceryList_ThrowsGroceryListNotFoundException()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();

        // Act
        var result = await _groceryRepository.ReadGroceryListByAccountId(nonExistentAccountId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
}