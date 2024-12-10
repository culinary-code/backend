using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DOM.Accounts;
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
            .Returns(userId);

        _accountManagerMock
            .Setup(manager => manager.GetAccountById(userId.ToString()))
            .ReturnsAsync(inviter);

        _invitationManagerMock
            .Setup(manager => manager.SendInvitationAsync(It.IsAny<SendInvitationRequestDto>()))
            .Returns(Task.CompletedTask);

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
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualInvitation = okResult.Value as SendInvitationRequestDto;

        Assert.NotNull(actualInvitation);
        Assert.Equal(request.Email, actualInvitation.Email);
        Assert.Equal(request.GroupId, actualInvitation.GroupId);
        Assert.Equal(request.InvitedUserName, actualInvitation.InvitedUserName);
        Assert.Equal(userId, actualInvitation.InviterId);
        Assert.Equal("InviterName", actualInvitation.InviterName); 
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
            .Returns(userId);

        _invitationManagerMock
            .Setup(manager => manager.ValidateInvitationTokenAsync(token))
            .ReturnsAsync(invitation);

        _groupManagerMock
            .Setup(manager => manager.AddUserToGroupAsync(invitation.GroupId, userId))
            .Returns(Task.CompletedTask);

        _invitationManagerMock
            .Setup(manager => manager.AcceptInvitationAsync(invitation))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AcceptInvitation(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value as Invitation;

        Assert.NotNull(value);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsBadRequest_WhenTokenIsInvalid()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();

        _invitationManagerMock
            .Setup(manager => manager.ValidateInvitationTokenAsync(token))
            .ReturnsAsync((Invitation)null);

        // Act
        var result = await _controller.AcceptInvitation(token);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid or expired invitation token.", badRequestResult.Value);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsServerError_WhenAnExceptionOccurs()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();

        _invitationManagerMock
            .Setup(manager => manager.ValidateInvitationTokenAsync(token))
            .Throws(new Exception("Unexpected error"));

        // Act
        var result = await _controller.AcceptInvitation(token);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while processing the invitation.", statusCodeResult.Value);
    }
}
