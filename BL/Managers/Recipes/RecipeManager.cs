using AutoMapper;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
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

    public RecipeDto GetRecipeDtoById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var recipe = _recipeRepository.ReadRecipeById(parsedGuid);
        return _mapper.Map<RecipeDto>(recipe);
    }

    public RecipeDto GetRecipeDtoByName(string name)
    {
        var recipe = _recipeRepository.ReadRecipeByName(name);
        return _mapper.Map<RecipeDto>(recipe);
    }

    public ICollection<RecipeDto> GetRecipesCollectionByName(string name)
    {
        var recipes = _recipeRepository.ReadRecipesCollectionByName(name);
        return _mapper.Map<ICollection<RecipeDto>>(recipes);
    }

    public RecipeDto? CreateRecipe(string name)
    {
        byte attempts = 0;

        while (attempts < 3)
        {
            try
            {
                var generatedRecipeJson = _llmService.GenerateRecipe(name);

                if (!RecipeValidation(generatedRecipeJson))
                {
                    _logger.LogError("Recipe validation failed");
                    throw new RecipeValidationFailException("Recipe validation failed");
                }

                var recipe = ConvertGeneratedRecipe(generatedRecipeJson);

                _recipeRepository.CreateRecipe(recipe);

                return _mapper.Map<RecipeDto>(recipe);
            }
            catch (JsonReaderException ex)
            {
                break;
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

    private bool RecipeValidation(string recipeJson)
    {
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

    private Recipe ConvertGeneratedRecipe(string generatedRecipe)
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

        Enum.TryParse<RecipeType>(recipeType!.ToString(), out var recipeTypeEnum);
        Enum.TryParse<Difficulty>(difficulty!.ToString(), out var difficultyEnum);

        var dietPreference = ConvertDietPreference(diet);

        var ingredientQuantities = ConvertIngredientQuantities(ingredients);

        var instructionSteps = ConvertInstructionSteps(instructions);

        Recipe recipe = new Recipe()
        {
            RecipeName = recipeName!.ToString(),
            Description = description!.ToString(),
            RecipeType = recipeTypeEnum,
            Preferences = new List<Preference>() { dietPreference },
            CookingTime = int.Parse(cookingTime!.ToString()),
            Difficulty = difficultyEnum,
            CreatedAt = DateTime.UtcNow,
            AmountOfPeople = int.Parse(amountOfPeople!.ToString()),

            Ingredients = ingredientQuantities,
            Instructions = instructionSteps
        };

        return recipe;
    }

    private Preference ConvertDietPreference(JToken? diet)
    {
        Preference dietPreference = null;
        ICollection<Preference> standardPreferences = _preferenceRepository.ReadStandardPreferences();
        if (diet is not null)
        {
            foreach (var preference in standardPreferences)
            {
                if (preference.PreferenceName == diet.ToString())
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
                PreferenceName = diet.ToString(),
                StandardPreference = false
            };
            _preferenceRepository.CreatePreference(dietPreference);
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

    private ICollection<IngredientQuantity> ConvertIngredientQuantities(JToken? ingredients)
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
                newIngredient =
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
                _ingredientRepository.CreateIngredient(newIngredient);
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