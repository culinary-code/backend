using BL.DTOs.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WEBAPI.Controllers;

namespace CulinaryCode.Tests.WEBAPI;

public class ReviewControllerTests
{
    private readonly Mock<IReviewManager> _reviewManagerMock;
    private readonly Mock<ILogger<ReviewController>> _loggerMock;
    private readonly Mock<IIdentityProviderService> _identityProviderService;
    private readonly ReviewController _controller;
    
    public ReviewControllerTests()
    {
        _reviewManagerMock = new Mock<IReviewManager>();
        _loggerMock = new Mock<ILogger<ReviewController>>();
        _identityProviderService = new Mock<IIdentityProviderService>();
        _controller = new ReviewController(_reviewManagerMock.Object, _loggerMock.Object, _identityProviderService.Object);
        
        // Mock the HttpContext and set up the Authorization header
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer dummyToken";  // Mocking the Authorization header
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
    
    [Fact]
    public async Task GetReviewById_ReturnsOk_WhenReviewExists()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var reviewIdString = reviewId.ToString();
        var expectedReview = new ReviewDto { ReviewId = reviewId, ReviewerUsername = "JohnDoe" };
        _reviewManagerMock
            .Setup(manager => manager.GetReviewDtoById(reviewId))
            .ReturnsAsync(expectedReview);
        
        // Act
        var result = await _controller.GetReviewById(reviewIdString);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedReview, okResult.Value);
    }
    
    [Fact]
    public async Task GetReviewById_ReturnsNotFound_WhenReviewDoesNotExist()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        _reviewManagerMock
            .Setup(manager => manager.GetReviewDtoById(reviewId))
            .Throws(new ReviewNotFoundException($"Review with ID {reviewId} not found."));
        
        // Act
        var result = await _controller.GetReviewById(reviewId.ToString());
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Review with ID {reviewId} not found.", notFoundResult.Value);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Review with ID {reviewId} not found.")), // Match the message content
                It.IsAny<Exception>(), // Ignore the exception
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the formatter/ Check the log message
            )
        );
    }
    
    [Fact]
    public async Task GetReviewsByRecipeId_ReturnsOk_WhenReviewsExist()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipeIdString = recipeId.ToString();
        var expectedReviews = new List<ReviewDto>
        {
            new() { ReviewId = Guid.NewGuid(), ReviewerUsername = "JohnDoe" },
            new() { ReviewId = Guid.NewGuid(), ReviewerUsername = "JaneDoe" }
        };
        _reviewManagerMock
            .Setup(manager => manager.GetReviewDtosByRecipeId(recipeId))
            .ReturnsAsync(expectedReviews);
        
        // Act
        var result = await _controller.GetReviewsByRecipeId(recipeIdString);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedReviews, okResult.Value);
    }
    
    [Fact]
    public async Task GetReviewsByRecipeId_ReturnsNotFound_WhenReviewsDoNotExist()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        _reviewManagerMock
            .Setup(manager => manager.GetReviewDtosByRecipeId(recipeId))
            .Throws(new ReviewNotFoundException($"Reviews for recipe with ID {recipeId} not found."));
        
        // Act
        var result = await _controller.GetReviewsByRecipeId(recipeId.ToString());
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Reviews for recipe with ID {recipeId} not found.", notFoundResult.Value);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Reviews for recipe with ID {recipeId} not found.")), // Match the message content
                It.IsAny<Exception>(), // Ignore the exception
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the formatter/ Check the log message
            )
        );
    }
    
    [Fact]
    public async Task CreateReview_ReturnsOk_WhenReviewIsCreated()
    {
        // Arrange
        var createReviewDto = new CreateReviewDto
        {
            RecipeId = Guid.NewGuid(),
            Description = "description",
            AmountOfStars = 5
        };
        var userId = Guid.NewGuid();
        var review = new ReviewDto { ReviewId = Guid.NewGuid(), ReviewerUsername = "JohnDoe" };
        _identityProviderService
            .Setup(x => x.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(userId);
        _reviewManagerMock
            .Setup(manager => manager.CreateReview(userId, createReviewDto.RecipeId, createReviewDto.Description, createReviewDto.AmountOfStars))
            .ReturnsAsync(review);
        
        // Act
        var result = await _controller.CreateReview(createReviewDto);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(review, okResult.Value);
    }
    
    [Fact]
    public async Task CreateReview_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var createReviewDto = new CreateReviewDto();
        _controller.ModelState.AddModelError("Description", "The Description field is required.");
        
        // Act
        var result = await _controller.CreateReview(createReviewDto);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReview_ReturnsConflict_WhenReviewAlreadyExists()
    {
        // Arrange
        var createReviewDto = new CreateReviewDto
        {
            RecipeId = Guid.NewGuid(),
            Description = "description",
            AmountOfStars = 5
        };
        var userId = Guid.NewGuid();
        var exceptionMessage = "Review already exists for this recipe.";

        // Mock the identity provider service to return the user ID
        _identityProviderService
            .Setup(x => x.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(userId);

        // Mock the review manager to throw the ReviewAlreadyExistsException
        _reviewManagerMock
            .Setup(manager => manager.CreateReview(userId, createReviewDto.RecipeId, createReviewDto.Description, createReviewDto.AmountOfStars))
            .ThrowsAsync(new ReviewAlreadyExistsException(exceptionMessage));

        // Act
        var result = await _controller.CreateReview(createReviewDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(exceptionMessage, conflictResult.Value);

        // Optionally verify the logger was called with the correct message
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(exceptionMessage)), // Match the exception message in the log
                It.IsAny<Exception>(), // Ignore the exception object
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the log formatter
            ), Times.Once // Ensure the log was called once
        );
    }
}