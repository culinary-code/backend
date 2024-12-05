using System.ClientModel;
using System.Collections.ObjectModel;
using System.Net.Mime;
using Azure;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Configuration.Options;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public AzureOpenAIService(ILogger<AzureOpenAIService> logger, IOptions<AzureOpenAIOptions> azureOpenAiOptions, IOptions<AzureStorageOptions> azureStorageOptions)
    {
        var azureOpenAiOptionsValue = azureOpenAiOptions.Value;
        var azureStorageOptionsValue = azureStorageOptions.Value;
        
        _apiKey = azureOpenAiOptionsValue.ApiKey;
        _endpoint = azureOpenAiOptionsValue.Endpoint;
        _blobConnectionString = azureStorageOptionsValue.ConnectionString;
        _blobContainerName = azureStorageOptionsValue.ContainerName;

        _logger = logger;
        _azureClient = new AzureOpenAIClient(
            new Uri(_endpoint),
            new ApiKeyCredential(_apiKey)
        );
        _chatClient = _azureClient.GetChatClient("gpt-4o-mini");
        _imageClient = _azureClient.GetImageClient("dall-e-3");
    }

    public string[] GenerateMultipleRecipeNamesAndDescriptions(string message, int amount)
    {
        ChatCompletionOptions completionOptions = new ChatCompletionOptions
        {
            Temperature = (float?)0.6,
            TopP = (float?)0.9,
        };
        
        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage($"Based on the input, generate {amount} different recipe names along with a short description. Place each recipe on its own line, no new line between them. Do not add order numbers, name and description should be on the same line. Output no other information. Always respond in the Dutch language."),

            new UserChatMessage(message),
        ], completionOptions);

        var response = completion.Content[0].Text;
        
        var recipePrompts = response.Split("\n");
        return recipePrompts;
    }

    public string GenerateMultipleRecipes(string message, int amount)
    {
        ICollection<string> recipePrompts = GenerateMultipleRecipeNamesAndDescriptions(message, amount);
        
        var output = "{ \"recipes\": [";
        foreach (var recipePrompt in recipePrompts)
        {
            _logger.LogInformation(recipePrompt);
            var recipe = GenerateRecipe(recipePrompt);
            var imageUri = GenerateRecipeImage(recipePrompt);
            output += recipe.Remove(recipe.Length - 1) + ", \"image_path\": \"" + imageUri + "\"}, ";
        }
        output = output.Remove(output.Length - 2); // remove the last comma
        output += "]}";
        
        return output;
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
        var imagePrompt = GetRecipeImagePrompt(recipePrompt);

        ImageGenerationOptions options = new ImageGenerationOptions
        {
            Size = GeneratedImageSize.W1024xH1024,
            Quality = GeneratedImageQuality.Standard
        };

        _logger.LogInformation("Sending image generation request to Azure OpenAI API");

        byte attempts = 0;
        while (attempts < 3)
        {
            try
            {
                var response = _imageClient.GenerateImage(imagePrompt, options);

                // Azure returns an image URI that only lasts for 24 hours and is not accessible, so we need to download the image and upload it to Blob Storage
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
            catch (ClientResultException ex)
            {
                _logger.LogError("Failed to generate image: {ErrorMessage}", ex.Message);
                _logger.LogInformation("Attempt: {Attempts}", attempts);
                _logger.LogInformation("Retrying to generate image");
                attempts++;
            }
        }

        return null;
    }

    private string GetRecipeImagePrompt(string recipePrompt)
    {
        ChatCompletionOptions completionOptions = new ChatCompletionOptions
        {
            Temperature = (float?)0.9,
            TopP = (float?)0.9,
        };

        var systemPrompt = $@"""
                           Write a long prompt for DALL-E to generate an image. 
                            Create a highly photorealistic close up image of the recipe. 
                            The scene is set in a cozy indoor kitchen, with soft, natural light coming from a large window to the side, casting delicate shadows around the plate or bowl. 
                            A few modern kitchen utensils and a wooden spoon are subtly visible in the blurred background, adding a warm and homey atmosphere.
                            Focus on creating a prompt to have an image generated.
                            The recipe is: {recipePrompt}.                           
                            """;

        _logger.LogInformation("Sending request for image prompt to Azure OpenAI API");

        ChatCompletion completion = _chatClient.CompleteChat(
        [
            new SystemChatMessage(systemPrompt),

            new UserChatMessage(recipePrompt),
        ], completionOptions);

        _logger.LogInformation("Received response for image prompt from Azure OpenAI API");
        _logger.LogInformation($"{completion.Content[0].Text}");

        var imagePrompt = completion.Content[0].Text;
        return imagePrompt;
    }

    private async Task<Uri?> UploadImage(byte[] imageBytes)
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
                await blobClient.UploadAsync(stream, new BlobHttpHeaders {ContentType = MediaTypeNames.Image.Jpeg});
            }

            var imageUri = blobClient.Uri;
            _logger.LogInformation($"Uploaded image to Blob Storage: {imageUri}");
            return imageUri;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Error uploading image to Blob Storage: {ex.Message}");
        }

        return null;
    }
}
