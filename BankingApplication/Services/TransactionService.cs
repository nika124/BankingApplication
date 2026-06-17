using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public bool DepositMoney(string iban, decimal amount, string currencyCode, string transactionType)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new ArgumentException("IBAN is required.", nameof(iban));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Deposit amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException(
                    "Currency code is required.",
                    nameof(currencyCode));
            }

            var transaction = new Transaction
            {
                AccountId = 1,
                Amount = amount,
                CurrencyCode = currencyCode.ToUpperInvariant(),
                TransactionType = "Test",
                Status = "Test",
                TransactionDate = DateTime.UtcNow
            };

            _transactionRepository.Add(transaction);

            return true;
        }
    }
}
