using BankingApplication.Infrastructure;
using BankingApplication.Repositories;
using BankingApplication.Services;

#region Dependency Setup
var logger = new FileLogger(Path.Combine("Logs", "errors.txt"));
var storageProvider = new JsonStorageProvider(logger);
var cardRepository = new CardRepository(storageProvider);
var cardPinRepository = new CardPinRepository(storageProvider);
var cardPinAttemptRepository = new CardPinAttemptRepository(storageProvider);
var pinService = new PinService(cardRepository, cardPinRepository, cardPinAttemptRepository);
var authenticationSessionStore = new AuthenticationSessionStore();
var authenticationService = new AuthenticationService(
    cardRepository,
    pinService,
    authenticationSessionStore);
#endregion

Console.WriteLine("ATM AUTHENTICATION TEST");

try
{
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
}
catch (Exception exception)
{
    Console.WriteLine($"Error: {exception.Message}");
}

Console.WriteLine("\nTEST FINISHED");
Console.ReadLine();