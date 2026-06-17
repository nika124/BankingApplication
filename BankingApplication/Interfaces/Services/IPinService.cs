using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Services
{
    public interface IPinService
    {
        bool ValidatePin(int cardId, string pin);
        bool ChangePin(int cardId, string newPin);
    }
}
