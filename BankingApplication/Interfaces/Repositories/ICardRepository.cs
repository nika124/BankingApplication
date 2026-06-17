using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface ICardRepository : IRepository<Card>
    {
        Card? GetByCardNumber(string cardNumber);
        bool Update(Card card);
        bool Delete(string cardNumber);
    }
}
