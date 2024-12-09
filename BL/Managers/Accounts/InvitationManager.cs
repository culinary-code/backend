using BL.DTOs.Accounts;
using BL.Services;
using DAL.Accounts;
using DOM.Accounts;

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

    public async Task SendInvitationAsync(SendInvitationRequestDto request)
    {
        var group = await _groupRepository.ReadGroupById(request.GroupId);
        if (group == null)
        {
            throw new Exception("Group does not exist");
        }
        
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
        Console.WriteLine("INVITATIONTOKEN:" + invitation.Token);
        await _invitationRepository.SaveInvitationAsync(invitation);
        await _emailService.SendInvitationEmailAsync(request.Email, invitation.Token, invitation.InvitedUserName, invitation.InviterName);
    }

    public async Task<Invitation> ValidateInvitationTokenAsync(string token)
    {
        var invitation = await _invitationRepository.GetInvitationByTokenAsync(token);
        if (invitation.ExpirationDate < DateTime.UtcNow)
        {
            await _invitationRepository.DeleteInvitationAsync(invitation);
        }

        return invitation;
    }

    // TODO: hier nog kijken, miss na acceptatie of wanneer expired, invitation verwijderen uit db
    // TODO: ik denk overbodig nu
    public async Task AcceptInvitationAsync(Invitation invitation)
    {
        invitation.isAccepted = true;
        await _invitationRepository.DeleteInvitationAsync(invitation);
    }
}