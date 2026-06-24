using BankingApplication.Models.DTOs.AccountBalance;
using BankingApplication.Models.DTOs.Currency;
using BankingApplication.Models.DTOs.Transactions;

namespace BankingApplication.Models.Results.AccountBalance;

public class ConvertMoneyResult : ServiceResult
{
    public CurrencyConversionDto? Conversion { get; set; }
    public AccountBalanceDto? SourceBalance { get; set; }
    public AccountBalanceDto? TargetBalance { get; set; }
    public IReadOnlyList<TransactionDto> Transactions { get; set; } = [];
}
