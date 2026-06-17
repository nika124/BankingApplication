using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingApplication.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        private readonly IJsonStorageProvider _storageProvider;
        private readonly string _filePath;

        protected Repository(
            IJsonStorageProvider storageProvider,
            string filePath)
        {
            _storageProvider = storageProvider;
            _filePath = filePath;
        }

        public IReadOnlyList<T> GetAll()
        {
            return Load();
        }

        public void Add(T entity)
        {
            var records = Load();

            records.Add(entity);

            Save(records);
        }

        protected T? FindOne(Func<T, bool> predicate)
        {
            return Load().FirstOrDefault(predicate);
        }

        protected IReadOnlyList<T> FindMany(Func<T, bool> predicate)
        {
            return Load().Where(predicate).ToList();
        }

        protected bool UpdateOne(
            Func<T, bool> predicate,
            T updatedEntity)
        {
            var records = Load();
            var index = records.FindIndex(record => predicate(record));

            if (index == -1)
            {
                return false;
            }

            records[index] = updatedEntity;

            Save(records);

            return true;
        }

        protected bool DeleteOne(Func<T, bool> predicate)
        {
            var records = Load();
            var record = records.FirstOrDefault(predicate);

            if (record is null)
            {
                return false;
            }

            records.Remove(record);

            Save(records);

            return true;
        }

        private List<T> Load()
        {
            return _storageProvider.ReadAll<T>(_filePath);
        }

        private void Save(List<T> records)
        {
            _storageProvider.WriteAll(_filePath, records);
        }
    }
}
