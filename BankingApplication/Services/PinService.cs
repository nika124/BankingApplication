using System;
using System.Collections.Generic;
using System.Text;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;

namespace BankingApplication.Services
{
    public class PinService : IPinService
    {
private readonly ICardRepository _cardRepository;
        private readonly ICardPinRepository _cardPinRepository;
        private readonly ICardPinAttemptRepository _cardPinAttemptRepository;

        public PinService(
            ICardRepository cardRepository,
            ICardPinRepository cardPinRepository,
            ICardPinAttemptRepository cardPinAttemptRepository)
        {
            _cardRepository = cardRepository;
            _cardPinRepository = cardPinRepository;
            _cardPinAttemptRepository = cardPinAttemptRepository;
        }

        public bool ValidatePin(int cardId, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                return false;
            }

            var card = _cardRepository.GetAll().FirstOrDefault(card => card.CardId == cardId);

            if (card is null)
            {
                return false;
            }

            if (card.Status == "Blocked" && card.BlockedUntil > DateTime.UtcNow)
            {
                return false;
            }

            var cardPin = _cardPinRepository.GetCardPin(cardId);

            if (cardPin is null)
            {
                return false;
            }

            var isCorrect = cardPin.Pin == pin;

            AddPinAttempt(cardId, isCorrect);

            if (!isCorrect)
            {
                BlockCardIfNecessary(card);
            }

            return isCorrect;
        }

        public bool ChangePin(int cardId, string newPin)
        {
            if (string.IsNullOrWhiteSpace(newPin))
            {
                return false;
            }

            if (newPin.Length != 4 || !newPin.All(char.IsDigit))
            {
                return false;
            }

            var cardPin = _cardPinRepository.GetCardPin(cardId);

            if (cardPin is null)
            {
                return false;
            }

            cardPin.Pin = newPin;
            cardPin.LastChangedAt = DateTime.UtcNow;

            return _cardPinRepository.Update(cardPin);
        }

        private void AddPinAttempt(int cardId, bool wasSuccessful)
        {
            var attempts = _cardPinAttemptRepository.GetAll();

            var nextAttemptId = attempts.Count == 0
                ? 1
                : attempts.Max(attempt => attempt.AttemptId) + 1;

            var attempt = new CardPinAttempt
            {
                AttemptId = nextAttemptId,
                CardId = cardId,
                WasSuccessful = wasSuccessful,
                AttemptedAt = DateTime.UtcNow
            };

            _cardPinAttemptRepository.Add(attempt);
        }

        private void BlockCardIfNecessary(Card card)
        {
            var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);

            var recentAttempts = _cardPinAttemptRepository
                .GetAll()
                .Where(attempt =>
                    attempt.CardId == card.CardId &&
                    attempt.AttemptedAt >= tenMinutesAgo)
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
}
