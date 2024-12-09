﻿using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Accounts;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace BL.Managers.Recipes;

public class RecipeManager : IRecipeManager
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IPreferenceRepository _preferenceRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IMapper _mapper;
    private readonly ILlmService _llmService;
    private readonly ILogger<RecipeManager> _logger;

    public RecipeManager(IRecipeRepository recipeRepository, IMapper mapper, ILlmService llmService,
        IPreferenceRepository preferenceRepository, IIngredientRepository ingredientRepository,
        ILogger<RecipeManager> logger)
    {
        _recipeRepository = recipeRepository;
        _mapper = mapper;
        _llmService = llmService;
        _preferenceRepository = preferenceRepository;
        _ingredientRepository = ingredientRepository;
        _logger = logger;
    }

    public async Task<RecipeDto> GetRecipeDtoById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var recipe = await _recipeRepository.ReadRecipeWithRelatedInformationByIdNoTracking(parsedGuid);
        return _mapper.Map<RecipeDto>(recipe);
    }

    public async Task<RecipeDto> GetRecipeDtoByName(string name)
    {
        var recipe = await _recipeRepository.ReadRecipeByNameNoTracking(name);
        return _mapper.Map<RecipeDto>(recipe);
    }

    public async Task<ICollection<RecipeDto>> GetRecipesCollectionByName(string name)
    {
        var recipes = await _recipeRepository.ReadRecipesCollectionByNameNoTracking(name);
        return _mapper.Map<ICollection<RecipeDto>>(recipes);
    }

    public async Task<ICollection<RecipeDto>> GetFilteredRecipeCollection(string recipeName, Difficulty difficulty,
        RecipeType recipeType, int cooktime, List<string> ingredients)
    {
        var recipes =
            await _recipeRepository.GetFilteredRecipesNoTracking(recipeName, difficulty, recipeType, cooktime, ingredients);
        return _mapper.Map<ICollection<RecipeDto>>(recipes);
    }

    public async Task<int> GetAmountOfRecipes()
    {
        return await _recipeRepository.GetRecipeCount();
    }

    public async Task<RecipeDto?> CreateRecipe(RecipeFilterDto request, List<PreferenceDto> preferences)
    {
        byte attempts = 0;
        var prompt = LlmSettingsService.BuildPrompt(request, preferences);
        while (attempts < 3)
        {
            try
            {
                var generatedRecipeJson = _llmService.GenerateRecipe(prompt);

                if (!RecipeValidation(generatedRecipeJson))
                {
                    _logger.LogError("Recipe validation failed");
                    throw new RecipeValidationFailException("Recipe validation failed");
                }

                var recipe = await ConvertGeneratedRecipe(generatedRecipeJson);

                await _recipeRepository.CreateRecipe(recipe);

                if (!string.IsNullOrEmpty(recipe.ImagePath))
                {
                    return _mapper.Map<RecipeDto>(recipe);
                }

                
                var imageUri = _llmService.GenerateRecipeImage($"{recipe.RecipeName} {recipe.Description}");
                if (imageUri is not null)
                {
                    recipe.ImagePath = imageUri!.ToString();
                    await _recipeRepository.UpdateRecipe(recipe);
                }

                return _mapper.Map<RecipeDto>(recipe);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError("Failed to parse JSON: {ErrorMessage}", ex.Message);
                _logger.LogInformation("Attempt: {Attempts}", attempts);
                _logger.LogInformation("Retrying to create recipe");
                attempts++;
            }
            catch (RecipeNotAllowedException ex)
            {
                _logger.LogError("Recipe not allowed: {ErrorMessage}", ex.ReasonMessage);
                throw new RecipeNotAllowedException(reasonMessage: ex.ReasonMessage);
            }
            catch (RecipeValidationFailException ex)
            {
                _logger.LogError("Failed to create recipe: {ErrorMessage}", ex.Message);
                _logger.LogInformation("Attempt: {Attempts}", attempts);
                _logger.LogInformation("Retrying to create recipe");
                attempts++;
            }
        }

        _logger.LogError("Failed to create recipe after 3 attempts");
        return null;
    }

    public async Task<ICollection<RecipeSuggestionDto>> CreateRecipeSuggestions(RecipeFilterDto request, List<PreferenceDto> preferences, int amount = 5)
    {
        _logger.LogInformation("Creating recipe suggestions with prompt:\n name: {recipeName} \n difficulty: {Difficulty} \n mealtype: {MealType} \n cooktime: {CookTime}", request.RecipeName, request.Difficulty, request.MealType, request.CookTime);
        
        var prompt = LlmSettingsService.BuildPrompt(request, preferences);
        var suggestions = _llmService.GenerateMultipleRecipeNamesAndDescriptions(prompt, amount);

        if (suggestions[0].StartsWith("NOT_POSSIBLE"))
        {
            _logger.LogError("Recipe generation failed: {ErrorMessage}", suggestions[0]);
            return new List<RecipeSuggestionDto>();
        }
        
        var recipeSuggestions = suggestions
            .Select(suggestion => suggestion.Split(":"))
            .Select(splitSuggestion => new RecipeSuggestionDto
            {
                RecipeName = splitSuggestion[0].Trim(),
                Description = splitSuggestion[1].Trim()
            })
            .ToList();

        return recipeSuggestions;
    }

    public async Task<ICollection<RecipeDto>> CreateBatchRecipes(string input)
    {
        var recipes = new List<RecipeDto>();
        var recipeJson = JObject.Parse(input);
        var recipeArray = recipeJson["recipes"];

        foreach (var recipe in recipeArray)
        {
            var recipeString = recipe.ToString();

            var createdRecipe = await ConvertGeneratedRecipe(recipeString);

            await _recipeRepository.CreateRecipe(createdRecipe);

            recipes.Add(_mapper.Map<RecipeDto>(createdRecipe));
        }

        return recipes;
    }
    
    public async Task CreateBatchRandomRecipes(int amount, List<PreferenceDto>? preferences)
    {
        if (preferences == null)
        {
            preferences = new List<PreferenceDto>();
        }
        if (amount <=0) return;  
        var recipeNames = _llmService.GenerateMultipleRecipeNamesAndDescriptions("random", amount);
        
        // List to hold all the tasks for concurrent execution
        var tasks = new List<Task>();

        for (int i = 0; i < recipeNames.Length && i < amount; i++)
        {
            // Capture the current index in the loop to avoid closure issues
            var recipeName = recipeNames[i];

            // Create a new request for each recipe
            var request = new RecipeFilterDto
            {
                RecipeName = recipeName
            };
            
            // Add the task to the list

            tasks.Add(CreateRecipe(request, preferences));
        }

        // Await all tasks to complete
        await Task.WhenAll(tasks);
    }

    public async Task RemoveUnusedRecipes()
    {
        await _recipeRepository.DeleteUnusedRecipes();
    }

    private bool RecipeValidation(string recipeJson)
    {
        if (recipeJson.StartsWith("\"NOT_POSSIBLE"))
        {
            var errorMessage = recipeJson.Substring(31).Trim('"');
            _logger.LogError("Recipe generation failed: {ErrorMessage}", errorMessage);
            throw new RecipeNotAllowedException(reasonMessage: errorMessage);
        }

        var jsonSchema = LlmSettingsService.RecipeJsonSchema;
        JSchema schema = JSchema.Parse(jsonSchema);
        JObject recipe = JObject.Parse(recipeJson);

        if (recipe.IsValid(schema, out IList<String> validationErrors))
            return true;

        _logger.LogError("Recipe validation failed");
        foreach (var error in validationErrors)
        {
            _logger.LogError("Recipe validation error: {Error}", error);
        }

        return false;
    }

    private async Task<Recipe> ConvertGeneratedRecipe(string generatedRecipe)
    {
        _logger.LogInformation("Converting generated recipe JSON object to Recipe object");

        var generatedRecipeJson = JObject.Parse(generatedRecipe);

        generatedRecipeJson.TryGetValue("recipeName", out var recipeName);
        generatedRecipeJson.TryGetValue("description", out var description);
        generatedRecipeJson.TryGetValue("recipeType", out var recipeType);
        generatedRecipeJson.TryGetValue("diet", out var diet);
        generatedRecipeJson.TryGetValue("cookingTime", out var cookingTime);
        generatedRecipeJson.TryGetValue("difficulty", out var difficulty);
        generatedRecipeJson.TryGetValue("amount_of_people", out var amountOfPeople);


        generatedRecipeJson.TryGetValue("ingredients", out var ingredients);
        generatedRecipeJson.TryGetValue("recipeSteps", out var instructions);

        generatedRecipeJson.TryGetValue("image_path", out var imagePath);

        Enum.TryParse<RecipeType>(recipeType!.ToString(), out var recipeTypeEnum);
        Enum.TryParse<Difficulty>(difficulty!.ToString(), out var difficultyEnum);

        var dietPreference = await ConvertDietPreference(diet);

        var ingredientQuantities = await ConvertIngredientQuantities(ingredients);

        var instructionSteps = ConvertInstructionSteps(instructions);

        Recipe recipe = new Recipe()
        {
            RecipeName = recipeName!.ToString(),
            Description = description!.ToString(),
            RecipeType = recipeTypeEnum,
            Preferences = new List<Preference>() {dietPreference},
            CookingTime = int.Parse(cookingTime!.ToString()),
            Difficulty = difficultyEnum,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow,
            AmountOfPeople = int.Parse(amountOfPeople!.ToString()),
            ImagePath = imagePath?.ToString() ?? string.Empty,
            Ingredients = ingredientQuantities,
            Instructions = instructionSteps
        };

        return recipe;
    }

    private async Task<Preference> ConvertDietPreference(JToken? diet)
    {
        Preference dietPreference = null;
        ICollection<Preference> standardPreferences = await _preferenceRepository.ReadStandardPreferences();
        string dietString = diet?.ToString()!;
        if (diet is not null)
        {
            foreach (var preference in standardPreferences)
            {
                if (string.Equals(preference.PreferenceName, dietString, StringComparison.InvariantCultureIgnoreCase))
                {
                    dietPreference = preference;
                    break;
                }
            }
        }

        if (dietPreference is null)
        {
            dietPreference = new Preference()
            {
                PreferenceName = dietString,
                StandardPreference = false
            };
            await _preferenceRepository.CreatePreference(dietPreference);
        }

        return dietPreference;
    }

    private static ICollection<InstructionStep> ConvertInstructionSteps(JToken? instructions)
    {
        ICollection<InstructionStep> instructionSteps = new List<InstructionStep>();
        foreach (var instruction in instructions)
        {
            var stepNumber = int.Parse(instruction["stepNumber"].ToString());
            var instructionText = instruction["instruction"].ToString();

            InstructionStep instructionStep = new InstructionStep()
            {
                StepNumber = stepNumber,
                Instruction = instructionText
            };
            instructionSteps.Add(instructionStep);
        }

        return instructionSteps;
    }

    private async Task<ICollection<IngredientQuantity>> ConvertIngredientQuantities(JToken? ingredients)
    {
        ICollection<IngredientQuantity> ingredientQuantities = new List<IngredientQuantity>();
        foreach (var ingredient in ingredients!)
        {
            var ingredientName = ingredient["name"]!.ToString().ToLower();
            var ingredientAmount = float.Parse(ingredient["amount"]!.ToString());
            var ingredientMeasurementTypeName = ingredient["measurementType"]!.ToString();
            MeasurementType ingredientMeasurementType =
                (MeasurementType)Enum.Parse(typeof(MeasurementType), ingredientMeasurementTypeName);

            _logger.LogInformation("Converting ingredient: {IngredientName}", ingredientName);
            Ingredient newIngredient;
            try
            {
                newIngredient = await 
                    _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredientName,
                        ingredientMeasurementType);
            }
            catch (IngredientNotFoundException ex)
            {
                _logger.LogInformation("Ingredient not found: {IngredientName}", ingredientName);
                _logger.LogInformation("Creating new ingredient: {IngredientName}", ingredientName);
                newIngredient = new Ingredient()
                {
                    IngredientName = ingredientName,
                    Measurement = ingredientMeasurementType
                };
                await _ingredientRepository.CreateIngredient(newIngredient);
                _logger.LogInformation("Created new ingredient: {IngredientName}", ingredientName);
            }

            _logger.LogInformation("Adding ingredient to recipe: {IngredientName}", ingredientName);
            IngredientQuantity ingredientQuantity = new IngredientQuantity()
            {
                Ingredient = newIngredient,
                Quantity = ingredientAmount
            };
            ingredientQuantities.Add(ingredientQuantity);
        }

        return ingredientQuantities;
    }
}