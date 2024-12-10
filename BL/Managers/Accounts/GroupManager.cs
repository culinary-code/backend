using DAL.Accounts;
using DOM.Accounts;
using DOM.Results;

namespace BL.Managers.Accounts;

public class GroupManager : IGroupManager
{
    private readonly IGroupRepository _groupRepository;
    private readonly IAccountRepository _accountRepository;

    public GroupManager(IGroupRepository groupRepository, IAccountRepository accountRepository)
    {
        _groupRepository = groupRepository;
        _accountRepository = accountRepository;
    }

    public async Task<Result<Unit>> CreateGroupAsync(string groupName, Guid ownerId)
    {
        var group = new Group
        {
            GroupName = groupName,
        };
        
        var ownerResult = await _accountRepository.ReadAccount(ownerId);
        if (!ownerResult.IsSuccess)
        {
            return Result<Unit>.Failure(ownerResult.ErrorMessage!, ownerResult.FailureType);
        }
        var owner = ownerResult.Value!;
        
        group.Accounts.Add(owner);
        await _groupRepository.CreateGroupAsync(group);
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Unit>> AddUserToGroupAsync(Guid groupId, Guid userId)
    {
        var groupResult = await _groupRepository.AddUserToGroupAsync(groupId, userId);
        if (!groupResult.IsSuccess)
        {
            return Result<Unit>.Failure(groupResult.ErrorMessage!, groupResult.FailureType);
        }
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<List<Group>>> GetAllGroupsByUserIdAsync(Guid userId)
    {
        return await _groupRepository.ReadGroupsByUserId(userId);
    }

    public async Task<Result<Unit>> RemoveUserFromGroup(Guid groupId, Guid userId)
    {
        return await _groupRepository.DeleteUserFromGroup(groupId, userId);
    }
}