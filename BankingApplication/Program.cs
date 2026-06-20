using BankingApplication.Infrastructure;
using BankingApplication.Models.Requests;
using BankingApplication.Repositories;
using BankingApplication.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

#region Dependency Setup
//Serilog
var projectDirectory = FindProjectDirectory();
var logsDirectory = Path.Combine(projectDirectory, "Logs");
var errorLogsDirectory = Path.Combine(logsDirectory, "errors");

Directory.CreateDirectory(errorLogsDirectory);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logsDirectory, "application-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.Logger(loggerConfiguration =>
        loggerConfiguration
            .Filter.ByIncludingOnly(logEvent =>
                logEvent.Level >= LogEventLevel.Error)
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

    var storageLogger = loggerFactory.CreateLogger<JsonStorageProvider>();
    var storageProvider = new JsonStorageProvider(storageLogger);
    var cardRepository = new CardRepository(storageProvider);
    var cardPinRepository = new CardPinRepository(storageProvider);
    var cardPinAttemptRepository = new CardPinAttemptRepository(storageProvider);
    var pinService = new PinService(cardRepository, cardPinRepository, cardPinAttemptRepository);
    var authenticationSessionStore = new AuthenticationSessionStore();
    var authenticationLogger = loggerFactory.CreateLogger<AuthenticationService>();
    var authenticationService = new AuthenticationService(
        cardRepository,
        pinService,
        authenticationSessionStore,
        authenticationLogger);

    Console.WriteLine("ATM AUTHENTICATION TEST");

    Console.Write("Enter card number: ");
    var cardNumber = Console.ReadLine() ?? string.Empty;

    var startResult = authenticationService.StartAtmAuthentication(
        new StartAuthenticationRequest { CardNumber = cardNumber });

    if (!startResult.IsSuccess)
    {
        Console.WriteLine(startResult.Message);
        foreach (var error in startResult.Errors.Values)
            Console.WriteLine(error);
        return;
    }

    Console.WriteLine(startResult.Message);
    Console.WriteLine($"Card: {startResult.PendingAuthentication!.MaskedCardNumber}");

    Console.Write("Enter PIN: ");
    var pin = Console.ReadLine() ?? string.Empty;

    var completeResult = authenticationService.CompleteAuthentication(
        new CompleteAuthenticationRequest
        {
            AuthenticationId = startResult.PendingAuthentication.AuthenticationId,
            Pin = pin
        });

    if (!completeResult.IsSuccess)
    {
        Console.WriteLine(completeResult.Message);
        foreach (var error in completeResult.Errors.Values)
            Console.WriteLine(error);
        return;
    }

    Console.WriteLine(completeResult.Message);
    Console.WriteLine($"Session ID: {completeResult.AuthorizedSession!.SessionId}");
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

static string FindProjectDirectory()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "BankingApplication.csproj")))
            return directory.FullName;

        directory = directory.Parent;
    }

    return Directory.GetCurrentDirectory();
}
