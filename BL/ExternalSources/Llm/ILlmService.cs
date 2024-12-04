namespace BL.ExternalSources.Llm;

public interface ILlmService
{
    string GenerateRecipe(string message);
    string GenerateMultipleRecipes(string message, int amount);
    string[] GenerateMultipleRecipeNamesAndDescriptions(string message, int amount);
    public Uri? GenerateRecipeImage(string recipePrompt);
}