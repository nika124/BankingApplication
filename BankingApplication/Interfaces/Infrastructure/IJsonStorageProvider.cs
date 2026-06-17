using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Infrastructure
{
    public interface IJsonStorageProvider
    {
        List<T> ReadAll<T>(string filePath);
        void WriteAll<T>(string filePath, List<T> records);
    }
}
