using AutoMapper;
using BL.DTOs.Accounts;
using BL.DTOs.Recipes;
using BL.DTOs.Recipes.Ingredients;
using BL.ExternalSources.Llm;
using BL.Managers.Recipes;
using CulinaryCode.Tests.Util;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;
using Moq;

namespace CulinaryCode.Tests.BL.Managers;

public class RecipeManagerTests
{
    private readonly Mock<IRecipeRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly RecipeManager _recipeManager;
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<LlmSettingsService> _mockLlmSettingsService;
    private readonly Mock<IPreferenceRepository> _mockPreferenceRepository;
    private readonly Mock<IIngredientRepository> _mockIngredientRepository;
    private readonly Mock<ILogger<RecipeManager>> _loggerMock;
    private readonly ILogger<RecipeManager> _logger;

    public RecipeManagerTests()
    {
        
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<RecipeManager>();
        
        _mockRepository = new Mock<IRecipeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLlmService = new Mock<ILlmService>();
        _mockLlmSettingsService = new Mock<LlmSettingsService>();
        _mockPreferenceRepository = new Mock<IPreferenceRepository>();
        _mockIngredientRepository = new Mock<IIngredientRepository>();
        _loggerMock = new Mock<ILogger<RecipeManager>>();
        _recipeManager = new RecipeManager(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLlmService.Object,
            _mockPreferenceRepository.Object,
            _mockIngredientRepository.Object,
            //_logger
            _loggerMock.Object
        );
    }

    private static Recipe CreateRecipe(
        string recipeName,
        RecipeType recipeType,
        string description,
        int cookingTime,
        string imagePath,
        DateTime createdAt,
        Difficulty difficulty,
        int amountOfPeople,
        List<IngredientQuantity> ingredientQuantities,
        List<Preference> preferences,
        List<InstructionStep> instructions,
        List<Review> reviews)
    {
        return new Recipe
        {
            RecipeName = recipeName,
            Ingredients = ingredientQuantities,
            Preferences = preferences,
            RecipeType = recipeType,
            Description = description,
            CookingTime = cookingTime,
            Difficulty = difficulty,
            AmountOfPeople = amountOfPeople,
            ImagePath = imagePath,
            CreatedAt = createdAt,
            Instructions = instructions,
            Reviews = reviews,
        };
    }

    private static Recipe CreateSampleRecipe()
    {
        var ingredient1 = new Ingredient { IngredientName = "Wortel" };
        var ingredient2 = new Ingredient { IngredientName = "Appel" };

        var ingredientQuantities = new List<IngredientQuantity>
        {
            new IngredientQuantity { Ingredient = ingredient1, Quantity = 1 },
            new IngredientQuantity { Ingredient = ingredient2, Quantity = 100 }
        };

        var preferences = new List<Preference>
        {
            new Preference { PreferenceName = "Veel wortels", StandardPreference = false }
        };

        var instructions = new List<InstructionStep>
        {
            new InstructionStep { Instruction = "Voeg water toe", StepNumber = 1 },
            new InstructionStep { Instruction = "Breng het water aan de kook", StepNumber = 2 }
        };

        var reviews = new List<Review>
        {
            new Review { AmountOfStars = 5, Description = "Heerlijke pasta!" }
        };

        return CreateRecipe(
            recipeName: "Sample Recipe",
            recipeType: RecipeType.Snack,
            description: "Dit is een voorbeeld recept.",
            cookingTime: 10,
            difficulty: Difficulty.Easy,
            amountOfPeople: 4,
            imagePath: "https://picsum.photos/200/300",
            createdAt: DateTime.UtcNow,
            ingredientQuantities: ingredientQuantities,
            preferences: preferences,
            instructions: instructions,
            reviews: reviews
        );
    }

    private static string SampleValidRecipeJson()
    {
        string recipeJson = """
                            {
                                "recipeName": "Stoofvlees met frietjes",
                                "description": "Een klassiek Vlaams gerecht van langzaam gegaard rundvlees in een rijke, donkere saus, geserveerd met knapperige frietjes.",
                                "diet": "None",
                                "recipeType": "Dinner",
                                "cookingTime": 180,
                                "difficulty": "Intermediate",
                                "amount_of_people": 4,
                                "ingredients": [
                                    {
                                        "name": "Rundvlees",
                                        "amount": 1.5,
                                        "measurementType": "Kilogram"
                                    },
                                    {
                                        "name": "Ui",
                                        "amount": 2,
                                        "measurementType": "Piece"
                                    },
                                    {
                                        "name": "Wortel",
                                        "amount": 2,
                                        "measurementType": "Piece"
                                    },
                                    {
                                        "name": "Bier",
                                        "amount": 500,
                                        "measurementType": "Millilitre"
                                    },
                                    {
                                        "name": "Rundvleesbouillon",
                                        "amount": 500,
                                        "measurementType": "Millilitre"
                                    },
                                    {
                                        "name": "Laurierblad",
                                        "amount": 2,
                                        "measurementType": "Piece"
                                    },
                                    {
                                        "name": "Peper",
                                        "amount": 1,
                                        "measurementType": "Teaspoon"
                                    },
                                    {
                                        "name": "Zout",
                                        "amount": 1,
                                        "measurementType": "Teaspoon"
                                    },
                                    {
                                        "name": "Frietjes",
                                        "amount": 800,
                                        "measurementType": "Gram"
                                    }
                                ],
                                "recipeSteps": [
                                    {
                                        "stepNumber": 1,
                                        "instruction": "Snijd het rundvlees in blokjes en kruid met zout en peper."
                                    },
                                    {
                                        "stepNumber": 2,
                                        "instruction": "Verhit een pan met wat olie en bak het vlees rondom bruin."
                                    },
                                    {
                                        "stepNumber": 3,
                                        "instruction": "Voeg de gesneden ui en wortel toe en bak deze mee tot ze zacht zijn."
                                    },
                                    {
                                        "stepNumber": 4,
                                        "instruction": "Giet het bier en de rundvleesbouillon in de pan, voeg het laurierblad toe."
                                    },
                                    {
                                        "stepNumber": 5,
                                        "instruction": "Laat het geheel op een laag vuur ongeveer 2 tot 3 uur sudderen tot het vlees zacht is."
                                    },
                                    {
                                        "stepNumber": 6,
                                        "instruction": "Bereid de frietjes volgens de instructies op de verpakking."
                                    },
                                    {
                                        "stepNumber": 7,
                                        "instruction": "Serveer het stoofvlees met de frietjes en geniet van uw maaltijd."
                                    }
                                ]
                            }
                            """;

        return recipeJson;
    }

    private static RecipeDto CreateSampleRecipeFromJsonDto()
    {
        var ingredient1 = new IngredientDto { IngredientName = "rundvlees", Measurement = MeasurementType.Kilogram };
        var ingredient2 = new IngredientDto { IngredientName = "ui", Measurement = MeasurementType.Piece };
        var ingredient3 = new IngredientDto { IngredientName = "wortel", Measurement = MeasurementType.Piece };
        var ingredient4 = new IngredientDto { IngredientName = "bier", Measurement = MeasurementType.Millilitre };
        var ingredient5 = new IngredientDto { IngredientName = "rundvleesbouillon", Measurement = MeasurementType.Millilitre };
        var ingredient6 = new IngredientDto { IngredientName = "laurierblad", Measurement = MeasurementType.Piece };
        var ingredient7 = new IngredientDto { IngredientName = "peper", Measurement = MeasurementType.Teaspoon };
        var ingredient8 = new IngredientDto { IngredientName = "zout", Measurement = MeasurementType.Teaspoon };
        var ingredient9 = new IngredientDto { IngredientName = "frietjes", Measurement = MeasurementType.Gram };

        var ingredientQuantities = new List<IngredientQuantityDto>
        {
            new() { Ingredient = ingredient1, Quantity = (float)1.5 },
            new() { Ingredient = ingredient2, Quantity = 2, },
            new() { Ingredient = ingredient3, Quantity = 2, },
            new() { Ingredient = ingredient4, Quantity = 500, },
            new() { Ingredient = ingredient5, Quantity = 500, },
            new() { Ingredient = ingredient6, Quantity = 2, },
            new() { Ingredient = ingredient7, Quantity = 1, },
            new() { Ingredient = ingredient8, Quantity = 1, },
            new() { Ingredient = ingredient9, Quantity = 800, }
        };
        
        var instructions = new List<InstructionStepDto>
        {
            new() { StepNumber = 1, Instruction = "Snijd het rundvlees in blokjes en kruid met zout en peper." },
            new() { StepNumber = 2, Instruction = "Verhit een pan met wat olie en bak het vlees rondom bruin." },
            new() { StepNumber = 3, Instruction = "Voeg de gesneden ui en wortel toe en bak deze mee tot ze zacht zijn." },
            new() { StepNumber = 4, Instruction = "Giet het bier en de rundvleesbouillon in de pan, voeg het laurierblad toe." },
            new() { StepNumber = 5, Instruction = "Laat het geheel op een laag vuur ongeveer 2 tot 3 uur sudderen tot het vlees zacht is." },
            new() { StepNumber = 6, Instruction = "Bereid de frietjes volgens de instructies op de verpakking." },
            new() { StepNumber = 7, Instruction = "Serveer het stoofvlees met de frietjes en geniet van uw maaltijd." }
        };
        
        var preferences = new List<PreferenceDto>
        {
            new() { PreferenceName = "None", StandardPreference = false }
        };
        
        return new RecipeDto
        {
            RecipeName = "Stoofvlees met frietjes",
            Description = "Een klassiek Vlaams gerecht van langzaam gegaard rundvlees in een rijke, donkere saus, geserveerd met knapperige frietjes.",
            AmountOfPeople = 4,
            CookingTime = 180,
            Difficulty = Difficulty.Intermediate,
            Preferences = preferences,
            Ingredients = ingredientQuantities,
            Instructions = instructions,
            RecipeType = RecipeType.Dinner
        };
    }

    [Fact]
    public async Task GetRecipeDtoById_ValidId_ReturnsRecipeDto()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var sampleRecipe = CreateSampleRecipe();
        sampleRecipe.RecipeId = recipeId;
        var recipeDto = new RecipeDto { RecipeId = recipeId, RecipeName = sampleRecipe.RecipeName };

        _mockRepository
            .Setup(repo => repo.ReadRecipeById(recipeId))
            .ReturnsAsync(sampleRecipe);
        _mockMapper
            .Setup(mapper => mapper.Map<RecipeDto>(sampleRecipe))
            .Returns(recipeDto);

        // Act
        var result = await _recipeManager.GetRecipeDtoById(recipeId.ToString());

        // Assert
        Assert.Equal(recipeDto, result);
        _mockRepository.Verify(repo => repo.ReadRecipeById(recipeId), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<RecipeDto>(sampleRecipe), Times.Once);
    }

    [Fact]
    public async Task GetRecipeDtoByName_ValidName_ReturnsRecipeDto()
    {
        // Arrange
        var sampleRecipe = CreateSampleRecipe();
        var recipeDto = new RecipeDto { RecipeId = sampleRecipe.RecipeId, RecipeName = sampleRecipe.RecipeName };

        _mockRepository
            .Setup(repo => repo.ReadRecipeByName(sampleRecipe.RecipeName))
            .ReturnsAsync(sampleRecipe);
        _mockMapper
            .Setup(mapper => mapper.Map<RecipeDto>(sampleRecipe))
            .Returns(recipeDto);

        // Act
        var result = await _recipeManager.GetRecipeDtoByName(sampleRecipe.RecipeName);

        // Assert
        Assert.Equal(recipeDto, result);
        _mockRepository.Verify(repo => repo.ReadRecipeByName(sampleRecipe.RecipeName), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<RecipeDto>(sampleRecipe), Times.Once);
    }

    [Fact]
    public async Task GetRecipesCollectionByName_ValidName_ReturnsCollectionOfRecipeDto()
    {
        // Arrange
        var sampleRecipe = CreateSampleRecipe();
        var sampleRecipes = new List<Recipe> { sampleRecipe };
        var recipeDtos = new List<RecipeDto>
            { new RecipeDto { RecipeId = sampleRecipe.RecipeId, RecipeName = sampleRecipe.RecipeName } };

        _mockRepository
            .Setup(repo => repo.ReadRecipesCollectionByName(sampleRecipe.RecipeName))
            .ReturnsAsync(sampleRecipes);
        _mockMapper
            .Setup(mapper => mapper.Map<ICollection<RecipeDto>>(sampleRecipes))
            .Returns(recipeDtos);

        // Act
        var result = await _recipeManager.GetRecipesCollectionByName(sampleRecipe.RecipeName);

        // Assert
        Assert.Equal(recipeDtos, result);
        _mockRepository.Verify(repo => repo.ReadRecipesCollectionByName(sampleRecipe.RecipeName), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<ICollection<RecipeDto>>(sampleRecipes), Times.Once);
    }

    [Fact]
    public async Task CreateRecipe_ValidRecipe_ReturnsRecipeDto()
    {
        // Arrange
        var sampleRecipeJson = SampleValidRecipeJson();
        var sampleRecipeDto = CreateSampleRecipeFromJsonDto();
        var sampleRecipeFilterDto = RecipeFilterDtoUtil.CreateRecipeFilterDto(recipeName: sampleRecipeDto.RecipeName);
        var samplePreferencesListDto = PreferenceListDtoUtil.CreatePreferenceListDto();
        Uri imageUri = new Uri("https://culinarycodestorage.blob.core.windows.net/recipe-images/2024-11-14-08-22-46-585065.jpg");

        var prompt = LlmSettingsService.BuildPrompt(sampleRecipeFilterDto, samplePreferencesListDto);
        _mockLlmService
            .Setup(service => service.GenerateRecipe(prompt))
            .Returns(sampleRecipeJson);
        _mockLlmService
            .Setup(service => service.GenerateRecipeImage($"{sampleRecipeDto.RecipeName} {sampleRecipeDto.Description}"))
            .Returns(imageUri);
        _mockRepository
            .Setup(repo => repo.CreateRecipe(It.IsAny<Recipe>()))
            .Verifiable();
        _mockPreferenceRepository
            .Setup(repo => repo.ReadStandardPreferences())
            .ReturnsAsync(new List<Preference>());
        _mockMapper
            .Setup(mapper => mapper.Map<RecipeDto>(It.IsAny<Recipe>()))
            .Returns(CreateSampleRecipeFromJsonDto);


        // Act
        var result = await _recipeManager.CreateRecipe(sampleRecipeFilterDto, samplePreferencesListDto);

        // Assert
        // Assert fields because the RecipeDto is not the same instance as the one returned by the method
        Assert.Equal(sampleRecipeDto.RecipeName, result.RecipeName);
        Assert.Equal(sampleRecipeDto.Description, result.Description);
        Assert.Equal(sampleRecipeDto.AmountOfPeople, result.AmountOfPeople);
        Assert.Equal(sampleRecipeDto.CookingTime, result.CookingTime);
        Assert.Equal(sampleRecipeDto.Difficulty, result.Difficulty);
        Assert.Equal(sampleRecipeDto.Preferences[0].PreferenceName, result.Preferences[0].PreferenceName);
        
        for(int i = 0; i < sampleRecipeDto.Ingredients.Count; i++)
        {
            Assert.Equal(sampleRecipeDto.Ingredients[i].Ingredient.IngredientName, result.Ingredients[i].Ingredient.IngredientName);
            Assert.Equal(sampleRecipeDto.Ingredients[i].Quantity, result.Ingredients[i].Quantity);
        }
        
        for(int i = 0; i < sampleRecipeDto.Instructions.Count; i++)
        {
            Assert.Equal(sampleRecipeDto.Instructions[i].Instruction, result.Instructions[i].Instruction);
            Assert.Equal(sampleRecipeDto.Instructions[i].StepNumber, result.Instructions[i].StepNumber);
        }
        
        Assert.Equal(sampleRecipeDto.RecipeType, result.RecipeType);
        
        _mockLlmService.Verify(service => service.GenerateRecipe(prompt), Times.Once);
        _mockRepository.Verify(repo => repo.CreateRecipe(It.IsAny<Recipe>()), Times.Once);
    }

    [Fact]
    public async Task CreateRecipe_RecipeNotPossible_ThrowsException()
    {
        // Arrange
        var recipeDto = RecipeFilterDtoUtil.CreateRecipeFilterDto("Lekkere baksteensoep");
        var preferencesListDto = PreferenceListDtoUtil.CreatePreferenceListDto();
        var prompt = LlmSettingsService.BuildPrompt(recipeDto,preferencesListDto);
        _mockLlmService
            .Setup(service => service.GenerateRecipe(prompt))
            .Returns("\"NOT_POSSIBLE with this reason Baksteensoep is niet eetbaar");
        
        // Act & Assert
        await Assert.ThrowsAsync<RecipeNotAllowedException>(async () => await _recipeManager.CreateRecipe(recipeDto, preferencesListDto));
    }

    [Fact]
    public async Task CreateRecipe_RecipeNotValid_ReturnsNull()
    {
        // Arrange
        var invalidJson = "{ \"recipeName\": \"Stoofvlees met frietjes\" }";
        
        var recipeDto = RecipeFilterDtoUtil.CreateRecipeFilterDto("Stoofvlees met frietjes");
        var preferencesListDto = PreferenceListDtoUtil.CreatePreferenceListDto();
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferencesListDto);
        _mockLlmService
            .Setup(service => service.GenerateRecipe(prompt))
            .Returns(invalidJson);
        
        // Act
        var result = await _recipeManager.CreateRecipe(recipeDto, preferencesListDto);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateRecipe_RecipeJsonNotValid_ReturnsNull()
    {
        var brokenJson = "{ \"recipeName\": \"Stoofvlees met frietjes\""; // missing closing bracket
        var preferencesListDto = PreferenceListDtoUtil.CreatePreferenceListDto();
        
        var recipeDto = RecipeFilterDtoUtil.CreateRecipeFilterDto("Stoofvlees met frietjes");
        var prompt = LlmSettingsService.BuildPrompt(recipeDto, preferencesListDto);
        _mockLlmService
            .Setup(service => service.GenerateRecipe(prompt))
            .Returns(brokenJson);
        
        
        // Act
        var result = await _recipeManager.CreateRecipe(recipeDto, preferencesListDto);
        
        // Assert
        Assert.Null(result);
    }
}