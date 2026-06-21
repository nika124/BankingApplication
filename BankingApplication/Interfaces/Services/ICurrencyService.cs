using BankingApplication.Models.Results;

namespace BankingApplication.Interfaces.Services;

public interface ICurrencyService
{
    ServiceResult GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, out decimal exchangeRate);
    ServiceResult Convert(decimal amount, string fromCurrencyCode, string toCurrencyCode, out decimal convertedAmount);
}
