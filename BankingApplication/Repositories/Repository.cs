using BankingApplication.Interfaces.Infrastructure;
using BankingApplication.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace BankingApplication.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        private readonly IJsonStorageProvider _storageProvider;
        private readonly string _filePath;

        protected Repository(IJsonStorageProvider storageProvider, string filePath)
        {
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            _filePath = filePath;
        }

        public IReadOnlyList<T> GetAll()
        {
            return Load().AsReadOnly();
        }

        public void Add(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

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
            return Load().Where(predicate).ToList().AsReadOnly();
        }

        protected bool UpdateOne(Func<T, bool> predicate, T updatedEntity)
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

        protected bool UpdateOne(Func<T, bool> predicate, Action<T> updateAction)
        {
            var records = Load();
            var entity = records.FirstOrDefault(predicate);

            if (entity is null)
            {
                return false;
            }

            updateAction(entity);
            Save(records);

            return true;
        }

        protected bool DeleteOne(Func<T, bool> predicate)
        {
            var records = Load();
            var index = records.FindIndex(record => predicate(record));

            if (index == -1)
            {
                return false;
            }

            records.RemoveAt(index);
            Save(records);

            return true;
        }

        protected int DeleteMany(Func<T, bool> predicate)
        {
            var records = Load();
            var removedCount = records.RemoveAll(record => predicate(record));

            if (removedCount == 0)
            {
                return 0;
            }

            Save(records);

            return removedCount;
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