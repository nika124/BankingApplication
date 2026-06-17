using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        IReadOnlyList<T> GetAll();
        void Add(T entity);
    }
}
