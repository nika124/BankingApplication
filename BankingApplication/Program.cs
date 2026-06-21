using BankingApplication.Infrastructure;
using BankingApplication.Models.Requests;
using BankingApplication.Models.Requests.AccountBalance;
using BankingApplication.Models.Results;
using BankingApplication.Models.Results.AccountBalance;
using BankingApplication.Models.Results.Transactions;
using BankingApplication.Repositories;
using BankingApplication.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

#region Application path and logging configuration

// Make sure the app always works from the project directory,
var projectDirectory = FindProjectDirectory();
Directory.SetCurrentDirectory(projectDirectory);

var logsDirectory = Path.Combine(projectDirectory, "Logs");
var errorLogsDirectory = Path.Combine(logsDirectory, "errors");
Directory.CreateDirectory(errorLogsDirectory);

// Serilog configuration for general logs and separate error logs.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine(logsDirectory, "application-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.Logger(configuration =>
        configuration
            .Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Error)
            .WriteTo.File(
                path: Path.Combine(errorLogsDirectory, "error-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30))
    .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog();
});

var programLogger = loggerFactory.CreateLogger<Program>();

#endregion

try
{
    programLogger.LogInformation("Application started");

    #region Infrastructure and repositories configuration

    // Infrastructure dependency used by all JSON-based repositories.
    var storageProvider = new JsonStorageProvider(
        loggerFactory.CreateLogger<JsonStorageProvider>());

    // Repository declarations.
    var cardRepository = new CardRepository(storageProvider);
    var cardPinRepository = new CardPinRepository(storageProvider);
    var cardPinAttemptRepository = new CardPinAttemptRepository(storageProvider);
    var accountRepository = new AccountRepository(storageProvider);
    var balanceRepository = new AccountBalanceRepository(storageProvider);
    var transactionRepository = new TransactionRepository(storageProvider);

    #endregion

    #region Services configuration

    // Shared session store for pending and authorized ATM authentication.
    var sessionStore = new AuthenticationSessionStore();

    // Service declarations.
    var cardService = new CardService(
        cardRepository,
        loggerFactory.CreateLogger<CardService>());

    var pinService = new PinService(
        cardRepository,
        cardPinRepository,
        cardPinAttemptRepository,
        loggerFactory.CreateLogger<PinService>());

    var authenticationService = new AuthenticationService(
        cardService,
        pinService,
        sessionStore,
        loggerFactory.CreateLogger<AuthenticationService>());

    var currencyService = new CurrencyService();

    var transactionService = new TransactionService(
        transactionRepository,
        sessionStore,
        loggerFactory.CreateLogger<TransactionService>());

    var accountBalanceService = new AccountBalanceService(
        accountRepository,
        balanceRepository,
        transactionService,
        currencyService,
        sessionStore,
        loggerFactory.CreateLogger<AccountBalanceService>());

    #endregion

    #region ATM flow

    Console.WriteLine("Welcome to BankingApplication ATM");
    Console.WriteLine();

    Console.Write("Enter card number: ");
    var cardNumber = Console.ReadLine() ?? string.Empty;

    var startResult = authenticationService.StartAtmAuthentication(
        new StartAuthenticationRequest
        {
            CardNumber = cardNumber
        });

    if (!PrintResult(startResult))
    {
        return;
    }

    Console.WriteLine($"Card: {startResult.PendingAuthentication!.MaskedCardNumber}");
    Console.Write("Enter PIN: ");
    var pin = Console.ReadLine() ?? string.Empty;

    var completeResult = authenticationService.CompleteAuthentication(
        new CompleteAuthenticationRequest
        {
            SessionId = startResult.PendingAuthentication.SessionId,
            Pin = pin
        });

    if (!PrintResult(completeResult))
    {
        return;
    }

    var activeSessionId = completeResult.ActiveSession!.SessionId;

    ShowOperationMenu();

    var choice = Console.ReadLine();
    RunSelectedOperation(
        choice,
        activeSessionId,
        accountBalanceService,
        transactionService);

    Console.WriteLine();
    Console.WriteLine("Card ejected. Please authenticate again for another operation.");

    #endregion
}
catch (Exception exception)
{
    programLogger.LogError(exception, "Unhandled application error");
    Console.WriteLine("An unexpected error occurred. Please try again later.");
}
finally
{
    programLogger.LogInformation("Application stopped");
    Log.CloseAndFlush();
}

#region Console menu helpers

static void ShowOperationMenu()
{
    Console.WriteLine();
    Console.WriteLine("Choose one operation:");
    Console.WriteLine("1. Check balance");
    Console.WriteLine("2. Deposit money");
    Console.WriteLine("3. Withdraw money");
    Console.WriteLine("4. Convert money");
    Console.WriteLine("5. Show last 5 transactions");
    Console.WriteLine("0. Exit");
    Console.Write("> ");
}

static void RunSelectedOperation(
    string? choice,
    Guid sessionId,
    AccountBalanceService accountBalanceService,
    TransactionService transactionService)
{
    switch (choice)
    {
        case "1":
            ShowBalance(accountBalanceService.GetBalance(
                new GetBalanceRequest
                {
                    SessionId = sessionId
                }));
            break;

        case "2":
            RunDeposit(sessionId, accountBalanceService);
            break;

        case "3":
            RunWithdrawal(sessionId, accountBalanceService);
            break;

        case "4":
            RunConversion(sessionId, accountBalanceService);
            break;

        case "5":
            ShowLastTransactions(transactionService.GetLastTransactions(sessionId));
            break;

        case "0":
            Console.WriteLine("No operation selected.");
            break;

        default:
            Console.WriteLine("Invalid operation.");
            break;
    }
}

static void RunDeposit(
    Guid sessionId,
    AccountBalanceService accountBalanceService)
{
    Console.WriteLine();
    Console.WriteLine("Deposit money");

    var currencyCode = ChooseCurrencyCode("Choose currency to deposit into:");
    var amount = ReadPositiveAmount($"Enter amount in {currencyCode}: ");

    ShowDeposit(accountBalanceService.DepositMoney(
        new DepositRequest
        {
            SessionId = sessionId,
            CurrencyCode = currencyCode,
            Amount = amount
        }));
}

static void RunWithdrawal(
    Guid sessionId,
    AccountBalanceService accountBalanceService)
{
    Console.WriteLine();
    Console.WriteLine("Withdraw money");

    var currencyCode = ChooseCurrencyCode("Choose currency to withdraw from:");
    var amount = ReadPositiveAmount($"Enter amount in {currencyCode}: ");

    ShowWithdrawal(accountBalanceService.WithdrawMoney(
        new WithdrawalRequest
        {
            SessionId = sessionId,
            CurrencyCode = currencyCode,
            Amount = amount
        }));
}

static void RunConversion(
    Guid sessionId,
    AccountBalanceService accountBalanceService)
{
    Console.WriteLine();
    Console.WriteLine("Convert money");

    var fromCurrencyCode = ChooseCurrencyCode("Choose source currency:");
    var toCurrencyCode = ChooseCurrencyCode(
        $"Choose target currency for {fromCurrencyCode}:",
        excludedCurrencyCode: fromCurrencyCode);

    var amount = ReadPositiveAmount($"Enter amount to convert from {fromCurrencyCode}: ");

    ShowConversion(accountBalanceService.ConvertMoney(
        new ConvertMoneyRequest
        {
            SessionId = sessionId,
            FromCurrencyCode = fromCurrencyCode,
            ToCurrencyCode = toCurrencyCode,
            Amount = amount
        }));
}

static decimal ReadPositiveAmount(string prompt)
{
    while (true)
    {
        Console.Write(prompt);

        if (decimal.TryParse(Console.ReadLine(), out var amount) && amount > 0)
        {
            return amount;
        }

        Console.WriteLine("Amount must be a number greater than zero.");
    }
}

static string ChooseCurrencyCode(
    string title,
    string? excludedCurrencyCode = null)
{
    var currencyCodes = GetSupportedCurrencyCodes()
        .Where(currencyCode =>
            !string.Equals(currencyCode, excludedCurrencyCode, StringComparison.OrdinalIgnoreCase))
        .ToArray();

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine(title);

        for (var index = 0; index < currencyCodes.Length; index++)
        {
            Console.WriteLine($"{index + 1}. {currencyCodes[index]}");
        }

        Console.Write("> ");

        if (int.TryParse(Console.ReadLine(), out var selectedNumber) &&
            selectedNumber >= 1 &&
            selectedNumber <= currencyCodes.Length)
        {
            return currencyCodes[selectedNumber - 1];
        }

        Console.WriteLine("Please choose a valid currency number.");
    }
}

static string[] GetSupportedCurrencyCodes()
{
    return ["GEL", "USD", "EUR"];
}

#endregion

#region Result printing helpers

static bool PrintResult(ServiceResult result)
{
    Console.WriteLine(result.Message);

    if (result.IsSuccess)
    {
        return true;
    }

    foreach (var error in result.Errors.Values)
    {
        Console.WriteLine($"- {error}");
    }

    return false;
}

static void ShowBalance(GetBalanceResult result)
{
    if (!PrintResult(result))
    {
        return;
    }

    foreach (var balance in result.AccountBalances!.Balances)
    {
        Console.WriteLine($"{balance.CurrencyCode}: {balance.Balance:F2}");
    }
}

static void ShowDeposit(DepositResult result)
{
    if (!PrintResult(result))
    {
        return;
    }

    var balance = result.UpdatedBalance!;
    Console.WriteLine($"New {balance.CurrencyCode} balance: {balance.Balance:F2}");
}

static void ShowWithdrawal(WithdrawalResult result)
{
    if (!PrintResult(result))
    {
        return;
    }

    var balance = result.UpdatedBalance!;
    Console.WriteLine($"New {balance.CurrencyCode} balance: {balance.Balance:F2}");
}

static void ShowConversion(ConvertMoneyResult result)
{
    if (!PrintResult(result))
    {
        return;
    }

    var conversion = result.Conversion!;

    Console.WriteLine(
        $"{conversion.OriginalAmount:F2} {conversion.FromCurrencyCode} = " +
        $"{conversion.ConvertedAmount:F2} {conversion.ToCurrencyCode}");
}

static void ShowLastTransactions(LastTransactionsResult result)
{
    if (!PrintResult(result))
    {
        return;
    }

    if (result.Transactions.Count == 0)
    {
        Console.WriteLine("No transactions found.");
        return;
    }

    foreach (var transaction in result.Transactions)
    {
        Console.WriteLine(
            $"{transaction.CreatedAt:yyyy-MM-dd HH:mm} | " +
            $"{transaction.TransactionType} | " +
            $"{transaction.Amount:F2} {transaction.CurrencyCode} | " +
            $"Balance: {transaction.BalanceAfter:F2}");
    }
}

#endregion

#region Application path helper

static string FindProjectDirectory()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "BankingApplication.csproj")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return Directory.GetCurrentDirectory();
}

#endregion
