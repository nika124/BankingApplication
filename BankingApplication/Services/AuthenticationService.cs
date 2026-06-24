using BankingApplication.Enums;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using BankingApplication.Models.DTOs.Authentication;
using BankingApplication.Models.Requests;
using BankingApplication.Models.Results;
using BankingApplication.Models.Results.Authentication;
using Microsoft.Extensions.Logging;

namespace BankingApplication.Services;

public class AuthenticationService : IAuthenticationService
{
    private const string CardIdKey = "CardId";
    private const string AccountIdKey = "AccountId";
    private const string SystemErrorMessage = "An unexpected error occurred. Please try again later.";
    private static readonly TimeSpan PendingSessionLifetime = TimeSpan.FromMinutes(2);

    private readonly ICardService _cardService;
    private readonly IPinService _pinService;
    private readonly IAuthenticationSessionStore _sessionStore;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        ICardService cardService,
        IPinService pinService,
        IAuthenticationSessionStore sessionStore,
        ILogger<AuthenticationService> logger)
    {
        _cardService = cardService;
        _pinService = pinService;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public StartAuthenticationResult StartAtmAuthentication(StartAuthenticationRequest request)
    {
        var result = new StartAuthenticationResult
        {
            Message = "Card verified. Please enter PIN."
        };

        if (request is null)
        {
            AddError(result, "Request", "Request is required.");
            return result;
        }

        try
        {
            var cardResult = _cardService.GetActiveCard(request.CardNumber, out var card);
            if (!cardResult.IsSuccess || card is null)
            {
                result.Message = "Authentication could not be started.";
                result.AddErrors(cardResult.Errors);
                return result;
            }

            var sessionId = _sessionStore.CreateSession(
                SessionType.Atm,
                SessionStatus.Pending,
                PendingSessionLifetime);

            if (!_sessionStore.SetSessionValue(sessionId, CardIdKey, card.CardId.ToString()) ||
                !_sessionStore.SetSessionValue(sessionId, AccountIdKey, card.AccountId.ToString()))
            {
                AddError(result, "SessionId", "Session storage could not be created.");
                return result;
            }

            result.PendingAuthentication = new PendingAuthenticationDto
            {
                SessionId = sessionId,
                MaskedCardNumber = MaskCardNumber(card.CardNumber)
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while starting authentication");
            result.Message = "Authentication could not be started.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    public CompleteAuthenticationResult CompleteAuthentication(CompleteAuthenticationRequest request)
    {
        var result = new CompleteAuthenticationResult
        {
            Message = "Authentication successful."
        };

        if (request is null)
        {
            AddError(result, "Request", "Request is required.");
            return result;
        }

        if (request.SessionId == Guid.Empty)
        {
            AddError(result, "SessionId", "Session ID is required.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(request.Pin))
        {
            AddError(result, "Pin", "PIN is required.");
            return result;
        }

        try
        {
            var pendingSession = _sessionStore.GetPendingSession(request.SessionId, SessionType.Atm);
            if (pendingSession is null)
            {
                AddError(result, "SessionId", "Pending session was not found or expired.");
                return result;
            }

            var storage = _sessionStore.GetSessionStorage(request.SessionId);
            if (storage is null || !TryGetIntValue(storage, CardIdKey, out var cardId))
            {
                AddError(result, "SessionId", "Card data was not found in the session.");
                return result;
            }

            var pinResult = _pinService.ValidatePin(cardId, request.Pin);
            if (!pinResult.IsSuccess)
            {
                result.Message = "Authentication failed.";
                result.AddErrors(pinResult.Errors);
                return result;
            }

            if (!_sessionStore.ActivateSession(request.SessionId, SessionType.Atm))
            {
                AddError(result, "SessionId", "Pending session was not found or expired.");
                return result;
            }

            result.ActiveSession = new ActiveSessionDto
            {
                SessionId = request.SessionId
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while completing authentication");
            result.Message = "Authentication failed.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    private static bool TryGetIntValue(SessionStorage storage, string key, out int value)
    {
        if (!storage.Values.TryGetValue(key, out var storedValue))
        {
            value = 0;
            return false;
        }

        return int.TryParse(storedValue, out value);
    }

    private static void AddError(ServiceResult result, string key, string message)
    {
        result.Message = result is StartAuthenticationResult
            ? "Authentication could not be started."
            : "Authentication failed.";
        result.AddError(key, message);
    }

    private static string MaskCardNumber(string cardNumber)
    {
        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        var lastFour = digits.Length <= 4 ? digits : digits[^4..];
        return $"**** **** **** {lastFour.PadLeft(4, '*')}";
    }
}
