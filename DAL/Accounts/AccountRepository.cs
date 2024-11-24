using System;
using DAL.EF;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DAL.Accounts;

public class AccountRepository : IAccountRepository
{
    
    private readonly CulinaryCodeDbContext _ctx;

    public AccountRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }
    
    public Account ReadAccount(Guid id)
    {
        Account? account = _ctx.Accounts.Find(id);
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        return account;
    }

    public Account ReadPreferencesByAccountId(Guid id)
    {
        var account = _ctx.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefault(a => a.AccountId == id);
        
        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }
        
        return account;
    }

    public void UpdateAccount(Account account)
    {
        _ctx.Accounts.Update(account);
        _ctx.SaveChanges();    
    }

    public void CreateAccount(Account account)
    {
        _ctx.Accounts.Add(account);
        _ctx.SaveChanges(); 
    }
}