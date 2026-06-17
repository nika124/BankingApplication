using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class AccountBalanceRepository : Repository<AccountBalance>, IAccountBalanceRepository
    {
        public AccountBalanceRepository(
            IJsonStorageProvider storageProvider)
            : base(
                storageProvider,
                Path.Combine("Storage", "accountBalances.json"))
        {
        }

        public AccountBalance? Get(
            int accountId,
            string currencyCode)
        {
            return FindOne(balance =>
                balance.AccountId == accountId &&
                balance.CurrencyCode.Equals(
                    currencyCode,
                    StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<AccountBalance> GetByAccountId(int accountId)
        {
            return FindMany(balance =>
                balance.AccountId == accountId);
        }

        public bool Update(AccountBalance balance)
        {
            return UpdateOne(
                existing =>
                    existing.AccountId == balance.AccountId &&
                    existing.CurrencyCode.Equals(
                        balance.CurrencyCode,
                        StringComparison.OrdinalIgnoreCase),
                balance);
        }

        public bool Delete(
            int accountId,
            string currencyCode)
        {
            return DeleteOne(balance =>
                balance.AccountId == accountId &&
                balance.CurrencyCode.Equals(
                    currencyCode,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
