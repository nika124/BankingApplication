using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface IAccountBalanceRepository : IRepository<AccountBalance>
    {
        AccountBalance? Get(int accountId, string currencyCode);
        IReadOnlyList<AccountBalance> GetByAccountId(int accountId);
        bool Update(AccountBalance accountBalance);
        bool Delete(int accountId, string currencyCode);
    }
}
