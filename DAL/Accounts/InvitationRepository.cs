using DAL.EF;
using DOM.Accounts;
using Microsoft.EntityFrameworkCore;

namespace DAL.Accounts;

public class InvitationRepository : IInvitationRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public InvitationRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task SaveInvitationAsync(Invitation invitation)
    {
        await _ctx.AddAsync(invitation);
        await _ctx.SaveChangesAsync();
    }

    public async Task<Invitation> GetInvitationByTokenAsync(string token)
    { 
        return await _ctx.Invitations
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task DeleteInvitationAsync(Invitation invitation)
    {
        _ctx.Invitations.Remove(invitation);
        await _ctx.SaveChangesAsync();
    }
}