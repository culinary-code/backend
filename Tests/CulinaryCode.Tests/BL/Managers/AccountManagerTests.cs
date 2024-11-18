using AutoMapper;
using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CulinaryCode.Tests.BL.Managers;

public class AccountManagerTests
{
    private readonly Mock<IAccountRepository> _mockRepository;
    private readonly Mock<ILogger<AccountManager>> _loggerMock;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AccountManager _accountManager;
    
    public AccountManagerTests()
    {
        _mockRepository = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<AccountManager>>();
        _mockMapper = new Mock<IMapper>();
        _accountManager = new AccountManager(_mockRepository.Object, _loggerMock.Object, _mockMapper.Object);
    }
    
    [Fact]
    public void GetAccountById_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accountIdString = accountId.ToString();
        var expectedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns(new Account());
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(expectedAccount);

        // Act
        var result = _accountManager.GetAccountById(accountIdString);

        // Assert
        Assert.Equal(expectedAccount, result);
    }
    
    [Fact]
    public void GetAccountById_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid().ToString();
        _mockRepository.Setup(manager => manager.ReadAccount(It.IsAny<Guid>())).Returns((Account)null);

        // Act
        var result = _accountManager.GetAccountById(accountId);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void UpdateAccount_ReturnsUpdatedAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accountIdString = accountId.ToString();
        var updatedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns(new Account());
        _mockMapper.Setup(mapper => mapper.Map<AccountDto>(It.IsAny<Account>())).Returns(updatedAccount);

        // Act
        var result = _accountManager.UpdateAccount(updatedAccount);

        // Assert
        Assert.Equal(updatedAccount, result);
    }
    
    [Fact]
    public void UpdateAccount_ThrowsAccountNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updatedAccount = new AccountDto { AccountId = accountId, Name = "JohnDoe" };
        _mockRepository.Setup(manager => manager.ReadAccount(accountId)).Returns((Account)null);

        // Act & Assert
        Assert.Throws<AccountNotFoundException>(() => _accountManager.UpdateAccount(updatedAccount));
    }

    [Fact]
    public void CreateAccount_CreatesAccount_WhenAccountDoesNotExist()
    {
        // Arrange
        var username = "JohnDoe";
        var email = "johndoe@example.org";
        var userId = Guid.NewGuid();
        _mockRepository.Setup(manager => manager.ReadAccount(userId)).Returns((Account)null);

        // Act
        _accountManager.CreateAccount(username, email, userId);

        // Assert
        _mockRepository.Verify(manager => manager.CreateAccount(It.IsAny<Account>()), Times.Once);
    }
}