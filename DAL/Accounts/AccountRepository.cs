using System;
using DAL.EF;
using DOM.Exceptions;

namespace DAL.Accounts;

public class AccountRepository : IAccountRepository
{
    
    private readonly CulinaryCodeDbContext _ctx;

    public AccountRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }


    public DOM.Accounts.Account ReadAccount(Guid id)
    {
        DOM.Accounts.Account? account = _ctx.Accounts.Find(id);
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        return account;
    }

    public void UpdateAccount(DOM.Accounts.Account account)
    {
        _ctx.Accounts.Update(account);
        _ctx.SaveChanges();    
    }

    public void CreateAccount(DOM.Accounts.Account account)
    {
        _ctx.Accounts.Add(account);
        _ctx.SaveChanges(); 
    }
}