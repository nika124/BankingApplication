namespace BankingApplication.Models.Requests;

public class CompleteAuthenticationRequest
{
    public Guid AuthenticationId { get; set; }
    public string Pin { get; set; } = string.Empty;
}
