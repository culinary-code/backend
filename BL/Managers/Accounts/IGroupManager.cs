using DOM.Accounts;
using DOM.Results;

namespace BL.Managers.Accounts;

public interface IGroupManager
{
    Task<Result<Unit>> CreateGroupAsync(string groupName, Guid ownerId);
    Task<Result<Unit>> AddUserToGroupAsync(Guid groupId, Guid userId);
    Task<Result<List<Group>>> GetAllGroupsByUserIdAsync(Guid userId);
    Task<Result<Unit>> RemoveUserFromGroup(Guid groupId, Guid userId);
}