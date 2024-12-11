using BL.DTOs.Accounts;
using BL.Managers.Accounts;
using BL.Services;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Results;
using Moq;

namespace CulinaryCode.Tests.BL.Managers
{
    public class InvitationManagerTests
    {
        private readonly Mock<IInvitationRepository> _mockInvitationRepository;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<IGroupRepository> _mockGroupRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly InvitationManager _invitationManager;

        public InvitationManagerTests()
        {
            _mockInvitationRepository = new Mock<IInvitationRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockGroupRepository = new Mock<IGroupRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _invitationManager = new InvitationManager(
                _mockInvitationRepository.Object
            );
        }

        [Fact]
        public async Task SendInvitationAsync_SendsEmail_WhenGroupExists()
        {
            // Arrange
            var request = new SendInvitationRequestDto
            {
                GroupId = Guid.NewGuid(),
                InviterId = Guid.NewGuid(),
                InviterName = "Nis",
            };

            var group = new Group { GroupId = request.GroupId, GroupName = "Nis's group" };
            _mockGroupRepository.Setup(repo => repo.ReadGroupById(request.GroupId)).ReturnsAsync(Result<Group>.Success(group));
            _mockInvitationRepository.Setup(repo => repo.SaveInvitationAsync(It.IsAny<Invitation>())).ReturnsAsync(Result<Unit>.Success(new Unit()));
            _mockEmailService.Setup(service => service.SendInvitationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Result<Unit>.Success(new Unit()));

            // Act
            await _invitationManager.SendInvitationAsync(request);

            // Assert
            _mockInvitationRepository.Verify(repo => repo.SaveInvitationAsync(It.IsAny<Invitation>()), Times.Once); 
        }

        [Fact]
        public async Task ValidateInvitationTokenAsync_ReturnsInvitation_WhenTokenIsValid()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var invitation = new Invitation
            {
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddDays(1)
            };

            _mockInvitationRepository.Setup(repo => repo.ReadInvitationByTokenAsync(token)).ReturnsAsync(Result<Invitation>.Success(invitation));

            // Act
            var result = await _invitationManager.ValidateInvitationTokenAsync(token);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(token, result.Value!.Token);
        }
    }
}
