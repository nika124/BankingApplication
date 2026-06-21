using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using BankingApplication.Models.Results;
using Microsoft.Extensions.Logging;

namespace BankingApplication.Services;

public class PinService : IPinService
{
    private const string SystemErrorMessage = "An unexpected error occurred. Please try again later.";

    private readonly ICardRepository _cardRepository;
    private readonly ICardPinRepository _cardPinRepository;
    private readonly ICardPinAttemptRepository _cardPinAttemptRepository;
    private readonly ILogger<PinService> _logger;

    public PinService(
        ICardRepository cardRepository,
        ICardPinRepository cardPinRepository,
        ICardPinAttemptRepository cardPinAttemptRepository,
        ILogger<PinService> logger)
    {
        _cardRepository = cardRepository;
        _cardPinRepository = cardPinRepository;
        _cardPinAttemptRepository = cardPinAttemptRepository;
        _logger = logger;
    }

    public ServiceResult ValidatePin(int cardId, string pin)
    {
        var result = new ServiceResult
        {
            Message = "PIN is correct."
        };

        if (string.IsNullOrWhiteSpace(pin))
        {
            result.Message = "PIN validation failed.";
            result.AddError("Pin", "PIN is required.");
            return result;
        }

        try
        {
            var card = _cardRepository.GetAll().FirstOrDefault(item => item.CardId == cardId);
            if (card is null)
            {
                result.Message = "PIN validation failed.";
                result.AddError("Card", "Card was not found.");
                return result;
            }

            if (string.Equals(card.Status, "Blocked", StringComparison.OrdinalIgnoreCase) &&
                card.BlockedUntil > DateTime.UtcNow)
            {
                result.Message = "PIN validation failed.";
                result.AddError("Card", "Card is temporarily blocked.");
                return result;
            }

            var cardPin = _cardPinRepository.GetCardPin(cardId);
            if (cardPin is null)
            {
                result.Message = "PIN validation failed.";
                result.AddError("Pin", "PIN information was not found.");
                return result;
            }

            var isCorrect = cardPin.Pin == pin;
            AddPinAttempt(cardId, isCorrect);

            if (!isCorrect)
            {
                BlockCardIfNecessary(card);
                result.Message = "PIN validation failed.";
                result.AddError("Pin", "PIN is incorrect.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while validating PIN");
            result.Message = "PIN validation failed.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    public ServiceResult ChangePin(int cardId, string newPin)
    {
        var result = new ServiceResult
        {
            Message = "PIN changed successfully."
        };

        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4 || !newPin.All(char.IsDigit))
        {
            result.Message = "PIN could not be changed.";
            result.AddError("Pin", "New PIN must contain exactly four digits.");
            return result;
        }

        try
        {
            var cardPin = _cardPinRepository.GetCardPin(cardId);
            if (cardPin is null)
            {
                result.Message = "PIN could not be changed.";
                result.AddError("Card", "PIN information was not found.");
                return result;
            }

            cardPin.Pin = newPin;
            cardPin.LastChangedAt = DateTime.UtcNow;

            if (!_cardPinRepository.Update(cardPin))
            {
                result.Message = "PIN could not be changed.";
                result.AddError("Pin", "Stored PIN could not be updated.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while changing PIN");
            result.Message = "PIN could not be changed.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    private void AddPinAttempt(int cardId, bool wasSuccessful)
    {
        var attempts = _cardPinAttemptRepository.GetAll();
        var nextAttemptId = attempts.Count == 0
            ? 1
            : attempts.Max(attempt => attempt.AttemptId) + 1;

        _cardPinAttemptRepository.Add(new CardPinAttempt
        {
            AttemptId = nextAttemptId,
            CardId = cardId,
            WasSuccessful = wasSuccessful,
            AttemptedAt = DateTime.UtcNow
        });
    }

    private void BlockCardIfNecessary(Card card)
    {
        var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
        var recentAttempts = _cardPinAttemptRepository
            .GetAll()
            .Where(attempt => attempt.CardId == card.CardId && attempt.AttemptedAt >= tenMinutesAgo)
            .OrderByDescending(attempt => attempt.AttemptedAt)
            .ToList();

        var consecutiveFailedAttempts = recentAttempts.TakeWhile(attempt => !attempt.WasSuccessful).Count();
        if (consecutiveFailedAttempts < 3)
        {
            return;
        }

        card.Status = "Blocked";
        card.BlockedUntil = DateTime.UtcNow.AddHours(1);
        card.BlockReason = "Three consecutive incorrect PIN attempts.";
        _cardRepository.Update(card);
    }
}
