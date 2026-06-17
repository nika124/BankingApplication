using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class CardRepository : Repository<Card>, ICardRepository
    {
        public CardRepository(IJsonStorageProvider storageProvider)
            : base(storageProvider, Path.Combine("Storage", "cards.json"))
        {
        }

        public Card? GetByCardNumber(string cardNumber)
        {
            return FindOne(card => card.CardNumber == cardNumber);
        }

        public bool Update(Card card)
        {
            return UpdateOne(
                existing => existing.CardNumber == card.CardNumber,
                card);
        }

        public bool Delete(string cardNumber)
        {
            return DeleteOne(card => card.CardNumber == cardNumber);
        }
    }
}
