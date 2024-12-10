using System;
using CulinaryCode.Tests.util;
using CulinaryCode.Tests.Util;
using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Recipes;

public class RecipeRepositoryTests : IClassFixture<TestPostgresContainerFixture>, IAsyncLifetime
{
    private CulinaryCodeDbContext _dbContext;
    private IRecipeRepository _recipeRepository;
    private readonly TestPostgresContainerFixture _fixture;

    public RecipeRepositoryTests(TestPostgresContainerFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _recipeRepository = new RecipeRepository(_dbContext);
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
    public async Task ReadRecipeById_RecipeExists_ReturnsRecipe()
    {
        // Arrange
        var recipe = RecipeUtil.CreateRecipe();
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.ReadRecipeWithRelatedInformationByIdNoTracking(recipe.RecipeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(recipe.RecipeId, result.Value!.RecipeId);
        Assert.Equal(recipe.RecipeName, result.Value.RecipeName);
    }

    [Fact]
    public async Task ReadRecipeById_RecipeDoesNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var recipeId = Guid.NewGuid();

        // Act 
        var result = await _recipeRepository.ReadRecipeWithRelatedInformationByIdNoTracking(recipeId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task ReadRecipeByName_RecipeExists_ReturnsRecipe()
    {
        // Arrange
        var recipe = RecipeUtil.CreateRecipe();
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.ReadRecipeByNameNoTracking(recipe.RecipeName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(recipe.RecipeId, result.Value!.RecipeId);
        Assert.Equal(recipe.RecipeName, result.Value.RecipeName);
    }

    [Fact]
    public async Task ReadRecipeByName_RecipeDoesNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var recipeName = "Test Recipe";

        // Act 
        var result = await _recipeRepository.ReadRecipeByNameNoTracking(recipeName);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task ReadRecipesCollectionByName_RecipesExist_ReturnsCollection()
    {
        // Arrange
        var recipe1 = RecipeUtil.CreateRecipe("Test Recipe 1");
        var recipe2 = RecipeUtil.CreateRecipe("Test Recipe 2");
        _dbContext.Recipes.Add(recipe1);
        _dbContext.Recipes.Add(recipe2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.ReadRecipesCollectionByNameNoTracking("Test Recipe");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task ReadRecipesCollectionByName_RecipesDoNotExist_ThrowsRecipeNotFoundException()
    {
        // Arrange
        var recipeName = "Test Recipe";

        // Act 
        var result = await _recipeRepository.ReadRecipesCollectionByNameNoTracking(recipeName);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task CreateRecipe_ValidRecipe_CreatesRecipe()
    {
        // Arrange
        var recipe = RecipeUtil.CreateRecipe();

        // Act
        await _recipeRepository.CreateRecipe(recipe);

        // Assert
        var result = await _dbContext.Recipes.FindAsync(recipe.RecipeId);
        Assert.NotNull(result);
        Assert.Equal(recipe.RecipeId, result.RecipeId);
        Assert.Equal(recipe.RecipeName, result.RecipeName);
    }

    [Fact]
    public async Task UpdateRecipe_ValidRecipe_UpdatesRecipe()
    {
        // Arrange
        var recipe = RecipeUtil.CreateRecipe();
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        recipe.RecipeName = "Updated Recipe";
        await _recipeRepository.UpdateRecipe(recipe);

        // Assert
        var result = await _dbContext.Recipes.FindAsync(recipe.RecipeId);
        Assert.NotNull(result);
        Assert.Equal(recipe.RecipeId, result.RecipeId);
        Assert.Equal(recipe.RecipeName, result.RecipeName);
    }


    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByName_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = RecipeUtil.CreateRecipe("Spaghetti Bolognese");
        var recipe2 = RecipeUtil.CreateRecipe("Chicken Curry");
        var recipe3 = RecipeUtil.CreateRecipe("Spaghetti Carbonara");
        _dbContext.Recipes.AddRange(recipe1, recipe2, recipe3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("Spaghetti", Difficulty.NotAvailable,
            RecipeType.NotAvailable, 0, new List<string>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, r => r.RecipeName == "Spaghetti Bolognese");
        Assert.Contains(result.Value, r => r.RecipeName == "Spaghetti Carbonara");
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByDifficulty_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = RecipeUtil.CreateRecipe("Easy Dish");
        recipe1.Difficulty = Difficulty.Easy;

        var recipe2 = RecipeUtil.CreateRecipe("Medium Dish");
        recipe2.Difficulty = Difficulty.Intermediate;

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.Easy, RecipeType.NotAvailable,
            0, new List<string>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Easy Dish", result.Value!.First().RecipeName);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByRecipeType_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = RecipeUtil.CreateRecipe("Soup");
        recipe1.RecipeType = RecipeType.Dinner;

        var recipe2 = RecipeUtil.CreateRecipe("Salad");
        recipe2.RecipeType = RecipeType.Breakfast;

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.NotAvailable, RecipeType.Dinner,
            0, new List<string>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Soup", result.Value!.First().RecipeName);
    }


    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByCookingTime_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = RecipeUtil.CreateRecipe(recipeName: "Quick Dish", cookTime: 15);

        var recipe2 = RecipeUtil.CreateRecipe(recipeName: "Slow Dish", cookTime: 60);

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.NotAvailable,
            RecipeType.NotAvailable, 30, new List<string>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Quick Dish", result.Value!.First().RecipeName);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByIngredients_ReturnsMatchingRecipes()
    {
        // Arrange
        var ingredient1 = IngredientUtil.CreateIngredient("Tomato");
        var ingredient2 = IngredientUtil.CreateIngredient("Basil");
        var ingredient3 = IngredientUtil.CreateIngredient("Chicken");

        _dbContext.Ingredients.AddRange(ingredient1, ingredient2, ingredient3);

        var iq1 = IngredientQuantityUtil.CreateIngredientQuantity(5, ingredient1);
        var iq2 = IngredientQuantityUtil.CreateIngredientQuantity(1, ingredient2);
        var iq3 = IngredientQuantityUtil.CreateIngredientQuantity(4, ingredient3);

        _dbContext.IngredientQuantities.AddRange(iq1, iq2, iq3);

        var recipe1 = RecipeUtil.CreateRecipe("Tomato Soup");
        recipe1.Ingredients = new List<IngredientQuantity> { iq1, iq2 };

        var recipe2 = RecipeUtil.CreateRecipe("Chicken Curry");
        recipe2.Ingredients = new List<IngredientQuantity> { iq3 };

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.NotAvailable,
            RecipeType.NotAvailable, 0, new List<string> { "Tomato", "Basil" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Tomato Soup", result.Value!.First().RecipeName);
    }


    [Fact]
    public async Task GetFilteredRecipesAsync_NoRecipesInDatabase_ReturnsEmptyList()
    {
        // Arrange
        // No recipes added to the database.

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.NotAvailable,
            RecipeType.NotAvailable, 0, new List<string>());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByNameAndCookTime_ReturnsMatchingRecipes()
    {
        // Arrange
        var ingredient1 = IngredientUtil.CreateIngredient("Tomato");
        var ingredient2 = IngredientUtil.CreateIngredient("Onion");

        _dbContext.Ingredients.AddRange(ingredient1, ingredient2);

        var iq1 = IngredientQuantityUtil.CreateIngredientQuantity(3, ingredient1);
        var iq2 = IngredientQuantityUtil.CreateIngredientQuantity(2, ingredient2);

        _dbContext.IngredientQuantities.AddRange(iq1, iq2);

        var recipe1 = RecipeUtil.CreateRecipe(recipeName: "Quick Tomato Pasta", cookTime: 20);
        recipe1.Ingredients = new List<IngredientQuantity> { iq1, iq2 };

        var recipe2 = RecipeUtil.CreateRecipe(recipeName: "Slow-Cooked Onion Soup", cookTime: 120);
        recipe2.Ingredients = new List<IngredientQuantity> { iq2 };

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("Tomato", Difficulty.NotAvailable,
            RecipeType.NotAvailable, 30, new List<string>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Quick Tomato Pasta", result.Value!.First().RecipeName);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByDifficultyAndIngredients_ReturnsMatchingRecipes()
    {
        // Arrange
        var ingredient1 = IngredientUtil.CreateIngredient("Chicken");
        var ingredient2 = IngredientUtil.CreateIngredient("Garlic");
        _dbContext.Ingredients.AddRange(ingredient1, ingredient2);

        var iq1 = IngredientQuantityUtil.CreateIngredientQuantity(2, ingredient1);
        var iq2 = IngredientQuantityUtil.CreateIngredientQuantity(1, ingredient2);
        var iq3 = IngredientQuantityUtil.CreateIngredientQuantity(2, ingredient2);

        _dbContext.IngredientQuantities.AddRange(iq1, iq2);

        var recipe1 = RecipeUtil.CreateRecipe("Garlic Chicken");
        recipe1.Difficulty = Difficulty.Intermediate;
        recipe1.Ingredients = new List<IngredientQuantity> { iq1, iq2 };

        var recipe2 = RecipeUtil.CreateRecipe("Simple Garlic Bread");
        recipe2.Difficulty = Difficulty.Easy;
        recipe2.Ingredients = new List<IngredientQuantity> { iq3 };

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.Intermediate,
            RecipeType.NotAvailable, 0, new List<string> { "Chicken", "Garlic" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Garlic Chicken", result.Value!.First().RecipeName);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByTypeAndCookTime_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 =
            RecipeUtil.CreateRecipe(recipeName: "Vegetarian Pizza", cookTime: 30, recipeType: RecipeType.Dinner);

        var recipe2 = RecipeUtil.CreateRecipe(recipeName: "Granola Bar", cookTime: 25, recipeType: RecipeType.Snack);

        var recipe3 = RecipeUtil.CreateRecipe(recipeName: "Vegan Salad", cookTime: 10, recipeType: RecipeType.Dinner);

        _dbContext.Recipes.AddRange(recipe1, recipe2, recipe3);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("", Difficulty.NotAvailable,
            RecipeType.Dinner, 25, new List<string>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Vegan Salad", result.Value!.First().RecipeName);
    }


    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByAllCriteria_ReturnsMatchingRecipes()
    {
        // Arrange
        var ingredient1 = IngredientUtil.CreateIngredient("Rice");
        var ingredient2 = IngredientUtil.CreateIngredient("Chicken");

        _dbContext.Ingredients.AddRange(ingredient1, ingredient2);

        var iq1 = IngredientQuantityUtil.CreateIngredientQuantity(1, ingredient1);
        var iq2 = IngredientQuantityUtil.CreateIngredientQuantity(2, ingredient2);
        var iq3 = IngredientQuantityUtil.CreateIngredientQuantity(2, ingredient2);

        _dbContext.IngredientQuantities.AddRange(iq1, iq2);

        var recipe1 = RecipeUtil.CreateRecipe(recipeName: "Chicken Fried Rice", cookTime: 20,
            recipeType: RecipeType.Dinner, difficulty: Difficulty.Easy);
        recipe1.Ingredients = new List<IngredientQuantity> { iq1, iq3 };

        var recipe2 = RecipeUtil.CreateRecipe(recipeName: "Granola Bar", cookTime: 20,
            recipeType: RecipeType.Snack, difficulty: Difficulty.Easy);
        recipe2.Ingredients = new List<IngredientQuantity> { iq2 };

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesNoTracking("Chicken", Difficulty.Easy,
            RecipeType.Dinner, 25, new List<string> { "Rice", "Chicken" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Chicken Fried Rice", result.Value!.First().RecipeName);
    }
    
    [Fact]
    public async Task DeleteUnusedRecipesAsync_ShouldRemoveRecipesOlderThan31Days_WithNoFavorites()
    {
        // Arrange
        var thresholdDate = DateTime.UtcNow.AddDays(-31);

        var recipe1 = RecipeUtil.CreateRecipe(recipeName: "Garlic Chicken");
        recipe1.LastUsedAt = thresholdDate.AddDays(-1);
        recipe1.FavoriteRecipes = new List<FavoriteRecipe>();

        var recipe2 = RecipeUtil.CreateRecipe(recipeName: "Garlic Chicken");
        recipe2.LastUsedAt = thresholdDate.AddDays(-10);
        recipe2.FavoriteRecipes = new List<FavoriteRecipe>();

        var recipe3 = RecipeUtil.CreateRecipe(recipeName: "Garlic Chicken");
        recipe3.LastUsedAt = thresholdDate.AddDays(-1);
        recipe3.FavoriteRecipes = new List<FavoriteRecipe> { new FavoriteRecipe() };

        var recipe4 = RecipeUtil.CreateRecipe(recipeName: "Garlic Chicken");
        recipe4.LastUsedAt = thresholdDate.AddDays(1);
        recipe4.FavoriteRecipes = new List<FavoriteRecipe>();

        await _dbContext.Recipes.AddRangeAsync(recipe1, recipe2, recipe3, recipe4);
        await _dbContext.SaveChangesAsync();

        // Act
        await _recipeRepository.DeleteUnusedRecipes();

        // Assert
        var remainingRecipes = await _dbContext.Recipes.ToListAsync();
        Assert.Equal(2, remainingRecipes.Count); // Only recipes 3 and 4 should remain
        Assert.DoesNotContain(remainingRecipes, r => r.RecipeId == recipe1.RecipeId || r.RecipeId == recipe2.RecipeId);
    }
}