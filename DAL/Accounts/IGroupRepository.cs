using DOM.Accounts;

namespace DAL.Accounts;

public interface IGroupRepository
{
    Task CreateGroupAsync(Group group);
    Task<Group> ReadGroupById(Guid groupId);
    Task<List<Group>> ReadGroupsByUserId(Guid userId);
    Task<Group> AddUserToGroupAsync(Guid groupId, Guid userId);
    Task DeleteUserFromGroup(Guid groupId, Guid userId);
}