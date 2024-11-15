using AutoMapper;
using BL.DTOs.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Recipes;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.Extensions.Logging;
using Moq;

namespace CulinaryCode.Tests.BL;

public class RecipeManagerTests
{
    private readonly Mock<IRecipeRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly RecipeManager _recipeManager;
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<IPreferenceRepository> _mockPreferenceRepository;
    private readonly Mock<IIngredientRepository> _mockIngredientRepository;
    private readonly Mock<ILogger<RecipeManager>> _loggerMock;

    public RecipeManagerTests()
    {
        _mockRepository = new Mock<IRecipeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLlmService = new Mock<ILlmService>();
        _mockPreferenceRepository = new Mock<IPreferenceRepository>();
        _mockIngredientRepository = new Mock<IIngredientRepository>();
        _loggerMock = new Mock<ILogger<RecipeManager>>();
        _recipeManager = new RecipeManager(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLlmService.Object,
            _mockPreferenceRepository.Object,
            _mockIngredientRepository.Object,
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

    [Fact]
    public void GetRecipeDtoById_ValidId_ReturnsRecipeDto()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var sampleRecipe = CreateSampleRecipe();
        sampleRecipe.RecipeId = recipeId;
        var recipeDto = new RecipeDto { RecipeId = recipeId, RecipeName = sampleRecipe.RecipeName };

        _mockRepository
            .Setup(repo => repo.ReadRecipeById(recipeId))
            .Returns(sampleRecipe);
        _mockMapper
            .Setup(mapper => mapper.Map<RecipeDto>(sampleRecipe))
            .Returns(recipeDto);

        // Act
        var result = _recipeManager.GetRecipeDtoById(recipeId.ToString());

        // Assert
        Assert.Equal(recipeDto, result);
        _mockRepository.Verify(repo => repo.ReadRecipeById(recipeId), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<RecipeDto>(sampleRecipe), Times.Once);
    }

    [Fact]
    public void GetRecipeDtoByName_ValidName_ReturnsRecipeDto()
    {
        // Arrange
        var sampleRecipe = CreateSampleRecipe();
        var recipeDto = new RecipeDto { RecipeId = sampleRecipe.RecipeId, RecipeName = sampleRecipe.RecipeName };

        _mockRepository
            .Setup(repo => repo.ReadRecipeByName(sampleRecipe.RecipeName))
            .Returns(sampleRecipe);
        _mockMapper
            .Setup(mapper => mapper.Map<RecipeDto>(sampleRecipe))
            .Returns(recipeDto);

        // Act
        var result = _recipeManager.GetRecipeDtoByName(sampleRecipe.RecipeName);

        // Assert
        Assert.Equal(recipeDto, result);
        _mockRepository.Verify(repo => repo.ReadRecipeByName(sampleRecipe.RecipeName), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<RecipeDto>(sampleRecipe), Times.Once);
    }

    [Fact]
    public void GetRecipesCollectionByName_ValidName_ReturnsCollectionOfRecipeDto()
    {
        // Arrange
        var sampleRecipe = CreateSampleRecipe();
        var sampleRecipes = new List<Recipe> { sampleRecipe };
        var recipeDtos = new List<RecipeDto>
            { new RecipeDto { RecipeId = sampleRecipe.RecipeId, RecipeName = sampleRecipe.RecipeName } };

        _mockRepository
            .Setup(repo => repo.ReadRecipesCollectionByName(sampleRecipe.RecipeName))
            .Returns(sampleRecipes);
        _mockMapper
            .Setup(mapper => mapper.Map<ICollection<RecipeDto>>(sampleRecipes))
            .Returns(recipeDtos);

        // Act
        var result = _recipeManager.GetRecipesCollectionByName(sampleRecipe.RecipeName);

        // Assert
        Assert.Equal(recipeDtos, result);
        _mockRepository.Verify(repo => repo.ReadRecipesCollectionByName(sampleRecipe.RecipeName), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<ICollection<RecipeDto>>(sampleRecipes), Times.Once);
    }
}