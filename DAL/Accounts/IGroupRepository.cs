using DOM.Accounts;
using DOM.Results;

namespace DAL.Accounts;

public interface IGroupRepository
{
    Task<Result<Unit>> CreateGroupAsync(Group group);
    Task<Result<Group>> ReadGroupById(Guid groupId);
    Task<Result<List<Group>>> ReadGroupsByUserId(Guid userId);
    Task<Result<Group>> ReadGroupWithMealPlannerNextWeekAndWithGroceryListNoTracking(Guid id);
    Task<Result<Group>> AddUserToGroupAsync(Guid groupId, Guid userId);
    Task<Result<Unit>> DeleteUserFromGroup(Guid groupId, Guid userId);
}