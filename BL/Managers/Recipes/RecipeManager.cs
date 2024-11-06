using System;
using System.Collections;
using System.Collections.Generic;
using AutoMapper;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;
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
        IPreferenceRepository preferenceRepository, IIngredientRepository ingredientRepository, ILogger<RecipeManager> logger)
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

    public RecipeDto CreateRecipe(string name)
    {
        byte attempts = 0;

        while (attempts < 3)
        {
            try
            {
                try
                {
                    var generatedRecipeJson = _llmService.GenerateRecipe(name);

                    if (!RecipeValidation(generatedRecipeJson)) throw new Exception("Recipe validation failed");

                    var recipe = ConvertGeneratedRecipe(generatedRecipeJson);

                    _recipeRepository.CreateRecipe(recipe);

                    return _mapper.Map<RecipeDto>(recipe);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to get recipe from LLM", e);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to create recipe: {ErrorMessage}", e.Message);
                _logger.LogInformation("Attempt: {Attempts}", attempts);
                _logger.LogInformation("Retrying to create recipe");
                attempts++;
            }
        }
        
        _logger.LogError("Failed to create recipe after 3 attempts");
        throw new Exception("Failed to create recipe");
    }

    private bool RecipeValidation(string recipeJson)
    {
        var jsonSchema = LlmSettingsService.RecipeJsonSchema;
        JSchema schema = JSchema.Parse(jsonSchema);
        JObject recipe = JObject.Parse(recipeJson);

        if (recipe.IsValid(schema))
        {
            return true;
        }

        return false;
    }

    private Recipe ConvertGeneratedRecipe(string generatedRecipe)
    {
        var generatedRecipeJson = JObject.Parse(generatedRecipe);

        generatedRecipeJson.TryGetValue("recipeName", out var recipeName);
        generatedRecipeJson.TryGetValue("description", out var description);
        generatedRecipeJson.TryGetValue("recipeType", out var recipeType);
        generatedRecipeJson.TryGetValue("diet", out var diet);
        generatedRecipeJson.TryGetValue("cookingTime", out var cookingTime);
        generatedRecipeJson.TryGetValue("difficulty", out var difficulty);
        generatedRecipeJson.TryGetValue("amountOfPeople", out var amountOfPeople);

        generatedRecipeJson.TryGetValue("ingredients", out var ingredients);
        generatedRecipeJson.TryGetValue("recipeSteps", out var instructions);


        Enum.TryParse<RecipeType>(recipeType!.ToString(), out var recipeTypeEnum);
        Enum.TryParse<Difficulty>(difficulty!.ToString(), out var difficultyEnum);

        ICollection<Preference> standardPreferences = _preferenceRepository.ReadStandardPreferences();
        Preference dietPreference = null;
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

        // TODO: amount_of_people not in Recipe class?

        ICollection<IngredientQuantity> ingredientQuantities = new List<IngredientQuantity>();
        foreach (var ingredient in ingredients)
        {
            var ingredientName = ingredient["name"].ToString().ToLower();
            var ingredientAmount = int.Parse(ingredient["amount"].ToString());
            var ingredientMeasurementTypeName = ingredient["measurementType"].ToString();
            var ingredientMeasurementType = Enum.Parse(typeof(MeasurementType), ingredientMeasurementTypeName);

            Ingredient newIngredient = null;
            try
            {
                newIngredient = _ingredientRepository.ReadIngredientByName(ingredientName);
            }
            catch (Exception e)
            {
                newIngredient = new Ingredient()
                {
                    IngredientName = ingredientName!.ToString(),
                    Measurement =
                        (MeasurementType)Enum.Parse(typeof(MeasurementType), ingredientMeasurementType!.ToString())
                };
                _ingredientRepository.CreateIngredient(newIngredient);
            }

            IngredientQuantity ingredientQuantity = new IngredientQuantity()
            {
                Ingredient = newIngredient,
                Quantity = ingredientAmount
            };
            ingredientQuantities.Add(ingredientQuantity);
        }

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

        Recipe recipe = new Recipe()
        {
            RecipeName = recipeName!.ToString(),
            Description = description!.ToString(),
            RecipeType = recipeTypeEnum,
            Preferences = new List<Preference>() { dietPreference },
            CookingTime = int.Parse(cookingTime!.ToString()),
            Difficulty = difficultyEnum,
            CreatedAt = DateTime.Now,

            Ingredients = ingredientQuantities,
            Instructions = instructionSteps
        };

        foreach (var ingredientQuantity in recipe.Ingredients)
        {
            ingredientQuantity.Recipe = recipe;
        }

        foreach (var instructionStep in recipe.Instructions)
        {
            instructionStep.Recipe = recipe;
        }

        _recipeRepository.CreateRecipe(recipe);

        return recipe;
    }
}