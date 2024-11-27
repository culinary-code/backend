namespace BL.ExternalSources.Llm;

public interface ILlmService
{
    public string GenerateRecipe(string message);
    public string GenerateMultipleRecipes(string message, int amount);
    string[] GenerateMultipleRecipeNamesAndDescriptions(string message, int amount);
    public Uri? GenerateRecipeImage(string recipePrompt);
}