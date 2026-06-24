using BankingApplication.Interfaces.Services;
using BankingApplication.Models.Results;

namespace BankingApplication.Services;

public class CurrencyService : ICurrencyService
{
    private static readonly IReadOnlyDictionary<string, decimal> RatesToGel =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["GEL"] = 1m,
            ["USD"] = 2.70m,
            ["EUR"] = 2.95m,
            ["GBP"] = 3.45m
        };

    public ServiceResult GetExchangeRate(
        string fromCurrencyCode,
        string toCurrencyCode,
        out decimal exchangeRate)
    {
        var result = new ServiceResult
        {
            Message = "Exchange rate found."
        };
        exchangeRate = 0;

        if (string.IsNullOrWhiteSpace(fromCurrencyCode))
        {
            result.Message = "Exchange rate could not be found.";
            result.AddError("FromCurrencyCode", "Source currency is required.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(toCurrencyCode))
        {
            result.Message = "Exchange rate could not be found.";
            result.AddError("ToCurrencyCode", "Target currency is required.");
            return result;
        }

        if (!RatesToGel.TryGetValue(fromCurrencyCode, out var fromRate))
        {
            result.Message = "Exchange rate could not be found.";
            result.AddError("FromCurrencyCode", "Source currency is not supported.");
            return result;
        }

        if (!RatesToGel.TryGetValue(toCurrencyCode, out var toRate))
        {
            result.Message = "Exchange rate could not be found.";
            result.AddError("ToCurrencyCode", "Target currency is not supported.");
            return result;
        }

        exchangeRate = fromRate / toRate;
        return result;
    }

    public ServiceResult Convert(
        decimal amount,
        string fromCurrencyCode,
        string toCurrencyCode,
        out decimal convertedAmount)
    {
        var result = new ServiceResult
        {
            Message = "Money converted successfully."
        };
        convertedAmount = 0;

        if (amount <= 0)
        {
            result.Message = "Money could not be converted.";
            result.AddError("Amount", "Amount must be greater than zero.");
            return result;
        }

        var rateResult = GetExchangeRate(fromCurrencyCode, toCurrencyCode, out var exchangeRate);
        if (!rateResult.IsSuccess)
        {
            result.Message = "Money could not be converted.";
            result.AddErrors(rateResult.Errors);
            return result;
        }

        convertedAmount = decimal.Round(amount * exchangeRate, 2, MidpointRounding.AwayFromZero);
        return result;
    }
}
