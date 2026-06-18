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

            var resolvedPath = ResolvePath(filePath);

            if (!File.Exists(resolvedPath))
            {
                _logger.LogDebug("Storage file not found for {EntityType} at {FilePath}.", typeof(T).Name, resolvedPath);
                return new List<T>();
            }

            try
            {
                var json = File.ReadAllText(resolvedPath);
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Failed to read {EntityType} records from {FilePath}", typeof(T).Name, resolvedPath);
                throw;
            }
        }

        public void WriteAll<T>(string filePath, List<T> records)
        {
            ValidateFilePath(filePath);
            ArgumentNullException.ThrowIfNull(records);

            var resolvedPath = ResolvePath(filePath);

            try
            {
                var directory = Path.GetDirectoryName(resolvedPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(records, _jsonOptions);
                File.WriteAllText(resolvedPath, json);
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Failed to write {EntityType} records to {FilePath}", typeof(T).Name, resolvedPath);
                throw;
            }
        }

        private static void ValidateFilePath(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
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