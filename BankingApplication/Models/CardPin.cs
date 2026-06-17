using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Models
{
    public class CardPin
    {
        public int CardId { get; set; }
        public string Pin { get; set; }
        public DateTime LastChangedAt { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
