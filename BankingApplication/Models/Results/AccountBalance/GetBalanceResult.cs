using BankingApplication.Models.DTOs.AccountBalance;

namespace BankingApplication.Models.Results.AccountBalance;

public class GetBalanceResult : ServiceResult
{
    public AccountBalancesDto? AccountBalances { get; set; }
}
