namespace KMSChanger.Services
{
    public class FileLogger : ILogger
    {
        private readonly string _logPath;

        public FileLogger(string logPath = "kms_changer.log")
        {
            _logPath = logPath;
        }

        public void LogInfo(string message) => LogMessage("INFO", message);
        public void LogError(string message) => LogMessage("ERROR", message);
        public void LogWarning(string message) => LogMessage("WARNING", message);

        private void LogMessage(string level, string message)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(_logPath, logMessage + Environment.NewLine);
        }
    }
}
