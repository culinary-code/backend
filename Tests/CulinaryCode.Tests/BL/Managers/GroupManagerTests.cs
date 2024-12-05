using BL.Managers.Accounts;
using DAL.Accounts;
using DOM.Accounts;
using Moq;
using Xunit.Abstractions;

namespace CulinaryCode.Tests.BL.Managers;

public class GroupManagerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly Mock<IAccountManager> _accountManagerMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IGroupManager> _groupManagerMock;
    private readonly GroupManager _groupManager;

    public GroupManagerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _accountManagerMock = new Mock<IAccountManager>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _groupManagerMock = new Mock<IGroupManager>();
        _groupManager = new GroupManager(_groupRepositoryMock.Object, _accountManagerMock.Object, _accountRepositoryMock.Object);
    }
    
    
    [Fact]
    public async Task CreateGroupAsync_CreatesGroup_WhenDataIsValid()
    {
        // Arrange
        var groupName = "Test Group";
        var ownerId = Guid.NewGuid();
        var ownerAccount = new Account { AccountId = ownerId, Name = "Owner" };

        _accountRepositoryMock.Setup(repo => repo.ReadAccount(ownerId)).ReturnsAsync(ownerAccount);
        
        // Act
        await _groupManager.CreateGroupAsync(groupName, ownerId);
        _testOutputHelper.WriteLine(groupName);

        // Assert
        _groupRepositoryMock.Verify(repo => repo.CreateGroupAsync(It.Is<Group>(g => g.GroupName == groupName && g.Accounts.Contains(ownerAccount))), Times.Once);
    }
}