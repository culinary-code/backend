using DAL.EF;
using DOM.Accounts;
using DOM.Results;
using Microsoft.EntityFrameworkCore;

namespace DAL.Accounts;

public class InvitationRepository : IInvitationRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public InvitationRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Result<Unit>> SaveInvitationAsync(Invitation invitation)
    {
        await _ctx.AddAsync(invitation);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }

    public async Task<Result<Invitation>> ReadInvitationByTokenAsync(string token)
    { 
        var invitation = await _ctx.Invitations
            .FirstOrDefaultAsync(i => i.Token == token);
        if (invitation is null)
        {
            return Result<Invitation>.Failure("Invitation not found", ResultFailureType.NotFound);
        }
        return Result<Invitation>.Success(invitation);
    }

    public async Task<Result<Unit>> DeleteInvitationAsync(Invitation invitation)
    {
        _ctx.Invitations.Remove(invitation);
        await _ctx.SaveChangesAsync();
        return Result<Unit>.Success(new Unit());
    }
}