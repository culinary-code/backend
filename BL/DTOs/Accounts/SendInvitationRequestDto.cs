namespace BL.DTOs.Accounts;

public class SendInvitationRequestDto
{
    public Guid GroupId { get; set; }
    public string Email { get; set; }
    public Guid InviterId { get; set; }
    public string InviterName { get; set; }
    public string InvitedUserName { get; set; }
}