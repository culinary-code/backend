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
    
    [Fact]
    public void ReadIngredientById_IngredientExists_ReturnsIngredient()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Test Ingredient",
            Measurement = MeasurementType.Gram
        };
        _dbContext.Ingredients.Add(ingredient);
        _dbContext.SaveChanges();
        
        // Act
        var result = _ingredientRepository.ReadIngredientById(ingredient.IngredientId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
    }
    
    [Fact]
    public void ReadIngredientById_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        
        // Act & Assert
        Assert.Throws<IngredientNotFoundException>(() => _ingredientRepository.ReadIngredientById(ingredientId));
    }
    
    [Fact]
    public void ReadIngredientByName_IngredientExists_ReturnsIngredient()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Test Ingredient",
            Measurement = MeasurementType.Gram
        };
        _dbContext.Ingredients.Add(ingredient);
        _dbContext.SaveChanges();
        
        // Act
        var result = _ingredientRepository.ReadIngredientByName(ingredient.IngredientName);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
    }
    
    [Fact]
    public void ReadIngredientByName_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientName = "Test Ingredient";
        
        // Act & Assert
        Assert.Throws<IngredientNotFoundException>(() => _ingredientRepository.ReadIngredientByName(ingredientName));
    }
    
    [Fact]
    public void ReadIngredientByNameAndMeasurementType_IngredientExists_ReturnsIngredient()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Test Ingredient",
            Measurement = MeasurementType.Gram
        };
        _dbContext.Ingredients.Add(ingredient);
        _dbContext.SaveChanges();
        
        // Act
        var result = _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredient.IngredientName, ingredient.Measurement);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
        Assert.Equal(ingredient.Measurement, result.Measurement);
    }
    
    [Fact]
    public void ReadIngredientByNameAndMeasurementType_IngredientDoesNotExist_ThrowsIngredientNotFoundException()
    {
        // Arrange
        var ingredientName = "Test Ingredient";
        var measurementType = MeasurementType.Gram;
        
        // Act & Assert
        Assert.Throws<IngredientNotFoundException>(() => _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredientName, measurementType));
    }
    
    [Fact]
    public void CreateIngredient_IngredientDoesNotExist_CreatesIngredient()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Test Ingredient",
            Measurement = MeasurementType.Gram
        };
        
        // Act
        _ingredientRepository.CreateIngredient(ingredient);
        
        // Assert
        var result = _dbContext.Ingredients.Find(ingredient.IngredientId);
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
        Assert.Equal(ingredient.Measurement, result.Measurement);
    }
    
    [Fact]
    public void UpdateIngredient_IngredientExists_UpdatesIngredient()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            IngredientName = "Test Ingredient",
            Measurement = MeasurementType.Gram
        };
        _dbContext.Ingredients.Add(ingredient);
        _dbContext.SaveChanges();
        
        // Act
        ingredient.IngredientName = "Updated Ingredient";
        _ingredientRepository.UpdateIngredient(ingredient);
        
        // Assert
        var result = _dbContext.Ingredients.Find(ingredient.IngredientId);
        Assert.NotNull(result);
        Assert.Equal(ingredient.IngredientId, result.IngredientId);
        Assert.Equal(ingredient.IngredientName, result.IngredientName);
        Assert.Equal(ingredient.Measurement, result.Measurement);
    }
}