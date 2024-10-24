namespace KMSChanger.Services
{
    public class FileLogger : ILogger
    {
        private readonly string _logPath;
        private readonly object _lockObject = new();

        public FileLogger(string logPath = "kms_changer.log")
        {
            _logPath = logPath;
            InitializeLogFile();
        }

        private void InitializeLogFile()
        {
            var logDirectory = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public void LogInfo(string message) => LogMessage("INFO", message);
        public void LogError(string message) => LogMessage("ERROR", message);
        public void LogWarning(string message) => LogMessage("WARNING", message);
        public void LogDebug(string message) => LogMessage("DEBUG", message);

        private void LogMessage(string level, string message)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            lock (_lockObject)
            {
                try
                {
                    File.AppendAllText(_logPath, logMessage + Environment.NewLine);
                }
                catch
                {
                    // Игнорируем ошибки записи лога
                }
            }
        }
    }
}
