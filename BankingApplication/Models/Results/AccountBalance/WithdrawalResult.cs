using BankingApplication.Models.DTOs.AccountBalance;
using BankingApplication.Models.DTOs.Transactions;

namespace BankingApplication.Models.Results.AccountBalance;

public class WithdrawalResult : ServiceResult
{
    public AccountBalanceDto? UpdatedBalance { get; set; }
    public TransactionDto? Transaction { get; set; }
}
