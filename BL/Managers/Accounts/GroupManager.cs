using DAL.Accounts;
using DOM.Accounts;

namespace BL.Managers.Accounts;

public class GroupManager : IGroupManager
{
    private readonly IGroupRepository _groupRepository;
    private readonly IAccountManager _accountManager;
    private readonly IAccountRepository _accountRepository;

    public GroupManager(IGroupRepository groupRepository, IAccountManager accountManager, IAccountRepository accountRepository)
    {
        _groupRepository = groupRepository;
        _accountManager = accountManager;
        _accountRepository = accountRepository;
    }

    public async Task CreateGroupAsync(string groupName, Guid ownerId)
    {
        var group = new Group
        {
            GroupName = groupName,
        };
        
        var owner = await _accountRepository.ReadAccount(ownerId);
        
        group.Accounts.Add(owner);
        await _groupRepository.CreateGroupAsync(group);
    }

    public async Task AddUserToGroupAsync(Guid groupId, Guid userId)
    {
        await _groupRepository.AddUserToGroupAsync(groupId, userId);
    }

    public async Task<List<Group>> GetAllGroupsByUserIdAsync(Guid userId)
    {
        return await _groupRepository.ReadGroupsByUserId(userId);
    }

    public async Task RemoveUserFromGroup(Guid groupId, Guid userId)
    {
        await _groupRepository.DeleteUserFromGroup(groupId, userId);
    }
}