using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class CardPinAttemptRepository : Repository<CardPinAttempt>, ICardPinAttemptRepository
    {
        public CardPinAttemptRepository(
            IJsonStorageProvider storageProvider)
            : base(
                storageProvider,
                Path.Combine("Storage", "cardPinAttempts.json"))
        {
        }

        public CardPinAttempt? GetByCardId(int cardId)
        {
            return FindOne(attempt => attempt.CardId == cardId);
        }

        public bool Update(CardPinAttempt cardPinAttempt)
        {
            return UpdateOne(
                existing => existing.CardId == cardPinAttempt.CardId,
                cardPinAttempt);
        }

        public bool Delete(int cardId)
        {
            return DeleteOne(attempt => attempt.CardId == cardId);
        }
    }
}
