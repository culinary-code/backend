using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Images;

namespace BL.ExternalSources.Llm;

public class AzureOpenAIService : ILlmService
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _blobConnectionString;
    private readonly string _blobContainerName;

    private readonly AzureOpenAIClient _azureClient;
    private readonly ChatClient _chatClient;
    private readonly ImageClient _imageClient;

    private readonly ILogger<AzureOpenAIService> _logger;

    public AzureOpenAIService(ILogger<AzureOpenAIService> logger)
    {
        DotNetEnv.Env.Load("../.env");
        
        _apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ??
                  throw new EnvironmentVariableNotAvailableException("AZURE_OPENAI_API_KEY environment variable is not set.");
        _endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ??
                    throw new EnvironmentVariableNotAvailableException("AZURE_OPENAI_ENDPOINT environment variable is not set.");
        _blobConnectionString =
            Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ??
            throw new EnvironmentVariableNotAvailableException("AZURE_STORAGE_CONNECTION_STRING environment variable is not set.");
        _blobContainerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME") ??
                             throw new EnvironmentVariableNotAvailableException("AZURE_STORAGE_CONTAINER_NAME environment variable is not set.");

        
        _logger = logger;
        _azureClient = new AzureOpenAIClient(
            new Uri(_endpoint),
            new ApiKeyCredential(_apiKey)
        );
        _chatClient = _azureClient.GetChatClient("gpt-4o-mini");
        _imageClient = _azureClient.GetImageClient("dall-e-3");
    }

    public string GenerateRecipe(string message)
    {
        var systemPrompt = LlmSettingsService.SystemPrompt;

        ChatCompletionOptions completionOptions = new ChatCompletionOptions
        {
            Temperature = (float?)0.6,
            TopP = (float?)0.9,
        };

        _logger.LogInformation("Sending chat completion request to Azure OpenAI API");

        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage(systemPrompt),

            new UserChatMessage(message),
        ], completionOptions);

        var response = completion.Content[0].Text;

        _logger.LogInformation("Received chat completion response from Azure OpenAI API");
        _logger.LogInformation(response);

        return response;
    }

    public Uri? GenerateRecipeImage(string recipePrompt)
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

        _logger.LogInformation("Sending chat completion request for image prompt to Azure OpenAI API");

        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage(systemPrompt),

            new UserChatMessage(recipePrompt),
        ], completionOptions);

        _logger.LogInformation("Received chat completion response for image prompt from Azure OpenAI API");
        _logger.LogInformation($"{completion.Content[0].Text}");

        ImageGenerationOptions options = new ImageGenerationOptions
        {
            Size = GeneratedImageSize.W1024xH1024,
        };

        _logger.LogInformation("Sending image generation request to Azure OpenAI API");

        var response = _imageClient.GenerateImage(completion.Content[0].Text, options);

        Uri azureImageUri = response.Value.ImageUri;
        HttpClient client = new HttpClient();
        var imageBytes = client.GetByteArrayAsync(azureImageUri).Result;

        _logger.LogInformation($"Generated image: {azureImageUri}");
        _logger.LogInformation($"Generated image bytes length: {imageBytes.Length}");

        Task<Uri?> result = UploadImage(imageBytes.ToArray());
        Uri? imageUri = result.Result;

        _logger.LogInformation($"Generated image: {imageUri}");
        return imageUri;
    }

    public async Task<Uri?> UploadImage(byte[] imageBytes)
    {
        try
        {
            _logger.LogInformation("Uploading image to Blob Storage");
            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_blobContainerName);

            var imageName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-{new Random().Next(100000, 999999)}.jpg";

            var blobClient = blobContainerClient.GetBlobClient(imageName);

            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            var imageUri = blobClient.Uri;
            _logger.LogInformation($"Uploaded image to Blob Storage: {imageUri}");
            return imageUri;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading image to Blob Storage: {ex.Message}");
        }

        return null;
    }
}