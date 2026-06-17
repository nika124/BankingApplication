using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Transaction? GetTransaction(int transactionId);
        bool Update(Transaction transaction);
        bool Delete(int transactionId);
    }
}
