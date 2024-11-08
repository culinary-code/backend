using System.Text;

namespace BL.ExternalSources.Llm;

using System;
using Newtonsoft.Json;

public class LocalLlmService : ILlmService
{
    private readonly string _localServerUrl;

    public LocalLlmService()
    {
        DotNetEnv.Env.Load();
        _localServerUrl = DotNetEnv.Env.GetString("LOCAL_LLM_SERVER_URL");
    }

    private static readonly HttpClient Client = new HttpClient();

    public string GenerateRecipe(string message)
    {
        var systemPrompt = LlmSettingsService.SystemPrompt;

        var endpoint = "/v1/chat/completions";

        var requestBody = new
        {
            model = "Llama 3.2 3B Instruct",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = message
                }
            },
            max_tokens = 100,
            temperature = 0.8
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = new Uri(_localServerUrl + endpoint);

        try
        {
            var response = Client.PostAsync(requestUri, content);

            response.Result.EnsureSuccessStatusCode();

            var responseBody = response.Result.Content.ReadAsStringAsync();

            var responseBodyJson = JsonConvert.DeserializeObject<dynamic>(responseBody.Result);
            var completion = responseBodyJson["choices"][0]["message"]["content"];

            Console.WriteLine(completion);
            return completion;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return "Sorry, I couldn't process your request. Please try again later.";
    }

    public string GenerateMultipleRecipes(string message, int amount)
    {
        throw new NotImplementedException();
    }

    public Uri? GenerateRecipeImage(string recipePrompt)
    {
        throw new NotImplementedException();
    }
}