namespace BankingApplication.Models.DTOs.AccountBalance;

public class AccountBalanceDto
{
    public int AccountId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
