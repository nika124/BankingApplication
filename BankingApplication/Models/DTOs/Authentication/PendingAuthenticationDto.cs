namespace BankingApplication.Models.DTOs.Authentication;

public class PendingAuthenticationDto
{
    public Guid SessionId { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
}
