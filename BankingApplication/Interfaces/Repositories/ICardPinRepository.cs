using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface ICardPinRepository : IRepository<CardPin>
    {
        CardPin? GetCardPin(int cardId);
        bool Update(CardPin cardPin);
        bool Delete(int cardId);
    }
}
