using BL.Managers.Accounts;
using BL.Services;
using DOM.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WEBAPI.Controllers;

namespace CulinaryCode.Tests.WEBAPI;

public class GroupControllerTests
{
    private readonly Mock<IGroupManager> _groupManagerMock;
    private readonly Mock<IAccountManager> _accountManagerMock;
    private readonly Mock<IIdentityProviderService> _identityProviderServiceMock;
    private readonly Mock<ILogger<GroupController>> _loggerMock;
    private readonly GroupController _groupController;

    public GroupControllerTests()
    {
        _groupManagerMock = new Mock<IGroupManager>();
        _accountManagerMock = new Mock<IAccountManager>();
        _identityProviderServiceMock = new Mock<IIdentityProviderService>();
        _loggerMock = new Mock<ILogger<GroupController>>();
        _groupController = new GroupController(_loggerMock.Object, _groupManagerMock.Object, _accountManagerMock.Object, _identityProviderServiceMock.Object);
    }
    
    [Fact]
    public async Task CreateGroup_ReturnsOk_WhenGroupIsCreated()
    {
        // Arrange
        var groupName = "Test Group";
        var ownerId = Guid.NewGuid();

        _identityProviderServiceMock
            .Setup(s => s.GetGuidFromAccessToken(It.IsAny<string>()))
            .Returns(Result<Guid>.Success(ownerId));
        _groupManagerMock.Setup(g=> g.CreateGroupAsync(groupName, ownerId)).ReturnsAsync(Result<Unit>.Success(new Unit()));

        // Mock the HttpContext and Authorization header
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary
        {
            { "Authorization", "Bearer fake-jwt-token" }
        };
    
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        
        _groupController.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };
        
        // Act
        var result = await _groupController.CreateGroup(groupName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _groupManagerMock.Verify(manager => manager.CreateGroupAsync(groupName, ownerId), Times.Once);
    }
} 