﻿using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.MealPlanning;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Moq;

namespace CulinaryCode.Tests.BL.Managers;

public class MealPlannerManagerTests
{
    private readonly Mock<IMealPlannerRepository> _mealPlannerRepositoryMock;
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
    private readonly Mock<IIngredientRepository> _ingredientRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MealPlannerManager _mealPlannerManager;

    public MealPlannerManagerTests()
    {
        _mealPlannerRepositoryMock = new Mock<IMealPlannerRepository>();
        _recipeRepositoryMock = new Mock<IRecipeRepository>();
        _ingredientRepositoryMock = new Mock<IIngredientRepository>();
        _mapperMock = new Mock<IMapper>();

        _mealPlannerManager = new MealPlannerManager(
            _mealPlannerRepositoryMock.Object,
            _mapperMock.Object,
            _recipeRepositoryMock.Object,
            _ingredientRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateNewPlannedMeal_DeletesExistingMealForDate_AddsNewMeal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plannedMealDto = new PlannedMealDto
        {
            PlannedDate = DateTime.UtcNow.Date,
            Recipe = new RecipeDto { RecipeId = Guid.NewGuid() },
            Ingredients = new List<IngredientQuantityDto>
            {
                new IngredientQuantityDto
                {
                    Quantity = 100,
                    Ingredient = new IngredientDto { IngredientId = Guid.NewGuid() }
                }
            },
            AmountOfPeople = 4
        };

        var mealPlanner = new MealPlanner
        {
            NextWeek = new List<PlannedMeal>
            {
                new PlannedMeal
                {
                    PlannedDate = DateTime.UtcNow.Date,
                    PlannedMealId = Guid.NewGuid()
                }
            }
        };

        var recipe = new Recipe { RecipeId = plannedMealDto.Recipe.RecipeId };

        _mealPlannerRepositoryMock
            .Setup(repo => repo.ReadMealPlannerByIdWithNextWeek(userId))
            .ReturnsAsync(mealPlanner);

        _mealPlannerRepositoryMock
            .Setup(repo => repo.DeletePlannedMeal(It.IsAny<PlannedMeal>()))
            .Returns(Task.CompletedTask);

        _recipeRepositoryMock
            .Setup(repo => repo.ReadRecipeById(plannedMealDto.Recipe.RecipeId))
            .Returns(recipe);

        _ingredientRepositoryMock
            .Setup(repo => repo.ReadIngredientById(It.IsAny<Guid>()))
            .Returns(new Ingredient());

        _mealPlannerRepositoryMock
            .Setup(repo => repo.CreatePlannedMeal(It.IsAny<PlannedMeal>()))
            .ReturnsAsync(new PlannedMeal());

        // Act
        await _mealPlannerManager.CreateNewPlannedMeal(userId, plannedMealDto);

        // Assert
        _mealPlannerRepositoryMock.Verify(repo => repo.DeletePlannedMeal(It.IsAny<PlannedMeal>()), Times.Once);
        _mealPlannerRepositoryMock.Verify(repo => repo.CreatePlannedMeal(It.IsAny<PlannedMeal>()), Times.Once);
    }

    [Fact]
    public async Task GetPlannedMealsFromUserAfterDate_ReturnsMappedPlannedMeals()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow.Date;
        var plannedMeals = new List<PlannedMeal>
        {
            new PlannedMeal { PlannedDate = dateTime, PlannedMealId = Guid.NewGuid() }
        };
        var plannedMealDtos = new List<PlannedMealDto>
        {
            new PlannedMealDto { PlannedDate = dateTime }
        };

        _mealPlannerRepositoryMock
            .Setup(repo => repo.ReadNextWeekPlannedMeals(dateTime, userId))
            .ReturnsAsync(plannedMeals);

        _mapperMock
            .Setup(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals))
            .Returns(plannedMealDtos);

        // Act
        var result = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(plannedMealDtos.Count, result.Count);
        _mealPlannerRepositoryMock.Verify(repo => repo.ReadNextWeekPlannedMeals(dateTime, userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals), Times.Once);
    }

    [Fact]
    public async Task GetPlannedMealsFromUserAfterDate_OnOtherDates_ReturnsMappedPlannedMeals()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dateTime = DateTime.UtcNow.Date.AddDays(1);
        var plannedMeals = new List<PlannedMeal>
        {
            new PlannedMeal { PlannedDate = dateTime, PlannedMealId = Guid.NewGuid() }
        };
        var plannedMealDtos = new List<PlannedMealDto>
        {
            new PlannedMealDto { PlannedDate = dateTime }
        };

        _mealPlannerRepositoryMock
            .Setup(repo => repo.ReadPlannedMealsAfterDate(dateTime, userId))
            .ReturnsAsync(plannedMeals);

        _mapperMock
            .Setup(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals))
            .Returns(plannedMealDtos);

        // Act
        var result = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(plannedMealDtos.Count, result.Count);
        _mealPlannerRepositoryMock.Verify(repo => repo.ReadPlannedMealsAfterDate(dateTime, userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals), Times.Once);
    }
}