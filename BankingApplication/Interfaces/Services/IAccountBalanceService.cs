using BankingApplication.Models.Requests.AccountBalance;
using BankingApplication.Models.Results.AccountBalance;

namespace BankingApplication.Interfaces.Services
{
    public interface IAccountBalanceService
    {
        GetBalanceResult GetBalance(GetBalanceRequest request);
        DepositResult DepositMoney(DepositRequest request);
        WithdrawalResult WithdrawMoney(WithdrawalRequest request);
        ConvertMoneyResult ConvertMoney(ConvertMoneyRequest request);
    }
}
