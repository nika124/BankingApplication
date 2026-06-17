using BankingApplication.Infrastructure;
using BankingApplication.Repositories;
using BankingApplication.Services;

#region Dependency Setup
var logger = new FileLogger(
    Path.Combine("Logs", "errors.txt"));
var storageProvider = new JsonStorageProvider(logger);
var transactionRepository = new TransactionRepository(storageProvider);
var transactionService = new TransactionService(logger, transactionRepository);
#endregion

Console.WriteLine("DEPOSIT TRANSACTION TEST");

try
{
    var result = transactionService.DepositMoney(
        iban: "GE00BANK000000000001",
        amount: 100m,
        currencyCode: "GEL",
        transactionType: "Deposit");

    Console.WriteLine(
        result
            ? "Deposit transaction added successfully."
            : "Deposit transaction failed.");
}
catch (Exception exception)
{
    Console.WriteLine($"Error: {exception.Message}");
}

Console.WriteLine("\nTEST FINISHED");
Console.ReadLine();