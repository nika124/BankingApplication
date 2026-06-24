namespace BankingApplication.Models.DTOs.Transactions;

public class TransactionDto
{
    public int TransactionId { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
}
