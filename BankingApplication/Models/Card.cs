using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Models
{
    public class Card
    {
        public int CardId { get; set; }
        public int AccountId { get; set; }
        public string CardNumber { get; set; }
        public string DisplayName { get; set; }
        public string CardType { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Status { get; set; }
        public DateTime BlockedUntil { get; set; }
        public string BlockReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
