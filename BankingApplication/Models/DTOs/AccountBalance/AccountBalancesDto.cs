namespace BankingApplication.Models.DTOs.AccountBalance;

public class AccountBalancesDto
{
    public int AccountId { get; set; }
    public IReadOnlyList<AccountBalanceDto> Balances { get; set; } = [];
}
