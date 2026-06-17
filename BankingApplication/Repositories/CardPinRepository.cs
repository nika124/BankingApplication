using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class CardPinRepository : Repository<CardPin>, ICardPinRepository
    {
        public CardPinRepository(IJsonStorageProvider storageProvider)
            : base(storageProvider, Path.Combine("Storage", "cardPins.json"))
        {
        }

        public CardPin? GetCardPin(int cardId)
        {
            return FindOne(cardPin => cardPin.CardId == cardId);
        }

        public bool Update(CardPin cardPin)
        {
            return UpdateOne(
                existing => existing.CardId == cardPin.CardId,
                cardPin);
        }

        public bool Delete(int cardId)
        {
            return DeleteOne(cardPin => cardPin.CardId == cardId);
        }
    }
}
