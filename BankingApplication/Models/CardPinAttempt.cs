using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Models
{
    public class CardPinAttempt
    {
        public int AttemptId { get; set; }
        public int CardId { get; set; }
        public bool WasSuccessful { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}
