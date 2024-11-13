namespace DAL.Accounts;

using DOM.Accounts;

public interface IAccountRepository
{
    Account ReadAccount(Guid id);
    void UpdateAccount(Account account);
    void CreateAccount(Account account);
}