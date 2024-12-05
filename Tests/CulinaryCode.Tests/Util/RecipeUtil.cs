using DOM.Accounts;
using DOM.Recipes;
using DOM.Recipes.Ingredients;

namespace CulinaryCode.Tests.Util;

public static class RecipeUtil
{
    public static Recipe CreateRecipe(string recipeName = "Test Recipe",
        RecipeType recipeType = RecipeType.NotAvailable, int cookTime = 0,
        Difficulty difficulty = Difficulty.NotAvailable, string description = "",
        int amountOfPeople = 4)
    {
        return new Recipe
        {
            RecipeId = Guid.NewGuid(),
            RecipeName = recipeName,
            Ingredients = new List<IngredientQuantity>(),
            Instructions = new List<InstructionStep>(),
            Reviews = new List<Review>(),
            Preferences = new List<Preference>(),
            RecipeType = recipeType,
            CookingTime = cookTime,
            Difficulty = difficulty,
            Description = description,
            AmountOfPeople = amountOfPeople
        };
    }
}