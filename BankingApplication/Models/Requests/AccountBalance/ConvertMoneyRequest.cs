namespace BankingApplication.Models.Requests.AccountBalance;

public class ConvertMoneyRequest
{
    public Guid SessionId { get; set; }
    public decimal Amount { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
}
