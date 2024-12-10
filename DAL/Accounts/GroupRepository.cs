using DAL.EF;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DAL.Accounts;

public class GroupRepository : IGroupRepository
{
    private readonly CulinaryCodeDbContext _ctx;
    private readonly IAccountRepository _accountRepository;

    public GroupRepository(CulinaryCodeDbContext ctx, IAccountRepository accountRepository)
    {
        _ctx = ctx;
        _accountRepository = accountRepository;
    }

    public async Task CreateGroupAsync(Group group)
    {
        if (group is null)
        {
            throw new ArgumentNullException(nameof(group));
        }
        
        await _ctx.Groups.AddAsync(group);
        await _ctx.SaveChangesAsync();
    }

    public async Task<Group> ReadGroupById(Guid groupId)
    {
        var group = await _ctx.Groups
            .Include(g => g.Accounts)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }
        
        return group;
    }

    public async Task<List<Group>> ReadGroupsByUserId(Guid userId)
    {
        var groups = await _ctx.Groups
            .Where(a => a.Accounts.Any(b => b.AccountId == userId))
            .ToListAsync();
            
        if (groups.Count == 0)
        {
            return [];
        }
        
        return groups;
    }

    public async Task<Group> AddUserToGroupAsync(Guid groupId, Guid userId)
    {
        var group = await ReadGroupById(groupId);

        var user = await _accountRepository.ReadAccount(userId);
        if (user is null)
        {
            throw new AccountNotFoundException(nameof(user));
        }
        
        group.Accounts.Add(user);
        await _ctx.SaveChangesAsync();
        return group;
    }

    public async Task DeleteUserFromGroup(Guid groupId, Guid userId)
    {
        var group = await ReadGroupById(groupId);

        group.Accounts.Remove(group.Accounts.First(a => a.AccountId == userId));
        if (!group.Accounts.Any())
        {
            _ctx.Groups.Remove(group);
        }
        await _ctx.SaveChangesAsync();
    }
}