namespace BankingApplication.Models.DTOs.Authentication;

public class PendingAuthenticationDto
{
    public Guid AuthenticationId { get; set; }
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
}
