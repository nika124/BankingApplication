using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(
            IJsonStorageProvider storageProvider)
            : base(
                storageProvider,
                Path.Combine("Storage", "transactions.json"))
        {
        }

        public Transaction? GetTransaction(int transactionId)
        {
            return FindOne(transaction =>
                transaction.TransactionId == transactionId);
        }

        public IReadOnlyList<Transaction> GetByAccountId(int accountId)
        {
            return FindMany(transaction => transaction.AccountId == accountId);
        }

        public bool Update(Transaction transaction)
        {
            return UpdateOne(
                existing => existing.TransactionId == transaction.TransactionId,
                transaction);
        }

        public bool Delete(int transactionid)
        {
            return DeleteOne(transaction =>
                transaction.TransactionId == transactionid);
        }
    }
}
