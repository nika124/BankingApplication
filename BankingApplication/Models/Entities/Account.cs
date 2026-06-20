using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string Iban { get; set; }
        public string DisplayName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
