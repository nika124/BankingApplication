namespace BankingApplication.Models.DTOs.Authentication;

public class AuthorizedSessionDto
{
    public Guid SessionId { get; set; }
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public string? MaskedCardNumber { get; set; }
}
