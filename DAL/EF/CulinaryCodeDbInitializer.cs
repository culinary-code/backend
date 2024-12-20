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

            //Seed(context);
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
            new() { IngredientName = "Wortel" },
            new() { IngredientName = "Appel" },
            new() { IngredientName = "Radijs" }
        };
        context.Ingredients.AddRange(ingredients);
        context.SaveChanges(); // Save changes after adding ingredients
    }

    if (!context.Preferences.Any())
    {
        // Add Preferences
        var preferences = new List<Preference>
        {
            new() { PreferenceName = "Veel wortels", StandardPreference = false },
            new() { PreferenceName = "Weinig wortels", StandardPreference = false },
            new() { PreferenceName = "Noten allergie", StandardPreference = true },
            new() { PreferenceName = "Vegan", StandardPreference = true },
            new() { PreferenceName = "Vegetarisch", StandardPreference = true },
            new() { PreferenceName = "Lactose Intolerant", StandardPreference = true }
        };
        context.Preferences.AddRange(preferences);
        context.SaveChanges(); // Save changes after adding preferences
    }

    if (!context.InstructionSteps.Any())
    {
        // Add InstructionSteps
        var instructionSteps = new List<InstructionStep>
        {
            new() { Instruction = "Voeg water toe", StepNumber = 1 },
            new() { Instruction = "Breng het water aan de kook", StepNumber = 2 },
            new() { Instruction = "Voeg de pasta toe en roer goed door", StepNumber = 3 },
            new() { Instruction = "Laat de pasta 10 minuten koken tot hij beetgaar is", StepNumber = 4 },
            new() { Instruction = "Giet de pasta af en spoel af met koud water", StepNumber = 5 },
            new() { Instruction = "Verhit olie in een pan op middelhoog vuur", StepNumber = 6 },
            new() { Instruction = "Voeg knoflook en ui toe en bak tot ze glazig zijn", StepNumber = 7 },
            new() { Instruction = "Voeg de tomatensaus toe en laat 5 minuten sudderen", StepNumber = 8 }
        };
        context.InstructionSteps.AddRange(instructionSteps);
        context.SaveChanges(); // Save changes after adding instruction steps
    }

    if (!context.Reviews.Any())
    {
        // Add Reviews
        var reviews = new List<Review>
        {
            new() { AmountOfStars = 1, Description = "Slechte pasta, heel vies", CreatedAt = DateTime.UtcNow },
            new()
            {
                AmountOfStars = 3, Description = "Redelijke pasta, smaakte oké maar niet bijzonder.",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                AmountOfStars = 5,
                Description = "Heerlijke pasta! Perfecte smaak en goede textuur. Zeker voor herhaling vatbaar!",
                CreatedAt = DateTime.UtcNow
            },
            new()
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
            new()
            {
                RecipeName = "Test Recept 1",
                Ingredients = new List<IngredientQuantity>
                {
                    new() { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 1 },
                    new() { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 2 },
                    new() { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 3 }
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
            new()
            {
                RecipeName = "Test Recept 2",
                Ingredients = new List<IngredientQuantity>
                {
                    new() { Ingredient = context.Ingredients.First(i => i.IngredientName == "Appel"), Quantity = 100 }
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
            new()
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
        context.SaveChanges(); // Save changes after adding recipes
    }

    if (!context.PlannedMeals.Any())
    {
        var recipe2 = context.Recipes.FirstOrDefault(r => r.RecipeName == "Test Recept 2");

        var meal1 = new PlannedMeal
        {
            Recipe = recipe2,
            Ingredients = new List<IngredientQuantity>
            {
                new IngredientQuantity { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 1 },
                new IngredientQuantity { Ingredient = context.Ingredients.First(i => i.IngredientName == "Appel"), Quantity = 150 }
            }
        };

        var meal2 = new PlannedMeal
        {
            Ingredients = new List<IngredientQuantity>
            {
                new IngredientQuantity { Ingredient = context.Ingredients.First(i => i.IngredientName == "Wortel"), Quantity = 2 },
                new IngredientQuantity { Ingredient = context.Ingredients.First(i => i.IngredientName == "Appel"), Quantity = 100 }
            }
        };

        context.PlannedMeals.Add(meal1);
        context.PlannedMeals.Add(meal2);
        context.SaveChanges();
    }

    if (!context.GroceryLists.Any())
    {
        var meal1 = context.PlannedMeals.FirstOrDefault();
        var groceryList = new GroceryList
        {
            GroceryListId = Guid.NewGuid(),
            Ingredients = meal1.Ingredients,
        };

        context.GroceryLists.Add(groceryList);
        context.SaveChanges();
    }

    if (!context.Accounts.Any(a => a.Email == "nis@n.n"))
    {
        var groceryList = context.GroceryLists.FirstOrDefault();
        var account1 = new Account
        {
            AccountId = Guid.Parse("d1ec841b-9646-4ca7-a1ef-eda7354547f3"),
            Name = "nis",
            Email = "nis@n.n",
            FamilySize = 4,
            GroceryList = groceryList
        };

        context.Accounts.Add(account1);
        context.SaveChanges();
    }

    if (!context.MealPlanners.Any())
    {
        var account1 = context.Accounts.FirstOrDefault(a => a.Email == "nis@n.n");
        var meal1 = context.PlannedMeals.FirstOrDefault();
        var meal2 = context.PlannedMeals.Skip(1).FirstOrDefault();

        var mealPlanner = new MealPlanner
        {
            MealPlannerId = Guid.NewGuid(),
            NextWeek = new List<PlannedMeal> { meal1, meal2 },
            Account = account1,
        };

        context.MealPlanners.Add(mealPlanner);
        context.SaveChanges();
    }

    if (!context.ItemQuantities.Any())
    {
        var groceryList = context.GroceryLists.FirstOrDefault();
        var ingredient = context.Ingredients.First(i => i.IngredientName == "Appel");
        var newIngredient = new Ingredient { IngredientId = Guid.NewGuid(), IngredientName = "Aardappel", Measurement = MeasurementType.Kilogram };
        var newItem = new GroceryItem { GroceryItemId = Guid.NewGuid(), GroceryItemName = "Waspoeder" };

        var addItem = new ItemQuantity
        {
            GroceryItem = new GroceryItem
            {
                GroceryItemId = ingredient.IngredientId,
                GroceryItemName = ingredient.IngredientName,
            },
            GroceryList = groceryList,
            Quantity = 2
        };

        var addNewItem = new ItemQuantity
        {
            GroceryItem = new GroceryItem
            {
                GroceryItemId = newIngredient.IngredientId,
                GroceryItemName = newIngredient.IngredientName,
            },
            GroceryList = groceryList,
            Quantity = 2
        };

        var addNewItem2 = new ItemQuantity
        {
            GroceryItem = new GroceryItem
            {
                GroceryItemId = newItem.GroceryItemId,
                GroceryItemName = newItem.GroceryItemName,
            },
            GroceryList = groceryList,
            Quantity = 7
        };

        context.ItemQuantities.Add(addItem);
        context.ItemQuantities.Add(addNewItem);
        context.ItemQuantities.Add(addNewItem2);
        context.SaveChanges();
    }

    if (!context.Accounts.Any(a => a.Email == "nisko@n.n"))
    {
        var account2 = new Account
        {
            AccountId = Guid.Parse("718a1b80-7ae4-4ae0-a26c-87770f54d517"),
            Name = "nikl",
            Email = "nisko@n.n",
            FamilySize = 4,
        };

        context.Accounts.Add(account2);
        context.SaveChanges();
    }

    if (!context.Groups.Any(g => g.GroupName == "nisso"))
    {
        var group1 = new Group
        {
            GroupId = Guid.Parse("1bde5dcd-816f-4d97-bb0f-e3d60cceb200"),
            GroupName = "nisso",
        };

        var account1 = context.Accounts.FirstOrDefault(a => a.Email == "nis@n.n");
        group1.Accounts.Add(account1);

        context.Groups.Add(group1);
        context.SaveChanges();
    }

    // Save changes
    context.SaveChanges();

    // Clear change-tracker for the data does not stay tracked all the time
    // and any requests will get it from the database instead of the change-tracker.
    context.ChangeTracker.Clear();
}
}