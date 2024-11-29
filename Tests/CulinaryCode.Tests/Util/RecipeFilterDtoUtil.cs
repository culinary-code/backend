using BL.DTOs.Recipes;
using DOM.Recipes;

namespace CulinaryCode.Tests.Util;

public static class RecipeFilterDtoUtil
{
    public static RecipeFilterDto CreateRecipeFilterDto(string recipeName = "", string difficulty = "0",
        string mealType = "0", int cookTime = 0)
    {
        return new RecipeFilterDto()
        {
            Ingredients = [],
            RecipeName = recipeName,
            Difficulty = difficulty,
            MealType = mealType,
            CookTime = cookTime,
        };
    }
}