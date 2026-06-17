using BankingApplication.Interfaces.Infrastructure;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BankingApplication.Infrastructure
{
    public class JsonStorageProvider : IJsonStorageProvider
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<JsonStorageProvider> _logger;

        public JsonStorageProvider(ILogger<JsonStorageProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public List<T> ReadAll<T>(string filePath)
        {
            ValidateFilePath(filePath);
            var resolvedPath = filePath;

            try
            {
                resolvedPath = ResolvePath(filePath);

                if (!File.Exists(resolvedPath))
                {
                    _logger.LogDebug("Storage file not found for {EntityType} at {FilePath}; returning empty collection", typeof(T).Name, resolvedPath);

                    return new List<T>();
                }

                var json = File.ReadAllText(resolvedPath);
                var records = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);

                if (records is null)
                {
                    throw new InvalidDataException($"Storage file for {typeof(T).Name} did not contain a JSON array.");
                }

                return records;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to read {EntityType} records from {FilePath}", typeof(T).Name, resolvedPath);

                throw;
            }
        }

        public void WriteAll<T>(string filePath, List<T> records)
        {
            ValidateFilePath(filePath);

            if (records is null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            var resolvedPath = filePath;

            try
            {
                resolvedPath = ResolvePath(filePath);
                var directory = Path.GetDirectoryName(resolvedPath);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(records, _jsonOptions);
                File.WriteAllText(resolvedPath, json);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to write {EntityType} records to {FilePath}", typeof(T).Name, resolvedPath);
                throw;
            }
        }

        private static void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required.", nameof(filePath));
            }
        }

        private static string ResolvePath(string filePath)
        {
            if (Path.IsPathRooted(filePath))
            {
                return filePath;
            }

            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null)
            {
                if (directory.GetFiles("*.csproj").Any())
                {
                    return Path.Combine(directory.FullName, filePath);
                }

                directory = directory.Parent;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), filePath);
        }
    }
}