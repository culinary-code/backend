using BL.DTOs.Llm;
using BL.DTOs.Recipes;
using BL.Managers.Recipes;
using DOM.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WEBAPI.Controllers;

namespace CulinaryCode.Tests.WEBAPI;

public class RecipeControllerTests
{
    private readonly Mock<IRecipeManager> _recipeManagerMock;
    private readonly Mock<ILogger<RecipeController>> _loggerMock;
    private readonly RecipeController _controller;

    public RecipeControllerTests()
    {
        _recipeManagerMock = new Mock<IRecipeManager>();
        _loggerMock = new Mock<ILogger<RecipeController>>();
        _controller = new RecipeController(_loggerMock.Object, _recipeManagerMock.Object );
    }

    [Fact]
    public void GetRecipeById_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipeIdString = recipeId.ToString();
        var expectedRecipe = new RecipeDto { RecipeId = recipeId, RecipeName = "Spaghetti Bolognese" };
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoById(recipeIdString)).Returns(expectedRecipe);

        // Act
        var result = _controller.GetRecipeById(recipeIdString);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRecipe, okResult.Value);
    }

    [Fact]
    public void GetRecipeById_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        const string recipeId = "2";
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoById(recipeId))
            .Throws(new RecipeNotFoundException($"Recipe with ID {recipeId} not found."));

        // Act
        var result = _controller.GetRecipeById(recipeId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Recipe with ID {recipeId} not found.", notFoundResult.Value);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Recipe with ID {recipeId} not found.")), // Match the message content
                It.IsAny<Exception>(), // Ignore any exception passed
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the formatter
            ),
            Times.Once
        );
    }
    
    [Fact]
    public void GetRecipeByName_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        var expectedRecipe = new RecipeDto { RecipeId = Guid.NewGuid(), RecipeName = recipeName };
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoByName(recipeName)).Returns(expectedRecipe);

        // Act
        var result = _controller.GetRecipeByName(recipeName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRecipe, okResult.Value);
    }
    
    [Fact]
    public void GetRecipeByName_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoByName(recipeName))
            .Throws(new RecipeNotFoundException($"Recipe with name {recipeName} not found."));

        // Act
        var result = _controller.GetRecipeByName(recipeName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Recipe with name {recipeName} not found.", notFoundResult.Value);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Recipe with name {recipeName} not found.")), // Match the message content
                It.IsAny<Exception>(), // Ignore any exception passed
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the formatter
            ),
            Times.Once
        );
    }
    
    [Fact]
    public void GetRecipesCollectionByName_ReturnsOk_WhenRecipesExist()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        var expectedRecipe = new RecipeDto { RecipeId = Guid.NewGuid(), RecipeName = recipeName };
        var expectedRecipes = new List<RecipeDto> { expectedRecipe };
        _recipeManagerMock.Setup(manager => manager.GetRecipesCollectionByName(recipeName)).Returns(expectedRecipes);

        // Act
        var result = _controller.GetRecipeCollectionByName(recipeName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRecipes, okResult.Value);
    }
    
    [Fact]
    public void GetRecipesCollectionByName_ReturnsNotFound_WhenRecipesDoNotExist()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        _recipeManagerMock.Setup(manager => manager.GetRecipesCollectionByName(recipeName))
            .Throws(new RecipeNotFoundException($"Recipes with name {recipeName} not found."));

        // Act
        var result = _controller.GetRecipeCollectionByName(recipeName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Recipes with name {recipeName} not found.", notFoundResult.Value);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Recipes with name {recipeName} not found.")), // Match the message content
                It.IsAny<Exception>(), // Ignore any exception passed
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the formatter
            ),
            Times.Once
        );
    }
    
    [Fact]
    public void CreateRecipe_ReturnsOk_WhenRecipeIsAdded()
    {
        // Arrange
        var recipeName = "Spaghetti Bolognese";
        var CreateRecipeDto = new CreateRecipeDto() {Name = "Spaghetti Bolognese"};
        var recipeDto = new RecipeDto { RecipeId = Guid.NewGuid(), RecipeName = recipeName };
        _recipeManagerMock.Setup(manager => manager.CreateRecipe(recipeName)).Returns(recipeDto);

        // Act
        var result = _controller.CreateRecipe(CreateRecipeDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(recipeDto, okResult.Value);
    }
    
    [Fact]
    public void CreateRecipe_ReturnsBadRequest_WhenRecipeIsNotAdded()
    {
        // Arrange
        var recipeName = "Spaghetti Bolognese";
        var CreateRecipeDto = new CreateRecipeDto() {Name = "Spaghetti Bolognese"};
        _recipeManagerMock.Setup(manager => manager.CreateRecipe(recipeName)).Returns(null as RecipeDto);
        
        // Act
        var result = _controller.CreateRecipe(CreateRecipeDto);
        
        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void CreateRecipe_ThrowsExceptionWithErrorMessage_WhenRecipeIsNotAllowed()
    {
        // Arrange
        var recipeName = "Baksteensoep";
        var CreateRecipeDto = new CreateRecipeDto() {Name = "Baksteensoep"};
        _recipeManagerMock.Setup(manager => manager.CreateRecipe(recipeName))
            .Throws(new RecipeNotAllowedException($"Recipe with name {recipeName} is not allowed."));
        
        // Act
        var result = _controller.CreateRecipe(CreateRecipeDto);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
