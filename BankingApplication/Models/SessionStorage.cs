namespace BankingApplication.Models;

public class SessionStorage
{
    public Guid SessionId { get; set; }
    public Dictionary<string, string> Values { get; set; } = [];
}
