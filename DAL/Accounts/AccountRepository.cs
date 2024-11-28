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

    public Account ReadAccountWithPreferencesByAccountId(Guid id)
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

    public void DeletePreferenceFromAccount(Guid accountId, Guid preferenceId)
    {
        var account = _ctx.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefault(a => a.AccountId == accountId);

        if (account == null)
        {
            throw new AccountNotFoundException("Account not found");
        }

        var preferenceToRemove = account.Preferences?.FirstOrDefault(p => p.PreferenceId == preferenceId);
        if (preferenceToRemove == null)
        {
            throw new Exception("Preference not found for this account");
        }

        if (preferenceToRemove.StandardPreference)
        {
            account.Preferences = account.Preferences.Where(p => p.PreferenceId != preferenceId).ToList();
        }
        else
        {
            _ctx.Preferences.Remove(preferenceToRemove);
        }

        _ctx.SaveChanges();
    }

}