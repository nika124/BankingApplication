using BankingApplication.Enums;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using BankingApplication.Models.DTOs.AccountBalance;
using BankingApplication.Models.DTOs.Currency;
using BankingApplication.Models.DTOs.Transactions;
using BankingApplication.Models.Requests.AccountBalance;
using BankingApplication.Models.Results;
using BankingApplication.Models.Results.AccountBalance;
using Microsoft.Extensions.Logging;

namespace BankingApplication.Services;

public class AccountBalanceService : IAccountBalanceService
{
    private const string AccountIdKey = "AccountId";
    private const string SystemErrorMessage = "An unexpected error occurred. Please try again later.";

    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceRepository _balanceRepository;
    private readonly ITransactionService _transactionService;
    private readonly ICurrencyService _currencyService;
    private readonly IAuthenticationSessionStore _sessionStore;
    private readonly ILogger<AccountBalanceService> _logger;

    public AccountBalanceService(
        IAccountRepository accountRepository,
        IAccountBalanceRepository balanceRepository,
        ITransactionService transactionService,
        ICurrencyService currencyService,
        IAuthenticationSessionStore sessionStore,
        ILogger<AccountBalanceService> logger)
    {
        _accountRepository = accountRepository;
        _balanceRepository = balanceRepository;
        _transactionService = transactionService;
        _currencyService = currencyService;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public GetBalanceResult GetBalance(GetBalanceRequest request)
    {
        var result = new GetBalanceResult
        {
            Message = "Balances loaded successfully."
        };

        if (!ValidateSessionRequest(request?.SessionId ?? Guid.Empty, result))
        {
            return result;
        }

        try
        {
            if (!TryUseAccountSession(request!.SessionId, result, out var accountId))
            {
                return result;
            }

            var balances = _balanceRepository.GetByAccountId(accountId);
            if (balances.Count == 0)
            {
                AddError(result, "Account", "No balances were found for this account.");
                return result;
            }

            result.AccountBalances = new AccountBalancesDto
            {
                AccountId = accountId,
                Balances = balances.Select(MapBalance).ToList()
            };
        }
        catch (Exception exception)
        {
            AddSystemError(result, exception, nameof(GetBalance));
        }

        return result;
    }

    public DepositResult DepositMoney(DepositRequest request)
    {
        var result = new DepositResult
        {
            Message = "Money deposited successfully."
        };

        if (!ValidateMoneyRequest(request?.SessionId ?? Guid.Empty, request?.Amount ?? 0,
                request?.CurrencyCode, result))
        {
            return result;
        }

        try
        {
            if (!TryUseAccountSession(request!.SessionId, result, out var accountId))
            {
                return result;
            }

            var currencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            var balance = _balanceRepository.Get(accountId, currencyCode);
            if (balance is null)
            {
                AddError(result, "CurrencyCode", "This account does not have the requested currency.");
                return result;
            }

            balance.Balance += request.Amount;
            balance.UpdatedAt = DateTime.UtcNow;

            if (!_balanceRepository.Update(balance))
            {
                AddError(result, "Balance", "Balance could not be updated.");
                return result;
            }

            var transactionResult = _transactionService.CreateTransaction(
                accountId,
                request.Amount,
                currencyCode,
                "Deposit",
                balance.Balance,
                "ATM cash deposit",
                out var transaction);

            if (!transactionResult.IsSuccess || transaction is null)
            {
                result.AddErrors(transactionResult.Errors);
                result.Message = "Money was deposited, but the transaction record could not be created.";
                return result;
            }

            result.UpdatedBalance = MapBalance(balance);
            result.Transaction = transaction;
        }
        catch (Exception exception)
        {
            AddSystemError(result, exception, nameof(DepositMoney));
        }

        return result;
    }

    public WithdrawalResult WithdrawMoney(WithdrawalRequest request)
    {
        var result = new WithdrawalResult
        {
            Message = "Money withdrawn successfully."
        };

        if (!ValidateMoneyRequest(request?.SessionId ?? Guid.Empty, request?.Amount ?? 0,
                request?.CurrencyCode, result))
        {
            return result;
        }

        try
        {
            if (!TryUseAccountSession(request!.SessionId, result, out var accountId))
            {
                return result;
            }

            var currencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            var balance = _balanceRepository.Get(accountId, currencyCode);
            if (balance is null)
            {
                AddError(result, "CurrencyCode", "This account does not have the requested currency.");
                return result;
            }

            if (balance.Balance < request.Amount)
            {
                AddError(result, "Balance", "Insufficient balance.");
                return result;
            }

            balance.Balance -= request.Amount;
            balance.UpdatedAt = DateTime.UtcNow;

            if (!_balanceRepository.Update(balance))
            {
                AddError(result, "Balance", "Balance could not be updated.");
                return result;
            }

            var transactionResult = _transactionService.CreateTransaction(
                accountId,
                -request.Amount,
                currencyCode,
                "Withdrawal",
                balance.Balance,
                "ATM cash withdrawal",
                out var transaction);

            if (!transactionResult.IsSuccess || transaction is null)
            {
                result.AddErrors(transactionResult.Errors);
                result.Message = "Money was withdrawn, but the transaction record could not be created.";
                return result;
            }

            result.UpdatedBalance = MapBalance(balance);
            result.Transaction = transaction;
        }
        catch (Exception exception)
        {
            AddSystemError(result, exception, nameof(WithdrawMoney));
        }

        return result;
    }

    public ConvertMoneyResult ConvertMoney(ConvertMoneyRequest request)
    {
        var result = new ConvertMoneyResult
        {
            Message = "Money converted successfully."
        };

        if (!ValidateConversionRequest(request, result))
        {
            return result;
        }

        try
        {
            if (!TryUseAccountSession(request!.SessionId, result, out var accountId))
            {
                return result;
            }

            var fromCurrencyCode = request.FromCurrencyCode.Trim().ToUpperInvariant();
            var toCurrencyCode = request.ToCurrencyCode.Trim().ToUpperInvariant();
            var sourceBalance = _balanceRepository.Get(accountId, fromCurrencyCode);
            var targetBalance = _balanceRepository.Get(accountId, toCurrencyCode);

            if (sourceBalance is null)
            {
                AddError(result, "FromCurrencyCode", "Source currency balance was not found.");
                return result;
            }

            if (targetBalance is null)
            {
                AddError(result, "ToCurrencyCode", "Target currency balance was not found.");
                return result;
            }

            if (sourceBalance.Balance < request.Amount)
            {
                AddError(result, "Balance", "Insufficient balance.");
                return result;
            }

            var rateResult = _currencyService.GetExchangeRate(
                fromCurrencyCode,
                toCurrencyCode,
                out var exchangeRate);
            if (!rateResult.IsSuccess)
            {
                result.Message = "Money could not be converted.";
                result.AddErrors(rateResult.Errors);
                return result;
            }

            var conversionResult = _currencyService.Convert(
                request.Amount,
                fromCurrencyCode,
                toCurrencyCode,
                out var convertedAmount);

            if (!conversionResult.IsSuccess)
            {
                result.Message = "Money could not be converted.";
                result.AddErrors(conversionResult.Errors);
                return result;
            }

            sourceBalance.Balance -= request.Amount;
            sourceBalance.UpdatedAt = DateTime.UtcNow;
            targetBalance.Balance += convertedAmount;
            targetBalance.UpdatedAt = DateTime.UtcNow;

            if (!_balanceRepository.Update(sourceBalance) || !_balanceRepository.Update(targetBalance))
            {
                AddError(result, "Balance", "Currency balances could not be updated.");
                return result;
            }

            var sourceTransaction = _transactionService.CreateTransaction(
                accountId,
                -request.Amount,
                fromCurrencyCode,
                "CurrencyConversion",
                sourceBalance.Balance,
                $"Converted to {toCurrencyCode}",
                out var sourceTransactionDto);
            var targetTransaction = _transactionService.CreateTransaction(
                accountId,
                convertedAmount,
                toCurrencyCode,
                "CurrencyConversion",
                targetBalance.Balance,
                $"Converted from {fromCurrencyCode}",
                out var targetTransactionDto);

            if (!sourceTransaction.IsSuccess ||
                !targetTransaction.IsSuccess ||
                sourceTransactionDto is null ||
                targetTransactionDto is null)
            {
                result.Message = "Money was converted, but a transaction record could not be created.";
                result.AddErrors(sourceTransaction.Errors);
                result.AddErrors(targetTransaction.Errors);
                return result;
            }

            result.Conversion = new CurrencyConversionDto
            {
                AccountId = accountId,
                FromCurrencyCode = fromCurrencyCode,
                ToCurrencyCode = toCurrencyCode,
                OriginalAmount = request.Amount,
                ConvertedAmount = convertedAmount,
                ExchangeRate = exchangeRate
            };
            result.SourceBalance = MapBalance(sourceBalance);
            result.TargetBalance = MapBalance(targetBalance);
            result.Transactions = new List<TransactionDto>
            {
                sourceTransactionDto,
                targetTransactionDto
            };
        }
        catch (Exception exception)
        {
            AddSystemError(result, exception, nameof(ConvertMoney));
        }

        return result;
    }

    private SessionStorage? UseAtmSession(Guid sessionId, ServiceResult result)
    {
        var sessionResult = _sessionStore.UseActiveSession(
            sessionId,
            SessionType.Atm,
            out var storage);

        if (!sessionResult.IsSuccess || storage is null)
        {
            result.Message = "ATM operation could not be completed.";
            result.AddErrors(sessionResult.Errors);
            return null;
        }

        return storage;
    }

    private bool TryUseAccountSession(Guid sessionId, ServiceResult result, out int accountId)
    {
        var storage = UseAtmSession(sessionId, result);
        if (storage is null || !TryGetAccountId(storage, result, out accountId))
        {
            accountId = 0;
            return false;
        }

        return ValidateAccount(accountId, result);
    }

    private static bool TryGetAccountId(SessionStorage storage, ServiceResult result, out int accountId)
    {
        if (storage.Values.TryGetValue(AccountIdKey, out var accountIdValue) &&
            int.TryParse(accountIdValue, out accountId))
        {
            return true;
        }

        accountId = 0;
        result.Message = "ATM operation could not be completed.";
        result.AddError("SessionId", "Account data was not found in the session.");
        return false;
    }

    private bool ValidateAccount(int accountId, ServiceResult result)
    {
        var account = _accountRepository.GetByAccountId(accountId);
        if (account is null)
        {
            result.Message = "ATM operation could not be completed.";
            result.AddError("Account", "Account was not found.");
            return false;
        }

        if (!string.Equals(account.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            result.Message = "ATM operation could not be completed.";
            result.AddError("Account", "Account is not active.");
            return false;
        }

        return true;
    }

    private static bool ValidateSessionRequest(Guid sessionId, ServiceResult result)
    {
        if (sessionId != Guid.Empty)
        {
            return true;
        }

        result.Message = "ATM operation could not be completed.";
        result.AddError("SessionId", "Session ID is required.");
        return false;
    }

    private static bool ValidateMoneyRequest(
        Guid sessionId,
        decimal amount,
        string? currencyCode,
        ServiceResult result)
    {
        if (!ValidateSessionRequest(sessionId, result))
        {
            return false;
        }

        if (amount <= 0)
        {
            result.Message = "ATM operation could not be completed.";
            result.AddError("Amount", "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            result.Message = "ATM operation could not be completed.";
            result.AddError("CurrencyCode", "Currency code is required.");
        }

        return result.IsSuccess;
    }

    private static bool ValidateConversionRequest(ConvertMoneyRequest? request, ServiceResult result)
    {
        if (request is null)
        {
            result.Message = "Money could not be converted.";
            result.AddError("Request", "Request is required.");
            return false;
        }

        if (!ValidateSessionRequest(request.SessionId, result))
        {
            return false;
        }

        if (request.Amount <= 0)
        {
            result.Message = "Money could not be converted.";
            result.AddError("Amount", "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.FromCurrencyCode))
        {
            result.Message = "Money could not be converted.";
            result.AddError("FromCurrencyCode", "Source currency is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ToCurrencyCode))
        {
            result.Message = "Money could not be converted.";
            result.AddError("ToCurrencyCode", "Target currency is required.");
        }

        if (string.Equals(
                request.FromCurrencyCode,
                request.ToCurrencyCode,
                StringComparison.OrdinalIgnoreCase))
        {
            result.Message = "Money could not be converted.";
            result.AddError("ToCurrencyCode", "Source and target currencies must be different.");
        }

        return result.IsSuccess;
    }

    private static AccountBalanceDto MapBalance(AccountBalance balance)
    {
        return new AccountBalanceDto
        {
            AccountId = balance.AccountId,
            CurrencyCode = balance.CurrencyCode,
            Balance = balance.Balance
        };
    }

    private static void AddError(ServiceResult result, string key, string message)
    {
        result.Message = "ATM operation could not be completed.";
        result.AddError(key, message);
    }

    private void AddSystemError(ServiceResult result, Exception exception, string operation)
    {
        _logger.LogError(exception, "Unexpected error during {Operation}", operation);
        result.Message = "ATM operation could not be completed.";
        result.AddError("System", SystemErrorMessage);
    }
}
