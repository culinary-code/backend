using System;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Recipes.Ingredients;

namespace DAL.EF;

internal static class CulinaryCodeDbInitializer
{
    private static bool _hasRunDuringAppExecution = false;

    public static void Initialize(CulinaryCodeDbContext context, bool dropCreateDatabase = false)
    {
        if (!_hasRunDuringAppExecution)
        {
            if (dropCreateDatabase)
                context.Database.EnsureDeleted();

            if (context.Database.EnsureCreated())
                Seed(context);
            
            _hasRunDuringAppExecution = true;
        }
    }

    private static void Seed(CulinaryCodeDbContext context)
    {
        // Add objects here

        // Ingredients
        Ingredient ingredient1 = new Ingredient()
        {
            IngredientName = "Wortel"
        };

        Ingredient ingredient2 = new Ingredient()
        {
            IngredientName = "Appel"
        };
        Ingredient ingredient3 = new Ingredient()
        {
            IngredientName = "Radijs"
        };
        context.Ingredients.Add(ingredient1);
        context.Ingredients.Add(ingredient2);
        context.Ingredients.Add(ingredient3);

        //IngredientQuantities
        IngredientQuantity ingredientQuantity1 = new IngredientQuantity()
        {
            Ingredient = ingredient1,
            Quantity = 1
        };
        IngredientQuantity ingredientQuantity2 = new IngredientQuantity()
        {
            Ingredient = ingredient1,
            Quantity = 2
        };
        IngredientQuantity ingredientQuantity3 = new IngredientQuantity()
        {
            Ingredient = ingredient1,
            Quantity = 3
        };
        IngredientQuantity ingredientQuantity4 = new IngredientQuantity()
        {
            Ingredient = ingredient2,
            Quantity = 100
        };
        IngredientQuantity ingredientQuantity5 = new IngredientQuantity()
        {
            Ingredient = ingredient2,
            Quantity = 200
        };
        IngredientQuantity ingredientQuantity6 = new IngredientQuantity()
        {
            Ingredient = ingredient3,
            Quantity = 1000
        };
        context.IngredientQuantities.Add(ingredientQuantity1);
        context.IngredientQuantities.Add(ingredientQuantity2);
        context.IngredientQuantities.Add(ingredientQuantity3);
        context.IngredientQuantities.Add(ingredientQuantity4);
        context.IngredientQuantities.Add(ingredientQuantity5);
        context.IngredientQuantities.Add(ingredientQuantity6);

        //Preferences
        Preference preference1 = new Preference()
        {
            PreferenceName = "Veel wortels",
            StandardPreference = false
        };
        Preference preference2 = new Preference()
        {
            PreferenceName = "Weinig wortels",
            StandardPreference = false
        };
        Preference preference3 = new Preference()
        {
            PreferenceName = "Noten allergie",
            StandardPreference = true
        };

        context.Preferences.Add(preference1);
        context.Preferences.Add(preference2);
        context.Preferences.Add(preference3);

        //Instructions
        InstructionStep instructionStep1 = new InstructionStep()
        {
            Instruction = "Voeg water toe",
            StepNumber = 1
        };
        InstructionStep instructionStep2 = new InstructionStep()
        {
            Instruction = "Breng het water aan de kook",
            StepNumber = 2
        };
        InstructionStep instructionStep3 = new InstructionStep()
        {
            Instruction = "Voeg de pasta toe en roer goed door",
            StepNumber = 3
        };
        InstructionStep instructionStep4 = new InstructionStep()
        {
            Instruction = "Laat de pasta 10 minuten koken tot hij beetgaar is",
            StepNumber = 4
        };
        InstructionStep instructionStep5 = new InstructionStep()
        {
            Instruction = "Giet de pasta af en spoel af met koud water",
            StepNumber = 5
        };
        InstructionStep instructionStep6 = new InstructionStep()
        {
            Instruction = "Verhit olie in een pan op middelhoog vuur",
            StepNumber = 6
        };
        InstructionStep instructionStep7 = new InstructionStep()
        {
            Instruction = "Voeg knoflook en ui toe en bak tot ze glazig zijn",
            StepNumber = 7
        };
        InstructionStep instructionStep8 = new InstructionStep()
        {
            Instruction = "Voeg de tomatensaus toe en laat 5 minuten sudderen",
            StepNumber = 8
        };

        context.InstructionSteps.Add(instructionStep1);
        context.InstructionSteps.Add(instructionStep2);
        context.InstructionSteps.Add(instructionStep3);
        context.InstructionSteps.Add(instructionStep4);
        context.InstructionSteps.Add(instructionStep5);
        context.InstructionSteps.Add(instructionStep6);
        context.InstructionSteps.Add(instructionStep7);
        context.InstructionSteps.Add(instructionStep8);

        // Reviews
        Review review1 = new Review()
        {
            AmountOfStars = 1,
            Description = "Slechte pasta, heel vies",
            CreatedAt = DateTime.UtcNow,
        };
        Review review2 = new Review()
        {
            AmountOfStars = 3,
            Description = "Redelijke pasta, smaakte oké maar niet bijzonder.",
            CreatedAt = DateTime.UtcNow,
        };
        Review review3 = new Review()
        {
            AmountOfStars = 5,
            Description = "Heerlijke pasta! Perfecte smaak en goede textuur. Zeker voor herhaling vatbaar!",
            CreatedAt = DateTime.UtcNow,
        };
        Review review4 = new Review()
        {
            AmountOfStars = 2,
            Description = "Niet zo lekker als verwacht, saus was te waterig en pasta was wat klef.",
            CreatedAt = DateTime.UtcNow,
        };
        context.Reviews.Add(review1);
        context.Reviews.Add(review2);
        context.Reviews.Add(review3);
        context.Reviews.Add(review4);

        // Recipes
        Recipe recipe1 = new Recipe()
        {
            RecipeName = "Test Recept 1",
            Ingredients = [],
            Preferences = [],
            RecipeType = RecipeType.Snack,
            Description = "Dit is een test recept voor een snack.",
            CookingTime = 5,
            Difficulty = Difficulty.Easy,
            AmountOfPeople = 1,
            ImagePath = "https://picsum.photos/200/300",
            CreatedAt = DateTime.UtcNow,
            Instructions = [],
            Reviews = [],
        };

        recipe1.Ingredients.Add(ingredientQuantity1);
        recipe1.Ingredients.Add(ingredientQuantity2);
        recipe1.Ingredients.Add(ingredientQuantity3);

        recipe1.Preferences.Add(preference1);
        recipe1.Preferences.Add(preference2);

        recipe1.Instructions.Add(instructionStep1);
        recipe1.Instructions.Add(instructionStep2);
        recipe1.Instructions.Add(instructionStep3);
        recipe1.Instructions.Add(instructionStep4);
        recipe1.Instructions.Add(instructionStep5);
        recipe1.Instructions.Add(instructionStep6);
        recipe1.Instructions.Add(instructionStep7);

        recipe1.Reviews.Add(review1);
        recipe1.Reviews.Add(review2);
        recipe1.Reviews.Add(review4);

        Recipe recipe2 = new Recipe()
        {
            RecipeName = "Test Recept 2",
            Ingredients = [],
            Preferences = [],
            RecipeType = RecipeType.Breakfast,
            Description = "Dit is een testrecept voor een ontbijt.",
            CookingTime = 20,
            Difficulty = Difficulty.Intermediate,
            AmountOfPeople = 1,
            ImagePath = "https://picsum.photos/200/300",
            CreatedAt = DateTime.UtcNow,
            Instructions = [],
            Reviews = [],
        };

        recipe2.Ingredients.Add(ingredientQuantity4);

        recipe2.Preferences.Add(preference3);

        recipe2.Instructions.Add(instructionStep8);

        recipe2.Reviews.Add(review3);


        Recipe recipe3 = new Recipe()
        {
            RecipeName = "Test Recept 3",
            Ingredients = [],
            Preferences = [],
            RecipeType = RecipeType.Dinner,
            Description = "Dit is een testrecept voor een avondmaal.",
            CookingTime = 120,
            Difficulty = Difficulty.Difficult,
            AmountOfPeople = 4,
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