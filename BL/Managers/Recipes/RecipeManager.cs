using System.Collections;
using AutoMapper;
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

    public async Task<Result<RecipeDto>> GetRecipeDtoById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var recipeResult = await _recipeRepository.ReadRecipeWithRelatedInformationByIdNoTracking(parsedGuid);
        if (!recipeResult.IsSuccess)
        {
            return Result<RecipeDto>.Failure(recipeResult.ErrorMessage!, recipeResult.FailureType);
        }

        var recipe = recipeResult.Value!;
        return Result<RecipeDto>.Success(_mapper.Map<RecipeDto>(recipe));
    }

    public async Task<Result<RecipeDto>> GetRecipeDtoByName(string name)
    {
        var recipeResult = await _recipeRepository.ReadRecipeByNameNoTracking(name);
        if (!recipeResult.IsSuccess)
        {
            return Result<RecipeDto>.Failure(recipeResult.ErrorMessage!, recipeResult.FailureType);
        }

        var recipe = recipeResult.Value!;
        return Result<RecipeDto>.Success(_mapper.Map<RecipeDto>(recipe));
    }

    public async Task<Result<ICollection<RecipeDto>>> GetRecipesCollectionByName(string name)
    {
        var recipesResult = await _recipeRepository.ReadRecipesCollectionByNameNoTracking(name);
        if (!recipesResult.IsSuccess)
        {
            return Result<ICollection<RecipeDto>>.Failure(recipesResult.ErrorMessage!, recipesResult.FailureType);
        }

        var recipes = recipesResult.Value!;

        return Result<ICollection<RecipeDto>>.Success(_mapper.Map<ICollection<RecipeDto>>(recipes));
    }

    public async Task<Result<ICollection<RecipeDto>>> GetFilteredRecipeCollection(string recipeName,
        Difficulty difficulty,
        RecipeType recipeType, int cooktime, List<string> ingredients)
    {
        var recipesResult =
            await _recipeRepository.GetFilteredRecipesNoTracking(recipeName, difficulty, recipeType, cooktime,
                ingredients);

        if (!recipesResult.IsSuccess)
        {
            return Result<ICollection<RecipeDto>>.Failure(recipesResult.ErrorMessage!, recipesResult.FailureType);
        }

        var recipes = recipesResult.Value!;

        return Result<ICollection<RecipeDto>>.Success(_mapper.Map<ICollection<RecipeDto>>(recipes));
    }

    public async Task<Result<int>> GetAmountOfRecipes()
    {
        return await _recipeRepository.GetRecipeCount();
    }

    public async Task<Result<RecipeDto?>> CreateRecipe(RecipeFilterDto request, List<PreferenceDto> preferences)
    {
        byte attempts = 0;
        var prompt = LlmSettingsService.BuildPrompt(request, preferences);

        while (attempts < 3)
        {
            _logger.LogInformation("Attempting to create recipe (Attempt: {Attempts})", attempts + 1);

            // Generate the recipe
            var generatedRecipeResult = await TryGenerateRecipe(prompt);

            if (!generatedRecipeResult.IsSuccess)
            {
                _logger.LogWarning("Recipe generation failed: {ErrorMessage}", generatedRecipeResult.ErrorMessage);
                if (generatedRecipeResult.FailureType == ResultFailureType.Error)
                {
                    _logger.LogInformation("Retrying to create recipe...");
                    attempts++;
                    continue;
                }

                return Result<RecipeDto?>.Failure(generatedRecipeResult.ErrorMessage!,
                    generatedRecipeResult.FailureType);
            }

            var recipe = generatedRecipeResult.Value!;

            // Save the recipe to the repository
            var saveResult = await _recipeRepository.CreateRecipe(recipe);
            if (!saveResult.IsSuccess)
            {
                return Result<RecipeDto?>.Failure(saveResult.ErrorMessage!, saveResult.FailureType);
            }

            // Optionally generate and save the image
            if (string.IsNullOrEmpty(recipe.ImagePath))
            {
                var imageResult = TryGenerateAndSaveImage(recipe);
                if (imageResult.IsSuccess)
                {
                    recipe.ImagePath = imageResult.Value!;
                    await _recipeRepository.UpdateRecipe(recipe);
                }
            }

            return Result<RecipeDto?>.Success(_mapper.Map<RecipeDto>(recipe));
        }

        _logger.LogError("Failed to create recipe after {Attempts} attempts", attempts);
        return Result<RecipeDto?>.Failure("Failed to create recipe after multiple attempts", ResultFailureType.Error);
    }

    private async Task<Result<Recipe>> TryGenerateRecipe(string prompt)
    {
        var generatedRecipeJson = _llmService.GenerateRecipe(prompt);
        
        var validationResult = RecipeValidation(generatedRecipeJson);
        if (!validationResult.IsSuccess)
        {
            return Result<Recipe>.Failure(validationResult.ErrorMessage!, validationResult.FailureType);
        }

        var recipeResult = await ConvertGeneratedRecipe(generatedRecipeJson);
        if (!recipeResult.IsSuccess)
        {
            return Result<Recipe>.Failure(recipeResult.ErrorMessage!, recipeResult.FailureType);
        }

        return Result<Recipe>.Success(recipeResult.Value!);
    }

    private Result<string?> TryGenerateAndSaveImage(Recipe recipe)
    {
        try
        {
            var imageUri = _llmService.GenerateRecipeImage($"{recipe.RecipeName} {recipe.Description}");
            return Result<string?>.Success(imageUri?.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to generate image for recipe {RecipeName}: {ErrorMessage}", recipe.RecipeName,
                ex.Message);
            return Result<string?>.Failure("Image generation failed", ResultFailureType.Error);
        }
    }


    public async Task<Result<ICollection<RecipeDto>>> CreateBatchRecipes(string input)
    {
        var recipes = new List<RecipeDto>();
        JObject recipeJson;
        try
        {
            recipeJson = JObject.Parse(input);
        }catch (JsonReaderException ex)
        {
            _logger.LogError("Failed to parse JSON: {ErrorMessage}", ex.Message);
            return Result<ICollection<RecipeDto>>.Failure("Failed to parse JSON", ResultFailureType.Error);
        }
        
        var recipeArray = recipeJson["recipes"];

        foreach (var recipe in recipeArray)
        {
            var recipeString = recipe.ToString();

            var createdRecipeResult = await ConvertGeneratedRecipe(recipeString);
            if (!createdRecipeResult.IsSuccess)
            {
                return Result<ICollection<RecipeDto>>.Failure(createdRecipeResult.ErrorMessage!,
                    createdRecipeResult.FailureType);
            }

            var createdRecipe = createdRecipeResult.Value!;

            await _recipeRepository.CreateRecipe(createdRecipe);

            recipes.Add(_mapper.Map<RecipeDto>(createdRecipe));
        }

        return Result<ICollection<RecipeDto>>.Success(recipes);
    }
    
    public async Task<Result<ICollection<RecipeSuggestionDto>>> CreateRecipeSuggestions(RecipeFilterDto request, List<PreferenceDto> preferences, int amount = 5)
    {
        _logger.LogInformation("Creating recipe suggestions with prompt:\n name: {recipeName} \n difficulty: {Difficulty} \n mealtype: {MealType} \n cooktime: {CookTime}", request.RecipeName, request.Difficulty, request.MealType, request.CookTime);
        
        var prompt = LlmSettingsService.BuildPrompt(request, preferences);
        var suggestions = _llmService.GenerateMultipleRecipeNamesAndDescriptions(prompt, amount);

        if (suggestions[0].StartsWith("NOT_POSSIBLE"))
        {
            _logger.LogError("Recipe generation failed: {ErrorMessage}", suggestions[0]);
            return Result<ICollection<RecipeSuggestionDto>>.Failure("Recipe generation failed", ResultFailureType.Error);
        }
        
        var recipeSuggestions = suggestions
            .Select(suggestion => suggestion.Split(":"))
            .Select(splitSuggestion => new RecipeSuggestionDto
            {
                RecipeName = splitSuggestion[0].Trim(),
                Description = splitSuggestion[1].Trim()
            })
            .ToList();

        return Result<ICollection<RecipeSuggestionDto>>.Success(recipeSuggestions);
    }

    public async Task<Result<Unit>> CreateBatchRandomRecipes(int amount, List<PreferenceDto>? preferences)
    {
        if (preferences == null)
        {
            preferences = new List<PreferenceDto>();
        }

        if (amount <= 0)
            return Result<Unit>.Failure("Amount of recipes must be greater than 0", ResultFailureType.Error);
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
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> RemoveUnusedRecipes()
    {
        return await _recipeRepository.DeleteUnusedRecipes();
    }

    private Result<Unit> RecipeValidation(string recipeJson)
    {
        if (recipeJson.StartsWith("\"NOT_POSSIBLE"))
        {
            var errorMessage = recipeJson.Substring(31).Trim('"');
            _logger.LogError("Recipe generation failed: {ErrorMessage}", errorMessage);
            return Result<Unit>.Failure(errorMessage, ResultFailureType.Error);
        }

        var jsonSchema = LlmSettingsService.RecipeJsonSchema;
        JSchema schema;
        JObject recipe;
        try
        {
            schema = JSchema.Parse(jsonSchema);
            recipe = JObject.Parse(recipeJson);
        }catch (JsonReaderException ex)
        {
            _logger.LogError("Failed to parse JSON: {ErrorMessage}", ex.Message);
            return Result<Unit>.Failure("Failed to parse JSON", ResultFailureType.Error);
        }
        

        if (recipe.IsValid(schema, out IList<string> validationErrors))
            return Result<Unit>.Success(new Unit());

        _logger.LogError("Recipe validation failed");
        foreach (var error in validationErrors)
        {
            _logger.LogError("Recipe validation error: {Error}", error);
        }

        return Result<Unit>.Failure("Recipe validation failed", ResultFailureType.Error);
    }

    private async Task<Result<Recipe>> ConvertGeneratedRecipe(string generatedRecipe)
    {
        _logger.LogInformation("Converting generated recipe JSON object to Recipe object");
        JObject generatedRecipeJson;
        try
        {
             generatedRecipeJson = JObject.Parse(generatedRecipe);
        }
        catch (JsonReaderException ex)
        {
            _logger.LogError("Failed to parse JSON: {ErrorMessage}", ex.Message);
            return Result<Recipe>.Failure("Failed to parse JSON", ResultFailureType.Error);
        }

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

        var dietPreferenceResult = await ConvertDietPreference(diet);
        if (!dietPreferenceResult.IsSuccess)
        {
            return Result<Recipe>.Failure(dietPreferenceResult.ErrorMessage!, dietPreferenceResult.FailureType);
        }

        var dietPreference = dietPreferenceResult.Value!;

        var ingredientQuantitiesResult = await ConvertIngredientQuantities(ingredients);
        if (!ingredientQuantitiesResult.IsSuccess)
        {
            return Result<Recipe>.Failure(ingredientQuantitiesResult.ErrorMessage!,
                ingredientQuantitiesResult.FailureType);
        }

        var ingredientQuantities = ingredientQuantitiesResult.Value!;

        var instructionStepsResult = ConvertInstructionSteps(instructions);
        if (!instructionStepsResult.IsSuccess)
        {
            return Result<Recipe>.Failure(instructionStepsResult.ErrorMessage!, instructionStepsResult.FailureType);
        }

        var instructionSteps = instructionStepsResult.Value!;

        Recipe recipe = new Recipe()
        {
            RecipeName = recipeName!.ToString(),
            Description = description!.ToString(),
            RecipeType = recipeTypeEnum,
            Preferences = new List<Preference>() { dietPreference },
            CookingTime = int.Parse(cookingTime!.ToString()),
            Difficulty = difficultyEnum,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow,
            AmountOfPeople = int.Parse(amountOfPeople!.ToString()),
            ImagePath = imagePath?.ToString() ?? string.Empty,
            Ingredients = ingredientQuantities,
            Instructions = instructionSteps
        };

        return Result<Recipe>.Success(recipe);
    }

    private async Task<Result<Preference>> ConvertDietPreference(JToken? diet)
    {
        Preference dietPreference = null;
        var standardPreferencesResult = await _preferenceRepository.ReadStandardPreferences();
        if (!standardPreferencesResult.IsSuccess)
        {
            return Result<Preference>.Failure(standardPreferencesResult.ErrorMessage!,
                standardPreferencesResult.FailureType);
        }

        var standardPreferences = standardPreferencesResult.Value!;

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
            return await _preferenceRepository.CreatePreference(dietPreference);
        }

        return Result<Preference>.Success(dietPreference);
    }

    private static Result<ICollection<InstructionStep>> ConvertInstructionSteps(JToken? instructions)
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

        return Result<ICollection<InstructionStep>>.Success(instructionSteps);
    }

    private async Task<Result<ICollection<IngredientQuantity>>> ConvertIngredientQuantities(JToken? ingredients)
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

            var newIngredientResult = await
                _ingredientRepository.ReadIngredientByNameAndMeasurementType(ingredientName,
                    ingredientMeasurementType);
            if (!newIngredientResult.IsSuccess)
            {
                _logger.LogInformation("Ingredient not found: {IngredientName}", ingredientName);
                _logger.LogInformation("Creating new ingredient: {IngredientName}", ingredientName);
                newIngredient = new Ingredient()
                {
                    IngredientName = ingredientName,
                    Measurement = ingredientMeasurementType
                };
                var createIngredientResult = await _ingredientRepository.CreateIngredient(newIngredient);
                if (!createIngredientResult.IsSuccess)
                {
                    return Result<ICollection<IngredientQuantity>>.Failure(createIngredientResult.ErrorMessage!, createIngredientResult.FailureType);
                }
                _logger.LogInformation("Created new ingredient: {IngredientName}", ingredientName);
            }
            else
            {
                newIngredient = newIngredientResult.Value!;
            }

            _logger.LogInformation("Adding ingredient to recipe: {IngredientName}", ingredientName);
            IngredientQuantity ingredientQuantity = new IngredientQuantity()
            {
                Ingredient = newIngredient,
                Quantity = ingredientAmount
            };
            ingredientQuantities.Add(ingredientQuantity);
        }

        return Result<ICollection<IngredientQuantity>>.Success(ingredientQuantities);
    }
}