namespace KMSChanger.Services
{
    public class WindowsActivationService
    {
        private readonly ILogger _logger;
        private readonly ConfigurationService _configService;
        private readonly SystemInfoService _systemInfoService;
        private readonly NetworkService _networkService;

        public WindowsActivationService(
            ILogger logger,
            ConfigurationService configService,
            SystemInfoService systemInfoService,
            NetworkService networkService)
        {
            _logger = logger;
            _configService = configService;
            _systemInfoService = systemInfoService;
            _networkService = networkService;
        }

        public async Task<WindowsActivationResult> ActivateWindowsAsync()
        {
            try
            {
                var config = await _configService.LoadConfigAsync();
                var windowsInfo = await _systemInfoService.GetWindowsInfoAsync();

                // Проверяем текущий статус активации
                if (await IsWindowsActivated())
                {
                    return new WindowsActivationResult(true, "Windows уже активирована", windowsInfo);
                }

                // Устанавливаем ключ продукта если необходим
                var productKey = config.GetProductKeyByEdition(windowsInfo.Edition);
                if (string.IsNullOrEmpty(windowsInfo.CurrentProductKey) && !string.IsNullOrEmpty(productKey))
                {
                    if (!await InstallProductKey(productKey))
                    {
                        return new WindowsActivationResult(false, "Ошибка установки ключа продукта", windowsInfo);
                    }
                }

                // Ищем работающий KMS сервер
                var workingServer = await _networkService.FindWorkingKmsServer(config.KmsServers);
                if (workingServer == null)
                {
                    return new WindowsActivationResult(false, "Не найден доступный KMS сервер", windowsInfo);
                }

                // Устанавливаем KMS сервер и активируем Windows
                if (!await SetKmsServer(workingServer))
                {
                    return new WindowsActivationResult(false, "Ошибка установки KMS сервера", windowsInfo);
                }

                if (!await ActivateWindows())
                {
                    return new WindowsActivationResult(false, "Ошибка активации Windows", windowsInfo);
                }

                // Получаем обновленную информацию о системе
                var updatedInfo = await _systemInfoService.GetWindowsInfoAsync();
                return new WindowsActivationResult(true, "Windows успешно активирована", updatedInfo)
                {
                    UsedKmsServer = workingServer,
                    AppliedProductKey = productKey
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Критическая ошибка при активации: {ex}");
                return new WindowsActivationResult(false, "Критическая ошибка активации", null, ex.Message);
            }
        }

        private async Task<bool> IsWindowsActivated()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cscript",
                        Arguments = "//nologo C:\\Windows\\System32\\slmgr.vbs /xpr",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output.Contains("постоянная") || output.Contains("permanent");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка проверки статуса активации: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> InstallProductKey(string key)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cscript",
                        Arguments = $"//nologo C:\\Windows\\System32\\slmgr.vbs /ipk {key}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка установки ключа продукта: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SetKmsServer(string server)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cscript",
                        Arguments = $"//nologo C:\\Windows\\System32\\slmgr.vbs /skms {server}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка установки KMS сервера: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ActivateWindows()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cscript",
                        Arguments = "//nologo C:\\Windows\\System32\\slmgr.vbs /ato",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка активации Windows: {ex.Message}");
                return false;
            }
        }
    }
}
