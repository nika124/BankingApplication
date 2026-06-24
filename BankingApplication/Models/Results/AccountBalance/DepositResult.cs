using BankingApplication.Models.DTOs.AccountBalance;
using BankingApplication.Models.DTOs.Transactions;

namespace BankingApplication.Models.Results.AccountBalance;

public class DepositResult : ServiceResult
{
    public AccountBalanceDto? UpdatedBalance { get; set; }
    public TransactionDto? Transaction { get; set; }
}
