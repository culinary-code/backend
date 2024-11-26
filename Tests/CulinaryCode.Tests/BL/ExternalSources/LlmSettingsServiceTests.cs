using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;

namespace CulinaryCode.Tests.BL.ExternalSources;

public class LlmSettingsServiceTests
{
    [Fact]
    public void BuildPrompt_OnlyRecipeNameProvided_ReturnsPromptWithRecipeName()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto
        {
            RecipeName = "Pasta"
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto);

        // Assert
        Assert.Equal("I want a recipe for Pasta.", prompt);
    }

    [Fact]
    public void BuildPrompt_NoFiltersProvided_ReturnsRandomRecipePrompt()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto();

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto);

        // Assert
        Assert.Equal("I want a random recipe.", prompt);
    }

    [Fact]
    public void BuildPrompt_IngredientsProvided_ReturnsPromptWithIngredients()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto
        {
            Ingredients = new List<string> { "Tomato", "Cheese", "Basil" }
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto);

        // Assert
        Assert.Equal(
            "I want a random recipe.\r\nHere are the ingredients I have:\r\nTomato, Cheese, Basil",
            prompt
        );
    }

    [Fact]
    public void BuildPrompt_FullFiltersProvided_ReturnsCompletePrompt()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto
        {
            RecipeName = "Pizza",
            Ingredients = new List<string> { "Flour", "Tomato", "Mozzarella" },
            Difficulty = "2",
            MealType = "3",
            CookTime = 30
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto);

        // Assert
        Assert.Equal(
            "I want a recipe for Pizza.\r\nHere are the ingredients I have:\r\nFlour, Tomato, Mozzarella\r\nThe recipe difficulty should be Intermediate.\r\nIt should be a Dinner recipe.\r\nThe cooking time should be around 30 minutes.",
            prompt
        );
    }

    [Fact]
    public void BuildPrompt_InvalidDifficultyOrMealType_HandlesGracefully()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto
        {
            Difficulty = "0",
            MealType = "0"
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto);

        // Assert
        Assert.Equal("I want a random recipe.", prompt);
    }

    [Fact]
    public void BuildPrompt_CookTimeProvided_ReturnsPromptWithCookTime()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto
        {
            CookTime = 15
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto);

        // Assert
        Assert.Equal("I want a random recipe.\r\nThe cooking time should be around 15 minutes.", prompt);
    }
}