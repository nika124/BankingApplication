using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Infrastructure
{
    public interface ILogger
    {
        void LogError(string message, Exception exception);
        void LogAction(string message, Action action);
    }
}
