using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace BL.ExternalSources.ChatGPT;

public class AzureOpenAIService
{
    private readonly string _apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ??
                                      throw new InvalidOperationException();

    private readonly AzureOpenAIClient _azureClient;
    private readonly ChatClient _chatClient;

    public AzureOpenAIService()
    {
        _azureClient = new AzureOpenAIClient(
            new Uri("https://culinarycode-openai.openai.azure.com/"),
            new ApiKeyCredential(_apiKey)
        );
        _chatClient = _azureClient.GetChatClient("gpt-4o-mini");
    }

    public string GetChatMessage(string message)
    {
        ChatCompletionOptions completionOptions = new ChatCompletionOptions
        {
            Temperature = (float?)0.8,
            TopP = (float?)0.9,
        };


        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage(
                "You are a star chef that provides detailed recipes for curious users looking for a tasty meal. You provide all the recipes in a JSON format. Return only the JSON formatted recipes and not any other text. Adhere to the following JSON schema: {\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"recipeName\":{\"type\":\"string\"},\"amount_of_persons\":{\"type\":\"integer\"},\"ingredients\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"amount\":{\"type\":\"number\"},\"measurementType\":{\"type\":\"string\"}},\"required\":[\"name\",\"amount\",\"measurementType\"]}},\"recipeSteps\":{\"type\":\"array\",\"items\":{\"type\":\"string\"}}},\"required\":[\"recipeName\",\"amount_of_persons\",\"ingredients\",\"recipeSteps\"]}\n"),

            new UserChatMessage(message),
        ], completionOptions);

        Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");

        return completion.Content[0].Text;
    }
}