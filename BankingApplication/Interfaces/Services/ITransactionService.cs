using BankingApplication.Models.DTOs.Transactions;
using BankingApplication.Models.Results;
using BankingApplication.Models.Results.Transactions;

namespace BankingApplication.Interfaces.Services;

public interface ITransactionService
{
    ServiceResult CreateTransaction(
        int accountId,
        decimal amount,
        string currencyCode,
        string transactionType,
        decimal balanceAfter,
        string description,
        out TransactionDto? transaction);

    ServiceResult GetTransactionsByAccountId(int accountId, out IReadOnlyList<TransactionDto> transactions);
    LastTransactionsResult GetLastTransactions(Guid sessionId);
}
