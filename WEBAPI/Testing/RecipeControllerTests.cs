using BL.DTOs.Recipes;
using BL.Managers.Recipes;
using DOM.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WEBAPI.Controllers;
using Xunit;

namespace WEBAPI.Testing;

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
}
