using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Recipes;

public class RecipeRepositoryTests
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly IRecipeRepository _recipeRepository;
    
    public RecipeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            // force unique database for each test so data is isolated
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;
        
        _dbContext = new CulinaryCodeDbContext(options);
        _recipeRepository = new RecipeRepository(_dbContext);
    }
    
    private Recipe CreateRecipe(string recipeName = "Test Recipe")
    {
        return new Recipe
        {
            RecipeId = Guid.NewGuid(),
            RecipeName = recipeName,
            Ingredients = new List<IngredientQuantity>(),
            Instructions = new List<InstructionStep>(),
            Reviews = new List<Review>(),
            Preferences = new List<Preference>()
        };
    }
    
    [Fact]
    public void ReadRecipeById_RecipeExists_ReturnsRecipe()
    {
        // Arrange
        var recipe = CreateRecipe();
        _dbContext.Recipes.Add(recipe);
        _dbContext.SaveChanges();
        
        // Act
        var result = _recipeRepository.ReadRecipeById(recipe.RecipeId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipe.RecipeId, result.RecipeId);
        Assert.Equal(recipe.RecipeName, result.RecipeName);
    }
    
    [Fact]
    public void ReadRecipeById_RecipeDoesNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        
        // Act & Assert
        Assert.Throws<RecipeNotFoundException>(() => _recipeRepository.ReadRecipeById(recipeId));
    }
    
    [Fact]
    public void ReadRecipeByName_RecipeExists_ReturnsRecipe()
    {
        // Arrange
        var recipe = CreateRecipe();
        _dbContext.Recipes.Add(recipe);
        _dbContext.SaveChanges();
        
        // Act
        var result = _recipeRepository.ReadRecipeByName(recipe.RecipeName);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipe.RecipeId, result.RecipeId);
        Assert.Equal(recipe.RecipeName, result.RecipeName);
    }
    
    [Fact]
    public void ReadRecipeByName_RecipeDoesNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var recipeName = "Test Recipe";
        
        // Act & Assert
        Assert.Throws<RecipeNotFoundException>(() => _recipeRepository.ReadRecipeByName(recipeName));
    }
    
    [Fact]
    public void ReadRecipesCollectionByName_RecipesExist_ReturnsCollection()
    {
        // Arrange
        var recipe1 = CreateRecipe("Test Recipe 1");
        var recipe2 = CreateRecipe("Test Recipe 2");
        _dbContext.Recipes.Add(recipe1);
        _dbContext.Recipes.Add(recipe2);
        _dbContext.SaveChanges();
        
        // Act
        var result = _recipeRepository.ReadRecipesCollectionByName("Test Recipe");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public void ReadRecipesCollectionByName_RecipesDoNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var recipeName = "Test Recipe";
        
        // Act & Assert
        Assert.Throws<RecipeNotFoundException>(() => _recipeRepository.ReadRecipesCollectionByName(recipeName));
    }
    
    [Fact]
    public void CreateRecipe_ValidRecipe_CreatesRecipe()
    {
        // Arrange
        var recipe = CreateRecipe();
        
        // Act
        _recipeRepository.CreateRecipe(recipe);
        
        // Assert
        var result = _dbContext.Recipes.Find(recipe.RecipeId);
        Assert.NotNull(result);
        Assert.Equal(recipe.RecipeId, result.RecipeId);
        Assert.Equal(recipe.RecipeName, result.RecipeName);
    }
    
    [Fact]
    public void UpdateRecipe_ValidRecipe_UpdatesRecipe()
    {
        // Arrange
        var recipe = CreateRecipe();
        _dbContext.Recipes.Add(recipe);
        _dbContext.SaveChanges();
        
        // Act
        recipe.RecipeName = "Updated Recipe";
        _recipeRepository.UpdateRecipe(recipe);
        
        // Assert
        var result = _dbContext.Recipes.Find(recipe.RecipeId);
        Assert.NotNull(result);
        Assert.Equal(recipe.RecipeId, result.RecipeId);
        Assert.Equal(recipe.RecipeName, result.RecipeName);
    }
}