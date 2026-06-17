using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using BankingApplication.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IJsonStorageProvider storageProvider)
            : base(storageProvider, Path.Combine("Storage", "users.json"))
        {
        }

        public User? GetByUserId(int userId)
        {
            return FindOne(user => user.UserId == userId);
        }

        public User? GetByPersonalNumber(string personalNumber)
        {
            return FindOne(user =>
                user.PersonalNumber == personalNumber);
        }

        public bool Update(User user)
        {
            return UpdateOne(
                existing => existing.UserId == user.UserId,
                user);
        }

        public bool Delete(int userId)
        {
            return DeleteOne(user => user.UserId == userId);
        }
    }
}