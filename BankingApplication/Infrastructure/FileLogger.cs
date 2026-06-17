using BankingApplication.Interfaces.Infrastructure;

namespace BankingApplication.Infrastructure
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            EnsureDirectoryExists();
        }

        public void LogError(string message, Exception exception)
        {
            EnsureDirectoryExists();

            var log =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}" +
                $"{exception}{Environment.NewLine}{Environment.NewLine}";

            File.AppendAllText(_filePath, log);
        }

        public void LogAction(string message, Action action)
        {
            EnsureDirectoryExists();

            try
            {
                action();
                var logEntry = $"{DateTime.Now}: ACTION - {message} - Status: Success{Environment.NewLine}";
                File.AppendAllText(_filePath, logEntry);
            }
            catch (Exception ex)
            {
                var logEntry = $"{DateTime.Now}: ACTION - {message} - Status: Failed - Exception: {ex.Message}{Environment.NewLine}";
                File.AppendAllText(_filePath, logEntry);
            }
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
