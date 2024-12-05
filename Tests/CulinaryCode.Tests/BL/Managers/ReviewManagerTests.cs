using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Recipes;
using DAL.Accounts;
using DAL.Recipes;
using DOM.Accounts;
using DOM.Exceptions;
using DOM.Recipes;
using Microsoft.Extensions.Logging;
using Moq;

namespace CulinaryCode.Tests.BL.Managers;

public class ReviewManagerTests
{
    private readonly ReviewManager _reviewManager;
    private readonly Mock<IReviewRepository> _reviewRepository;
    private readonly Mock<IRecipeRepository> _recipeRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILogger<ReviewManager>> _logger;

    public ReviewManagerTests()
    {
        _reviewRepository = new Mock<IReviewRepository>();
        _recipeRepository = new Mock<IRecipeRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILogger<ReviewManager>>();
        _reviewManager = new ReviewManager(
            _reviewRepository.Object, _mapper.Object, _logger.Object,
            _accountRepository.Object, _recipeRepository.Object
        );
    }

    [Fact]
    public async Task GetReviewDtoById_ReviewExists_ReturnsReviewDto()
    {
        // Arrange
        var review = new Review();
        var reviewDto = new ReviewDto() { ReviewerUsername = "JohnDoe" };
        _reviewRepository.Setup(x => x.ReadReviewWithAccountByReviewIdNoTracking(It.IsAny<Guid>())).ReturnsAsync(review);
        _mapper.Setup(x => x.Map<ReviewDto>(review)).Returns(reviewDto);

        // Act
        var result = await _reviewManager.GetReviewDtoById(Guid.NewGuid());

        // Assert
        Assert.Equal(reviewDto, result);
    }

    [Fact]
    public async Task GetReviewDtosByRecipeId_ReviewsExist_ReturnsReviewDtos()
    {
        // Arrange
        var reviews = new List<Review> { new Review(), new Review() };
        var reviewDtos = new List<ReviewDto>
            { new() { ReviewerUsername = "JohnDoe" }, new() { ReviewerUsername = "JaneDoe" } };
        _reviewRepository.Setup(x => x.ReadReviewsWithAccountByRecipeIdNoTracking(It.IsAny<Guid>())).ReturnsAsync(reviews);
        _mapper.Setup(x => x.Map<ICollection<ReviewDto>>(reviews)).Returns(reviewDtos);

        // Act
        var result = await _reviewManager.GetReviewDtosByRecipeId(Guid.NewGuid());

        // Assert
        Assert.Equal(reviewDtos, result);
    }
    
    [Fact]
    public async Task CreateReview_AccountAlreadyReviewedRecipe_ThrowsReviewAlreadyExistsException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var account = new Account();
        var recipe = new Recipe();
        var existingReview = new List<Review> { new Review { Account = new Account { AccountId = accountId } } };
        _accountRepository.Setup(x => x.ReadAccount(accountId)).ReturnsAsync(account);
        _recipeRepository.Setup(x => x.ReadRecipeWithRelatedInformationByIdNoTracking(recipeId)).ReturnsAsync(recipe);
        _reviewRepository.Setup(x => x.ReviewExistsForAccountAndRecipe(accountId, recipeId)).ReturnsAsync(true);

        // Act
        async Task Act() => await _reviewManager.CreateReview(accountId, recipeId, "description", 5);

        // Assert
        await Assert.ThrowsAsync<ReviewAlreadyExistsException>(Act);
    }
    
    [Fact]
    public async Task CreateReview_AccountHasNotReviewedRecipe_CreatesReview()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var account = new Account();
        var recipe = new Recipe();
        var existingReview = new List<Review>();
        _accountRepository.Setup(x => x.ReadAccount(accountId)).ReturnsAsync(account);
        _recipeRepository.Setup(x => x.ReadRecipeWithRelatedInformationByIdNoTracking(recipeId)).ReturnsAsync(recipe);
        _reviewRepository.Setup(x => x.ReadReviewsWithAccountByRecipeIdNoTracking(recipeId)).ReturnsAsync(existingReview);

        // Act
        await _reviewManager.CreateReview(accountId, recipeId, "description", 5);

        // Assert
        _reviewRepository.Verify(x => x.CreateReview(It.IsAny<Review>()), Times.Once);
    }
}