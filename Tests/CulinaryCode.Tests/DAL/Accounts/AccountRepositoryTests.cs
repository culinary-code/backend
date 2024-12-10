using DAL.Accounts;
using DAL.EF;
using DOM.Accounts;
using DOM.Results;
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
    public async Task ReadAccount_AccountExists_ReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _accountRepository.ReadAccount(account.AccountId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(account.AccountId, result.Value!.AccountId);
        Assert.Equal(account.Name, result.Value.Name);
        Assert.Equal(account.Email, result.Value.Email);
    }
    
    [Fact]
    public async Task ReadAccount_AccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        
        // Act
        var result = await _accountRepository.ReadAccount(accountId);
        
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task UpdateAccount_AccountExists_UpdatesAccount()
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
        await _accountRepository.UpdateAccount(account);

        // Assert
        var result = await _dbContext.Accounts.FindAsync(account.AccountId);
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal("Updated Account", result.Name);
    }

    [Fact]
    public async Task CreateAccount_ValidAccount_CreatesAccount()
    {
        // Arrange
        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Test Account",
            Email = "test@example.org"
        };

        // Act
        await _accountRepository.CreateAccount(account);

        // Assert
        var result = await _dbContext.Accounts.FindAsync(account.AccountId);
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(account.Name, result.Name);
        Assert.Equal(account.Email, result.Email);
    }
}