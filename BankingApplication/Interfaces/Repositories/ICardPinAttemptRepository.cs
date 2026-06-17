using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface ICardPinAttemptRepository : IRepository<CardPinAttempt>
    {
        CardPinAttempt? GetByCardId(int cardId);
        bool Update(CardPinAttempt cardPinAttempt);
        bool Delete(int cardId);
    }
}
