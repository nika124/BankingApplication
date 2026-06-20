using BankingApplication.Interfaces.Repositories;
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
    private const string StartFailureMessage = "Authentication could not be started.";
    private const string CompleteFailureMessage = "Authentication failed.";
    private const string SystemErrorMessage = "An unexpected error occurred. Please try again later.";

    private readonly ICardRepository _cardRepository;
    private readonly IPinService _pinService;
    private readonly IAuthenticationSessionStore _sessionStore;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        ICardRepository cardRepository,
        IPinService pinService,
        IAuthenticationSessionStore sessionStore,
        ILogger<AuthenticationService> logger)
    {
        _cardRepository = cardRepository;
        _pinService = pinService;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public StartAuthenticationResult StartAtmAuthentication(StartAuthenticationRequest request)
    {
        var result = new StartAuthenticationResult { Message = "Card verified. Please enter PIN." };
        LogStartRequest(request);

        try
        {
            if (!ValidateStartAuthenticationRequest(request, result))
            {
                return LogResponse(nameof(StartAtmAuthentication), result);
            }

            var card = _cardRepository.GetByCardNumber(request.CardNumber);
            if (!ValidateCard(card, result))
            {
                return LogResponse(nameof(StartAtmAuthentication), result);   
            }

            CreatePendingAuthentication(card!, result);
        }
        catch (Exception exception)
        {
            AddSystemError(result, StartFailureMessage, exception, nameof(StartAtmAuthentication));
        }

        return LogResponse(nameof(StartAtmAuthentication), result);
    }

    public CompleteAuthenticationResult CompleteAuthentication(CompleteAuthenticationRequest request)
    {
        var result = new CompleteAuthenticationResult { Message = "Authentication successful." };
        LogCompleteRequest(request);

        try
        {
            if (!ValidateCompleteAuthenticationRequest(request, result))
            {
                return LogResponse(nameof(CompleteAuthentication), result);
            }

            var pending = GetPendingAuthentication(request!.AuthenticationId, result);
            if (pending is null)
            {
                return LogResponse(nameof(CompleteAuthentication), result);
            }

            if (!_pinService.ValidatePin(pending.CardId, request.Pin))
            {
                return LogResponse(nameof(CompleteAuthentication), AddError(result, "Pin", "Incorrect PIN."));
            }

            if (!AuthorizeSession(request.AuthenticationId, pending, result))
            {
                return LogResponse(nameof(CompleteAuthentication), result);
            }
        }
        catch (Exception exception)
        {
            AddSystemError(result, CompleteFailureMessage, exception, nameof(CompleteAuthentication));
        }

        return LogResponse(nameof(CompleteAuthentication), result);
    }

    private bool ValidateStartAuthenticationRequest(StartAuthenticationRequest? request, StartAuthenticationResult result)
    {
        if (request is null)
        {
            AddError(result, "Request", "Request is required.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.CardNumber))
        {
            return true;
        }

        AddError(result, "CardNumber", "Card number is required.");
        return false;
    }

    private bool ValidateCard(Card? card, StartAuthenticationResult result)
    {
        if (card is null)
        {
            AddError(result, "Card", "Card was not found.");
            return false;
        }

        if (string.Equals(card.Status, "Active", StringComparison.OrdinalIgnoreCase))
            return true;

        AddError(result, "Card", "This card is not active.");
        return false;
    }

    private bool ValidateCompleteAuthenticationRequest(CompleteAuthenticationRequest? request, CompleteAuthenticationResult result)
    {
        if (request is null)
        {
            AddError(result, "Request", "Request is required.");
            return false;
        }

        if (request.AuthenticationId == Guid.Empty)
        {
            AddError(result, "AuthenticationId", "Authentication ID is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Pin))
        {
            AddError(result, "Pin", "PIN is required.");
            return false;
        }

        return true;
    }

    private PendingAtmAuthentication? GetPendingAuthentication(
        Guid authenticationId,
        CompleteAuthenticationResult result)
    {
        var pending = _sessionStore.GetPendingAuthentication(authenticationId);
        if (pending != null)
        {
            return pending;
        }

        AddError(result, "Authentication", "Pending authentication was not found or expired.");
        return null;
    }

    private void CreatePendingAuthentication(Card card, StartAuthenticationResult result)
    {
        var authenticationId = _sessionStore.CreatePendingAuthentication(card.CardId, card.AccountId);
        var maskedCardNumber = MaskCardNumber(card.CardNumber);

        result.PendingAuthentication = new PendingAuthenticationDto
        {
            AuthenticationId = authenticationId,
            CardId = card.CardId,
            AccountId = card.AccountId,
            MaskedCardNumber = maskedCardNumber
        };
    }

    private bool AuthorizeSession(
        Guid authenticationId,
        PendingAtmAuthentication pending,
        CompleteAuthenticationResult result)
    {
        if (!_sessionStore.RemovePendingAuthentication(authenticationId))
        {
            AddError(result, "Authentication", "Pending authentication was not found or expired.");
            return false;
        }

        var sessionId = _sessionStore.CreateAuthorizedSession(pending.CardId, pending.AccountId);
        var card = _cardRepository.GetAll().FirstOrDefault(card => card.CardId == pending.CardId);

        result.AuthorizedSession = new AuthorizedSessionDto
        {
            SessionId = sessionId,
            CardId = pending.CardId,
            AccountId = pending.AccountId,
            MaskedCardNumber = card is null ? null : MaskCardNumber(card.CardNumber)
        };

        return true;
    }

    private void LogStartRequest(StartAuthenticationRequest? request)
    {
        _logger.LogInformation("Authentication request received: {Operation}; Card: {MaskedCardNumber}",
            nameof(StartAtmAuthentication), MaskCardNumber(request?.CardNumber));
    }

    private void LogCompleteRequest(CompleteAuthenticationRequest? request)
    {
        _logger.LogInformation(
            "Authentication request received: {Operation}; AuthenticationId: {AuthenticationId}; PIN supplied: {HasPin}",
            nameof(CompleteAuthentication),
            request?.AuthenticationId,
            !string.IsNullOrWhiteSpace(request?.Pin));
    }

    private T LogResponse<T>(string operation, T result) where T : ServiceResult
    {
        _logger.LogInformation(
            "Authentication response: {Operation}; Success: {IsSuccess}; Message: {Message}; ErrorKeys: {ErrorKeys}; Timestamp: {Timestamp}",
            operation, result.IsSuccess, result.Message, result.Errors.Keys, DateTime.UtcNow);
        return result;
    }

    private void AddSystemError(ServiceResult result, string failureMessage, Exception exception, string operation)
    {
        _logger.LogError(exception, "Unexpected error during {Operation}", operation);
        result.Message = failureMessage;
        result.AddError("System", SystemErrorMessage);
    }

    private static T AddError<T>(T result, string key, string value) where T : ServiceResult
    {
        result.Message = result is StartAuthenticationResult ? StartFailureMessage : CompleteFailureMessage;
        result.AddError(key, value);
        return result;
    }

    private static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return "Not provided";
        }

        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        var lastFour = digits.Length <= 4 ? digits : digits[^4..];
        return $"**** **** **** {lastFour.PadLeft(4, '*')}";
    }
}
