using DOM.Accounts;

namespace BL.Managers.Accounts;

public interface IGroupManager
{
    Task CreateGroupAsync(string groupName, Guid ownerId);
    Task AddUserToGroupAsync(Guid groupId, Guid userId);
}