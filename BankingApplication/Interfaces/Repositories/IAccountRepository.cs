using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Account? GetByAccountId(int accountId);
        bool Update(Account account);
        bool Delete(int accountId);
    }
}
