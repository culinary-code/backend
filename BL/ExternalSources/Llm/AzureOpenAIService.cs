using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace BL.ExternalSources.Llm;

public class AzureOpenAIService : ILlmService
{
    private readonly string _apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ??
                                      throw new Exception("AZURE_OPENAI_API_KEY environment variable is not set.");

    private readonly string _endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ??
                                        throw new Exception("AZURE_OPENAI_ENDPOINT environment variable is not set.");

    private readonly AzureOpenAIClient _azureClient;
    private readonly ChatClient _chatClient;
    private readonly ImageClient _imageClient;

    public AzureOpenAIService()
    {
        _azureClient = new AzureOpenAIClient(
            new Uri(_endpoint),
            new ApiKeyCredential(_apiKey)
        );
        _chatClient = _azureClient.GetChatClient("gpt-4o-mini");
        _imageClient = _azureClient.GetImageClient("dall-e-3");
    }

    public string GetChatMessage(string message)
    {
        var systemPrompt = LlmSettingsService.SystemPrompt;

        ChatCompletionOptions completionOptions = new ChatCompletionOptions
        {
            Temperature = (float?)0.8,
            TopP = (float?)0.9,
            PresencePenalty = 0.5f,
            FrequencyPenalty = 0.5f,
        };

        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage(systemPrompt),

            new UserChatMessage(message),
        ], completionOptions);

        Console.WriteLine($"{completion.Content[0].Text}");

        return completion.Content[0].Text;
    }

    public Uri GetImage(string recipePrompt)
    {
        ChatCompletionOptions completionOptions = new ChatCompletionOptions
        {
            Temperature = (float?)0.8,
            TopP = (float?)0.9,
            PresencePenalty = 0.5f,
            FrequencyPenalty = 0.5f,
        };

        var systemPrompt = $@"""
                           Write a long prompt for DALL-E to generate an image. 
                            Create a highly photorealistic close up image of the recipe. 
                            The scene is set in a cozy indoor kitchen, with soft, natural light coming from a large window to the side, casting delicate shadows around the bowl. 
                            A few modern kitchen utensils and a wooden spoon are subtly visible in the blurred background, adding a warm and homey atmosphere.
                            Focus on creating a prompt to have an image generated.
                            The recipe is: {recipePrompt}.                           
                            """;

        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage(systemPrompt),

            new UserChatMessage(recipePrompt),
        ], completionOptions);

        Console.WriteLine($"{completion.Content[0].Text}");


        ImageGenerationOptions options = new ImageGenerationOptions
        {
            Size = GeneratedImageSize.W1024xH1024,
        };

        var response = _imageClient.GenerateImage(completion.Content[0].Text, options);

        Uri imageUri = response.Value.ImageUri;

        Console.WriteLine($"Generated image: {imageUri}");
        return imageUri;
    }
}