using DAL.Accounts;
using DAL.EF;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CulinaryCode.Tests.DAL.Accounts;

public class AccountRepositoryTests
{
    private readonly CulinaryCodeDbContext _dbContext;
    private readonly IAccountRepository _accountRepository;
    
    public AccountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            // force unique database for each test so data is isolated
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;
        
        _dbContext = new CulinaryCodeDbContext(options);
        _accountRepository = new AccountRepository(_dbContext);
    }

    [Fact]
    public void ReadAccount_AccountExists_ReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        _dbContext.SaveChanges();

        // Act
        var result = _accountRepository.ReadAccount(account.AccountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(account.Name, result.Name);
        Assert.Equal(account.Email, result.Email);
    }
    
    [Fact]
    public void ReadAccount_AccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        
        // Act & Assert
        Assert.Throws<AccountNotFoundException>(() => _accountRepository.ReadAccount(accountId));
    }

    [Fact]
    public void UpdateAccount_AccountExists_UpdatesAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        _dbContext.SaveChanges();

        // Act
        account.Name = "Updated Account";
        _accountRepository.UpdateAccount(account);

        // Assert
        var result = _dbContext.Accounts.Find(account.AccountId);
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
    }

    [Fact]
    public void CreateAccount_ValidAccount_CreatesAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        // Act
        _accountRepository.CreateAccount(account);

        // Assert
        var result = _dbContext.Accounts.Find(account.AccountId);
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(account.Name, result.Name);
        Assert.Equal(account.Email, result.Email);
    }
}