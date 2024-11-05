namespace BL.ExternalSources.Llm;

public interface ILlmService
{
    public string GetChatMessage(string message);
    public Uri GetImage(string recipePrompt);
}