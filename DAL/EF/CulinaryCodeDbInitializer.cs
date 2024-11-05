using System;
using DOM.Recipes;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

internal static class CulinaryCodeDbInitializer
{
    public static void Initialize(CulinaryCodeDbContext context, bool dropCreateDatabase = false)
    {
        if (dropCreateDatabase)
            context.Database.EnsureDeleted();

        if (context.Database.EnsureCreated())
            Seed(context);
    }

    private static void Seed(CulinaryCodeDbContext context)
    {
        // Add objects here
        Recipe recipe1 = new Recipe()
        {
            RecipeName = "Test Recipe 1",
            Ingredients = [],
            Preferences = [],
            RecipeType = RecipeType.Snack,
            Description = "This is a test recipe",
            CookingTime = 5,
            Difficulty = Difficulty.Easy,
            ImagePath = "https://picsum.photos/200/300",
            CreatedAt = DateTime.UtcNow,
            Instructions = [],
            Reviews = [],
        };
        Recipe recipe2 = new Recipe()
        {
            RecipeName = "Test Recipe 2",
            Ingredients = [],
            Preferences = [],
            RecipeType = RecipeType.Snack,
            Description = "This is a test recipe",
            CookingTime = 5,
            Difficulty = Difficulty.Easy,
            ImagePath = "https://picsum.photos/200/300",
            CreatedAt = DateTime.UtcNow,
            Instructions = [],
            Reviews = [],
        };
        Recipe recipe3 = new Recipe()
        {
            RecipeName = "Test Recipe 3",
            Ingredients = [],
            Preferences = [],
            RecipeType = RecipeType.Snack,
            Description = "This is a test recipe",
            CookingTime = 5,
            Difficulty = Difficulty.Easy,
            ImagePath = "https://picsum.photos/200/300",
            CreatedAt = DateTime.UtcNow,
            Instructions = [],
            Reviews = [],
        };
        context.Recipes.Add(recipe1);
        context.Recipes.Add(recipe2);
        context.Recipes.Add(recipe3);
        
        // Save changes
        context.SaveChanges();
        
        // Clear change-tracker for the data does not stay tracked all the time
        // and any requests will get it from the database instead of the change-tracker.
        
        context.ChangeTracker.Clear();
    }
}