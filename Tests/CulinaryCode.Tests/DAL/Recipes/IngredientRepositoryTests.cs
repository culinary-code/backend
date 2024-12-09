using DAL.EF;
using DAL.Recipes;
using DOM.Exceptions;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Recipes;

public class IngredientRepositoryTests
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly IIngredientRepository _ingredientRepository;
    
    public IngredientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            // force unique database for each test so data is isolated
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;
        
        _dbContext = new CulinaryCodeDbContext(options);
        _ingredientRepository = new IngredientRepository(_dbContext);
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
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
    }
    
    [Fact]
    public async Task ReadIngredientById_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        
        // Act & Assert
        await Assert.ThrowsAsync<IngredientNotFoundException>(async () => await _ingredientRepository.ReadIngredientById(ingredientId));
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
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
        Assert.Equal(ingredient.Measurement, result.Measurement);
    }
    
    [Fact]
    public async Task ReadIngredientByNameAndMeasurementType_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientName = "Test Ingredient";
        var measurementType = MeasurementType.Gram;
        
        // Act & Assert
        await Assert.ThrowsAsync<IngredientNotFoundException>(async () => await _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredientName, measurementType));
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