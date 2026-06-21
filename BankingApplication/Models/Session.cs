using BankingApplication.Enums;

namespace BankingApplication.Models;

public class Session
{
    public Guid SessionId { get; set; }
    public SessionType SessionType { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
}
