namespace KMSChanger.Services
{
    public class SystemInfoService
    {
        private readonly ILogger _logger;

        public SystemInfoService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<WindowsInfo> GetWindowsInfoAsync()
        {
    try
    {
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        using var results = searcher.Get();
        var os = results.Cast<ManagementObject>().FirstOrDefault() 
            ?? throw new InvalidOperationException("Не удалось получить информацию о системе");

        var info = new WindowsInfo
        {
            Edition = os["Caption"]?.ToString() ?? "Unknown",
            Version = os["Version"]?.ToString() ?? "Unknown",
            Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
            CurrentProductKey = await GetCurrentProductKey(),
            ActivationStatus = await GetActivationStatus(),
            InstallDate = ManagementDateTimeConverter.ToDateTime(os["InstallDate"]?.ToString() ?? string.Empty),
            RegisteredOwner = os["RegisteredUser"]?.ToString() ?? "Unknown",
            RegisteredOrganization = os["Organization"]?.ToString() ?? "Unknown"
        };

        _logger.LogInfo($"Получена информация о системе: {info.Edition}");
        return info;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Ошибка получения информации о системе: {ex.Message}");
        throw;
    }
}


        private async Task<string> GetCurrentProductKey()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmic",
                        Arguments = "path softwarelicensingservice get OA3xOriginalProductKey",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .LastOrDefault()?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения текущего ключа продукта: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> GetActivationStatus()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "slmgr.vbs",
                        Arguments = "/dli",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения статуса активации: {ex.Message}");
                return "Неизвестно";
            }
        }
    }
}
