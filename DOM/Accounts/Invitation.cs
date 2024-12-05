using System.ComponentModel.DataAnnotations;

namespace DOM.Accounts;

public class Invitation
{
    [Key]
    public Guid InvitationId { get; set; }
    public Guid GroupId { get; set; }
    public Guid InviterId { get; set; }
    public string InviterName { get; set; }
    public string InvitedUserName { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool isAccepted { get; set; }
    public DateTime? AcceptedDate { get; set; }
    
    
    // Navigation Properties
    public Group Group { get; set; }
    public Account Inviter { get; set; }
}