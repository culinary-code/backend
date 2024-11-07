namespace BL.ExternalSources.Llm;

public interface ILlmService
{
    public string GenerateRecipe(string message);
    public Uri? GenerateRecipeImage(string recipePrompt);
}