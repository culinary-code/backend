using BL.DTOs.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using DOM.Results;
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
            .ReturnsAsync(Result<ReviewDto>.Success(expectedReview));
        
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
            .ReturnsAsync(Result<ReviewDto>.Failure($"Review with ID {reviewId} not found.", ResultFailureType.NotFound));
        
        // Act
        var result = await _controller.GetReviewById(reviewId.ToString());
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Review with ID {reviewId} not found.", notFoundResult.Value);
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
            .ReturnsAsync(Result<ICollection<ReviewDto>>.Success(expectedReviews));
        
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
            .ReturnsAsync(Result<ICollection<ReviewDto>>.Failure($"Reviews for recipe with ID {recipeId} not found.", ResultFailureType.NotFound));
        
        // Act
        var result = await _controller.GetReviewsByRecipeId(recipeId.ToString());
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Reviews for recipe with ID {recipeId} not found.", notFoundResult.Value);
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
            .Returns(Result<Guid>.Success(userId));
        _reviewManagerMock
            .Setup(manager => manager.CreateReview(userId, createReviewDto.RecipeId, createReviewDto.Description, createReviewDto.AmountOfStars))
            .ReturnsAsync(Result<ReviewDto>.Success(review));
        
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
        _identityProviderService.Setup(manager => manager.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(Guid.NewGuid()));
        
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
            .Returns(Result<Guid>.Success(userId));

        // Mock the review manager to throw the ReviewAlreadyExistsException
        _reviewManagerMock
            .Setup(manager => manager.CreateReview(userId, createReviewDto.RecipeId, createReviewDto.Description, createReviewDto.AmountOfStars))
            .ReturnsAsync(Result<ReviewDto>.Failure(exceptionMessage, ResultFailureType.Error));

        // Act
        var result = await _controller.CreateReview(createReviewDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}