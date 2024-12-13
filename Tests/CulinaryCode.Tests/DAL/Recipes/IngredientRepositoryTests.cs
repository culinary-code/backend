using CulinaryCode.Tests.util;
using DAL.EF;
using DAL.Recipes;
using DOM.Recipes.Ingredients;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Recipes;

public class IngredientRepositoryTests : IClassFixture<TestPostgresContainerFixture>, IAsyncLifetime
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly TestPostgresContainerFixture _fixture;
    
    public IngredientRepositoryTests(TestPostgresContainerFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _ingredientRepository = new IngredientRepository(_dbContext);
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
    
    private Ingredient CreateIngredient()
    {
        return new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Test Ingredient",
            Measurement = MeasurementType.Gram
        };
    }
    
    [Fact]
    public async Task ReadIngredientById_IngredientExists_ReturnsIngredient()
    {
        // Arrange
        var ingredient = CreateIngredient();
        _dbContext.Ingredients.Add(ingredient);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _ingredientRepository.ReadIngredientById(ingredient.IngredientId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ingredient.IngredientId, result.Value!.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.Value.IngredientName);
    }
    
    [Fact]
    public async Task ReadIngredientById_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        
        // Act
        var result = await _ingredientRepository.ReadIngredientById(ingredientId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task ReadIngredientByNameAndMeasurementType_IngredientExists_ReturnsIngredient()
    {
        // Arrange
        var ingredient = CreateIngredient();
        _dbContext.Ingredients.Add(ingredient);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredient.IngredientName, ingredient.Measurement);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ingredient.IngredientId, result.Value!.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.Value.IngredientName);
        Assert.Equal(ingredient.Measurement, result.Value.Measurement);
    }
    
    [Fact]
    public async Task ReadIngredientByNameAndMeasurementType_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientName = "Test Ingredient";
        var measurementType = MeasurementType.Gram;
        
        // Act 
        var result = await _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredientName, measurementType);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }
    
    [Fact]
    public async Task CreateIngredient_IngredientDoesNotExist_CreatesIngredient()
    {
        // Arrange
        var ingredient = CreateIngredient();
        
        // Act
        await _ingredientRepository.CreateIngredient(ingredient);
        
        // Assert
        var result = await _dbContext.Ingredients.FindAsync(ingredient.IngredientId);
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
        Assert.Equal(ingredient.Measurement, result.Measurement);
    }
}