namespace BankingApplication.Models.Requests;

public class CompleteAuthenticationRequest
{
    public Guid SessionId { get; set; }
    public string Pin { get; set; } = string.Empty;
}
