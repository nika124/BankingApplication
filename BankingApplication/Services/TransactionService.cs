using BankingApplication.Enums;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using BankingApplication.Models.DTOs.Transactions;
using BankingApplication.Models.Results;
using BankingApplication.Models.Results.Transactions;
using Microsoft.Extensions.Logging;

namespace BankingApplication.Services;

public class TransactionService : ITransactionService
{
    private const string AccountIdKey = "AccountId";
    private const string SystemErrorMessage = "An unexpected error occurred. Please try again later.";

    private readonly ITransactionRepository _transactionRepository;
    private readonly IAuthenticationSessionStore _sessionStore;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAuthenticationSessionStore sessionStore,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public ServiceResult CreateTransaction(
        int accountId,
        decimal amount,
        string currencyCode,
        string transactionType,
        decimal balanceAfter,
        string description,
        out TransactionDto? transactionDto)
    {
        var result = new ServiceResult
        {
            Message = "Transaction created successfully."
        };
        transactionDto = null;

        if (accountId <= 0)
        {
            result.Message = "Transaction could not be created.";
            result.AddError("AccountId", "Account ID is invalid.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            result.Message = "Transaction could not be created.";
            result.AddError("CurrencyCode", "Currency code is required.");
            return result;
        }

        try
        {
            var transactions = _transactionRepository.GetAll();
            var transaction = new Transaction
            {
                TransactionId = transactions.Count == 0
                    ? 1
                    : transactions.Max(item => item.TransactionId) + 1,
                AccountId = accountId,
                Amount = amount,
                CurrencyCode = currencyCode.ToUpperInvariant(),
                TransactionType = transactionType,
                BalanceAfter = balanceAfter,
                Status = "Completed",
                TransactionDate = DateTime.UtcNow,
                Description = description
            };

            _transactionRepository.Add(transaction);
            transactionDto = Map(transaction);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating transaction");
            result.Message = "Transaction could not be created.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    public ServiceResult GetTransactionsByAccountId(
        int accountId,
        out IReadOnlyList<TransactionDto> transactions)
    {
        var result = new ServiceResult
        {
            Message = "Transactions loaded successfully."
        };
        transactions = [];

        if (accountId <= 0)
        {
            result.Message = "Transactions could not be loaded.";
            result.AddError("AccountId", "Account ID is invalid.");
            return result;
        }

        try
        {
            transactions = _transactionRepository
                .GetByAccountId(accountId)
                .OrderByDescending(transaction => transaction.TransactionDate)
                .Select(Map)
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while loading transactions");
            result.Message = "Transactions could not be loaded.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    public LastTransactionsResult GetLastTransactions(Guid sessionId)
    {
        var result = new LastTransactionsResult
        {
            Message = "Last transactions loaded successfully."
        };

        if (sessionId == Guid.Empty)
        {
            result.Message = "Transactions could not be loaded.";
            result.AddError("SessionId", "Session ID is required.");
            return result;
        }

        var sessionResult = _sessionStore.UseActiveSession(
            sessionId,
            SessionType.Atm,
            out var storage);

        if (!sessionResult.IsSuccess || storage is null)
        {
            result.Message = "Transactions could not be loaded.";
            result.AddErrors(sessionResult.Errors);
            return result;
        }

        if (!TryGetAccountId(storage, out var accountId))
        {
            result.Message = "Transactions could not be loaded.";
            result.AddError("SessionId", "Account data was not found in the session.");
            return result;
        }

        try
        {
            result.Transactions = _transactionRepository
                .GetByAccountId(accountId)
                .OrderByDescending(transaction => transaction.TransactionDate)
                .Take(5)
                .Select(Map)
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while loading last transactions");
            result.Message = "Transactions could not be loaded.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    private static bool TryGetAccountId(SessionStorage storage, out int accountId)
    {
        if (!storage.Values.TryGetValue(AccountIdKey, out var accountIdValue))
        {
            accountId = 0;
            return false;
        }

        return int.TryParse(accountIdValue, out accountId);
    }

    private static TransactionDto Map(Transaction transaction)
    {
        return new TransactionDto
        {
            TransactionId = transaction.TransactionId,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            CurrencyCode = transaction.CurrencyCode,
            TransactionType = transaction.TransactionType,
            BalanceAfter = transaction.BalanceAfter,
            CreatedAt = transaction.TransactionDate,
            Description = transaction.Description
        };
    }
}
