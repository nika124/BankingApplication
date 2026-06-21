namespace BankingApplication.Models.Requests.AccountBalance;

public class DepositRequest
{
    public Guid SessionId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}
