using BankingApplication.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace BankingApplication.Infrastructure
{
    public sealed class JsonStorageProvider : IJsonStorageProvider
    {
        private static readonly ConcurrentDictionary<string, object> FileLocks = new();

        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<JsonStorageProvider> _logger;
        private readonly string _storageRootPath;

        public JsonStorageProvider(ILogger<JsonStorageProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageRootPath = Path.GetFullPath(Directory.GetCurrentDirectory());

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public List<T> ReadAll<T>(string filePath)
        {
            return ExecuteStorageOperation(filePath, resolvedPath =>
            {
                if (!File.Exists(resolvedPath))
                {
                    _logger.LogDebug("File not found for {Type} at {Path}.", typeof(T).Name, resolvedPath);
                    return new List<T>();
                }

                var json = File.ReadAllText(resolvedPath, Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Empty file for {Type} at {Path}.", typeof(T).Name, resolvedPath);
                    return new List<T>();
                }

                var records = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();

                _logger.LogDebug("Read {Count} {Type} records from {Path}.", records.Count, typeof(T).Name, resolvedPath);
                return records;
            });
        }

        public void WriteAll<T>(string filePath, List<T> records)
        {
            ArgumentNullException.ThrowIfNull(records);

            ExecuteStorageOperation(filePath, resolvedPath =>
            {
                string? tempFilePath = null;

                try
                {
                    var directory = Path.GetDirectoryName(resolvedPath)
                                    ?? throw new InvalidOperationException($"Cannot resolve directory for {typeof(T).Name}.");

                    Directory.CreateDirectory(directory);
                    var json = JsonSerializer.Serialize(records, _jsonOptions);

                    tempFilePath = Path.Combine(directory, $".{Path.GetFileName(resolvedPath)}.{Guid.NewGuid():N}.tmp");
                    File.WriteAllText(tempFilePath, json, Encoding.UTF8);
                    File.Move(tempFilePath, resolvedPath, overwrite: true);

                    _logger.LogInformation("Wrote {Count} {Type} records to {Path}.", records.Count, typeof(T).Name, resolvedPath);
                    return true;
                }
                finally
                {
                    DeleteTempFileIfExists(tempFilePath);
                }
            });
        }

        private string ResolvePath(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            var combinedPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(_storageRootPath, filePath);

            var fullPath = Path.GetFullPath(combinedPath);

            if (!IsInsideStorageRoot(fullPath))
            {
                throw new ArgumentException($"The file path must be inside the storage root directory. FilePath: {filePath}", nameof(filePath));
            }

            return fullPath;
        }

        private T ExecuteStorageOperation<T>(string filePath, Func<string, T> operation)
        {
            var resolvedPath = ResolvePath(filePath);
            var fileLock = GetFileLock(resolvedPath);

            lock (fileLock)
            {
                try
                {
                    return operation(resolvedPath);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON error at {FilePath}. Path: {JsonPath}, Line: {Line}.", resolvedPath, ex.Path, ex.LineNumber);
                    throw;
                }
                catch (NotSupportedException ex)
                {
                    _logger.LogError(ex, "Unsupported type at {FilePath}.", resolvedPath);
                    throw;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "I/O error at {FilePath}.", resolvedPath);
                    throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Access denied at {FilePath}.", resolvedPath);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected storage error at {FilePath}.", resolvedPath);
                    throw;
                }
            }
        }
        
        private bool IsInsideStorageRoot(string fullPath)
        {
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var rootPath = EnsureTrailingDirectorySeparator(_storageRootPath);
            return fullPath.StartsWith(rootPath, comparison);
        }

        private static string EnsureTrailingDirectorySeparator(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath.EndsWith(Path.DirectorySeparatorChar) ? fullPath : fullPath + Path.DirectorySeparatorChar;
        }

        private static object GetFileLock(string resolvedPath)
        {
            return FileLocks.GetOrAdd(resolvedPath, _ => new object());
        }

        private void DeleteTempFileIfExists(string? tempFilePath)
        {
            if (string.IsNullOrWhiteSpace(tempFilePath) || !File.Exists(tempFilePath))
            {
                return;
            }

            try
            {
                File.Delete(tempFilePath);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary storage file at {TempFilePath}.", tempFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied while deleting temporary storage file at {TempFilePath}.", tempFilePath);
            }
        }
    }
}