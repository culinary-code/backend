using BL.DTOs.Accounts;
using BL.Services;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Results;

namespace BL.Managers.Accounts;

public class InvitationManager : IInvitationManager
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IEmailService _emailService;

    public InvitationManager(IInvitationRepository invitationRepository, IAccountRepository accountRepository, IGroupRepository groupRepository, IEmailService emailService)
    {
        _invitationRepository = invitationRepository;
        _accountRepository = accountRepository;
        _groupRepository = groupRepository;
        _emailService = emailService;
    }

    public async Task<Result<Unit>> SendInvitationAsync(SendInvitationRequestDto request)
    {
        var invitation = new Invitation
        {
            GroupId = request.GroupId,
            InviterId = request.InviterId,
            InviterName = request.InviterName,
            InvitedUserName = request.InvitedUserName,
            Email = request.Email,
            Token = Guid.NewGuid().ToString(),
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            isAccepted = false
        };
        var saveInvitationResult = await _invitationRepository.SaveInvitationAsync(invitation);
        if (!saveInvitationResult.IsSuccess) return saveInvitationResult;
        var sendInvitationResult = await _emailService.SendInvitationEmailAsync(request.Email, invitation.Token, invitation.InvitedUserName, invitation.InviterName);
        return sendInvitationResult;
    }

    public async Task<Result<Invitation>> ValidateInvitationTokenAsync(string token)
    {
        var invitationResult = await _invitationRepository.ReadInvitationByTokenAsync(token);
        if (!invitationResult.IsSuccess)
        {
            return invitationResult;
        }
        var invitation = invitationResult.Value!;
        
        if (invitation.ExpirationDate < DateTime.UtcNow)
        {
            var deleteInvitationResult = await _invitationRepository.DeleteInvitationAsync(invitation);
            if (!deleteInvitationResult.IsSuccess) return Result<Invitation>.Failure(deleteInvitationResult.ErrorMessage!, deleteInvitationResult.FailureType);
        }
        return Result<Invitation>.Success(invitation);
    }

    public async Task<Result<Unit>> AcceptInvitationAsync(Invitation invitation)
    {
        invitation.isAccepted = true;
        var deleteInvitationResult = await _invitationRepository.DeleteInvitationAsync(invitation);
        return deleteInvitationResult;
    }
}