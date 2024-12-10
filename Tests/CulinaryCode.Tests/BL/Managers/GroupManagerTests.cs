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
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly GroupManager _groupManager;

    public GroupManagerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _groupManager = new GroupManager(_groupRepositoryMock.Object, _accountRepositoryMock.Object);
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
    
    [Fact]
    public async Task AddUserToGroupAsync_AddsUser_WhenGroupExists()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var group = new Group { GroupId = groupId, GroupName = "Test Group" };
        var user = new Account { AccountId = userId, Name = "Test User" };

        _groupRepositoryMock.Setup(repo => repo.ReadGroupById(groupId)).ReturnsAsync(group);
        _groupRepositoryMock.Setup(repo => repo.AddUserToGroupAsync(groupId, userId));

        // Act
        await _groupManager.AddUserToGroupAsync(groupId, userId);
        _testOutputHelper.WriteLine(group.Accounts.Count.ToString());
        _testOutputHelper.WriteLine(group.GroupId.ToString());
        _testOutputHelper.WriteLine(group.GroupName);

        // Assert
        _groupRepositoryMock.Verify(repo => repo.AddUserToGroupAsync(groupId, userId), Times.Once);
    }
    
    [Fact]
    public async Task RemoveUserFromGroup_RemovesUser_WhenUserIsInGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var group = new Group { GroupId = groupId, GroupName = "Test Group" };
        var user = new Account { AccountId = userId, Name = "Test User" };

        group.Accounts.Add(user);
        _groupRepositoryMock.Setup(repo => repo.ReadGroupById(groupId)).ReturnsAsync(group);
        _groupRepositoryMock.Setup(repo => repo.DeleteUserFromGroup(groupId, userId)).Callback<Guid, Guid>((gId, uId) =>
        {
            var groupToModify = group;
            var accountToRemove = groupToModify.Accounts.FirstOrDefault(a => a.AccountId == uId);
            if (accountToRemove != null)
            {
                groupToModify.Accounts.Remove(accountToRemove);
            }
        }).Returns(Task.CompletedTask);

        // Act
        await _groupManager.RemoveUserFromGroup(groupId, userId);

        // Assert
        _groupRepositoryMock.Verify(repo => repo.DeleteUserFromGroup(groupId, userId), Times.Once);
        Assert.DoesNotContain(user, group.Accounts);
    }
}