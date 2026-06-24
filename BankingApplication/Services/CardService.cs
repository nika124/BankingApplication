using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using BankingApplication.Models.Results;
using Microsoft.Extensions.Logging;

namespace BankingApplication.Services;

public class CardService : ICardService
{
    private const string SystemErrorMessage = "An unexpected error occurred. Please try again later.";

    private readonly ICardRepository _cardRepository;
    private readonly ILogger<CardService> _logger;

    public CardService(ICardRepository cardRepository, ILogger<CardService> logger)
    {
        _cardRepository = cardRepository;
        _logger = logger;
    }

    public ServiceResult GetActiveCard(string cardNumber, out Card? card)
    {
        var result = new ServiceResult
        {
            Message = "Card is active."
        };
        card = null;

        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            result.Message = "Card validation failed.";
            result.AddError("CardNumber", "Card number is required.");
            return result;
        }

        try
        {
            card = _cardRepository.GetByCardNumber(cardNumber);
            if (card is null)
            {
                result.Message = "Card validation failed.";
                result.AddError("CardNumber", "Card was not found.");
                return result;
            }

            if (!string.Equals(card.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                result.Message = "Card validation failed.";
                result.AddError("Card", "This card is not active.");
                return result;
            }

            if (IsExpired(card))
            {
                result.Message = "Card validation failed.";
                result.AddError("Card", "This card has expired.");
                return result;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while validating card");
            result.Message = "Card validation failed.";
            result.AddError("System", SystemErrorMessage);
        }

        return result;
    }

    private static bool IsExpired(Card card)
    {
        if (!int.TryParse(card.ExpiryMonth, out var month) ||
            !int.TryParse(card.ExpiryYear, out var year) ||
            month is < 1 or > 12)
        {
            return true;
        }

        var expiresAt = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
        return expiresAt <= DateTime.UtcNow;
    }
}
