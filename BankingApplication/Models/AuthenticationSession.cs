namespace BankingApplication.Models;

public class AuthenticationSession
{
    public Guid SessionId { get; set; }
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public DateTime ExpiresAt { get; set; }
}