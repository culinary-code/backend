using DOM.Accounts;

namespace BL.Managers.Accounts;

public interface IGroupManager
{
    Task CreateGroupAsync(string groupName, Guid ownerId);
    Task AddUserToGroupAsync(Guid groupId, Guid userId);
    Task<List<Group>> GetAllGroupsByUserIdAsync(Guid userId);
    Task RemoveUserFromGroup(Guid groupId, Guid userId);
}