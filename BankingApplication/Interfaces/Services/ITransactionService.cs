using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Services
{
    public interface ITransactionService
    {
        bool DepositMoney(string Iban, decimal amount, string currencyCode, string transactionType);
    }
}
