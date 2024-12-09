using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WEBAPI.Controllers;

namespace CulinaryCode.Tests.WEBAPI;

public class AccountControllerTests
{
    private readonly Mock<IAccountManager> _accountManagerMock;
    private readonly Mock<ILogger<AccountController>> _loggerMock;
    private readonly AccountController _controller;
    private readonly Mock<IIdentityProviderService> _identityProviderServiceMock;

    public AccountControllerTests()
    {
        _accountManagerMock = new Mock<IAccountManager>();
        _loggerMock = new Mock<ILogger<AccountController>>();
        _identityProviderServiceMock = new Mock<IIdentityProviderService>();
        _controller = new AccountController(_accountManagerMock.Object, _loggerMock.Object, _identityProviderServiceMock.Object);
    }
    
    [Fact]
    public async Task GetUserById_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIdString = userId.ToString();
        var expectedUser = new AccountDto() { AccountId = userId, Name = "JohnDoe" };
        _accountManagerMock.Setup(manager => manager.GetAccountById(userIdString)).ReturnsAsync(Result<AccountDto>.Success(expectedUser));

        // Act
        var result = await _controller.GetUserById(userIdString);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedUser, okResult.Value);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenNoUserExists()
    {
        // Arrange
        const string userId = "2";
        const string expectedErrorMessage = $"User with ID {userId} not found.";
    
        _accountManagerMock.Setup(manager => manager.GetAccountById(userId))
            .ReturnsAsync(Result<AccountDto>.Failure(expectedErrorMessage, ResultFailureType.NotFound));

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(expectedErrorMessage, notFoundResult.Value);
    }
    
    [Fact]
    public async Task UpdateAccount_ReturnsOk_WhenAccountIsUpdatedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountDto = new AccountDto { AccountId = userId, Name = "JohnDoe" };
        var updatedAccount = new AccountDto { AccountId = userId, Name = "JohnDoeUpdated" };

        // Mock identity provider to return the userId from the token
        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(userId));

        // Mock the account manager update method
        _accountManagerMock.Setup(manager => manager.UpdateAccount(updatedAccount)).ReturnsAsync(Result<AccountDto>.Success(updatedAccount));

        // Mock the identity provider update
        _identityProviderServiceMock
            .Setup(s => s.UpdateUsername(It.IsAny<AccountDto>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Mock the HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }  // Simulate a valid authorization header
        };
    
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        // Act
        var result = await _controller.UpdateAccount(updatedAccount, "updateusername");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var accountResult = Assert.IsType<AccountDto>(okResult.Value);
        Assert.Equal(updatedAccount.AccountId, accountResult.AccountId);
        Assert.Equal(updatedAccount.Email, accountResult.Email);
        Assert.Equal(updatedAccount.FamilySize, accountResult.FamilySize);
        Assert.Equal(updatedAccount.Name, accountResult.Name);
    }
    
    [Fact]
    public async Task UpdateAccount_ReturnsBadRequestAndLogsError_WhenUpdateFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountDto = new AccountDto { AccountId = userId, Name = "JohnDoe" };
        var expectedErrorMessage = "An error occurred during the update.";

        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(userId));

        // Mock the account manager to throw an exception
        _accountManagerMock
            .Setup(manager => manager.UpdateAccount(accountDto))
            .Throws(new Exception(expectedErrorMessage));

        // Mock the HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }  // Simulate a valid authorization header
        };
    
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        // Act
        var result = await _controller.UpdateAccount(accountDto, "updateusername");

        // Assert 1: Ensure the result is a BadRequestObjectResult with the expected message
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update account.", badRequestResult.Value);

        // Assert 2: Verify that the error was logged once with the expected error message
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error, // Specify the log level
                It.IsAny<EventId>(), // Ignore the event ID
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred during the update.")), // Match the message content
                It.IsAny<Exception>(), // Ignore any exception passed
                It.IsAny<Func<It.IsAnyType, Exception, string>>()! // Ignore the formatter
            ),
            Times.Once
        );
    }
}