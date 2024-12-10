using AutoMapper;
using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes;
using BL.DTOs.Recipes.Ingredients;
using BL.Managers.MealPlanning;
using DAL.Groceries;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using DOM.Results;
using Moq;

namespace CulinaryCode.Tests.BL.Managers;

public class MealPlannerManagerTests
{
    private readonly Mock<IMealPlannerRepository> _mealPlannerRepositoryMock;
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
    private readonly Mock<IIngredientRepository> _ingredientRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MealPlannerManager _mealPlannerManager;
    private readonly Mock<IGroceryRepository> _groceryRepositoryMock;

    public MealPlannerManagerTests()
    {
        _mealPlannerRepositoryMock = new Mock<IMealPlannerRepository>();
        _recipeRepositoryMock = new Mock<IRecipeRepository>();
        _ingredientRepositoryMock = new Mock<IIngredientRepository>();
        _groceryRepositoryMock = new Mock<IGroceryRepository>();
        _mapperMock = new Mock<IMapper>();

        _mealPlannerManager = new MealPlannerManager(
            _mealPlannerRepositoryMock.Object,
            _mapperMock.Object,
            _recipeRepositoryMock.Object,
            _ingredientRepositoryMock.Object,
            _groceryRepositoryMock.Object
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
            .Setup(repo => repo.ReadMealPlannerByIdWithNextWeekNoTracking(userId))
            .ReturnsAsync(Result<MealPlanner>.Success(mealPlanner));

        _mealPlannerRepositoryMock
            .Setup(repo => repo.DeletePlannedMeal(It.IsAny<PlannedMeal>()))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        _recipeRepositoryMock
            .Setup(repo => repo.ReadRecipeById(plannedMealDto.Recipe.RecipeId))
            .ReturnsAsync(Result<Recipe>.Success(recipe));

        _ingredientRepositoryMock
            .Setup(repo => repo.ReadIngredientById(It.IsAny<Guid>()))
            .ReturnsAsync(Result<Ingredient>.Success(new Ingredient()));

        _mealPlannerRepositoryMock
            .Setup(repo => repo.CreatePlannedMeal(It.IsAny<PlannedMeal>()))
            .ReturnsAsync(Result<PlannedMeal>.Success(new PlannedMeal()));

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
            .Setup(repo => repo.ReadNextWeekPlannedMealsNoTracking(userId))
            .ReturnsAsync(Result<List<PlannedMeal>>.Success(plannedMeals));

        _mapperMock
            .Setup(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals))
            .Returns(plannedMealDtos);

        // Act
        var result = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(plannedMealDtos.Count, result.Value!.Count);
        _mealPlannerRepositoryMock.Verify(repo => repo.ReadNextWeekPlannedMealsNoTracking(userId), Times.Once);
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
            .Setup(repo => repo.ReadPlannedMealsAfterDateNoTracking(dateTime, userId))
            .ReturnsAsync(Result<List<PlannedMeal>>.Success(plannedMeals));

        _mapperMock
            .Setup(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals))
            .Returns(plannedMealDtos);

        // Act
        var result = await _mealPlannerManager.GetPlannedMealsFromUserAfterDate(dateTime, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(plannedMealDtos.Count, result.Value!.Count);
        _mealPlannerRepositoryMock.Verify(repo => repo.ReadPlannedMealsAfterDateNoTracking(dateTime, userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<PlannedMealDto>>(plannedMeals), Times.Once);
    }
}