using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
using CulinaryCode.Tests.Util;

namespace CulinaryCode.Tests.BL.ExternalSources;

public class LlmSettingsServiceTests
{
    private string NormalizeLineEndings(string input) => input.Replace("\r\n", "\n").Replace("\r", "\n");

    [Fact]
    public void BuildPrompt_OnlyRecipeNameProvided_ReturnsPromptWithRecipeName()
    {
        // Arrange
        var preferences = PreferenceListDtoUtil.CreateEmptyPreferenceListDto();
        var recipeDto = new RecipeFilterDto
        {
            RecipeName = "Pasta"
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferences!);

        // Assert
        Assert.Equal(NormalizeLineEndings("I want a recipe for Pasta."), NormalizeLineEndings(prompt));
    }

    [Fact]
    public void BuildPrompt_NoFiltersProvided_ReturnsRandomRecipePrompt()
    {
        // Arrange
        var recipeDto = new RecipeFilterDto();
        var preferences = PreferenceListDtoUtil.CreateEmptyPreferenceListDto();

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferences!);

        // Assert
        Assert.Equal(NormalizeLineEndings("I want a random recipe."),
            NormalizeLineEndings(prompt));
    }

    [Fact]
    public void BuildPrompt_IngredientsProvided_ReturnsPromptWithIngredients()
    {
        // Arrange
        var preferences = PreferenceListDtoUtil.CreateEmptyPreferenceListDto();
        var recipeDto = new RecipeFilterDto
        {
            Ingredients = new List<string> { "Tomato", "Cheese", "Basil" }
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferences!);

        // Assert
        Assert.Equal(
            NormalizeLineEndings(
                "I want a random recipe.\nHere are the ingredients I have:\nTomato, Cheese, Basil"),
            NormalizeLineEndings(prompt)
        );
    }

    [Fact]
    public void BuildPrompt_FullFiltersProvided_ReturnsCompletePrompt()
    {
        // Arrange
        var preferences = PreferenceListDtoUtil.CreateEmptyPreferenceListDto();
        var recipeDto = new RecipeFilterDto
        {
            RecipeName = "Pizza",
            Ingredients = new List<string> { "Flour", "Tomato", "Mozzarella" },
            Difficulty = "2",
            MealType = "3",
            CookTime = 30
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferences!);

        // Assert
        Assert.Equal(
            NormalizeLineEndings(
                "I want a recipe for Pizza.\nHere are the ingredients I have:\nFlour, Tomato, Mozzarella\nThe recipe difficulty should be Intermediate.\nIt should be a Dinner recipe.\nThe cooking time should be around 30 minutes."),
            NormalizeLineEndings(prompt)
        );
    }

    [Fact]
    public void BuildPrompt_InvalidDifficultyOrMealType_HandlesGracefully()
    {
        // Arrange
        var preferences = PreferenceListDtoUtil.CreateEmptyPreferenceListDto();
        var recipeDto = new RecipeFilterDto
        {
            Difficulty = "0",
            MealType = "0"
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferences!);

        // Assert
        Assert.Equal(NormalizeLineEndings("I want a random recipe."), NormalizeLineEndings(prompt));
    }

    [Fact]
    public void BuildPrompt_CookTimeProvided_ReturnsPromptWithCookTime()
    {
        // Arrange
        var preferences = PreferenceListDtoUtil.CreateEmptyPreferenceListDto();
        var recipeDto = new RecipeFilterDto
        {
            CookTime = 15
        };

        // Act
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferences!);

        // Assert
        Assert.Equal(NormalizeLineEndings("I want a random recipe.\nThe cooking time should be around 15 minutes."),
            NormalizeLineEndings(prompt));
    }
}