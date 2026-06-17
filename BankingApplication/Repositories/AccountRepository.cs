using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(IJsonStorageProvider storageProvider)
            : base(storageProvider, Path.Combine("Storage", "accounts.json"))
        {
        }

        public Account? GetByAccountId(int accountId)
        {
            return FindOne(account => account.AccountId == accountId);
        }

        public bool Update(Account account)
        {
            return UpdateOne(
                existing => existing.AccountId == account.AccountId,
                account);
        }

        public bool Delete(int accountId)
        {
            return DeleteOne(account => account.AccountId == accountId);
        }
    }
}
