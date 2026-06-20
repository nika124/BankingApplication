namespace BankingApplication.Models;

public class PendingAtmAuthentication
{
    public Guid AuthenticationId { get; set; }
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public DateTime ExpiresAt { get; set; }
}