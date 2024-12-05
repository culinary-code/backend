using DOM.Accounts;

namespace DAL.Accounts;

public interface IGroupRepository
{
    Task CreateGroupAsync(Group group);
    Task<Group> ReadGroupById(Guid groupId);
    Task<Group> AddUserToGroupAsync(Guid groupId, Guid userId);
}