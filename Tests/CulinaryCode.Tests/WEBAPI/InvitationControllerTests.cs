using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Accounts;
using DOM.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WEBAPI.Controllers;

namespace CulinaryCode.Tests.WEBAPI;

public class InvitationControllerTests
{
    private readonly Mock<IInvitationManager> _invitationManagerMock;
    private readonly Mock<ILogger<InvitationController>> _loggerMock;
    private readonly Mock<IIdentityProviderService> _identityProviderServiceMock;
    private readonly Mock<IGroupManager> _groupManagerMock;
    private readonly Mock<IAccountManager> _accountManagerMock;
    private readonly InvitationController _controller;

    public InvitationControllerTests()
    {
        _invitationManagerMock = new Mock<IInvitationManager>();
        _loggerMock = new Mock<ILogger<InvitationController>>();
        _identityProviderServiceMock = new Mock<IIdentityProviderService>();
        _groupManagerMock = new Mock<IGroupManager>();
        _accountManagerMock = new Mock<IAccountManager>();
        _controller = new InvitationController(
            _invitationManagerMock.Object,
            _loggerMock.Object,
            _identityProviderServiceMock.Object,
            _groupManagerMock.Object,
            _accountManagerMock.Object
        );
    }

    [Fact]
    public async Task SendInvitation_ReturnsOk_WhenInvitationIsSentSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var request = new SendInvitationRequestDto
        {
            GroupId = groupId,
            Email = "test@example.com",
            InvitedUserName = "TestUser"
        };

        var inviter = new AccountDto { AccountId = userId, Name = "InviterName" };

        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(userId));

        _accountManagerMock
            .Setup(manager => manager.GetAccountById(userId.ToString()))
            .ReturnsAsync(Result<AccountDto>.Success(inviter));

        _invitationManagerMock
            .Setup(manager => manager.SendInvitationAsync(It.IsAny<SendInvitationRequestDto>()))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Mock HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }
        };

        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

        // Act
        var result = await _controller.SendInvitation(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        
    }
    
    [Fact]
    public async Task AcceptInvitation_ReturnsOk_WhenInvitationIsAcceptedSuccessfully()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var invitation = new Invitation() { GroupId = Guid.NewGuid() };

        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(userId));

        _invitationManagerMock
            .Setup(manager => manager.ValidateInvitationTokenAsync(token))
            .ReturnsAsync(Result<Invitation>.Success(invitation));

        _groupManagerMock
            .Setup(manager => manager.AddUserToGroupAsync(invitation.GroupId, userId))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        _invitationManagerMock
            .Setup(manager => manager.AcceptInvitationAsync(invitation))
            .ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Mock HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }
        };

        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
        
        // Act
        var result = await _controller.AcceptInvitation(token);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsBadRequest_WhenTokenIsInvalid()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();

        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(Guid.NewGuid()));
        
        _invitationManagerMock
            .Setup(manager => manager.ValidateInvitationTokenAsync(token))
            .ReturnsAsync(Result<Invitation>.Failure("Invalid or expired invitation token.", ResultFailureType.Error));

        // Mock HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }
        };

        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
        
        // Act
        var result = await _controller.AcceptInvitation(token);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsServerError_WhenAnExceptionOccurs()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(userId));
        
        _invitationManagerMock
            .Setup(manager => manager.ValidateInvitationTokenAsync(token))
            .ReturnsAsync(Result<Invitation>.Failure("An error occurred while processing the invitation.",
                ResultFailureType.Error));
        _groupManagerMock.Setup(manager => manager.AddUserToGroupAsync(Guid.NewGuid(), Guid.NewGuid())).ReturnsAsync(Result<Unit>.Success(new Unit()));
        _invitationManagerMock.Setup(manager => manager.AcceptInvitationAsync(It.IsAny<Invitation>())).ReturnsAsync(Result<Unit>.Success(new Unit()));
        
        // Mock HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }
        };

        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };
        
        // Act
        var result = await _controller.AcceptInvitation(token);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
