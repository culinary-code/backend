using System;
using System.ComponentModel.Design;
using DAL.Groceries;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

internal static class CulinaryCodeDbInitializer
{
    private static bool _hasRunDuringAppExecution = false;

    public static void Initialize(CulinaryCodeDbContext context, bool dropCreateDatabase = false)
    {
        if (!_hasRunDuringAppExecution)
        {
            if (dropCreateDatabase)
            {
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }
            else
            {
                context.Database.Migrate();
            }

            Seed(context);
            _hasRunDuringAppExecution = true;
        }
    }

   private static void Seed(CulinaryCodeDbContext context)
{
    // Check if the data already exists before adding it
    if (!context.Ingredients.Any())
    {
        // Add Ingredients
        var ingredients = new List<Ingredient>
        {
            new Ingredient { IngredientName = "Wortel" },
            new Ingredient { IngredientName = "Appel" },
            new Ingredient { IngredientName = "Radijs" }
        };
        context.Ingredients.AddRange(ingredients);
        context.SaveChanges(); // Save changes after adding ingredients
    }

    if (!context.Preferences.Any())
    {
        // Add Preferences
        var preferences = new List<Preference>
        {
            new Preference { PreferenceName = "Veel wortels", StandardPreference = false },
            new Preference { PreferenceName = "Weinig wortels", StandardPreference = false },
            new Preference { PreferenceName = "Noten allergie", StandardPreference = true },
            new Preference { PreferenceName = "Vegan", StandardPreference = true },
            new Preference { PreferenceName = "Vegetarisch", StandardPreference = true },
            new Preference { PreferenceName = "Lactose Intolerant", StandardPreference = true }
        };
        context.Preferences.AddRange(preferences);
        context.SaveChanges(); // Save changes after adding preferences
    }

    if (!context.InstructionSteps.Any())
    {
        // Add InstructionSteps
        var instructionSteps = new List<InstructionStep>
        {
            new InstructionStep { Instruction = "Voeg water toe", StepNumber = 1 },
            new InstructionStep { Instruction = "Breng het water aan de kook", StepNumber = 2 },
            new InstructionStep { Instruction = "Voeg de pasta toe en roer goed door", StepNumber = 3 },
            new InstructionStep { Instruction = "Laat de pasta 10 minuten koken tot hij beetgaar is", StepNumber = 4 },
            new InstructionStep { Instruction = "Giet de pasta af en spoel af met koud water", StepNumber = 5 },
            new InstructionStep { Instruction = "Verhit olie in een pan op middelhoog vuur", StepNumber = 6 },
            new InstructionStep { Instruction = "Voeg knoflook en ui toe en bak tot ze glazig zijn", StepNumber = 7 },
            new InstructionStep { Instruction = "Voeg de tomatensaus toe en laat 5 minuten sudderen", StepNumber = 8 }
        };
        context.InstructionSteps.AddRange(instructionSteps);
        context.SaveChanges(); // Save changes after adding instruction steps
    }

    if (!context.Reviews.Any())
    {
        // Add Reviews
        var reviews = new List<Review>
        {
            new Review { AmountOfStars = 1, Description = "Slechte pasta, heel vies", CreatedAt = DateTime.UtcNow },
            new Review
            {
                AmountOfStars = 3, Description = "Redelijke pasta, smaakte oké maar niet bijzonder.",
                CreatedAt = DateTime.UtcNow
            },
            new Review
            {
                AmountOfStars = 5,
                Description = "Heerlijke pasta! Perfecte smaak en goede textuur. Zeker voor herhaling vatbaar!",
                CreatedAt = DateTime.UtcNow
            },
            new Review
            {
                AmountOfStars = 2, Description = "Niet zo lekker als verwacht, saus was te waterig en pasta was wat klef.",
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Reviews.AddRange(reviews);
        context.SaveChanges(); // Save changes after adding reviews
    }

    if (!context.Recipes.Any())
    {
        // Add Recipes
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                RecipeName = "Test Recept 1",
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity
                        { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 1 },
                    new IngredientQuantity
                        { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 2 },
                    new IngredientQuantity
                        { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 3 }
                },
                Preferences = new List<Preference>
                {
                    context.Preferences.First(p => p.PreferenceName == "Veel wortels"),
                    context.Preferences.First(p => p.PreferenceName == "Weinig wortels")
                },
                RecipeType = RecipeType.Snack,
                Description = "Dit is een test recept voor een snack.",
                CookingTime = 5,
                Difficulty = Difficulty.Easy,
                AmountOfPeople = 1,
                ImagePath = "https://picsum.photos/200/300",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow.AddDays(-32),
                Instructions = new List<InstructionStep>
                {
                    context.InstructionSteps.First(i => i.StepNumber == 1),
                    context.InstructionSteps.First(i => i.StepNumber == 2),
                    context.InstructionSteps.First(i => i.StepNumber == 3),
                    context.InstructionSteps.First(i => i.StepNumber == 4),
                    context.InstructionSteps.First(i => i.StepNumber == 5),
                    context.InstructionSteps.First(i => i.StepNumber == 6),
                    context.InstructionSteps.First(i => i.StepNumber == 7)
                },
                Reviews = new List<Review>
                {
                    context.Reviews.First(r => r.AmountOfStars == 1),
                    context.Reviews.First(r => r.AmountOfStars == 3),
                    context.Reviews.First(r => r.AmountOfStars == 2)
                },
                AmountOfRatings = 3,
                AverageRating = (context.Reviews.First(r => r.AmountOfStars == 1).AmountOfStars +
                                 context.Reviews.First(r => r.AmountOfStars == 3).AmountOfStars +
                                 context.Reviews.First(r => r.AmountOfStars == 2).AmountOfStars) / 3.0
            },
            new Recipe
            {
                RecipeName = "Test Recept 2",
                Ingredients = new List<IngredientQuantity>
                {
                    new IngredientQuantity
                        { Ingredient = context.Ingredients.First(i => i.IngredientName == "Appel"), Quantity = 100 }
                },
                Preferences = new List<Preference>
                {
                    context.Preferences.First(p => p.PreferenceName == "Noten allergie")
                },
                RecipeType = RecipeType.Breakfast,
                Description = "Dit is een testrecept voor een ontbijt.",
                CookingTime = 20,
                Difficulty = Difficulty.Intermediate,
                AmountOfPeople = 1,
                ImagePath = "https://picsum.photos/200/300",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow.AddDays(-32),
                Instructions = new List<InstructionStep>
                {
                    context.InstructionSteps.First(i => i.StepNumber == 8)
                },
                Reviews = new List<Review>
                {
                    context.Reviews.First(r => r.AmountOfStars == 5)
                },
                AmountOfRatings = 1,
                AverageRating = context.Reviews.First(r => r.AmountOfStars == 5).AmountOfStars
            },
            new Recipe
            {
                RecipeName = "Test Recept 3",
                RecipeType = RecipeType.Dinner,
                Description = "Dit is een testrecept voor een avondmaal.",
                CookingTime = 120,
                Difficulty = Difficulty.Difficult,
                AmountOfPeople = 4,
                ImagePath = "https://picsum.photos/200/300",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            }
        };
        context.Recipes.AddRange(recipes);
    }

    // Save changes
    context.SaveChanges();

    // Clear change-tracker for the data does not stay tracked all the time
    // and any requests will get it from the database instead of the change-tracker.
    context.ChangeTracker.Clear();
}
}