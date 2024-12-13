using DAL.EF;
using DOM.Accounts;
using DOM.Results;
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

    public async Task<Result<Unit>> CreateGroupAsync(Group group)
    {
        await _ctx.Groups.AddAsync(group);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Group>> ReadGroupById(Guid groupId)
    {
        var group = await _ctx.Groups
            .Include(g => g.Accounts)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        if (group == null)
        {
            return Result<Group>.Failure("Group not found", ResultFailureType.NotFound);
        }
        
        return Result<Group>.Success(group);
    }

    public async Task<Result<List<Group>>> ReadGroupsByUserId(Guid userId)
    {
        var groups = await _ctx.Groups
            .Where(a => a.Accounts.Any(b => b.AccountId == userId))
            .ToListAsync();
        
        return Result<List<Group>>.Success(groups);
    }

    public async Task<Result<Group>> ReadGroupWithMealPlannerNextWeekAndWithGroceryListNoTracking(Guid id)
    {
        var group = await _ctx.Groups
            .Include(g => g.GroceryList)
            .ThenInclude(gl => gl!.Ingredients)
            .ThenInclude(iq => iq.Ingredient)
            .Include(g => g.GroceryList)
            .ThenInclude(gl => gl!.Ingredients)
            .ThenInclude(iq => iq.PlannedMeal)
            .ThenInclude(p => p.Recipe )
            .Include(a => a.GroceryList)
            .ThenInclude(gl => gl!.Items)
            .ThenInclude(i => i.GroceryItem)
            .Include(a => a.MealPlanner)
            .ThenInclude(p => p!.NextWeek)
            .ThenInclude(n => n.Ingredients)
            .ThenInclude(iq => iq.Ingredient)
            .Include(g => g.MealPlanner)
            .ThenInclude(p => p!.NextWeek)
            .ThenInclude(p => p.Recipe)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(g => g.GroupId == id);

        if (group == null)
        {
            return Result<Group>.Failure("Group not found", ResultFailureType.NotFound);
        }
        return Result<Group>.Success(group);
    }

    public async Task<Result<Group>> AddUserToGroupAsync(Guid groupId, Guid userId)
    {
        var groupResult = await ReadGroupById(groupId);

        if (!groupResult.IsSuccess)
        {
            return groupResult;
        }

        var userResult = await _accountRepository.ReadAccount(userId);
        if (!userResult.IsSuccess)
        {
            return Result<Group>.Failure(userResult.ErrorMessage!, userResult.FailureType);
        }
        var group = groupResult.Value!;
        var user  = userResult.Value!;
        group.Accounts.Add(user);
        await _ctx.SaveChangesAsync();
        return Result<Group>.Success(group);
    }

    public async Task<Result<Unit>> DeleteUserFromGroup(Guid groupId, Guid userId)
    {
        var groupResult = await ReadGroupById(groupId);

        if (!groupResult.IsSuccess)
        {
            return Result<Unit>.Failure(groupResult.ErrorMessage!, groupResult.FailureType);
        }
        var group = groupResult.Value!;

        group.Accounts.Remove(group.Accounts.First(a => a.AccountId == userId));
        if (group.Accounts.Count == 0)
        {
            _ctx.Groups.Remove(group);
        }
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }
}