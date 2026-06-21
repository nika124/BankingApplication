using BankingApplication.Models.DTOs.Transactions;

namespace BankingApplication.Models.Results.Transactions;

public class LastTransactionsResult : ServiceResult
{
    public IReadOnlyList<TransactionDto> Transactions { get; set; } = [];
}
