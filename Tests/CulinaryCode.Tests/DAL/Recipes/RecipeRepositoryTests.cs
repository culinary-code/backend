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
        public async Task ReadRecipeById_RecipeExists_ReturnsRecipe()
        {
            // Arrange
            var recipe = CreateRecipe();
            _dbContext.Recipes.Add(recipe);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = _recipeRepository.ReadRecipeById(recipe.RecipeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(recipe.RecipeId, result.RecipeId);
            Assert.Equal(recipe.RecipeName, result.RecipeName);
        }

        [Fact]
        public async Task ReadRecipeById_RecipeDoesNotExist_ThrowsRecipeNotFoundException()
        {
            // Arrange
            var recipeId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<RecipeNotFoundException>(() => _recipeRepository.ReadRecipeById(recipeId));
        }

        [Fact]
        public async Task CreateRecipe_ValidRecipe_CreatesRecipe()
        {
            // Arrange
            var recipe = CreateRecipe();

            // Act
            _recipeRepository.CreateRecipe(recipe);

            // Assert
            var result = await _dbContext.Recipes.FindAsync(recipe.RecipeId);
            Assert.NotNull(result);
            Assert.Equal(recipe.RecipeId, result.RecipeId);
            Assert.Equal(recipe.RecipeName, result.RecipeName);
        }

        // Add other test methods here
    }
}
