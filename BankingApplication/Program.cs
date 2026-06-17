using BankingApplication.Infrastructure;
using BankingApplication.Repositories;
using BankingApplication.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

#region Dependency Setup
//Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/application-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.Logger(loggerConfiguration =>
        loggerConfiguration
            .Filter.ByIncludingOnly(logEvent =>
                logEvent.Level >= LogEventLevel.Error)
            .WriteTo.File(
                path: "Logs/errors/error-.txt",
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

    var storageLogger = loggerFactory.CreateLogger<JsonStorageProvider>();
    var storageProvider = new JsonStorageProvider(storageLogger);
    var cardRepository = new CardRepository(storageProvider);
    var cardPinRepository = new CardPinRepository(storageProvider);
    var cardPinAttemptRepository = new CardPinAttemptRepository(storageProvider);
    var pinService = new PinService(cardRepository, cardPinRepository, cardPinAttemptRepository);
    var authenticationSessionStore = new AuthenticationSessionStore();
    var authenticationService = new AuthenticationService(cardRepository, pinService, authenticationSessionStore);

    Console.WriteLine("ATM AUTHENTICATION TEST");

    Console.Write("Enter card number: ");
    var cardNumber = Console.ReadLine() ?? string.Empty;

    var authenticationId = authenticationService.StartAtmAuthentication(cardNumber);

    if (authenticationId is null)
    {
        Console.WriteLine("Card was not recognized.");
        return;
    }

    Console.WriteLine("Card recognized.");

    Console.Write("Enter PIN: ");
    var pin = Console.ReadLine() ?? string.Empty;

    var sessionId = authenticationService.VerifyAtmPin(authenticationId.Value, pin);

    if (sessionId is null)
    {
        Console.WriteLine("Incorrect PIN or authentication failed.");
        return;
    }

    Console.WriteLine("Authentication successful.");
    Console.WriteLine($"Session ID: {sessionId}");
    Console.WriteLine("The session can be used for one ATM operation.");

    Console.WriteLine("\nTEST FINISHED");
    Console.ReadLine();
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
