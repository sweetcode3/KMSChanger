using System;
using System.Diagnostics;
using System.Text.Json;
using System.Management;
using System.Net.NetworkInformation;

namespace KMSChanger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var config = await LoadConfigAsync();
                var windowsInfo = GetWindowsInfo();
                var currentKey = GetCurrentProductKey();
                
                if (string.IsNullOrEmpty(currentKey))
                {
                    var newKey = GetKeyForProduct(config.ProductKeys, windowsInfo.Edition);
                    if (!await InstallProductKey(newKey))
                    {
                        ShowError("Ошибка установки ключа продукта");
                        return;
                    }
                }

                foreach (var server in config.KmsServers)
                {
                    if (await TryActivateWithServer(server))
                    {
                        ShowSuccess(windowsInfo);
                        return;
                    }
                }

                ShowError("Не удалось активировать Windows с доступными KMS серверами");
            }
            catch (Exception ex)
            {
                ShowError($"Критическая ошибка: {ex.Message}");
            }
        }

        private static async Task<Config> LoadConfigAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync("config.json");
                return JsonSerializer.Deserialize<Config>(json);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка чтения конфигурации", ex);
            }
        }

        private static WindowsInfo GetWindowsInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                using var results = searcher.Get();
                var os = results.Cast<ManagementObject>().First();
                
                return new WindowsInfo
                {
                    Edition = os["Caption"].ToString(),
                    Version = os["Version"].ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения информации о Windows", ex);
            }
        }

        private static string GetCurrentProductKey()
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
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static async Task<bool> InstallProductKey(string key)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "slmgr.vbs",
                        Arguments = $"/ipk {key}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> TryActivateWithServer(string server)
        {
            try
            {
                if (!await PingServer(server))
                    return false;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "slmgr.vbs",
                        Arguments = $"/skms {server}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                await process.WaitForExitAsync();
                
                if (process.ExitCode != 0)
                    return false;

                process.StartInfo.Arguments = "/ato";
                process.Start();
                await process.WaitForExitAsync();
                
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> PingServer(string server)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(server);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private static void ShowSuccess(WindowsInfo info)
        {
            MessageBox.Show(
                $"Windows успешно активирована\n\nВерсия: {info.Edition}\nСборка: {info.Version}",
                "Успех",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    class Config
    {
        public Dictionary<string, string> ProductKeys { get; set; }
        public List<string> KmsServers { get; set; }
    }

    class WindowsInfo
    {
        public string Edition { get; set; }
        public string Version { get; set; }
    }
}
