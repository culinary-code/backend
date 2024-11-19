using System;
using System.Collections.Generic;
using DAL.EF;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace CulinaryCode.Tests.DAL.Recipes
{
    public class RecipeRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        private CulinaryCodeDbContext _dbContext;
        private IRecipeRepository _recipeRepository;

        public RecipeRepositoryTests()
        {
            // Initialize the PostgreSQL Testcontainer
            _postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("culinarycode_test")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .Build();
        }

        public async Task InitializeAsync()
        {
            // Start the container
            await _postgresContainer.StartAsync();

            // Configure the DbContext with the container's connection string
            var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _dbContext = new CulinaryCodeDbContext(options);
            _recipeRepository = new RecipeRepository(_dbContext);

            // Ensure database is created
            await _dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            // Dispose DbContext and stop the container
            await _dbContext.DisposeAsync();
            await _postgresContainer.StopAsync();
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
    
    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByName_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = new Recipe { RecipeName = "Spaghetti Bolognese" };
        var recipe2 = new Recipe { RecipeName = "Chicken Curry" };
        var recipe3 = new Recipe { RecipeName = "Spaghetti Carbonara" };
        _dbContext.Recipes.AddRange(recipe1, recipe2, recipe3);
        _dbContext.SaveChanges();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesAsync("Spaghetti", Difficulty.NotAvailable, RecipeType.NotAvailable, 0, new List<string>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RecipeName == "Spaghetti Bolognese");
        Assert.Contains(result, r => r.RecipeName == "Spaghetti Carbonara");
    }
    
    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByDifficulty_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = new Recipe { RecipeName = "Easy Dish", Difficulty = Difficulty.Easy };
        var recipe2 = new Recipe { RecipeName = "Medium Dish", Difficulty = Difficulty.Intermediate };
        _dbContext.Recipes.AddRange(recipe1, recipe2);
        _dbContext.SaveChanges();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesAsync("", Difficulty.Easy, RecipeType.NotAvailable, 0, new List<string>());

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Easy Dish", result.First().RecipeName);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByRecipeType_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = new Recipe { RecipeName = "Soup", RecipeType = RecipeType.Dinner };
        var recipe2 = new Recipe { RecipeName = "Salad", RecipeType = RecipeType.Breakfast };
        _dbContext.Recipes.AddRange(recipe1, recipe2);
        _dbContext.SaveChanges();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesAsync("", Difficulty.NotAvailable, RecipeType.Dinner, 0, new List<string>());

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Soup", result.First().RecipeName);
    }


    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByCookingTime_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = new Recipe { RecipeName = "Quick Dish", CookingTime = 15 };
        var recipe2 = new Recipe { RecipeName = "Slow Dish", CookingTime = 60 };
        _dbContext.Recipes.AddRange(recipe1, recipe2);
        _dbContext.SaveChanges();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesAsync("", Difficulty.NotAvailable, RecipeType.NotAvailable, 30, new List<string>());

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Quick Dish", result.First().RecipeName);
    }
    
    [Fact]
    public async Task GetFilteredRecipesAsync_FilterByIngredients_ReturnsMatchingRecipes()
    {
        // Arrange
        var ingredient1 = new Ingredient { IngredientName = "Tomato" };
        var ingredient2 = new Ingredient { IngredientName = "Basil" };
        var ingredient3 = new Ingredient { IngredientName = "Chicken" };
        
        _dbContext.Ingredients.AddRange(ingredient1, ingredient2, ingredient3);

        var iq1 = new IngredientQuantity { Ingredient = ingredient1, Quantity = 5 };
        var iq2 = new IngredientQuantity { Ingredient = ingredient2, Quantity = 1 };
        var iq3 = new IngredientQuantity { Ingredient = ingredient3, Quantity = 4};
        
        _dbContext.IngredientQuantities.AddRange(iq1, iq2, iq3);
        
        var recipe1 = new Recipe
        {
            RecipeName = "Tomato Soup",
            Ingredients = new List<IngredientQuantity> { iq1, iq2  }
        };
        var recipe2 = new Recipe
        {
            RecipeName = "Chicken Curry",
            Ingredients = new List<IngredientQuantity> { iq3 }
        };

        _dbContext.Recipes.AddRange(recipe1, recipe2);
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeRepository.GetFilteredRecipesAsync("", Difficulty.NotAvailable, RecipeType.NotAvailable, 0, new List<string> { "Tomato", "Basil" });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Tomato Soup", result.First().RecipeName);
    }

    [Fact]
    public async Task GetFilteredRecipesAsync_NoRecipesInDatabase_ReturnsEmptyList()
    {
        // Arrange
        // No recipes added to the database.

        // Act
        var result = await _recipeRepository.GetFilteredRecipesAsync("", Difficulty.NotAvailable, RecipeType.NotAvailable, 0, new List<string>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}