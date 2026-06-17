using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByUserId (int userId);
        User? GetByPersonalNumber (string personalNumber);
        bool Update(User user);
        bool Delete(int userId);    
    }
}
