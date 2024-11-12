using DAL.Accounts;
using DAL.EF;
using DOM.Accounts;

namespace DAL.Accounts;

public class AccountRepository : IAccountRepository
{
    
    private readonly CulinaryCodeDbContext _ctx;

    public AccountRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }
    

    public void UpdateAccount(Account account)
    {
        _ctx.Accounts.Update(account);
        _ctx.SaveChanges();    }
}