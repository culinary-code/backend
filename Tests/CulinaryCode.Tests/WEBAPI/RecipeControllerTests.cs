﻿using BL.DTOs.Accounts;
using BL.DTOs.Llm;
using BL.DTOs.Recipes;
using BL.Managers.Accounts;
using BL.Managers.Recipes;
using BL.Services;
using CulinaryCode.Tests.Util;
using DOM.Accounts;
using DOM.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WEBAPI.Controllers;

namespace CulinaryCode.Tests.WEBAPI;

public class RecipeControllerTests
{
    private readonly Mock<IRecipeManager> _recipeManagerMock;
    private readonly Mock<IIdentityProviderService> _identityProviderService;
    private readonly Mock<IAccountManager> _accountManagerMock;
    private readonly Mock<ILogger<RecipeController>> _loggerMock;
    private readonly RecipeController _controller;

    public RecipeControllerTests()
    {
        _identityProviderService = new Mock<IIdentityProviderService>();
        _accountManagerMock = new Mock<IAccountManager>();
        _recipeManagerMock = new Mock<IRecipeManager>();
        _loggerMock = new Mock<ILogger<RecipeController>>();
        _controller = new RecipeController(_loggerMock.Object, _recipeManagerMock.Object, _identityProviderService.Object, _accountManagerMock.Object);
    }

    [Fact]
    public async Task GetRecipeById_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipeIdString = recipeId.ToString();
        var expectedRecipe = new RecipeDto { RecipeId = recipeId, RecipeName = "Spaghetti Bolognese" };
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoById(recipeIdString)).ReturnsAsync(Result<RecipeDto>.Success(expectedRecipe));

        // Act
        var result = await _controller.GetRecipeById(recipeIdString);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRecipe, okResult.Value);
    }

    [Fact]
    public async Task GetRecipeById_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        const string recipeId = "2";
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoById(recipeId))
            .ReturnsAsync(Result<RecipeDto>.Failure($"Recipe with ID {recipeId} not found.", ResultFailureType.NotFound));

        // Act
        var result = await _controller.GetRecipeById(recipeId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Recipe with ID {recipeId} not found.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task GetRecipeByName_ReturnsOk_WhenRecipeExists()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        var expectedRecipe = new RecipeDto { RecipeId = Guid.NewGuid(), RecipeName = recipeName };
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoByName(recipeName)).ReturnsAsync(Result<RecipeDto>.Success(expectedRecipe));

        // Act
        var result = await _controller.GetRecipeByName(recipeName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRecipe, okResult.Value);
    }
    
    [Fact]
    public async Task GetRecipeByName_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        _recipeManagerMock.Setup(manager => manager.GetRecipeDtoByName(recipeName))
            .ReturnsAsync(Result<RecipeDto>.Failure($"Recipe with name {recipeName} not found.", ResultFailureType.NotFound));

        // Act
        var result = await _controller.GetRecipeByName(recipeName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Recipe with name {recipeName} not found.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task GetRecipesCollectionByName_ReturnsOk_WhenRecipesExist()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        var expectedRecipe = new RecipeDto { RecipeId = Guid.NewGuid(), RecipeName = recipeName };
        var expectedRecipes = new List<RecipeDto> { expectedRecipe };
        _recipeManagerMock.Setup(manager => manager.GetRecipesCollectionByName(recipeName)).ReturnsAsync(Result<ICollection<RecipeDto>>.Success(expectedRecipes));

        // Act
        var result = await _controller.GetRecipeCollectionByName(recipeName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRecipes, okResult.Value);
    }
    
    [Fact]
    public async Task GetRecipesCollectionByName_ReturnsNotFound_WhenRecipesDoNotExist()
    {
        // Arrange
        const string recipeName = "Spaghetti Bolognese";
        _recipeManagerMock.Setup(manager => manager.GetRecipesCollectionByName(recipeName))
            .ReturnsAsync(Result<ICollection<RecipeDto>>.Failure($"Recipes with name {recipeName} not found.", ResultFailureType.NotFound));

        // Act
        var result = await _controller.GetRecipeCollectionByName(recipeName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Recipes with name {recipeName} not found.", notFoundResult.Value);
        
    }
    
    [Fact]
    public async Task CreateRecipe_ReturnsOk_WhenRecipeIsAdded()
    {
        // Arrange
        var token = "Bearer testtoken";

        var recipeName = "Spaghetti Bolognese";
        var recipeDto = new RecipeDto { RecipeId = Guid.NewGuid(), RecipeName = recipeName };
        var recipeFilterDto = RecipeFilterDtoUtil.CreateRecipeFilterDto(recipeName: recipeName);
        var preferences = PreferenceListDtoUtil.CreatePreferenceListDto();
        var userid = new Guid();
        
        // Set up the controller context to simulate HTTP request and Authorization header
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;
        
        _identityProviderService
            .Setup(x => x.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(userid));
        _accountManagerMock
            .Setup(service => service.GetPreferencesByUserId(It.Is<Guid>(id => id == userid)))
            .ReturnsAsync(Result<List<PreferenceDto>>.Success(preferences));
        _recipeManagerMock.Setup(manager => manager.CreateRecipe(recipeFilterDto, preferences)).ReturnsAsync(Result<RecipeDto?>.Success(recipeDto));


        // Act
        var result = await _controller.CreateRecipe(recipeFilterDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(recipeDto, okResult.Value);
    }
    
    [Fact]
    public async Task CreateRecipe_ReturnsBadRequest_WhenRecipeIsNotAdded()
    {
        // Arrange
        var token = "Bearer testtoken";

        var recipeName = "Spaghetti Bolognese";
        var recipeFilterDto = RecipeFilterDtoUtil.CreateRecipeFilterDto(recipeName: recipeName);
        var preferences = PreferenceListDtoUtil.CreatePreferenceListDto();
        _recipeManagerMock.Setup(manager => manager.CreateRecipe(recipeFilterDto, preferences)).ReturnsAsync(Result<RecipeDto?>.Failure("", ResultFailureType.Error));
        _identityProviderService.Setup(manager => manager.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(Guid.NewGuid()));
        _accountManagerMock.Setup(manager => manager.GetPreferencesByUserId(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<PreferenceDto>>.Success(preferences));
        
        // Set up the controller context to simulate HTTP request and Authorization header
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;
        
        // Act
        var result = await _controller.CreateRecipe(recipeFilterDto);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRecipe_ThrowsExceptionWithErrorMessage_WhenRecipeIsNotAllowed()
    {
        // Arrange
        var token = "Bearer testtoken";

        var recipeName = "Baksteensoep";
        var recipeFilterDto = RecipeFilterDtoUtil.CreateRecipeFilterDto(recipeName: recipeName);
        var preferences = PreferenceListDtoUtil.CreatePreferenceListDto();

        _recipeManagerMock.Setup(manager => manager.CreateRecipe(recipeFilterDto, preferences))
            .ReturnsAsync(Result<RecipeDto?>.Failure($"Recipe with name {recipeName} is not allowed.", ResultFailureType.Error));
        _identityProviderService.Setup(manager => manager.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(Guid.NewGuid()));
        _accountManagerMock.Setup(manager => manager.GetPreferencesByUserId(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<PreferenceDto>>.Success(preferences));
                
        // Set up the controller context to simulate HTTP request and Authorization header
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;
        
        // Act
        var result = await _controller.CreateRecipe(recipeFilterDto);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
