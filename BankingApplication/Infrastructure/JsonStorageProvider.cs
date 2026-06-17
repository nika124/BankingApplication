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
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public List<T> ReadAll<T>(string filePath)
        {
            var resolvedPath = ResolvePath(filePath);

            try
            {
                if (!File.Exists(resolvedPath))
                {
                    return new List<T>();
                }

                var json = File.ReadAllText(resolvedPath);
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public void WriteAll<T>(string filePath, List<T> records)
        {
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
            catch (Exception ex)
            {
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