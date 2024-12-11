using BL.DTOs.Accounts;
using BL.Services;
using DAL.Accounts;
using DOM.Accounts;
using DOM.Results;

namespace BL.Managers.Accounts;

public class InvitationManager : IInvitationManager
{
    private readonly IInvitationRepository _invitationRepository;

    public InvitationManager(IInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;
    }
    
    public async Task<Result<string>> SendInvitationAsync(SendInvitationRequestDto request)
    {
        var invitation = new Invitation
        {
            GroupId = request.GroupId,
            InviterId = request.InviterId,
            InviterName = request.InviterName,
            InvitedUserName = request.InvitedUserName,
            Token = Guid.NewGuid().ToString(),
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            isAccepted = false
        };

        var saveInvitationResult = await _invitationRepository.SaveInvitationAsync(invitation);
        if (!saveInvitationResult.IsSuccess) return Result<string>.Failure(saveInvitationResult.ErrorMessage!, saveInvitationResult.FailureType);
        var invitationLink = $"com.culinarycode://accept-invitation/{invitation.Token}";
        return Result<string>.Success(invitationLink);
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