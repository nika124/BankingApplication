using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Models
{
    public class AccountBalance
    {
        public int AccountId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Balance { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
